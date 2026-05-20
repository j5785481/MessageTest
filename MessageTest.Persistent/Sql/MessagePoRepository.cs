using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;

namespace MessageTest.Persistent.Sql
{
    public class MessagePoRepository : IMessagePoRepository
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        private string connectionString;

        public MessagePoRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public (Exception exception, Message message) Add(AddMessageRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var newId = Guid.NewGuid().ToString();
                    var result = cn.QueryFirstOrDefault<MessagePo>(
                        "pro_messageAdd",
                        new
                        {
                            f_userId = req.UserId,
                            f_content = req.Content,
                            f_subjectId = req.SubjectId,
                            f_id = newId,
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (null, null);

                    // 轉換 Po -> Domain Object
                    var message = new Message
                    {
                        SubjectId = req.SubjectId,
                        Id = result.f_id,
                        Content = result.f_content,
                        UserId = result.f_userId,
                        CreatedAt = result.f_createdAt,
                    };
                    return (null, message);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public (Exception exception, Message message) Delete(DeleteMessageRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var deleteResult = cn.QueryFirstOrDefault<MessagePo>(
                        "pro_messageDelete",
                        new
                        {
                            f_id = req.MessageId
                        },
                        commandType: CommandType.StoredProcedure);
                    if (deleteResult == null) return (new Exception("刪除失敗"), null);

                    // 轉換 Po -> Domain Object
                    var message = new Message
                    {
                        SubjectId = deleteResult.f_subjectId,
                        Id = deleteResult.f_id,
                        Content = deleteResult.f_content,
                        UserId = deleteResult.f_userId,
                        CreatedAt = deleteResult.f_createdAt,
                    };
                    return (null, message);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public (Exception exception, List<Message> messages) GetByAccount(string userId)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var messageGetByAccountResult = cn.Query<MessagePo>(
                        "pro_messageGetByAccount",
                        new
                        {
                            f_userId = userId
                        },
                        commandType: CommandType.StoredProcedure);
                    if (messageGetByAccountResult == null || !messageGetByAccountResult.Any()
                        || messageGetByAccountResult.Count() == 0)
                        return (null, null);

                    var accountMessagesList = messageGetByAccountResult.Select(po => new Message
                    {
                        // 在這裡把 PO 的屬性賦值給 Message
                        SubjectId = po.f_subjectId,
                        Id = po.f_id,
                        Content = po.f_content,
                        UserId = po.f_userId,
                        CreatedAt = po.f_createdAt,
                    }).ToList();

                    return (null, accountMessagesList);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public (Exception exception, List<Message> messages) Query(QueryMessageRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.Query<MessagePo>(
                        "pro_messageQuery",
                        new
                        {
                            f_subjectId = req.SubjectId,
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null || !result.Any())
                        return (null, new List<Message>());
                    var messagesList = result.Select(po => new Message
                    {
                        // 在這裡把 PO 的屬性賦值給 Message
                        SubjectId = po.f_subjectId,
                        Id = po.f_id,
                        Content = po.f_content,
                        UserId = po.f_userId,
                        CreatedAt = po.f_createdAt,
                    }).ToList();

                    return (null, messagesList);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }
    }
}
