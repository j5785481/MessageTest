using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Repository;
using MessageTest.Persistent.Sql;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MessageTest.Persistent.Tests.RepositoryPo
{
    [TestClass]
    public class MessagePoRepositoryTest
    {
        private const string connectionString = @"Data Source=DESKTOP-DMHFTKJ\SQLEXPRESS;database=MessageTest;Integrated Security=True";

        private IMessagePoRepository repo;

        [TestInitialize]
        public void Init()
        {
            var sqlTruncate = @"TRUNCATE TABLE t_message;";

            using (var cn = new SqlConnection(connectionString))
            {
                cn.Execute(sqlTruncate);
            }

            this.repo = new MessagePoRepository(connectionString);
        }

        [TestMethod]
        public void 新增留言()
        {
            var addMessageReqDto = new AddMessageRequestDto
            {
                UserId = "115051801",
                SubjectId = 1,
                Content = "Message Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var addMessageResult = this.repo.Add(addMessageReqDto);
            Assert.IsNull(addMessageResult.exception);
            Assert.IsNotNull(addMessageResult.message);
            Assert.IsNotNull(addMessageResult.message.Id);
            Assert.AreEqual(addMessageResult.message.SubjectId, 1);
        }

        [TestMethod]
        public void 刪除主題()
        {
            var addMessageReqDto = new AddMessageRequestDto
            {
                UserId = "115051801",
                SubjectId = 1,
                Content = "Message Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var addMessageResult = this.repo.Add(addMessageReqDto);
            Assert.IsNull(addMessageResult.exception);
            Assert.IsNotNull(addMessageResult.message);
            Assert.IsNotNull(addMessageResult.message.Id);
            Assert.AreEqual(addMessageResult.message.SubjectId, 1);
            var userId = addMessageResult.message.UserId;
            var messageId = addMessageResult.message.Id;
            var subjectId = addMessageResult.message.SubjectId;
            var deleteSubjectReqDto = new DeleteMessageRequestDto
            {
                UserId = userId,
                MessageId = messageId,
                SubjectId = subjectId
            };

            var deleteResult = this.repo.Delete(deleteSubjectReqDto);

            Assert.IsNull(deleteResult.exception);
            Assert.IsNotNull(deleteResult.message);
            Assert.AreEqual(deleteResult.message.Id, messageId);
            Assert.AreEqual(deleteResult.message.UserId, userId);
        }

        [TestMethod]
        public void 批次刪除留言()
        {
            var addMessageReqDtos = new List<AddMessageRequestDto>
            {
                new AddMessageRequestDto
                {
                    UserId = "115051801",
                    SubjectId = 1,
                    Content = "第一筆測試訊息",
                    ClientTimeStamp = 1778549400,
                    CreateTimeStamp = 1778549400
                },
                new AddMessageRequestDto
                {
                    UserId = "115051802", // 不同的使用者
                    SubjectId = 1,
                    Content = "第二筆測試訊息",
                    ClientTimeStamp = 1778549500,
                    CreateTimeStamp = 1778549500
                }
            };
            var messageId = new List<string>();
            addMessageReqDtos.ForEach(dto =>
            {
                var addMessageResult = this.repo.Add(dto);
                Assert.IsNull(addMessageResult.exception);
                Assert.IsNotNull(addMessageResult.message);
                Assert.IsNotNull(addMessageResult.message.Id);
                Assert.AreEqual(addMessageResult.message.SubjectId, 1);
                messageId.Add(addMessageResult.message.Id);
            });

            var batchDeleteRequestDto = new List<DeleteMessageRequestDto>
            {
                new DeleteMessageRequestDto
                {
                    SubjectId = 1,
                    UserId = "115051801",
                    MessageId = messageId[0]
                },
                new DeleteMessageRequestDto
                {
                    SubjectId = 1,
                    UserId = "115051802",
                    MessageId = messageId[1]
                }
            };
        }

        [TestMethod]
        public void 查詢主題所有留言()
        {
            var addMessageReqDtos = new List<AddMessageRequestDto>
            {
                new AddMessageRequestDto
                {
                    UserId = "115051801",
                    SubjectId = 1,
                    Content = "第一筆測試訊息",
                    ClientTimeStamp = 1778549400,
                    CreateTimeStamp = 1778549400
                },
                new AddMessageRequestDto
                {
                    UserId = "115051802", // 不同的使用者
                    SubjectId = 1,
                    Content = "第二筆測試訊息",
                    ClientTimeStamp = 1778549500,
                    CreateTimeStamp = 1778549500
                } 
            };
            addMessageReqDtos.ForEach(dto =>
            {
                var addMessageResult = this.repo.Add(dto);
                Assert.IsNull(addMessageResult.exception);
                Assert.IsNotNull(addMessageResult.message);
                Assert.IsNotNull(addMessageResult.message.Id);
                Assert.AreEqual(addMessageResult.message.SubjectId, 1);
            });

            var queryRequestDto = new QueryMessageRequestDto
            {
                SubjectId = 1,
                LimitNumber = 0,
                Page = 0
            };
            var getByIdResult = this.repo.Query(queryRequestDto);
            Assert.IsNull(getByIdResult.exception);
            Assert.IsNotNull(getByIdResult.messages);
            Assert.AreEqual(getByIdResult.messages.Count, 2);
        }
    }
}