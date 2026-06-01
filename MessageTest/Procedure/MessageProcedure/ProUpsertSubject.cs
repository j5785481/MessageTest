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
    public interface IProUpsertSubjects : IProcedureProcess<CtxMessage>
    {

    }
    public class ProUpsertSubject : IProUpsertSubjects, IProcedureProcess<CtxMessage>
    {
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProUpsertSubject));

        public ProUpsertSubject(ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository)
        {
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
        }

        public CtxMessage Process(CtxMessage ctx)
        {
            var mssqlUpsertResult = this.subjectPoRepository.Upsert(ctx.subject);
            if (mssqlUpsertResult.exception != null) 
            {
                ctx.Exception = mssqlUpsertResult.exception;
                logger.Error(mssqlUpsertResult.exception, "process error: mssql subject upsert expection");
                return ctx;
            }
            if (mssqlUpsertResult.subject == null)
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.MssqlSubjectUpsertFail, $"process error: mssql subject upsert fail");
                logger.Warn($"process error: mssql batch upsert fail {ctx.subject}");
            }
            var mongoUpsertExpcetion = this.subjectRepository.Save(ctx.subject);
            if (mongoUpsertExpcetion != null) 
            {
                ctx.Exception = mongoUpsertExpcetion;
                logger.Error(mongoUpsertExpcetion, "process error: mongo subject upsert expection");
                return ctx;
            }
            return ctx;
        }
    }
}
