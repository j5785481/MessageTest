using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForumMessageSystem.Persistent.Core;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using Newtonsoft.Json;
using StackExchange.Redis;
using static System.Collections.Specialized.BitVector32;

namespace MessageTest.Persistent.Redis
{
    public class MessageCacheRepository : IRedisRepository, IMessageCacheRepository
    {
        public ConnectionMultiplexer Conn { get; set; }
        public string AffixKey { get; set; }
        public int DataBase { get; set; }
        public const string CacheKey = "MessageCache";

        public Exception Set<TAction>(string id, IEnumerable<TAction> actions)
        {
            try
            {
                string fullKey = $"{AffixKey}:{CacheKey}:{id}";
                Conn.GetDatabase(DataBase)
                    .ListRightPush(fullKey, JsonConvert.SerializeObject(actions));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public (Exception exception, IEnumerable<TAction> actions) Pop<TAction>(string id, int count)
        {
            try
            {
                var values = (RedisValue[])Conn.GetDatabase(DataBase).ScriptEvaluate(
                    @"local result = redis.call('LRANGE', KEYS[1], 0, ARGV[1])
                        redis.call('LTRIM', KEYS[1], ARGV[1] + 1 , -1)
                        return result 
					",
                    new RedisKey[] { $"{AffixKey}:{CacheKey}:{id}" }, new RedisValue[] { count - 1 });

                var rs = new List<TAction>();
                if (values != null && values.Any())
                    rs = values.Where(x => !x.IsNullOrEmpty).Select(x => JsonConvert.DeserializeObject<TAction>(x))
                        .ToList();

                return (null, rs);
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }
    }
}
