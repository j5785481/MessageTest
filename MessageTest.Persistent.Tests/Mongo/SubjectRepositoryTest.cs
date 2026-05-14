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
    public class SubjectRepositoryTest
    {
        private ISubjectRepository repo;

        [TestInitialize]
        public void Initialize()
        {
            var client = new MongoClient(ConfigHelper.MongoConn);
            var db = client.GetDatabase(ConfigHelper.MongoDataBaseName);
            db.DropCollection("MessageTest");

            repo = new SubjectRepository(client, ConfigHelper.MongoDataBaseName);
        }

        [TestMethod]
        public void UpsertSubject()
        {
            var exception = repo.Save(new Subject
            {
                Id = 1,
                Title = "Test",
                Content = "Test",
                CreatorId = "115051401",
                CreatedAt = DateTime.Now,
                MessageCount = 0
            });

            Assert.IsNull(exception);
        }

        [TestMethod]
        public void RemoveSubject()
        {
            var saveException = repo.Save(new Subject
            {
                Id = 1,
                Title = "Test",
                Content = "Test",
                CreatorId = "115051401",
                CreatedAt = DateTime.Now,
                MessageCount = 0
            });

            Assert.IsNull(saveException);

            var removeException = repo.Delete(1);
            Assert.IsNull(removeException);
        }

        [TestMethod]
        public void GetSubject()
        {
            var saveException = repo.Save(new Subject
            {
                Id = 1,
                Title = "Test",
                Content = "Test",
                CreatorId = "115051401",
                CreatedAt = DateTime.Now,
                MessageCount = 0
            });

            Assert.IsNull(saveException);

            var getBtIdResult = repo.GetById(1);
            Assert.IsNull(getBtIdResult.exception);
            Assert.IsNotNull(getBtIdResult.subject);
        }
    }
}
