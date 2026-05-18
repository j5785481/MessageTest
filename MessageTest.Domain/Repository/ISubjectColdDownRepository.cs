using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Repository
{
    public interface ISubjectColdDownRepository
    {
        /// <summary>
		/// 確認是否可以發布主題
		/// </summary>
		(Exception ex, bool ok) TryLock(string creatorId);
    }
}
