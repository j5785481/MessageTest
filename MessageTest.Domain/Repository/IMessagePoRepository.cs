using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.Repository
{
    public interface IMessagePoRepository
    {
        /// <summary>
        /// 使用者留言
        /// </summary>
        /// <param name="req">留言Request</param>
        /// <returns>留言結果</returns>
        (Exception exception, Message message) Add(AddMessageRequestDto req);

        /// <summary>
        /// 刪除主題
        /// </summary>
        /// <param name="req">刪除主題Request</param>
        /// <returns>刪除主題結果</returns>
        (Exception exception, Message message) Delete(DeleteMessageRequestDto req);

        /// <summary>
        /// 查詢留言數
        /// </summary>
        /// <param name="req">查詢主題留言數Request</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, List<Message> messages) Query(QueryMessageRequestDto req);
    }
}
