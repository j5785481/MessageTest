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
            var addMessage = ctx.messages.Count;
            ctx.subject.MessageCount += addMessage;
            return ctx;
        }
    }
}
