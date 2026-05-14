using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public enum QuerySubjectStatus
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
        /// 沒有該主題
        /// </summary>
        NoHaveSubject = 2,
    }
}
