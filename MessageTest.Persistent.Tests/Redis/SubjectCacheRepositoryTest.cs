using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Persistent.Redis;
using MessageTest.Persistent.Tests.Applibs;
using StackExchange.Redis;
namespace MessageTest.Persistent.Tests.Redis
{
    [TestClass]
    public class SubjectCacheRepositoryTest
    {
        private ISubjectCacheRepository subjectCacheRepository;

        [TestInitialize]
        public void Initialize()
        {
            var conn = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(ConfigHelper.RedisConn));
            subjectCacheRepository = new SubjectCacheRepository()
            {
                Conn = conn,
                AffixKey = ConfigHelper.AffixKey,
                DataBase = ConfigHelper.DataBase
            };
            var keys = conn.GetServer(ConfigHelper.RedisConn)
                .Keys(ConfigHelper.DataBase, $"{ConfigHelper.AffixKey}*", 10, CommandFlags.None).ToList();
            keys.ForEach(key => conn.GetDatabase(ConfigHelper.DataBase).KeyDelete(key));
        }

        [TestMethod]
        public void Set_Success()
        {
            var exception = subjectCacheRepository.Set(new Subject
            {
                Id = 1,
                Title = "Test111",
                Content = "Test111",
                CreatorId = "115051401",
                MessageCount = 0
            });

            Assert.IsNull(exception);
        }

        [TestMethod]
        public void Remove()
        {
            var exception = subjectCacheRepository.Set(new Subject
            {
                Id = 1,
                Title = "Test111",
                Content = "Test111",
                CreatorId = "115051401",
                MessageCount = 0
            });

            Assert.IsNull(exception);
            var removeResult = subjectCacheRepository.Remove(1);
            Assert.IsNull(removeResult.ex);
            Assert.IsTrue(removeResult.ok);
        }

        [TestMethod]
        public void Get_Success()
        {
            var exception = subjectCacheRepository.Set(new Subject
            {
                Id = 1,
                Title = "Test111",
                Content = "Test111",
                CreatorId = "115051401",
                MessageCount = 0
            });

            Assert.IsNull(exception);
            var subjectResult = subjectCacheRepository.FindInSubjectId(1);
            
            Assert.AreEqual(1, subjectResult.subject.Id);
            Assert.IsNull(subjectResult.ex);
        }

        [TestMethod]
        public void Get_EmptyValueFail()
        {
            var subjectResult = subjectCacheRepository.FindInSubjectId(1);

            Assert.IsNull(subjectResult.subject);
            Assert.IsNull(subjectResult.ex);
        }
    }
}
