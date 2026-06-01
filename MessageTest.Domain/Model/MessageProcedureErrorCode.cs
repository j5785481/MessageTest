using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    /// <summary>
    /// 集成留言Exception Code
    /// </summary>
    public enum MessageProcedureErrorCode
    {
        /// <summary>
        /// 未知錯誤
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 主題不存在
        /// </summary>
        QuerySubjectNoExsit,

        /// <summary>
        /// Redis Pop沒有資料
        /// </summary>
        RedisHasNotData,

        /// <summary>
        /// MSSQL批次新增沒返回資料
        /// </summary>
        MssqlBatchAddFail,

        /// <summary>
        /// MSSQL批次查詢主題沒返回資料
        /// </summary>
        MssqlSubjectGetByIdsFail,

        /// <summary>
        /// MSSQL Upsert沒返回資料
        /// </summary>
        MssqlSubjectUpsertFail,
    }
}
