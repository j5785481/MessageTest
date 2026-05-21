using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.Repository
{
    public interface IMessageRepository
    {
        /// <summary>
        /// 儲存留言
        /// </summary>
        /// <param name="message">儲存的留言Request</param>
        /// <returns>例外</returns>
        Exception Save(Message message);

        /// <summary>
        /// 刪除留言
        /// </summary>
        /// <param name="messageId">刪除留言的ID</param>
        /// <returns>例外</returns>
        Exception Delete(string messageId);

        /// <summary>
        /// 批次刪除留言
        /// </summary>
        /// <param name="messageIds">要刪除留言的ID</param>
        /// <returns>例外</returns>
        Exception BatchDelete(List<string> messageIds);

        /// <summary>
        /// 查詢主題by SubjectId
        /// </summary>
        /// <param name="subjectId">查詢主題的ID</param>
        /// <returns>主題所有留言</returns>
        (Exception exception, List<Message> messages) GetById(int subjectId);

        /// <summary>
        /// 查詢留言
        /// </summary>
        /// <param name="req">取得該頁留言request</param>
        /// <returns>主題所有留言</returns>
        (Exception exception, List<Message> messages) GetPageMessage(QueryMessageRequestDto req);
    }
}
