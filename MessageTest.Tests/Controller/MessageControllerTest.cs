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
using Live.Libs;
using MessageTest.Controller;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
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
        private Mock<ILifetimeScope> lifetimeScope = new Mock<ILifetimeScope>();

        [TestMethod]
        public void PostTest()
        {
            var addMessageReqDto = new AddMessageRequestDto
            {
                UserId = "115051801",
                SubjectId = 1,
                Content = "Message Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var timeStampTime = TimeStampHelper.ToLocalDateTime(addMessageReqDto.ClientTimeStamp);
            var guId = Guid.NewGuid().ToString();
            messagePoRepository.Setup(p => p.Add(It.IsAny<AddMessageRequestDto>()))
                .Returns((null, new Message()
                {
                    SubjectId = 1,
                    Id = guId,
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
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            subjectPoRepository.Setup(p => p.Upsert(It.IsAny<Subject>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 1
                }));

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.PostMessage(addMessageReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            messagePoRepository.Verify(p => p.Add(It.Is<AddMessageRequestDto>(s => s.Content != "")), Times.Once, "MSSQL Add 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "subjectPoRepository GetById 應該要被呼叫一次");
            subjectPoRepository.Verify(p => p.Upsert(It.Is<Subject>(s => s.Id == 1)), Times.Once, "subjectPoRepository Upsert 應該要被呼叫一次");
            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddMessageResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(AddMessageStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Message.SubjectId); // 驗證是否拿到 Mock 給的 Id
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
            

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, lifetimeScope.Object);
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


            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, lifetimeScope.Object);
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

            var controller = new MessageController(messagePoRepository.Object, subjectPoRepository.Object, lifetimeScope.Object);
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
    }
}
