using System;
using MongoDB.Driver;
using NLog;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace MessageTest.Applibs
{
    internal static class NoSqlService
    {
        private static readonly ILogger logger = LogManager.GetLogger("MessageTest");

        private static Lazy<ConnectionMultiplexer> lazyRedisConnections;

        private static Lazy<MongoClient> lazyMongoConnetion;

        private static Lazy<RedLockFactory> lazyDistributedLockService;

        public static ConnectionMultiplexer RedisConnections
        {
            get
            {
                if (lazyRedisConnections == null) NoSqlInit();

                return lazyRedisConnections.Value;
            }
        }

        public static string RedisAffixKey => ConfigHelper.RedisAffixKey;

        public static int RedisDataBase => ConfigHelper.RedisDataBase;

        public static MongoClient MongoConnetion
        {
            get
            {
                if (lazyMongoConnetion == null) NoSqlInit();

                return lazyMongoConnetion.Value;
            }
        }

        public static RedLockFactory DistributedLockService
        {
            get
            {
                if (lazyDistributedLockService == null)
                    lazyDistributedLockService = new Lazy<RedLockFactory>(() =>
                    {
                        var connMulti = new RedLockMultiplexer[] { RedisConnections };
                        return RedLockFactory.Create(connMulti);
                    });

                return lazyDistributedLockService.Value;
            }
        }

        private static void NoSqlInit()
        {
            lazyRedisConnections = new Lazy<ConnectionMultiplexer>(() =>
            {
                var options = ConfigurationOptions.Parse(ConfigHelper.RedisConn);
                options.AbortOnConnectFail = false;

                var muxer = ConnectionMultiplexer.Connect(options);
                muxer.ConnectionFailed += (sender, e) =>
                {
                    logger.Error(
                        "redis failed: " + EndPointCollection.ToString(e.EndPoint) + "/" + e.ConnectionType);
                };
                muxer.ConnectionRestored += (sender, e) =>
                {
                    logger.Error("redis restored: " + EndPointCollection.ToString(e.EndPoint) + "/" +
                                 e.ConnectionType);
                };

                return muxer;
            });

            lazyMongoConnetion = new Lazy<MongoClient>(() => { return new MongoClient(ConfigHelper.MongoDBConn); });
        }
    }
}