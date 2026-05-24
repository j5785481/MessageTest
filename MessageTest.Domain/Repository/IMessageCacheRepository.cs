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
        Exception Set<T>(IEnumerable<T> actions);

        /// <summary>
        ///     pop
        /// </summary>
        (Exception exception, IEnumerable<T> actions) Pop<T>(int count);
    }
}
