using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForumMessageSystem.Persistent.Core;
using MessageTest.Domain.Repository;
using StackExchange.Redis;

namespace MessageTest.Persistent.Redis
{
    public class SubjectColdDownRepository : IRedisRepository, ISubjectColdDownRepository
    {
        public ConnectionMultiplexer Conn { get; set; }
        public string AffixKey { get; set; }
        public int DataBase { get; set; }
        public const string CacheKey = "SubjectColdDown";
        public (Exception ex, bool ok) TryLock(string creatorId)
        {
            try
            {
                string lockKey = $"{AffixKey}:{CacheKey}:SubjectCreate:{creatorId}";
                bool isSuccess = Conn.GetDatabase(DataBase).StringSet(lockKey, "locked", TimeSpan.FromSeconds(3), When.NotExists);
                return (null, isSuccess);
            }
            catch (Exception ex) 
            {
                return (ex, false);
            }
        }
    }
}
