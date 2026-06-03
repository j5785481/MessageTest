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

        public (Exception exception, List<Message> messages) BatchAdd(List<Message> input)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    if (input == null || !input.Any())
                        return (null, null);

                    // 1. 準備 TVP 資料：型態指定為 string
                    var udt = new DataTable();
                    udt.Columns.Add(nameof(MessagePo.f_id), typeof(string));
                    udt.Columns.Add(nameof(MessagePo.f_subjectId), typeof(int));
                    udt.Columns.Add(nameof(MessagePo.f_content), typeof(string));
                    udt.Columns.Add(nameof(MessagePo.f_userId), typeof(string));
                    udt.Columns.Add(nameof(MessagePo.f_floor), typeof(int));
                    udt.Columns.Add(nameof(MessagePo.f_createdAt), typeof(DateTime));

                    foreach (var message in input)
                    {
                        var dr = udt.NewRow();
                        dr[nameof(MessagePo.f_id)] = message.Id;
                        dr[nameof(MessagePo.f_subjectId)] = message.SubjectId;
                        dr[nameof(MessagePo.f_content)] = message.Content;
                        dr[nameof(MessagePo.f_userId)] = message.UserId;
                        dr[nameof(MessagePo.f_floor)] = message.Floor;
                        dr[nameof(MessagePo.f_createdAt)] = message.CreatedAt;
                        udt.Rows.Add(dr);
                    }

                    // 2. 呼叫 SP，並對應到剛剛建立的 dbo.StringListType
                    var batchAddResult = cn.Query<MessagePo>(
                        "pro_messageBatchUpsert",
                        new
                        {
                            BatchMessages = udt.AsTableValuedParameter("dbo.type_batchMessage")
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    if (batchAddResult == null || !batchAddResult.Any())
                        return (null, null);

                    // 3. 將 Po 轉成 Domain Object List
                    var messages = batchAddResult.Select(po => new Message
                    {
                        SubjectId = po.f_subjectId,
                        Id = po.f_id, 
                        Content = po.f_content,
                        UserId = po.f_userId,
                        Floor = po.f_floor,
                        CreatedAt = po.f_createdAt,
                    }).ToList();

                    return (null, messages);
                }
            }
            catch (Exception ex)
            {
                return (ex, (List<Message>)null);
            }
        }

        public (Exception exception, List<Message> messages) BatchDelete(List<DeleteMessageRequestDto> reqs)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    if (reqs == null || !reqs.Any())
                        return (null, null);

                    // 1. 準備 TVP 資料：型態指定為 string
                    var udt = new DataTable();
                    udt.Columns.Add("f_messageId", typeof(string));

                    foreach (var req in reqs)
                    {
                        // 💡 關鍵：轉成字串存入 DataTable
                        // 如果你當初存進資料庫是帶有連字號且小寫的格式，可以用 ToString().ToLower() 確保一致
                        string idString = req.MessageId.ToString().ToLower();
                        udt.Rows.Add(idString);
                    }

                    // 2. 呼叫 SP，並對應到剛剛建立的 dbo.StringListType
                    var deleteResults = cn.Query<MessagePo>(
                        "pro_messageBatchDelete",
                        new
                        {
                            MessageIds = udt.AsTableValuedParameter("dbo.type_message")
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    if (deleteResults == null || !deleteResults.Any())
                        return (null, null);

                    // 3. 將 Po 轉成 Domain Object List
                    var messages = deleteResults.Select(po => new Message
                    {
                        SubjectId = po.f_subjectId,
                        Id = po.f_id, // 此時 po.f_id 已經是 string 欄位了
                        Content = po.f_content,
                        UserId = po.f_userId,
                        Floor = po.f_floor,
                        CreatedAt = po.f_createdAt,
                    }).ToList();

                    return (null, messages);
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
                        Floor = po.f_floor,
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
                        Floor = po.f_floor,
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
