
namespace Live.PubSub.Applibs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Live.PubSub.Core;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    /// <summary>
    /// RMQ消費者
    /// </summary>
    public class RabbitMqConsumer
    {
        private IPubSubDispatcher<RabbitMqEventStream> dispatcher;

        private IEnumerable<string> queueNames;

        private string queueId;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="queueNames">訂閱Topics</param>
        /// <param name="dispatcher">調度員</param>
        /// <param name="queueId">當前服務Topics</param>
        public RabbitMqConsumer(IEnumerable<string> queueNames, IPubSubDispatcher<RabbitMqEventStream> dispatcher, string queueId)
        {
            this.queueNames = queueNames;
            this.dispatcher = dispatcher;
            this.queueId = queueId;
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <param name="shotDownCallBack">Shut事件</param>
        public void Register(Action<ShutdownEventArgs> shotDownCallBack)
        {
            this.queueNames.ToList().ForEach(topicName =>
            {
                var channel = RabbitMqFactory.GetChannel(topicName);
                //// generate queue
                var queueName = channel.QueueDeclare($"{this.queueId}-{topicName}", false, false, false, null).QueueName;

                //// bind queue to exchange
                channel.QueueBind(queueName, $"Exchange-{ExchangeType.Direct}-{topicName}", string.Empty, null);
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var @event = JsonConvert.DeserializeObject<RabbitMqEventStream>(Encoding.UTF8.GetString(ea.Body));
                    if (this.dispatcher.DispatchMessage(@event))
                    {
                        channel.BasicAck(ea.DeliveryTag, true);
                    }
                    else
                    {
                        bool runing = true;
                        while (runing)
                        {
                            //// 休息0.5秒丟回RMQ讓下一次循環處理 減少負擔
                            SpinWait.SpinUntil(() => runing = false, 500);
                        }

                        channel.BasicNack(ea.DeliveryTag, true, true);
                    }
                };

                // Shut callback
                consumer.Shutdown += (o, args) =>
                {
                    shotDownCallBack(args);
                };

                var consumerTag = channel.BasicConsume(queueName, false, $"{Environment.MachineName}", false, false, null, consumer);
            });
        }
    }
}
