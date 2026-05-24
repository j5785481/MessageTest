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
        /// <param name="subjectId">刪除主題Id</param>
        /// <returns>刪除主題結果</returns>
        (Exception exception, Subject subject) Delete(int subjectId);

        /// <summary>
        /// 查詢留言數
        /// </summary>
        /// <param name="req">查詢主題留言數Request</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, Subject subject) Query(QuerySubjectRequestDto req);

        /// <summary>
        /// 查詢By SubjectId
        /// </summary>
        /// <param name="subjectId">主題Id</param>
        /// <returns></returns>
        (Exception exception, Subject subject) GetById(int subjectId);

        /// <summary>
        /// 查詢By SubjectIds
        /// </summary>
        /// <param name="subjectIds">主題Ids</param>
        /// <returns></returns>
        (Exception exception, List<Subject> subjects) GetByIds(List<int> subjectIds);

        /// <summary>
        /// 新增更新主題
        /// </summary>
        /// <param name="input">主題物件</param>
        /// <returns></returns>
        (Exception exception, Subject subject) Upsert(Subject input);

        /// <summary>
        /// 批次新增更新主題
        /// </summary>
        /// <param name="input">主題物件List</param>
        /// <returns></returns>
        (Exception exception, List<Subject> subjects) BatchUpsert(List<Subject> input);
    }
}
