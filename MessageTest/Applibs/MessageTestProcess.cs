
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
                logger.Info("批次處理留言背景任務已啟動（每秒偵測一次）...");

                while (_isProcessing)
                {
                    try
                    {
                        // 每次循環都建立一個獨立的生命週期範圍（Lifetime Scope）
                        using (var scope = container.BeginLifetimeScope())
                        {
                            //// 1. 定義對應 Autofac 具名註冊的事件名稱
                            //string eventName = "ProcessMessageAddJobEvent";

                            //// 2. 透過 ResolveNamed 解析出對應的 Handler 介面
                            //var handler = scope.ResolveNamed<IPubSubHandler<RabbitMqEventStream>>(eventName);

                            //// 3. 【完全對齊】帶入建構子要求的三個必要參數 (type, data, utcTimeStamp)
                            //long currentUtcTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                            //var dummyStream = new RabbitMqEventStream(
                            //    type: eventName,
                            //    data: string.Empty,
                            //    utcTimeStamp: currentUtcTimestamp
                            //);

                            //// 4. 執行 Handle 觸發你的批次處理邏輯
                            //handler.Handle(dummyStream);
                            // 💡 直接解析類別本體，不要透過 Named 介面
                            var handler = scope.Resolve<ProcessMessageAddJobEventHandler>();

                            // 💡 直接呼叫，這時絕對會直接踩進你 Handle 方法的第一行斷點！
                            handler.Handle(new RabbitMqEventStream("ProcessMessageAddJobEvent", string.Empty, 0L));
                        }
                    }
                    catch (Exception ex)
                    {
                        // 遵循前輩指示：捕捉所有可能崩潰的錯誤，維持排程不中斷，並記錄 Log
                        logger.Error($"[背景排程驅動失敗] 詳細原因: {ex.ToString()}");
                    }

                    // 休息 1000 毫秒（1秒）後，再進下一次迴圈
                    await Task.Delay(1000);
                }

                logger.Info("批次處理留言背景任務已安全停止。");
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
