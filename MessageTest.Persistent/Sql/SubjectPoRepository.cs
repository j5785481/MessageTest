using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;

namespace MessageTest.Persistent.Sql
{
    public class SubjectPoRepository : ISubjectRepository
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        private string connectionString;

        public SubjectPoRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public (Exception exception, Subject subject) Add(AddSubjectRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_subjectAdd",
                        new
                        {
                            f_title = req.SubjectTitle,
                            f_content = req.SubjectContent,
                            f_creatorId = req.UserId,
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (new Exception("新增失敗"), null);

                    // 轉換 Po -> Domain Object
                    var subject = new Subject
                    {
                        Id = result.f_id,
                        Title = result.f_title,
                        Content = result.f_content,
                        CreatorId = result.f_creatorId,
                        CreatedAt = result.f_createdAt,
                        MessageCount = result.f_messageCount,
                    };

                    return (null, subject);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }
    }
}
