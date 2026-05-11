using MessageTest.Domain.Repository;
using StackExchange.Redis;

namespace ForumMessageSystem.Persistent.Core
{
    public interface IRedisRepository : IRepository
    {
        ConnectionMultiplexer Conn { get; set; }

        string AffixKey { get; set; }

        int DataBase { get; set; }
    }
}