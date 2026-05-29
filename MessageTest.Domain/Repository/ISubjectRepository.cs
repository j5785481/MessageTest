using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.Repository
{
    public interface ISubjectRepository
    {
        /// <summary>
        /// 儲存主題
        /// </summary>
        /// <param name="subject">儲存的主題物件</param>
        /// <returns>例外</returns>
        Exception Save(Subject subject);

        /// <summary>
        /// 批次儲存主題
        /// </summary>
        /// <param name="subjects">儲存的主題物件</param>
        /// <returns></returns>
        Exception BatchSave(List<Subject> subjects);

        /// <summary>
        /// 刪除主題
        /// </summary>
        /// <param name="subjectId">刪除主題的ID</param>
        /// <returns>例外</returns>
        Exception Delete(int subjectId);

        /// <summary>
        /// 查詢主題資訊by SubjectId
        /// </summary>
        /// <param name="subjectId">查詢主題的ID</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, Subject subject) GetById(int subjectId);

        /// <summary>
        /// 批次查詢主題資訊by SubjectIds
        /// </summary>
        /// <param name="subjectIds">查詢主題的IDs</param>
        /// <returns>主題所有資訊</returns>
        (Exception exception, List<Subject> subjects) GetByIds(List<int> subjectIds);
    }
}
