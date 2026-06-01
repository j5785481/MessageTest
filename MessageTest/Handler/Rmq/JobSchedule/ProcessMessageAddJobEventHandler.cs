using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Live.PubSub.Core;
using MessageTest.Applibs;
using MessageTest.DistributedLock;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Hubs;
using MessageTest.Lib.Procedure.Implements;
using MessageTest.Procedure.MessageProcedure;
using Newtonsoft.Json;
using NLog;

namespace MessageTest.Handler.Rmq.JobSchedule
{
    public class ProcessMessageAddJobEventHandler : IRabbitMqPubSubHandler
    {
        private readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(ProcessMessageAddJobEventHandler));

        private readonly IMessageCacheRepository messageCacheRepository;
        private readonly IMessagePoRepository messagePoRepository;
        private readonly IMessageRepository messageRepository;
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly ILifetimeScope lifetimeScope;

        public ProcessMessageAddJobEventHandler(IMessageCacheRepository messageCacheRepository, IMessagePoRepository messagePoRepository, IMessageRepository messageRepository, 
            ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository, ILifetimeScope lifetimeScope)
        {
            this.messageCacheRepository = messageCacheRepository;
            this.messagePoRepository = messagePoRepository;
            this.messageRepository = messageRepository;
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
            this.lifetimeScope = lifetimeScope;
        }

        public bool Handle(RabbitMqEventStream stream)
        {
            try
            {
                using (var scope = lifetimeScope.BeginLifetimeScope())
                {
                    var cache = messageCacheRepository.Pop<Message>(5);
                    if (cache.exception != null)
                    {
                        logger.Error(cache.exception, "process error: load redis cache expection");
                        return true;
                    }
                    if (cache.actions == null || !cache.actions.Any())
                    {
                        logger.Warn("process error: load redis cache pop null or no data");
                        return true;
                    }

                    //將Pop出來的資料透過SubjectId分組
                    var integrateMessageRequests = cache.actions
                        .GroupBy(m => m.SubjectId)
                        .Select(g => new IntegrateMessageRequestDto
                        {
                            SubjectId = g.Key,
                            Messages = g.ToList()
                        }).ToList();
                    var locker = scope.Resolve<ISubjectLocker>();
                    foreach (var messageRequest in integrateMessageRequests)
                    {
                        using(var red = locker.GrabLock(messageRequest.SubjectId))
                        {
                            if (!red.IsAcquired) continue;

                            var ctx = BaseProcedure<CtxMessage>
                            .From(new CtxMessage())
                            .Execute(scope.Resolve<IProQuerySubject>(), messageRequest)
                            .Execute(scope.Resolve<IProSaveMessage>())
                            .Execute(scope.Resolve<IProUpdateMessageCount>())
                            .Execute(scope.Resolve<IProUpsertSubjects>())
                            .GetResult();
                            if (ctx.TryGetException(out var exception) && exception is MessageException msgEx)
                            {
                                switch (msgEx.ErrorCode)
                                {
                                    case MessageProcedureErrorCode.QuerySubjectNoExsit:
                                        break;
                                    case MessageProcedureErrorCode.MssqlBatchAddFail:
                                        break;
                                    case MessageProcedureErrorCode.MssqlSubjectGetByIdsFail:
                                        break;
                                    case MessageProcedureErrorCode.MssqlSubjectUpsertFail:
                                        break;
                                    default:
                                        throw exception;
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            catch (MessageException ex)
            {
                logger.Error(ex, "Process MessageException");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Process Fail");
            }

            return true;
        }
    }
}
