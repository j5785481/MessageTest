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
    public class SubjectControllerTests
    {
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
            subjectRepository.Setup(p => p.Add(It.IsAny<AddSubjectRequestDto>()))
                .Returns((null, new Subject()
                {
                    Id = 1,
                    Title = "Test",
                    Content = "Test",
                    CreatorId = "115051201",
                    CreatedAt = timeStampTime,
                    MessageCount = 0
                }));

            var controller = new SubjectController(subjectRepository.Object, lifetimeScope.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            var postResult = controller.PostSubject(addSubjectReqDto);

            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode);

            var responseString = postResult.Content.ReadAsStringAsync().Result;
            var responseDto = JsonConvert.DeserializeObject<AddSubjectResponseDto>(responseString);

            Assert.IsNotNull(responseDto);
            Assert.AreEqual(AddSubjectStatus.Success, responseDto.Status);
            Assert.AreEqual(1, responseDto.Subject.Id); // 驗證是否拿到 Mock 給的 Id
        }
    }
}
