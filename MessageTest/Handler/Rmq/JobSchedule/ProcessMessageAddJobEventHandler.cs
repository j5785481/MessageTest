using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Live.PubSub.Core;
using MessageTest.Applibs;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Hubs;
using Newtonsoft.Json;
using NLog;

namespace MessageTest.Handler.Rmq.JobSchedule
{
    public class ProcessMessageAddJobEventHandler : IRabbitMqPubSubHandler
    {
        private readonly ILifetimeScope lifetimeScope;
        private readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(ProcessMessageAddJobEventHandler));

        private readonly IMessageCacheRepository messageCacheRepository;
        private readonly IMessagePoRepository messagePoRepository;
        private readonly IMessageRepository messageRepository;
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;

        public ProcessMessageAddJobEventHandler(ILifetimeScope lifetimeScope, 
            IMessageCacheRepository messageCacheRepository, IMessagePoRepository messagePoRepository, IMessageRepository messageRepository, 
            ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository)
        {
            this.lifetimeScope = lifetimeScope;
            this.messageCacheRepository = messageCacheRepository;
            this.messagePoRepository = messagePoRepository;
            this.messageRepository = messageRepository;
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
        }

        public bool Handle(RabbitMqEventStream stream)
        {
            try
            {
                using (var scope = this.lifetimeScope.BeginLifetimeScope())
                {
                    var cache = messageCacheRepository.Pop<Message>(5);
                    if (cache.exception != null) throw cache.exception;
                    if (cache.actions == null || !cache.actions.Any()) return true;

                    List<Message> messages = cache.actions.ToList();

                    // MSSQL 批次儲存訊息
                    var batchAddResult = this.messagePoRepository.BatchAdd(messages);
                    if (batchAddResult.exception != null)
                    {
                        logger.Info($"ProcessMessageAddJobEventHandler messagePoRepository.BatchAdd expcetion{JsonConvert.SerializeObject(batchAddResult.exception)}");
                    }
                    List<int> subjectId = messages.Select(x => x.SubjectId).ToList();
                    // 透過MSSQL查subject物件
                    var subjectGetByIds = this.subjectPoRepository.GetByIds(subjectId);
                    if (subjectGetByIds.exception != null)
                    {
                        logger.Info($"ProcessMessageAddJobEventHandler messagePoRepository.GetByIds expcetion{JsonConvert.SerializeObject(subjectGetByIds.exception)}");
                    }
                    if (subjectGetByIds.subjects != null && subjectGetByIds.subjects.Count > 0)
                    {
                        // 先統計這批訊息中，每個 SubjectId 分別有幾筆新留言
                        var messageCountsBySubject = messages
                            .GroupBy(x => x.SubjectId)
                            .ToDictionary(g => g.Key, g => g.Count());

                        // 再依據統計出來的數量去累加
                        foreach (var s in subjectGetByIds.subjects)
                        {
                            if (messageCountsBySubject.TryGetValue(s.Id, out int newCommentCount))
                            {
                                s.MessageCount += newCommentCount; // 有幾筆就加幾筆
                            }
                        }
                        // 批次更新到MSSQL subject
                        var batchUpsert = this.subjectPoRepository.BatchUpsert(subjectGetByIds.subjects);
                        if (batchUpsert.exception != null) 
                        {
                            logger.Info($"ProcessMessageAddJobEventHandler subjectPoRepository.BatchUpsert expcetion{JsonConvert.SerializeObject(batchUpsert.exception)}");
                        }
                        if (batchUpsert.subjects != null && batchUpsert.subjects.Count > 0)
                        {
                            // 批次儲存Mongo subject
                            var batchUpsertExpection = this.subjectRepository.BatchSave(batchUpsert.subjects);
                            if (batchUpsertExpection != null)
                            {
                                logger.Info($"ProcessMessageAddJobEventHandler subjectRepository.BatchSave expcetion{JsonConvert.SerializeObject(batchUpsertExpection)}");
                            }
                        }
                    } 
                    if (batchAddResult.messages != null && batchAddResult.messages.Count > 0) 
                    {
                        // 批次存入Mongo message
                        var batchSaveExpection = this.messageRepository.BatchSave(batchAddResult.messages);
                        if (batchSaveExpection != null)
                        {
                            logger.Info($"ProcessMessageAddJobEventHandler messageRepository.BatchSave expcetion{JsonConvert.SerializeObject(batchSaveExpection)}");
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Process Fail");
            }

            return true;
        }
    }
}
