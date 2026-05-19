using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.DTO
{
    public class QueryMessageResponseDto
    {
        /// <summary>
		///  該查詢下的總留言數量
		/// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        ///     訊息List
        /// </summary>
        public IEnumerable<Message> Items { get; set; } = new List<Message>();

        /// <summary>
        /// 查詢的 request
        /// </summary>
        public QueryMessageRequestDto Request { get; set; }
    }
}
