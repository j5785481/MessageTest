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
        /// 刪除留言
        /// </summary>
        /// <param name="req">刪除留言Request</param>
        /// <returns>刪除留言結果</returns>
        (Exception exception, Message message) Delete(DeleteMessageRequestDto req);

        /// <summary>
        /// 批次刪除留言
        /// </summary>
        /// <param name="reqs">刪除留言Request</param>
        /// <returns>刪除留言結果</returns>
        (Exception exception, List<Message> messages) BatchDelete(List<DeleteMessageRequestDto> reqs);

        /// <summary>
        /// 查詢主題留言
        /// </summary>
        /// <param name="req">查詢主題留言Request</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, List<Message> messages) Query(QueryMessageRequestDto req);

        /// <summary>
        /// 查詢會員的留言
        /// </summary>
        /// <param name="userId">會員Id</param>
        /// <returns>會員所有資訊</returns>
        (Exception exception, List<Message> messages) GetByAccount(string userId);
    }
}
