using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Repository
{
    /// <summary>
    ///     訊息集成快取
    /// </summary>
    public interface IMessageCacheRepository
    {
        /// <summary>
        ///     set
        /// </summary>
        Exception Set<T>(string id, IEnumerable<T> actions);

        /// <summary>
        ///     pop
        /// </summary>
        (Exception exception, IEnumerable<T> actions) Pop<T>(string id, int count);
    }
}
