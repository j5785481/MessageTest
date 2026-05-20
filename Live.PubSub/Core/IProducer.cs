
namespace Live.PubSub.Core
{
    /// <summary>
    /// 事件生產者介面
    /// </summary>
    public interface IProducer
    {
        /// <summary>
        /// 發布事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        void Publish<T>(T data);

        /// <summary>
        /// Redis發布事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        void RedisPublish<T>(T data);
    }
}
