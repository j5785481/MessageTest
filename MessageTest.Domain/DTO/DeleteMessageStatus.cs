using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public enum DeleteMessageStatus
    {
        /// <summary>
        ///     未知錯誤
        /// </summary>
        UnknownError = 0,

        /// <summary>
        ///     成功
        /// </summary>
        Success = 1,

        /// <summary>
        ///     失敗
        /// </summary>
        Fail = 2,

        /// <summary>
        /// User沒有留言
        /// </summary>
        UserHasNotMessage = 3,
    }
}
