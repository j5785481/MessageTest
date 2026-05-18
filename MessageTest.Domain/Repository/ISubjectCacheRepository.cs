using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.Repository
{
    public interface ISubjectCacheRepository
    {
        /// <summary>
		///	set subject cache
		/// </summary>
        Exception Set(Subject subject);

        /// <summary>
		/// remove subject cache
		/// </summary>
		(Exception ex, bool ok) Remove(int subjectId);

        /// <summary>
		/// 查詢Subject
		/// </summary>
        (Exception ex, Subject subject) FindInSubjectId(int subjectId);
    }
}
