using MessageTest.Domain.Model;
using MessageTest.Lib.Procedure.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Procedure.MessageProcedure
{
    public class CtxMessage : BaseProcedureContext
    {
        public Subject subject;
        /// <summary>
        ///     訊息集合。
        /// </summary>
        public List<Message> messages { get; set; } = new List<Message>();
    }
}
