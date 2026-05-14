using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Repository;
using MessageTest.Persistent.Sql;

namespace MessageTest.Persistent.Tests.RepositoryPo
{
    [TestClass]
    public class SubjectPoRepositoryTests
    {
        private const string connectionString = @"Data Source=DESKTOP-DMHFTKJ\SQLEXPRESS;database=MessageTest;Integrated Security=True";

        private ISubjectRepository repo;

        [TestInitialize]
        public void Init()
        {
            var sqlStr = @"TRUNCATE TABLE t_subject;";

            using (var cn = new SqlConnection(connectionString))
            {
                cn.Execute(sqlStr);
            }

            this.repo = new SubjectPoRepository(connectionString);
        }

        [TestMethod]
        public void 新增主題()
        {
            var addSubjectReqDto = new AddSubjectRequestDto
            {
                UserId = "115051302",
                SubjectTitle = "Test",
                SubjectContent = "Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var result = this.repo.Add(addSubjectReqDto);

            Assert.IsNull(result.exception);
            Assert.IsNotNull(result.subject);
            Assert.AreEqual(result.subject.Id, 1);
            Assert.AreEqual(result.subject.Title, "Test");
        }

        [TestMethod]
        public void 刪除主題()
        {
            var addSubjectReqDto = new AddSubjectRequestDto
            {
                UserId = "115051302",
                SubjectTitle = "Test",
                SubjectContent = "Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var addResult = this.repo.Add(addSubjectReqDto);

            Assert.IsNull(addResult.exception);
            Assert.IsNotNull(addResult.subject);
            Assert.AreEqual(addResult.subject.Id, 1);
            Assert.AreEqual(addResult.subject.Title, "Test");

            int subjectId = 1;
            var getByIdResult = this.repo.GetById(subjectId);
            Assert.IsNull(getByIdResult.exception);
            Assert.IsNotNull(getByIdResult.subject);
            Assert.AreEqual(getByIdResult.subject.Id, 1);
            Assert.AreEqual(getByIdResult.subject.Title, "Test");
            string userId = "115051302";
            if (getByIdResult.subject.CreatorId.Equals(userId))
            {
                var deleteSubjectReqDto = new DeleteSubjectRequestDto
                {
                    UserId = userId,
                    Id = 1
                };

                var deleteResult = this.repo.Delete(deleteSubjectReqDto);

                Assert.IsNull(deleteResult.exception);
                Assert.IsNotNull(deleteResult.subject);
                Assert.AreEqual(deleteResult.subject.Id, 1);
                Assert.AreEqual(deleteResult.subject.Title, "Test");
            }
            else
            {
                Assert.IsNotNull(null);
            }
        }

        [TestMethod]
        public void 查詢主題留言數()
        {
            var addSubjectReqDto = new AddSubjectRequestDto
            {
                UserId = "115051302",
                SubjectTitle = "Test",
                SubjectContent = "Test",
                ClientTimeStamp = 1778549400,
                CreateTimeStamp = 1778549400
            };
            var addResult = this.repo.Add(addSubjectReqDto);
            Assert.IsNull(addResult.exception);
            Assert.IsNotNull(addResult.subject);
            Assert.AreEqual(addResult.subject.Id, 1);
            Assert.AreEqual(addResult.subject.Title, "Test");

            int subjectId = 1;
            var queryRequestDto = new QueryMessageCountRequestDto
            {
                SubjectId = 1
            };
            var getByIdResult = this.repo.Query(queryRequestDto);
            Assert.IsNull(getByIdResult.exception);
            Assert.IsNotNull(getByIdResult.subject);
            Assert.AreEqual(getByIdResult.subject.Id, 1);
            Assert.AreEqual(getByIdResult.subject.Title, "Test");
            Assert.AreEqual(getByIdResult.subject.CreatorId, "115051302");
            Assert.AreEqual(getByIdResult.subject.MessageCount, 0);
        }
    }
}
