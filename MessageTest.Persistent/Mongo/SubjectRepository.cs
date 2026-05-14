using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForumMessageSystem.Persistent.Core;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MessageTest.Persistent.Mongo
{
    public class SubjectRepository : BaseMongoRepository, ISubjectRepository
    {
        private const string CollectionName = "MessageTest";

        static SubjectRepository()
        {
            BsonClassMap.RegisterClassMap<Subject>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(p => p.Id);
            });
        }
        public SubjectRepository(MongoClient mongoClient, string dataBaseName) : base(mongoClient, dataBaseName)
        {
            db = MongoClient.GetDatabase(DataBaseName);
            collection = db.GetCollection<Subject>(CollectionName);

            collection.Indexes.CreateOne(
                new CreateIndexModel<Subject>(Builders<Subject>.IndexKeys.Descending(x => x.CreatedAt))
            );
        }

        private IMongoCollection<Subject> collection { get; }

        private IMongoDatabase db { get; }

        public Exception Add(AddSubjectRequestDto req)
        {
            throw new NotImplementedException();
        }

        public Exception Delete(DeleteSubjectRequestDto req)
        {
            throw new NotImplementedException();
        }

        public (Exception exception, Subject subject) GetById(int subjectId)
        {
            throw new NotImplementedException();
        }

        public (Exception exception, Subject subject) Query(QueryMessageCountRequestDto req)
        {
            throw new NotImplementedException();
        }
    }
}
