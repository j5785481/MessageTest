using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Core.Lifetime;
using MessageTest.Controller;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Lib;
using MessageTest.Persistent.Mongo;
using MessageTest.Persistent.Redis;
using MessageTest.Persistent.Sql;
using Moq;
using Newtonsoft.Json;

namespace MessageTest.Tests.Controller
{
    [TestClass]
    public class MessageControllerTest
    {
        private Mock<IMessagePoRepository> messagePoRepository = new Mock<IMessagePoRepository>();
        private Mock<ISubjectPoRepository> subjectPoRepository = new Mock<ISubjectPoRepository>();
        private Mock<IMessageRepository> messageRepository = new Mock<IMessageRepository>();
        private Mock<ISubjectRepository> subjectRepository = new Mock<ISubjectRepository>();
        private Mock<IMessageCacheRepository> messageCacheRepository = new Mock<IMessageCacheRepository>();
        private Mock<ILifetimeScope> lifetimeScope = new Mock<ILifetimeScope>();

        [TestMethod]
        public void PostTest_Success()
        {
            // 1. Arrange - 準備測試資料
            var addMessageReqDto = new AddMessageRequestDto
            {
                UserId = "115051801",
                SubjectId = 1,
                Content = "Message Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var timeStampTime = TimeStampHelper.ToLocalDateTime(addMessageReqDto.ClientTimeStamp);

            // Mock: 模擬撈取 Subject 成功
            subjectPoRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));

            // Mock: 模擬寫入 Redis 成功 (回傳 null 代表沒有 Exception)
            messageCacheRepository.Setup(p => p.Set(It.IsAny<Message[]>()))
                .Returns((Exception)null);

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            // 2. Act - 執行方法
            var postResult = controller.PostMessage(addMessageReqDto);
            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddMessageResponseDto>(responseString);

            // 3. Assert - 驗證結果
            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);
            Assert.IsNotNull(responseDto);
            Assert.AreEqual(AddMessageStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Message.SubjectId);
            Assert.AreEqual("Message Test", responseDto.Message.Content);

