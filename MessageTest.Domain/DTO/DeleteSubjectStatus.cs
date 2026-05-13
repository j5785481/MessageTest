using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public enum DeleteSubjectStatus
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
        /// 非原作者或主題不存在
        /// </summary>
        NotOrginAuthorOrSubjectNotExist = 2,
    }
}
