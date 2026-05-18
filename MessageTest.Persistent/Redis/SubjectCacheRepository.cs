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

namespace MessageTest.Persistent.Redis
{
    public class SubjectCacheRepository : IRedisRepository, ISubjectCacheRepository
    {
        public ConnectionMultiplexer Conn { get; set; }
        public string AffixKey { get; set; }
        public int DataBase { get; set; }
        public const string CacheKey = "SubjectCache";
        public (Exception ex, Subject subject) FindInSubjectId(int subjectId)
        {
            try
            {
                string fullKey = $"{AffixKey}:{CacheKey}:{subjectId}";
                var value = Conn.GetDatabase(DataBase)
                    .StringGet(fullKey);
                if (value.HasValue)
                {
                    var rs = JsonConvert.DeserializeObject<Subject>(value);
                    return (null, rs);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public Exception Set(Subject subject)
        {
            try
            {
                string fullKey = $"{AffixKey}:{CacheKey}:{subject.Id}";
                Conn.GetDatabase(DataBase)
                    .StringSet(fullKey, JsonConvert.SerializeObject(subject));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public (Exception ex, bool ok) Remove(int subjectId)
        {
            try
            {
                string fullKey = $"{AffixKey}:{CacheKey}:{subjectId}";
                var result = Conn.GetDatabase(DataBase)
                    .KeyDelete($"{AffixKey}:{CacheKey}:{subjectId}");
                return (null, result);
            }
            catch (Exception ex)
            {
                return (ex, false);
            }
        }
    }
}
