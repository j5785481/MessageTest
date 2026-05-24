
namespace Live.PubSub.Applibs
{
    using System;
    using System.Text;
    using Live.PubSub.Core;
    using MessageTest.Lib;
    using Newtonsoft.Json;
    using RabbitMQ.Client;

    /// <summary>
    /// RMQ生產者
    /// </summary>
    public static class RabbitMqProducer
    {
        /// <summary>
        /// 發布事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topicName">發布Topic目標</param>
        /// <param name="data">事件內容</param>
        /// <param name="rmqExpiration">訊息存活時間(預設1天)</param>
        public static void Publish<T>(string topicName, T data, string rmqExpiration = "86400000")
        {
            var channel = RabbitMqFactory.GetChannel(topicName);
            var es = new RabbitMqEventStream(
                data.GetType().Name,
                JsonConvert.SerializeObject(data),
                TimeStampHelper.ToUtcTimeStamp(DateTime.Now));

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(es));
            var prop = channel.CreateBasicProperties();
            prop.Expiration = rmqExpiration;

            channel.BasicPublish(
                $"Exchange-{ExchangeType.Direct}-{topicName}",
                string.Empty,
                prop,
                body);
        }
    }
}
