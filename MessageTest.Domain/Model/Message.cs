using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    /// <summary>
    /// 訊息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 主題ID
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// 訊息ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 訊息內容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 留言的UserID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 樓層
        /// </summary>
        public int Floor {  get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
