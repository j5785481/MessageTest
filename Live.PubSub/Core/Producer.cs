
namespace Live.PubSub.Core
{
    using Live.PubSub.Applibs;

    /// <summary>
    /// 事件生產者介面
    /// </summary>
    public class Producer : IProducer
    {
        private string topicName;

        private string rmqExpiration;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="rmqExpiration"></param>
        public Producer(string topicName, string rmqExpiration = "86400000")
        {
            this.topicName = topicName;
            this.rmqExpiration = rmqExpiration;
        }

        /// <summary>
        /// 發布事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void Publish<T>(T data)
        {
           RabbitMqProducer.Publish(this.topicName, data, this.rmqExpiration);
        }

        /// <summary>
        /// 發布Redis事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void RedisPublish<T>(T data)
        {
            RedisProducer.Publish(this.topicName, data);
        }
    }
}
