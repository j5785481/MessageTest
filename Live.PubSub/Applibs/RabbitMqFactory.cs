
namespace Live.PubSub.Applibs
{
    using System;
    using System.Collections.Concurrent;
    using RabbitMQ.Client;

    /// <summary>
    /// RMQ工廠
    /// </summary>
    public static class RabbitMqFactory
    {
        private static ConnectionFactory factory;

        private static IConnection connection;

        private static ConcurrentDictionary<string, IModel> models = new ConcurrentDictionary<string, IModel>();

        private static bool TryAddModel(string topicName)
        {
            if (!models.ContainsKey(topicName))
            {
                var channel = connection.CreateModel();
                channel.ExchangeDeclare($"Exchange-{ExchangeType.Direct}-{topicName}", ExchangeType.Direct);  
                models.TryAdd(topicName, channel);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 取得channel
        /// </summary>
        /// <param name="topicName">Topic</param>
        /// <returns></returns>
        public static IModel GetChannel(string topicName)
        {
            TryAddModel(topicName);
            return models[topicName];
        }

        /// <summary>
        /// 工廠上班
        /// </summary>
        /// <param name="userName">RMQ帳號</param>
        /// <param name="password">RMQ密碼</param>
        /// <param name="rabbitMqUri">RMQ服務網址</param>
        public static void Start(string userName, string password, string rabbitMqUri)
        {
            if (factory != null)
            {
                return;
            }

            factory = new ConnectionFactory()
            {
                UserName = userName,
                Password = password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            connection = factory.CreateConnection(AmqpTcpEndpoint.ParseMultiple(rabbitMqUri));       
        }

        /// <summary>
        /// 工廠下班
        /// </summary>
        public static void Stop()
        {
            if (factory == null || models == null)
            {
                return;
            }

            foreach (var model in models)
            {
                model.Value.Abort();
                model.Value.Close();
            }

            models = new ConcurrentDictionary<string, IModel>();
            connection.Abort();
            connection.Close();
            factory = null;
        }
    }
}
