using Newtonsoft.Json;
using System.Collections.Generic;

namespace Live.PubSub.Core
{
    /// <summary>
    /// RMQ事件流
    /// </summary>
    public class RabbitMqEventStream : EventStream
    {
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="utcTimeStamp"></param>
        /// <param name="key"></param>
        /// <param name="eventName"></param>
        /// <param name="records"></param>
        public RabbitMqEventStream(string type, string data, long utcTimeStamp, string key = null, string eventName = null, IList<object> records = null)
        {
            Type = type;
            Data = data;
            UtcTimeStamp = utcTimeStamp;
        }
    }

    /// <summary>
    /// RMQ處裡事件介面
    /// </summary>
    public interface IRabbitMqPubSubHandler : IPubSubHandler<RabbitMqEventStream>
    {
    }
}