            // 驗證是否有依照新流程正確呼叫對應的方法
            subjectPoRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "subjectPoRepository GetById 應該要被呼叫一次");
            messageCacheRepository.Verify(p => p.Set(It.IsAny<Message[]>()), Times.Once, "寫入 Redis 緩存 應該要被呼叫一次");

            // 驗證「不應該」呼叫直接寫入資料庫的方法
            messagePoRepository.Verify(p => p.Add(It.IsAny<AddMessageRequestDto>()), Times.Never, "已經改為寫入快取，不應再直接呼叫 MSSQL Add");
        }

        [TestMethod]
        public void PostTest_SubjectNotFound_ShouldReturnQueryHasNotSubject()
        {
            // 1. Arrange
            var addMessageReqDto = new AddMessageRequestDto { UserId = "11", SubjectId = 999, Content = "Test" };

            // Mock: 模擬找不到 Subject (回傳 subject 為 null)
            subjectPoRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, (Subject)null));

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            // 2. Act
            var postResult = controller.PostMessage(addMessageReqDto);
            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddMessageResponseDto>(responseString);

            // 3. Assert
            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);
            Assert.AreEqual(AddMessageStatus.QueryHasNotSubject, responseDto.Status); // 驗證找不到主題的狀態
            Assert.IsNull(responseDto.Message);

            // 驗證流程在找不到 Subject 時，絕對不能呼叫 Redis 寫入
            messageCacheRepository.Verify(p => p.Set(It.IsAny<Message[]>()), Times.Never);
        }

        [TestMethod]
        public void PostTest_RedisSetFail_ShouldReturnFailStatus()
        {
            // 1. Arrange
            var addMessageReqDto = new AddMessageRequestDto { UserId = "11", SubjectId = 1, Content = "Test" };

            subjectPoRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new Subject() { Id = 1 }));

            // Mock: 模擬寫入 Redis 發生例外
            messageCacheRepository.Setup(p => p.Set(It.IsAny<Message[]>()))
                .Returns(new Exception("Redis 連線失敗"));

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            // 2. Act
            var postResult = controller.PostMessage(addMessageReqDto);
            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddMessageResponseDto>(responseString);

            // 3. Assert
            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);
            Assert.AreEqual(AddMessageStatus.Fail, responseDto.Status); // 驗證 Redis 失敗會回傳 Fail
            Assert.IsNull(responseDto.Message);
        }

        [TestMethod]
        public void DeleteTest()
        {
            var messageId = Guid.NewGuid().ToString();
            var deleteMessageReqDto = new DeleteMessageRequestDto
            {
                UserId = "115051801",
                MessageId = messageId,
                SubjectId = 1,
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            messagePoRepository.Setup(p => p.GetByAccount(It.IsAny<string>()))
                .Returns((null, new List<Message>
                {
                    new Message()
                    {
                        SubjectId = 1,
                        Id = messageId,
                        Content = "Test",
                        UserId = "115051801",
                        CreatedAt = timeStampTime
                    }
                }));
            messagePoRepository.Setup(p => p.Delete(It.IsAny<DeleteMessageRequestDto>()))
                .Returns((null, new Message()
                {
                    SubjectId = 1,
                    Id = messageId,
                    Content = "Test",
                    UserId = "115051801",
                    CreatedAt = timeStampTime
                }));
            subjectPoRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051801",
                    CreatedAt = timeStampTime,
                    MessageCount = 1
                }));
            subjectPoRepository.Setup(p => p.Upsert(It.IsAny<Subject>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051801",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.DeleteMessage(deleteMessageReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            messagePoRepository.Verify(p => p.GetByAccount(It.Is<string>(s => s == "115051801")), Times.Once, "GetByAccount 應該要被呼叫一次");
            messagePoRepository.Verify(p => p.Delete(It.Is<DeleteMessageRequestDto>(s => s.UserId == "115051801")), Times.Once, "MSSQL Delete 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "GetById 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.Upsert(It.Is<Subject>(s => s.CreatorId == "115051801")), Times.Once, "Upsert 應該要被呼叫一次");

            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<DeleteMessageResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(DeleteMessageStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Message.SubjectId); // 驗證是否拿到 Mock 給的 Id
        }

        [TestMethod]
        public void BatchDeleteTest()
        {
            var messageId = Guid.NewGuid().ToString();
            var deleteMessageReqDto = new DeleteMessageRequestDto
            {
                UserId = "115051801",
                MessageId = messageId,
                SubjectId = 1,
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            messagePoRepository.Setup(p => p.GetByAccount(It.IsAny<string>()))
                .Returns((null, new List<Message>
                {
                    new Message()
                    {
                        SubjectId = 1,
                        Id = messageId,
                        Content = "Test",
                        UserId = "115051801",
                        CreatedAt = timeStampTime
                    }
                }));
            messagePoRepository.Setup(p => p.Delete(It.IsAny<DeleteMessageRequestDto>()))
                .Returns((null, new Message()
                {
                    SubjectId = 1,
                    Id = messageId,
                    Content = "Test",
                    UserId = "115051801",
                    CreatedAt = timeStampTime
                }));
            subjectPoRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051801",
                    CreatedAt = timeStampTime,
                    MessageCount = 1
                }));
            subjectPoRepository.Setup(p => p.Upsert(It.IsAny<Subject>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051801",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));


            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.DeleteMessage(deleteMessageReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            messagePoRepository.Verify(p => p.GetByAccount(It.Is<string>(s => s == "115051801")), Times.Once, "GetByAccount 應該要被呼叫一次");
            messagePoRepository.Verify(p => p.Delete(It.Is<DeleteMessageRequestDto>(s => s.UserId == "115051801")), Times.Once, "MSSQL Delete 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "GetById 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.Upsert(It.Is<Subject>(s => s.CreatorId == "115051801")), Times.Once, "Upsert 應該要被呼叫一次");

            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<DeleteMessageResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(DeleteMessageStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Message.SubjectId); // 驗證是否拿到 Mock 給的 Id
        }

        [TestMethod]
        public void QueryTest()
        {
            var messageId = Guid.NewGuid().ToString();
            var queryMessageReqDto = new QueryMessageRequestDto
            {
                SubjectId = 1,
                LimitNumber = 0,
                Page = 0,
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            messagePoRepository.Setup(p => p.Query(It.IsAny<QueryMessageRequestDto>()))
                .Returns((null, new List<Message>()
                {
                    new Message
                    {
                        SubjectId = 1,
                        Id = new Guid().ToString(),
                        Content = "第一筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                    new Message
                    {
                        SubjectId = 1,
                        Id = new Guid().ToString(),
                        Content = "第二筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    }
                }));

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var queryResult = controller.QueryMessage(queryMessageReqDto);

            Assert.AreEqual(HttpStatusCode.OK, queryResult.StatusCode);

            messagePoRepository.Verify(p => p.Query(It.Is<QueryMessageRequestDto>(s => s.SubjectId == 1)), Times.Once, "MSSQL Query 應該要被呼叫一次");

            var responseString = queryResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<QueryMessageResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(2, responseDto.TotalCount); // 驗證是否拿到 Mock 給的 筆數
        }

        [TestMethod]
        public void QueryMessageTest()
        {
            var messageId = Guid.NewGuid().ToString();
            var queryMessageReqDto = new QueryMessageRequestDto
            {
                SubjectId = 1,
                LimitNumber = 1,
                Page = 1,
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            messageRepository.Setup(p => p.GetPageMessage(It.IsAny<QueryMessageRequestDto>()))
                .Returns((null, new List<Message>()
                {
                    new Message
                    {
                        SubjectId = 1,
                        Id = new Guid().ToString(),
                        Content = "第一筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                    //new Message
                    //{
                    //    SubjectId = 1,
                    //    Id = new Guid().ToString(),
                    //    Content = "第二筆測試訊息",
                    //    UserId = "115051801",
                    //    CreatedAt = timeStampTime,
                    //},
                    //new Message
                    //{
                    //    SubjectId = 2,
                    //    Id = new Guid().ToString(),
                    //    Content = "第二筆測試訊息",
                    //    UserId = "115051801",
                    //    CreatedAt = timeStampTime,
                    //}
                }));

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, messageRepository.Object, subjectRepository.Object, messageCacheRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var queryResult = controller.QueryMessage(queryMessageReqDto);

            Assert.AreEqual(HttpStatusCode.OK, queryResult.StatusCode);

            messageRepository.Verify(p => p.GetPageMessage(It.Is<QueryMessageRequestDto>(s => s.SubjectId == 1)), Times.Once, "messageRepository GetPageMessage 應該要被呼叫一次");

            var responseString = queryResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<QueryMessageResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(1, responseDto.TotalCount); // 驗證是否拿到 Mock 給的 筆數
        }
    }
}
