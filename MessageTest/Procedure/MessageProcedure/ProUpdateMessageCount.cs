using MessageTest.Lib.Procedure.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Procedure.MessageProcedure
{
    public interface IProUpdateMessageCount : IProcedureProcess<CtxMessage> 
    {
    }
    public class ProUpdateMessageCount : IProUpdateMessageCount, IProcedureProcess<CtxMessage>
    {
        public CtxMessage Process(CtxMessage ctx)
        {
            var messageCountsBySubject = ctx.messages
                    .GroupBy(x => x.SubjectId)
                    .ToDictionary(g => g.Key, g => g.Count());

            // 再依據統計出來的數量去累加
            foreach (var s in ctx.subjects)
            {
                if (messageCountsBySubject.TryGetValue(s.Id, out int newCommentCount))
                {
                    s.MessageCount += newCommentCount; // 有幾筆就加幾筆
                }
            }
            return ctx;
        }
    }
}
