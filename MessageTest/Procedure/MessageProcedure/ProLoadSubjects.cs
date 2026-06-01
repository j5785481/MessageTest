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
    public interface IProLoadSubjects : IProcedureProcess<CtxMessage>
    {

    }
    public class ProLoadSubjects : IProLoadSubjects, IProcedureProcess<CtxMessage>
    {
        private readonly ILogger logger =
        LogManager.GetLogger("MessageTest").WithProperty("Type", nameof(ProLoadSubjects));

        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;

        public ProLoadSubjects(ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository)
        {
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
        }

        public CtxMessage Process(CtxMessage ctx)
        {
            var subjectIds = ctx.messages.Select(m => m.SubjectId).ToList();
            var mongoGetByIdsResult = this.subjectRepository.GetByIds(subjectIds);
            if (mongoGetByIdsResult.exception != null) 
            {
                ctx.Exception = mongoGetByIdsResult.exception;
                logger.Error(mongoGetByIdsResult.exception, "process error: mongo subject get by ids expection");
                return ctx;
            }
            if (mongoGetByIdsResult.subjects.Count > 0)
            {
                //ctx.subjects = mongoGetByIdsResult.subjects;
                return ctx;
            }
            var mssqlGetByIdsResult = this.subjectPoRepository.GetByIds(subjectIds);
            if (mssqlGetByIdsResult.exception != null) 
            {
                ctx.Exception = mssqlGetByIdsResult.exception;
                logger.Error(mssqlGetByIdsResult.exception, "process error: mssql subject get by ids expection");
                return ctx;
            }
            if (mssqlGetByIdsResult.subjects == null || !mssqlGetByIdsResult.subjects.Any())
            {
                ctx.Exception = new MessageException(MessageProcedureErrorCode.MssqlSubjectGetByIdsFail, "process error: mssql subject get by ids fail");
                logger.Warn(mssqlGetByIdsResult.exception, "process error: mssql subject get by ids fail");
                return ctx;
            }
            //ctx.subjects = mssqlGetByIdsResult.subjects;
            return ctx;
        }
    }
}
