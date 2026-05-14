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
using Moq;
using Newtonsoft.Json;

namespace MessageTest.Tests.Controller
{
    [TestClass]
    public class SubjectControllerTests
    {
        private Mock<ISubjectPoRepository> subjectPoRepository = new Mock<ISubjectPoRepository>();
        private Mock<ISubjectRepository> subjectRepository = new Mock<ISubjectRepository>();
        private Mock<ILifetimeScope> lifetimeScope = new Mock<ILifetimeScope>();

        [TestMethod]
        public void PostTest()
        {
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

            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, lifetimeScope.Object);
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
            var deleteSubjectReqDto = new DeleteSubjectRequestDto
            {
                UserId = "115051201",
                Id = 1
            };
            var clientTimeStamp = 1778549400;
            var timeStampTime = TimeStampHelper.ToLocalDateTime(clientTimeStamp);
            subjectPoRepository.Setup(p => p.Delete(It.IsAny<DeleteSubjectRequestDto>()))
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
            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.DeleteSubject(deleteSubjectReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            subjectRepository.Verify(p => p.Delete(It.Is<int>(s => s == 1)), Times.Once, "Mongo Save 應該要被呼叫一次且 Id 要對應");

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
            var controller = new SubjectController(subjectPoRepository.Object, subjectRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.QueryMessageCount(queryMessageCountReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            subjectRepository.Verify(p => p.GetById(It.Is<int>(s => s == 1)), Times.Once, "Mongo Save 應該要被呼叫一次且 Id 要對應");
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
