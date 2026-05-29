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
        /// <summary>
        ///     訊息集合。
        /// </summary>
        public List<Message> messages { get; set; } = new List<Message>();

        /// <summary>
        ///     主題集合。
        /// </summary>
        public List<Subject> subjects { get; set; } = new List<Subject>();
    }
}
