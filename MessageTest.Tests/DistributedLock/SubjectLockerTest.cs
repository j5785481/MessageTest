using MessageTest.DistributedLock;
using MessageTest.Tests.Applibs;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Tests.DistributedLock
{
    [TestClass]
    public class SubjectLockerTest
    {
        private ISubjectLocker subjectLocker;

        [TestInitialize]
        public void Initialize()
        {
            var conn = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(ConfigHelper.RedisConn));
            var redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> { conn });
            subjectLocker = new SubjectLocker
            {
                RedLockFactory = redLockFactory,
                AffixKey = ConfigHelper.AffixKey
            };
        }

        [TestMethod]
        public void LockSubject()
        {
            using (var redLock = subjectLocker.GrabLock(1))
            {
                Assert.IsNotNull(redLock);
                Assert.IsTrue(redLock.IsAcquired);
            }

            using (var redLock = subjectLocker.GrabLock(1))
            {
                Assert.IsNotNull(redLock);
                Assert.IsTrue(redLock.IsAcquired);
            }
        }

        [TestMethod]
        public async Task LockSubject_ShouldShowContention_WithTimeCheck()
        {
            var readySignal = new CountdownEvent(2);
            var startSignal = new ManualResetEventSlim(false);

            bool? thread1Result = null;
            bool? thread2Result = null;
            string time1 = "";
            string time2 = "";

            var task1 = Task.Run(() =>
            {
                readySignal.Signal();
                startSignal.Wait();
                try
                {
                    using (var redLock = subjectLocker.GrabLock(1))
                    {
                        thread1Result = redLock.IsAcquired;
                        time1 = DateTime.Now.ToString("HH:mm:ss.fff");
                        if (redLock.IsAcquired)
                        {
                            // 故意霸佔 5 秒，拉長戰線
                            Thread.Sleep(5000);
                        }
                    }
                }
                catch (Exception ex) 
                {
                    Assert.IsNull(ex);
                }
            });

            var task2 = Task.Run(() =>
            {
                readySignal.Signal();
                startSignal.Wait();

                // 🌟 這裡故意晚 500 毫秒出發，確保執行緒 1 已經把鎖拿走了
                Thread.Sleep(500);
                try
                {
                    using (var redLock = subjectLocker.GrabLock(1))
                    {
                        thread2Result = redLock.IsAcquired;
                        time2 = DateTime.Now.ToString("HH:mm:ss.fff");
                    }
                }
                catch (Exception ex)
                {
                    Assert.IsNull(ex); 
                }
            });

            readySignal.Wait();
            startSignal.Set();

            await Task.WhenAll(task1, task2);

            // 觀看時間差
            Console.WriteLine($"執行緒 1 搶鎖結果: {thread1Result}，時間: {time1}");
            Console.WriteLine($"執行緒 2 搶鎖結果: {thread2Result}，時間: {time2}");
        }

        [TestMethod]
        public void LockSubject_ShouldFail_WhenLockIsAlreadyHeld()
        {
            try
            {
                // 1. 第一個人進去，並把門鎖上 (此時鎖被佔用了)
                using (var firstLock = subjectLocker.GrabLock(1))
                {
                    Assert.IsTrue(firstLock.IsAcquired, "第一把鎖應該要成功取得");

                    // 2. 🌟 關鍵：在第一個 using 還沒結束（鎖沒釋放）前，第二個人硬要進去
                    using (var secondLock = subjectLocker.GrabLock(1))
                    {
                        // 輸出結果
                        Console.WriteLine($"第一把鎖狀態: {firstLock.IsAcquired}");
                        Console.WriteLine($"第二把鎖狀態: {secondLock.IsAcquired}");

                        // 驗證：因為第一把鎖還在作用中，第二把鎖此時必定搶失敗 (IsAcquired 應為 false)
                        Assert.IsFalse(secondLock.IsAcquired, "因為鎖被佔用了，第二把鎖應該要搶鎖失敗！");
                    }
                } // 第一把鎖在這裡才釋放
            }
            catch (Exception ex)
            {
                Assert.IsNull(ex); 
            }
        }

        [TestMethod]
        public void GrabLock_Timeout_ShouldReturnFalse_AfterWaitTime()
        {
            // 這個測試驗證「取得鎖超時」的行為
            // 目前 GrabLock 設定 WaitTime 為 3 秒
            
            using (var lock1 = subjectLocker.GrabLock(999))
            {
                Assert.IsTrue(lock1.IsAcquired);

                var startTime = DateTime.Now;
                
                // 嘗試取得同一個 ID 的鎖，這會觸發等待
                using (var lock2 = subjectLocker.GrabLock(999))
                {
                    var endTime = DateTime.Now;
                    var duration = (endTime - startTime).TotalSeconds;

                    Console.WriteLine($"等待耗時: {duration} 秒");
                    
                    // 驗證：應該在等待約 3 秒後回傳失敗
                    Assert.IsFalse(lock2.IsAcquired, "超時後應回傳 IsAcquired = false");
                    Assert.IsTrue(duration >= 3, "應該至少等待了 3 秒的 WaitTime");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void GrabLock_ShouldThrowException_WhenFactoryIsNull()
        {
            // 這個測試用來模擬「Exception」的情境
            // 當相依組件沒註冊好或為空時，GrabLock 會拋出例外
            var brokenLocker = new SubjectLocker { RedLockFactory = null };
            
            // 這行會拋出 NullReferenceException，觸發預期的 Exception 測試
            brokenLocker.GrabLock(1);
        }
    }
}
