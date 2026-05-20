using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Persistent.Mongo;
using MessageTest.Persistent.Tests.Applibs;
using MongoDB.Driver;

namespace MessageTest.Persistent.Tests.Mongo
{
    [TestClass]
    public class MessageRepositoryTest
    {
        private IMessageRepository repo;

        [TestInitialize]
        public void Initialize()
        {
            var client = new MongoClient(ConfigHelper.MongoConn);
            var db = client.GetDatabase(ConfigHelper.MongoDataBaseName);
            db.DropCollection("MessageTest");

            repo = new MessageRepository(client, ConfigHelper.MongoDataBaseName);
        }

        [TestMethod]
        public void UpsertMessage()
        {
            var guid = Guid.NewGuid().ToString();
            var exception = repo.Save(new Message
            {
                SubjectId = 1,
                Id = guid,
                Content = "Test",
                UserId = "115051401",
                CreatedAt = DateTime.Now
            });

            Assert.IsNull(exception);
        }

        [TestMethod]
        public void RemoveMessage()
        {
            var guid = Guid.NewGuid().ToString();
            var saveException = repo.Save(new Message
            {
                SubjectId = 1,
                Id = guid,
                Content = "Test",
                UserId = "115051401",
                CreatedAt = DateTime.Now
            });

            Assert.IsNull(saveException);

            var removeException = repo.Delete(guid);
            Assert.IsNull(removeException);
        }

        [TestMethod]
        public void GetMessage()
        {
            var guid = Guid.NewGuid().ToString();
            var saveException1 = repo.Save(new Message
            {
                SubjectId = 1,
                Id = guid,
                Content = "Test",
                UserId = "115051401",
                CreatedAt = DateTime.Now
            });

            Assert.IsNull(saveException1);
            guid = Guid.NewGuid().ToString();
            var saveException2 = repo.Save(new Message
            {
                SubjectId = 1,
                Id = guid,
                Content = "Test",
                UserId = "115051402",
                CreatedAt = DateTime.Now
            });
            Assert.IsNull(saveException2);

            var getBtIdResult = repo.GetById(1);
            Assert.IsNull(getBtIdResult.exception);
            Assert.IsNotNull(getBtIdResult.messages);
            Assert.AreEqual(2, getBtIdResult.messages.Count);
        }
    }
}
