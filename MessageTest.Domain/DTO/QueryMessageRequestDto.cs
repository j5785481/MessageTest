using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class QueryMessageRequestDto
    {
        /// <summary>
        ///     主題ID
        /// </summary>
        public int SubjectId { get; set; }
        /// <summary>
        ///     單頁筆數
        /// </summary>
        public int LimitNumber { get; set; }
        /// <summary>
        ///     頁數
        /// </summary>
        public int Page {  get; set; }
    }
}
