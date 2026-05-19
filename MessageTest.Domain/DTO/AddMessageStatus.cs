using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public enum AddMessageStatus
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
        /// 新增留言已被處理
        /// </summary>
        AddSujectAlreadyExist = 2,

        /// <summary>
        /// 新增留言冷卻中
        /// </summary>
        AddMessageColdDown = 3,
    }
}
