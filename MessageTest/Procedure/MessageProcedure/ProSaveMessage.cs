using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Lib.Procedure.Implements;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Procedure.MessageProcedure
{
    public interface IProSaveMessage : IProcedureProcess<CtxMessage>
    {

    }
    public class ProSaveMessage : IProSaveMessage, IProcedureProcess<CtxMessage>
    {
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProSaveMessage));

        private IMessagePoRepository messagePoRepository;

        private IMessageRepository messageRepository;

        public ProSaveMessage(IMessagePoRepository messagePoRepository, IMessageRepository messageRepository)
        {
            this.messagePoRepository = messagePoRepository;
            this.messageRepository = messageRepository;
        }

        public CtxMessage Process(CtxMessage ctx)
        {
            var batchAddResult = this.messagePoRepository.BatchAdd(ctx.messages);
            if (batchAddResult.exception != null)
            {
                ctx.Exception = batchAddResult.exception;
                logger.Error(batchAddResult.exception, "process error: mssql batch add expection");
                return ctx;
            }
            if (batchAddResult.messages.Count == 0)
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.MssqlBatchAddFail, $"mssql batch add failed");
                logger.Warn($"process error: mssql batch add failed {ctx.messages}");
                return ctx;
            }
            var batchSaveExpection = this.messageRepository.BatchSave(ctx.messages);
            if (batchSaveExpection != null)
            {
                ctx.Exception = batchSaveExpection;
                logger.Error(batchSaveExpection, "process error: mssql batch add expection");
                return ctx;
            }
            return ctx;
        }
    }
}
