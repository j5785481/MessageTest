using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.Repository
{
    public interface ISubjectPoRepository
    {
        /// <summary>
        /// 發布主題
        /// </summary>
        /// <param name="req">發布主題Request</param>
        /// <returns>發布主題結果</returns>
        (Exception exception, Subject subject) Add(AddSubjectRequestDto req);

        /// <summary>
        /// 刪除主題
        /// </summary>
        /// <param name="req">刪除主題Request</param>
        /// <returns>刪除主題結果</returns>
        (Exception exception, Subject subject) Delete(DeleteSubjectRequestDto req);

        /// <summary>
        /// 查詢留言數
        /// </summary>
        /// <param name="req">查詢主題留言數Request</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, Subject subject) Query(QuerySubjectRequestDto req);

        (Exception exception, Subject subject) GetById(int subjectId);

        (Exception exception, Subject subject) Upsert(Subject input);
    }
}
