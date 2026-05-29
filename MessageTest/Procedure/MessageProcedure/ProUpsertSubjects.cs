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
    public class ProUpsertSubjects : IProUpsertSubjects, IProcedureProcess<CtxMessage>
    {
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProUpsertSubjects));

        public ProUpsertSubjects(ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository)
        {
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
        }

        public CtxMessage Process(CtxMessage ctx)
        {
            var mssqlUpsertResult = this.subjectPoRepository.BatchUpsert(ctx.subjects);
            if (mssqlUpsertResult.exception != null) 
            {
                ctx.Exception = mssqlUpsertResult.exception;
                logger.Error(mssqlUpsertResult.exception, "process error: mssql batch upsert expection");
                return ctx;
            }
            if (mssqlUpsertResult.subjects == null || mssqlUpsertResult.subjects.Any())
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.MssqlSubjectBatchUpsertFail, $"process error: mssql batch upsert fail");
                logger.Warn($"process error: mssql batch upsert fail {ctx.subjects}");
            }
            var mongoUpsertExpcetion = this.subjectRepository.BatchSave(ctx.subjects);
            if (mongoUpsertExpcetion != null) 
            {
                ctx.Exception = mongoUpsertExpcetion;
                return ctx;
            }
            return ctx;
        }
    }
}
