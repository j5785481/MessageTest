using MessageTest.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Procedure.MessageProcedure
{
    
    public class MessageException : Exception
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public MessageProcedureErrorCode ErrorCode { get; set; }

        public MessageException(MessageProcedureErrorCode code) : base(code.ToString())
        {
            ErrorCode = code;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public MessageException(MessageProcedureErrorCode code, string message) : base($"{code.ToString()}: {message}")
        {
            ErrorCode = code;
        }
    }
}
