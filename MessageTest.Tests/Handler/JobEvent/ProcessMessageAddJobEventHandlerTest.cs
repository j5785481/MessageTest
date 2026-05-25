using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Live.PubSub.Core;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Handler.Rmq.JobSchedule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MessageTest.Tests.Handler.JobEvent
{
    [TestClass]
    public class ProcessMessageAddJobEventHandlerTest
    {
        private Mock<IMessageCacheRepository> messageCacheRepoMock;
        private Mock<IMessagePoRepository> messagePoRepoMock;
        private Mock<IMessageRepository> messageRepoMock;
        private Mock<ISubjectPoRepository> subjectPoRepoMock;
        private Mock<ISubjectRepository> subjectRepoMock;
        private Mock<ILifetimeScope> lifetimeScopeMock;

        private ProcessMessageAddJobEventHandler handler;

        [TestInitialize]
        public void Setup()
        {
            messageCacheRepoMock = new Mock<IMessageCacheRepository>();
            messagePoRepoMock = new Mock<IMessagePoRepository>();
            messageRepoMock = new Mock<IMessageRepository>();
            subjectPoRepoMock = new Mock<ISubjectPoRepository>();
            subjectRepoMock = new Mock<ISubjectRepository>();
            
            // Mock Autofac 的 LifetimeScope
            lifetimeScopeMock = new Mock<ILifetimeScope>();
            lifetimeScopeMock.Setup(x => x.BeginLifetimeScope()).Returns(new Mock<ILifetimeScope>().Object);

            handler = new ProcessMessageAddJobEventHandler(
                lifetimeScopeMock.Object,
                messageCacheRepoMock.Object,
                messagePoRepoMock.Object,
                messageRepoMock.Object,
                subjectPoRepoMock.Object,
                subjectRepoMock.Object
            );
        }

        [TestMethod]
        public void Handle_Success_With5Messages()
        {
            // 1. Arrange - 準備假資料 (模擬從 Redis 取出 5 筆留言)
            var fakeMessages = new List<Message>();
            for (int i = 0; i < 5; i++)
            {
                fakeMessages.Add(new Message { Id = $"M{i}", SubjectId = 1, Content = $"Test {i}" });
            }

            // 模擬 Redis Pop 出 5 筆資料
            messageCacheRepoMock.Setup(x => x.Pop<Message>(5))
                .Returns((null, fakeMessages));

            // 模擬 MSSQL 批次新增留言成功
            messagePoRepoMock.Setup(x => x.BatchAdd(It.IsAny<List<Message>>()))
                .Returns((null, fakeMessages));

            // 模擬 MSSQL 查詢 Subject (找到 SubjectId = 1 的主題，目前 MessageCount = 10)
            var fakeSubjects = new List<Subject> { new Subject { Id = 1, MessageCount = 10 } };
            subjectPoRepoMock.Setup(x => x.GetByIds(It.IsAny<List<int>>()))
                .Returns((null, fakeSubjects));

            // 模擬批次更新 Subject 成功
            subjectPoRepoMock.Setup(x => x.BatchUpsert(It.IsAny<List<Subject>>()))
                .Returns((null, fakeSubjects));

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act - 執行處理
            var result = handler.Handle(fakeStream);

            // 3. Assert - 驗證
            Assert.IsTrue(result, "Handler 應該回傳 true");
            
            // 驗證是否真的有更新 Subject 的留言總數 (原本 10 筆 + 新增 5 筆 = 15 筆)
            Assert.AreEqual(15, fakeSubjects.First().MessageCount);

            // 驗證每個儲存庫的方法都有被「精準呼叫一次」
            messagePoRepoMock.Verify(x => x.BatchAdd(It.Is<List<Message>>(m => m.Count == 5)), Times.Once);
            subjectPoRepoMock.Verify(x => x.GetByIds(It.IsAny<List<int>>()), Times.Once);
            subjectPoRepoMock.Verify(x => x.BatchUpsert(It.IsAny<List<Subject>>()), Times.Once);
            subjectRepoMock.Verify(x => x.BatchSave(It.IsAny<List<Subject>>()), Times.Once);
            messageRepoMock.Verify(x => x.BatchSave(It.Is<List<Message>>(m => m.Count == 5)), Times.Once);
        }

        [TestMethod]
        public void Handle_EmptyCache_ShouldReturnTrueAndDoNothing()
        {
            // 1. Arrange - 模擬 Redis 空空如也 (回傳 null actions)
            messageCacheRepoMock.Setup(x => x.Pop<Message>(5))
                .Returns((null, null));

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act
            var result = handler.Handle(fakeStream);

            // 3. Assert - 應該提早結束，不呼叫任何資料庫
            Assert.IsTrue(result);
            messagePoRepoMock.Verify(x => x.BatchAdd(It.IsAny<List<Message>>()), Times.Never);
            subjectPoRepoMock.Verify(x => x.GetByIds(It.IsAny<List<int>>()), Times.Never);
        }

        [TestMethod]
        public void Handle_RedisThrowsException_ShouldCatchAndReturnTrue()
        {
            // 1. Arrange - 模擬 Redis 當機
            messageCacheRepoMock.Setup(x => x.Pop<Message>(5))
                .Returns((new Exception("Redis is down"), null));

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act
            var result = handler.Handle(fakeStream);

            // 3. Assert - 應該捕捉例外並安全回傳 true，避免 RabbitMQ 狂發重試
            Assert.IsTrue(result);
            messagePoRepoMock.Verify(x => x.BatchAdd(It.IsAny<List<Message>>()), Times.Never);
        }
    }
}
