using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Live.PubSub.Core;
using MessageTest.DistributedLock;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Handler.Rmq.JobSchedule;
using MessageTest.Procedure.MessageProcedure;
using MessageTest.Tests.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RedLockNet;

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
        
        private Mock<ISubjectLocker> subjectLockerMock;

        private ContainerBuilder builder;
        private IContainer container;
        private ILifetimeScope scope;

        private ProcessMessageAddJobEventHandler handler;

        private Mock<IProQuerySubject> proQuerySubjectMock;
        private Mock<IProSaveMessage> proSaveMessageMock;
        private Mock<IProUpdateMessageCount> proUpdateMessageCountMock;
        private Mock<IProUpsertSubjects> proUpsertSubjectsMock;

        [TestInitialize]
        public void Setup()
        {
            messageCacheRepoMock = new Mock<IMessageCacheRepository>();
            messagePoRepoMock = new Mock<IMessagePoRepository>();
            messageRepoMock = new Mock<IMessageRepository>();
            subjectPoRepoMock = new Mock<ISubjectPoRepository>();
            subjectRepoMock = new Mock<ISubjectRepository>();

            subjectLockerMock = new Mock<ISubjectLocker>();
            // 預設給予成功的 Lock
            subjectLockerMock.Setup(x => x.GrabLock(It.IsAny<int>())).Returns(RedLockHelper.GetRedLock(true).Object);

            // 使用 Autofac ContainerBuilder 註冊實體與 Mock
            builder = new ContainerBuilder();

            // 註冊 Repository Mocks
            builder.Register(c => messageCacheRepoMock.Object).As<IMessageCacheRepository>();
            builder.Register(c => messagePoRepoMock.Object).As<IMessagePoRepository>();
            builder.Register(c => messageRepoMock.Object).As<IMessageRepository>();
            builder.Register(c => subjectPoRepoMock.Object).As<ISubjectPoRepository>();
            builder.Register(c => subjectRepoMock.Object).As<ISubjectRepository>();
            
            // 註冊 Locker Mock
            builder.Register(c => subjectLockerMock.Object).As<ISubjectLocker>();

            // 註冊 Procedure Mocks (預設直接回傳 ctx)
            proQuerySubjectMock = new Mock<IProQuerySubject>();
            proQuerySubjectMock.Setup(p => p.Process(It.IsAny<CtxMessage>(), It.IsAny<IntegrateMessageRequestDto>())).Returns<CtxMessage, IntegrateMessageRequestDto>((ctx, p) => ctx);
            proSaveMessageMock = new Mock<IProSaveMessage>();
            proSaveMessageMock.Setup(p => p.Process(It.IsAny<CtxMessage>())).Returns<CtxMessage>(ctx => ctx);
            proUpdateMessageCountMock = new Mock<IProUpdateMessageCount>();
            proUpdateMessageCountMock.Setup(p => p.Process(It.IsAny<CtxMessage>())).Returns<CtxMessage>(ctx => ctx);
            proUpsertSubjectsMock = new Mock<IProUpsertSubjects>();
            proUpsertSubjectsMock.Setup(p => p.Process(It.IsAny<CtxMessage>())).Returns<CtxMessage>(ctx => ctx);
            
            builder.Register(c => proQuerySubjectMock.Object).As<IProQuerySubject>();
            builder.Register(c => proSaveMessageMock.Object).As<IProSaveMessage>();
            builder.Register(c => proUpdateMessageCountMock.Object).As<IProUpdateMessageCount>();
            builder.Register(c => proUpsertSubjectsMock.Object).As<IProUpsertSubjects>();

            container = builder.Build();
            scope = container.BeginLifetimeScope();

            handler = new ProcessMessageAddJobEventHandler(
                messageCacheRepoMock.Object,
                messagePoRepoMock.Object,
                messageRepoMock.Object,
                subjectPoRepoMock.Object,
                subjectRepoMock.Object,
                scope // 將 scope 作為 ILifetimeScope 傳入
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

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act - 執行處理
            var result = handler.Handle(fakeStream);

            // 3. Assert - 驗證
            Assert.IsTrue(result, "Handler 應該回傳 true");
            
            // 驗證 Procedure 是否有被執行
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.IsAny<IntegrateMessageRequestDto>()), Times.Once);
            proSaveMessageMock.Verify(x => x.Process(It.IsAny<CtxMessage>()), Times.Once);
            proUpdateMessageCountMock.Verify(x => x.Process(It.IsAny<CtxMessage>()), Times.Once);
            proUpsertSubjectsMock.Verify(x => x.Process(It.IsAny<CtxMessage>()), Times.Once);
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

            // 3. Assert - 應該提早結束，不執行 Procedure
            Assert.IsTrue(result);
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.IsAny<IntegrateMessageRequestDto>()), Times.Never);
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
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.IsAny<IntegrateMessageRequestDto>()), Times.Never);
        }

        [TestMethod]
        public void Handle_RedLock_Failure_ShouldRollbackToCache()
        {
            // 1. Arrange - 準備 5 筆資料 (SubjectId = 1)
            var fakeMessages = new List<Message>();
            for (int i = 0; i < 5; i++)
            {
                fakeMessages.Add(new Message { Id = $"M{i}", SubjectId = 1, Content = $"Test {i}" });
            }

            // 模擬 Redis Pop 出 5 筆資料
            messageCacheRepoMock.Setup(x => x.Pop<Message>(5))
                .Returns((null, fakeMessages));

            // 模擬 RedLock 取得失敗 (IsAcquired = false)
            subjectLockerMock.Setup(x => x.GrabLock(1)).Returns(RedLockHelper.GetRedLock(false).Object);

            // 模擬 Rollback 回 Redis 成功
            messageCacheRepoMock.Setup(x => x.Set(It.Is<List<Message>>(m => m.All(msg => msg.SubjectId == 1))))
                .Returns((Exception)null);

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act
            var result = handler.Handle(fakeStream);

            // 3. Assert
            Assert.IsTrue(result, "即使 Lock 失敗，Handler 也應該回傳 true 避免 MQ 重試");
            
            // 驗證是否有呼叫 Set 進行 Rollback
            messageCacheRepoMock.Verify(x => x.Set(It.Is<List<Message>>(m => m.Count == 5)), Times.Once);

            // 驗證流程是否在 Lock 失敗後提早結束 (不應該執行 Procedure)
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.IsAny<IntegrateMessageRequestDto>()), Times.Never);
        }

        [TestMethod]
        public void Handle_RedLock_PartialFailure_ShouldRollbackOnlyFailedSubject()
        {
            // 1. Arrange - 準備 5 筆資料，分屬於 Subject 1 與 Subject 2
            var fakeMessages = new List<Message>
            {
                new Message { Id = "M1", SubjectId = 1, Content = "Content 1" },
                new Message { Id = "M2", SubjectId = 1, Content = "Content 2" },
                new Message { Id = "M3", SubjectId = 2, Content = "Content 3" },
                new Message { Id = "M4", SubjectId = 2, Content = "Content 4" },
                new Message { Id = "M5", SubjectId = 2, Content = "Content 5" }
            };

            // 模擬 Redis Pop
            messageCacheRepoMock.Setup(x => x.Pop<Message>(5))
                .Returns((null, fakeMessages));

            // 模擬 Subject 1 取得 Lock 失敗，Subject 2 取得 Lock 成功
            subjectLockerMock.Setup(x => x.GrabLock(1)).Returns(RedLockHelper.GetRedLock(false).Object);
            subjectLockerMock.Setup(x => x.GrabLock(2)).Returns(RedLockHelper.GetRedLock(true).Object);

            // 模擬 Rollback Subject 1 的資料
            messageCacheRepoMock.Setup(x => x.Set(It.Is<List<Message>>(m => m.All(msg => msg.SubjectId == 1))))
                .Returns((Exception)null);

            var fakeStream = new RabbitMqEventStream("ProcessMessageAddJobEvent", "{}", 123);

            // 2. Act
            var result = handler.Handle(fakeStream);

            // 3. Assert
            Assert.IsTrue(result);

            // 驗證是否只對 Subject 1 的 2 筆資料進行 Rollback
            messageCacheRepoMock.Verify(x => x.Set(It.Is<List<Message>>(m => m.Count == 2 && m.All(msg => msg.SubjectId == 1))), Times.Once);

            // 驗證 Subject 2 是否有正常執行 Procedure (發生在 Lock 成功後)
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.Is<IntegrateMessageRequestDto>(p => p.SubjectId == 2)), Times.Once);
            
            // 驗證 Subject 1 是否沒有執行 Procedure
            proQuerySubjectMock.Verify(x => x.Process(It.IsAny<CtxMessage>(), It.Is<IntegrateMessageRequestDto>(p => p.SubjectId == 1)), Times.Never);
        }
    }
}
