using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Lib.Procedure.Implements;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Procedure.MessageProcedure
{

    /// <summary>
    ///     載入快取的 Process 介面。
    /// </summary>
    public interface IProLoadCaches : IProcedureProcess<CtxMessage>
    {
    }
    public class ProLoadCaches : IProLoadCaches, IProcedureProcess<CtxMessage>
    {
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProLoadCaches));

        private readonly IMessageCacheRepository messageCacheRepository;

        public ProLoadCaches(IMessageCacheRepository messageCacheRepository)
        {
            this.messageCacheRepository = messageCacheRepository;
        }
        public CtxMessage Process(CtxMessage ctx)
        {
            var cache = messageCacheRepository.Pop<Message>(5);
            if (cache.exception != null) 
            {
                ctx.Exception = cache.exception;
                logger.Error(cache.exception, "process error: load redis cache expection");
                return ctx;
            }
            
            if (cache.actions == null || !cache.actions.Any())
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.RedisHasNotData, $"Pop Redis data null or no data");
                logger.Warn("process error: load redis cache pop null or no data");
                return ctx;
            }
            ctx.messages = cache.actions.ToList();
            return ctx;
        }
    }
}
