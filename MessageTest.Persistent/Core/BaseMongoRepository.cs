using MessageTest.Domain.Repository;
using MongoDB.Driver;

namespace ForumMessageSystem.Persistent.Core
{
    /// <summary>
    ///     Mongo DB 介面搭配autofac with property
    /// </summary>
    public abstract class BaseMongoRepository : IRepository
    {
        public BaseMongoRepository(MongoClient mongoClient, string dataBaseName)
        {
            DataBaseName = dataBaseName;
            MongoClient = mongoClient;
        }

        protected MongoClient MongoClient { get; set; }

        protected string DataBaseName { get; set; }
    }
}