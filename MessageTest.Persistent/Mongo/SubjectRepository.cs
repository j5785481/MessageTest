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

        public Exception Save(Subject subject)
        {
            try
            {
                var updateScript = Builders<Subject>.Update

                    .SetOnInsert(p => p.Id, subject.Id)
                    .SetOnInsert(p => p.Title, subject.Title)
                    .SetOnInsert(p => p.Content, subject.Content)
                    .SetOnInsert(p => p.CreatorId, subject.CreatorId)
                    .SetOnInsert(p => p.CreatedAt, subject.CreatedAt)
                    .Set(p => p.MessageCount, subject.MessageCount);
                var filter = Builders<Subject>.Filter.Eq(p => p.Id, subject.Id);
                collection.UpdateOne(filter, updateScript, new UpdateOptions
                {
                    IsUpsert = true
                });
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception Delete(int subjectId)
        {
            try
            {
                collection.DeleteOne(Builders<Subject>.Filter.Eq(p => p.Id, subjectId));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public (Exception exception, Subject subject) GetById(int subjectId)
        {
            try
            {
                var rs = collection.Find(Builders<Subject>.Filter.Eq(p => p.Id, subjectId)).FirstOrDefault();
                return (null, rs);
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }
    }
}
