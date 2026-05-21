using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
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
    public class SubjectControllerTests
    {
        private Mock<ISubjectPoRepository> subjectPoRepository = new Mock<ISubjectPoRepository>();
        private Mock<ISubjectRepository> subjectRepository = new Mock<ISubjectRepository>();
        private Mock<ISubjectCacheRepository> subjectCacheRepository = new Mock<ISubjectCacheRepository>();
        private Mock<ISubjectColdDownRepository> subjectColdDownRepository = new Mock<ISubjectColdDownRepository>();
        private Mock<IMessagePoRepository> messagePoRepository = new Mock<IMessagePoRepository>();
        private Mock<IMessageRepository> messageRepository = new Mock<IMessageRepository>();
        private Mock<ILifetimeScope> lifetimeScope = new Mock<ILifetimeScope>();

        [TestMethod]
        public void PostTest()
        {   
            subjectColdDownRepository.Setup(p => p.TryLock(It.IsAny<string>()))
                .Returns((null, true));
            var addSubjectReqDto = new AddSubjectRequestDto
            {
                UserId = "115051201",
                SubjectTitle = "Test",
                SubjectContent = "Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var timeStampTime = TimeStampHelper.ToLocalDateTime(addSubjectReqDto.ClientTimeStamp);
            subjectPoRepository.Setup(p => p.Add(It.IsAny<AddSubjectRequestDto>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            subjectRepository.Setup(p => p.Save(It.IsAny<Subject>()))
                .Returns((Exception)null);

            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, subjectCacheRepository.Object, subjectColdDownRepository.Object, messagePoRepository.Object, messageRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.PostSubject(addSubjectReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            subjectRepository.Verify(p => p.Save(It.Is<Subject>(s => s.Id == 1)), Times.Once, "Mongo Save 應該要被呼叫一次且 Id 要對應");

            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddSubjectResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(AddSubjectStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Subject.Id); // 驗證是否拿到 Mock 給的 Id
        }

        [TestMethod]
        public void DeleteTest()
        {
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            var id1 = new Guid().ToString();
            var id2 = new Guid().ToString();
            messageRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new List<Message>()
                {
                    new Message
                    {
                        SubjectId = 1,
                        Id = id1,
                        Content = "第一筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                    new Message
                    {
                        SubjectId = 1,
                        Id = id2,
                        Content = "第二筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                }));
            messagePoRepository.Setup(p => p.BatchDelete(It.IsAny<List<DeleteMessageRequestDto>>()))
                .Returns((null, new List<Message>()
                {
                    new Message
                    {
                        SubjectId = 1,
                        Id = id1,
                        Content = "第一筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                    new Message
                    {
                        SubjectId = 1,
                        Id = id2,
                        Content = "第二筆測試訊息",
                        UserId = "115051801",
                        CreatedAt = timeStampTime,
                    },
                }));
            messageRepository.Setup(p => p.BatchDelete(It.IsAny<List<string>>()))
                .Returns((Exception)null);
            subjectPoRepository.Setup(p => p.Delete(It.IsAny<int>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            subjectRepository.Setup(p => p.Delete(It.IsAny<int>()))
                .Returns((Exception)null);
            subjectCacheRepository.Setup(p => p.Remove(It.IsAny<int>()))
                .Returns((null, true));
            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, subjectCacheRepository.Object, subjectColdDownRepository.Object, messagePoRepository.Object, messageRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.DeleteSubject(1);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            messageRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "messageRepository GetById 應該要被呼叫一次且 Id 要對應");
            messagePoRepository.Verify(p => p.BatchDelete(It.Is<List<DeleteMessageRequestDto>>(s => s.Count == 2)), Times.Once, "messagePoRepository BatchDelete 應該要被呼叫一次且筆數要對應");
            messageRepository.Verify(p => p.BatchDelete(It.Is<List<string>>(s => s[0] == id1)), Times.Once, "messageRepository BatchDelete 應該要被呼叫一次且第一筆資料要是砍messageId = id1");
            subjectRepository.Verify(p => p.Delete(It.Is<int>(s => s == 1)), Times.Once, "Mongo Delete 應該要被呼叫一次且 Id 要對應");
            subjectCacheRepository.Verify(p => p.Remove(It.Is<int>(s => s == 1)), Times.Once, "Redise Delete 應該要被呼叫一次且 Id 要對應");
            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<DeleteSubjectResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(DeleteSubjectStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Subject.Id); // 驗證是否拿到 Mock 給的 Id
        }

        [TestMethod]
        public void QueryTest()
        {
            var queryMessageCountReqDto = new QuerySubjectRequestDto
            {
                SubjectId = 1
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            //subjectCacheRepository.Setup(p => p.FindInSubjectId(It.IsAny<int>()))
            //    .Returns((null, new Subject()
            //    {
            //        Id = 1,
            //        Title = "Test",
            //        Content = "Test",
            //        CreatorId = "115051201",
            //        CreatedAt = timeStampTime,
            //        MessageCount = 0
            //    }));
            subjectCacheRepository.Setup(p => p.FindInSubjectId(It.IsAny<int>()))
                .Returns((null, null));
            subjectRepository.Setup(p => p.GetById(It.IsAny<int>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            subjectCacheRepository.Setup(p => p.Set(It.IsAny<Subject>()))
                .Returns((Exception)null);
            subjectPoRepository.Setup(p => p.Query(It.IsAny<QuerySubjectRequestDto>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));
            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, subjectCacheRepository.Object, subjectColdDownRepository.Object, messagePoRepository.Object, messageRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.QuerySubject(queryMessageCountReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            subjectCacheRepository.Verify(p => p.FindInSubjectId(It.Is<int>(s => s == 1)), Times.Once, "Redis Query 應該要被呼叫一次且 Id 要對應");
            subjectRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "Mongo Query 應該要被呼叫一次且 Id 要對應");
            subjectCacheRepository.Verify(p => p.Set(It.Is<Subject>(s => s.Id == 1)), Times.Once, "Redis Set 應該要被呼叫一次且 Id 要對應");
            subjectPoRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Never, "MMSQL不應該被call");

            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<QuerySubjectResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(QuerySubjectStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Subject.Id); // 驗證是否拿到 Mock 給的 Id
            Assert.AreEqual(0, responseDto.Subject.MessageCount);
        }
    }
}
