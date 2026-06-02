using MessageTest.Domain.DTO;
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
    public interface IProQuerySubject: IProcedureProcess<CtxMessage, IntegrateMessageRequestDto>
    {

    }
    public class ProQuerySubject : IProQuerySubject, IProcedureProcess<CtxMessage, IntegrateMessageRequestDto>
    {
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProQuerySubject));

        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        public CtxMessage Process(CtxMessage ctx, IntegrateMessageRequestDto param)
        {
            var querySubjectResult = this.subjectRepository.GetById(param.SubjectId);
            if (querySubjectResult.exception != null)
            {
                ctx.Exception = querySubjectResult.exception;
                logger.Warn("process error: mongo get by id has not subject");
            }
            int currentMessageCount;
            if (querySubjectResult.subject != null)
            {
                currentMessageCount = querySubjectResult.subject.MessageCount;
                foreach (var message in param.Messages)
                {
                    currentMessageCount++;
                    message.Floor = currentMessageCount;
                }
                ctx.subject = querySubjectResult.subject;
                ctx.messages = param.Messages;
                return ctx;
            }

            var querySubjectPoResult = this.subjectPoRepository.GetById(param.SubjectId);
            if (querySubjectPoResult.exception != null)
            {
                ctx.Exception = querySubjectPoResult.exception;
                logger.Error(querySubjectPoResult.exception, "process error: mssql get by id expection");
                return ctx;
            }

            if (querySubjectPoResult.subject == null)
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.QuerySubjectNoExsit, $"mssql get by id has not subject");
                logger.Warn("process error: mssql get by id has not subject");
                return ctx;
            }
            
            currentMessageCount = querySubjectPoResult.subject.MessageCount;
            foreach (var message in param.Messages)
            {
                currentMessageCount++;
                message.Floor = currentMessageCount;
            }
            ctx.subject = querySubjectPoResult.subject;
            ctx.messages = param.Messages;
            return ctx;
        }
    }
}
