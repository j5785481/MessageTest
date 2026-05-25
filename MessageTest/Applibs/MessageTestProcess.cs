
namespace MessageTest.Applibs
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Live.PubSub.Applibs;
    using Live.PubSub.Core;
    using MessageTest.Domain.JobEvent;
    using MessageTest.Handler.Rmq.JobSchedule;
    using Newtonsoft.Json;
    using NLog;
    using RabbitMQ.Client;

    internal static class MessageTestProcess
    {
        private static readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(MessageTestProcess));

        public static void ProcessStart()
        {
            logger.Info("MessageTest.Server Application_Start");
            var container = AutofacConfig.Container;

            //// 啟動RMQ工廠
            RabbitMqFactory.Start(ConfigHelper.RabbitUserName, ConfigHelper.RabbitPassword, ConfigHelper.RabbitMqUri);

            //// 建立消費者物件
            var consumer = new RabbitMqConsumer(
                //// 訂閱Topics
                ConfigHelper.SubscribeTopics,
                //// 調度員
                new PubSubDispatcher<RabbitMqEventStream>(AutofacConfig.Container, OnAlert),
                //// 當前服務Topic
                ConfigHelper.Topic);

            //// 不要忘了註冊喔
            consumer.Register(ShutDownCallback);

            var _isProcessing = true;

            Task.Run(async () =>
            {
                logger.Info("批次處理留言 Producer 任務已啟動（每 5 秒發送一次事件）...");

                while (_isProcessing)
                {
                    try
                    {
                        // 💡 實作 Producer 端：不再直接呼叫 Handler，而是透過 RMQ 發送事件
                        // 根據專案慣例，我們將事件發送到 "JobSchedule" 這個 Topic
                        var eventData = new ProcessMessageAddJobEvent();

                        RabbitMqProducer.Publish("JobSchedule", eventData);

                        logger.Info($"[Producer] 已發送 ProcessMessageAddJobEvent 到 RMQ (JobSchedule) - {DateTime.Now:HH:mm:ss}");
                    }
                    catch (Exception ex)
                    {
                        // 捕捉錯誤，避免背景任務崩潰
                        logger.Error($"[Producer 派發失敗] 詳細原因: {ex.Message}");
                    }

                    // 依照需求：每 5 秒 (5000 毫秒) 執行一次
                    await Task.Delay(5000);
                }

                logger.Info("批次處理留言 Producer 任務已安全停止。");
            });

            Task.Run(() =>
            {
                while (!SpinWait.SpinUntil(() => false, 1000))
                {
                    Console.Clear();
                    Console.WriteLine($"Current Memory Usage:{(int)((GC.GetTotalMemory(true) / 1024f))}(KB)");
                    Console.WriteLine($"Process Memory Usage:{(int)((Process.GetCurrentProcess().PrivateMemorySize64 / 1024f))}(KB)");
                    Console.WriteLine($"Handle count:{Process.GetCurrentProcess().HandleCount}");
                    Console.WriteLine($"Thread count:{Process.GetCurrentProcess().Threads.Count}");
                }
            });

            logger.Info("MessageTest.Server Started");
        }

        public static void ProcessStop()
        {
            logger.Info("MessageTest.Server Ended");
        }

        /// <summary>
		///     RMQ 消費者shut監聽
		/// </summary>
		/// <param name="obj"></param>
		private static void ShutDownCallback(ShutdownEventArgs obj)
        {
            logger.Error($"RMQ Consummer ShutDown:{JsonConvert.SerializeObject(obj)}");
        }

        /// <summary>
		///     寫字到畫面
		/// </summary>
		/// <param name="str"></param>
		private static void OnAlert(string str)
        {
            logger.Warn($"ForumMessageSystem RMQ OnAlert:{str}");
            Console.WriteLine(str);
        }
    }
}
