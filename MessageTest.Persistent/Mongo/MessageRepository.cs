using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
    public class MessageRepository : BaseMongoRepository, IMessageRepository
    {
        private const string CollectionName = "MessageTestMessage";

        static MessageRepository()
        {
            BsonClassMap.RegisterClassMap<Message>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(p => p.Id);
            });
        }
        public MessageRepository(MongoClient mongoClient, string dataBaseName) : base(mongoClient, dataBaseName)
        {
            db = MongoClient.GetDatabase(DataBaseName);
            collection = db.GetCollection<Message>(CollectionName);

            collection.Indexes.CreateOne(
                new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Descending(x => x.SubjectId))
            );

            collection.Indexes.CreateOne(
                new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Descending(x => x.CreatedAt))
            );

        }

        private IMongoCollection<Message> collection { get; }

        private IMongoDatabase db { get; }
        public Exception Delete(string messageId)
        {
            try
            {
                collection.DeleteOne(Builders<Message>.Filter.Eq(p => p.Id, messageId));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception BatchDelete(List<string> messageIds)
        {
            try
            {
                collection.DeleteMany(Builders<Message>.Filter.In(p => p.Id, messageIds));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public (Exception exception, List<Message> messages) GetById(int subjectId)
        {
            try
            {
                var rs = collection.Find(Builders<Message>.Filter.Eq(p => p.SubjectId, subjectId)).ToList();
                return (null, rs);
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public (Exception exception, List<Message> messages) GetPageMessage(QueryMessageRequestDto req)
        {
            try
            {
                int page = req.Page;             // 頁碼，例如第 2 頁
                int limit = req.LimitNumber;     // 每頁筆數，例如 20 筆
                int skipRows = (page - 1) * limit; // 計算要跳過幾筆
                var filter = Builders<Message>.Filter.Eq(x => x.SubjectId, req.SubjectId);
                var sort = Builders<Message>.Sort.Ascending(x => x.CreatedAt);

                var rs = collection.Find(filter)
                                   .Sort(sort)       // 命中索引的排序，速度極快
                                   .Skip(skipRows)   // 跳過前面的筆數
                                   .Limit(limit)     // 只拿需要的筆數
                                   .ToList();
                return (null, rs);
            }
            catch (Exception ex) 
            {
                return (ex, null);
            }
        }

        public Exception Save(Message message)
        {
            try
            {
                var updateScript = Builders<Message>.Update
                    .SetOnInsert(p => p.SubjectId, message.SubjectId)
                    .SetOnInsert(p => p.Id, message.Id)
                    .SetOnInsert(p => p.Content, message.Content)
                    .SetOnInsert(p => p.UserId, message.UserId)
                    .SetOnInsert(p => p.CreatedAt, message.CreatedAt);
                var filter = Builders<Message>.Filter.Eq(p => p.Id, message.Id);
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

        public Exception BatchSave(List<Message> messages)
        {
            try
            {
                var bulk = messages.Select(msg =>
                {
                    var update = Builders<Message>.Update
                        .SetOnInsert(p => p.SubjectId, msg.SubjectId)
                        .SetOnInsert(p => p.Id, msg.Id)
                        .SetOnInsert(p => p.Content, msg.Content)
                        .SetOnInsert(p => p.UserId, msg.UserId)
                        .SetOnInsert(p => p.CreatedAt, msg.CreatedAt);
                    var filter = Builders<Message>.Filter.Eq(p => p.Id, msg.Id);
                    return (WriteModel<Message>)new UpdateOneModel<Message>(filter, update)
                    {
                        IsUpsert = true
                    };
                });

                collection.BulkWrite(bulk);
                return null;
            }
            catch (Exception ex) 
            {
                return ex;
            }
        }
    }
}
