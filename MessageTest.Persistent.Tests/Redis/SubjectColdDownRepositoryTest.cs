using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Repository;
using MessageTest.Persistent.Redis;
using MessageTest.Persistent.Tests.Applibs;
using StackExchange.Redis;

namespace MessageTest.Persistent.Tests.Redis
{
    [TestClass]
    public class SubjectColdDownRepositoryTest
    {
        private ISubjectColdDownRepository subjectColdDownRepository;

        [TestInitialize]
        public void Initialize()
        {
            var conn = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(ConfigHelper.RedisConn));
            subjectColdDownRepository = new SubjectColdDownRepository()
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
        public void TryLock_Success()
        {
            var rs = subjectColdDownRepository.TryLock("115051501");
            Assert.IsNull(rs.ex);
            Assert.IsTrue(rs.ok);
        }


        [TestMethod]
        public void TryLock_AlreadyLocked()
        {
            var rs = subjectColdDownRepository.TryLock("115051501");
            rs = subjectColdDownRepository.TryLock("115051501");
            Assert.IsNull(rs.ex);
            Assert.IsFalse(rs.ok);
        }
    }
}
