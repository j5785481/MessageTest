using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;

namespace MessageTest.Persistent.Sql
{
    public class SubjectPoRepository : ISubjectPoRepository
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

        public (Exception exception, List<Subject> subjects) BatchUpsert(List<Subject> input)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var udt = new DataTable();
                    udt.Columns.Add(nameof(SubjectPo.f_id), typeof(int));
                    udt.Columns.Add(nameof(SubjectPo.f_title), typeof(string));
                    udt.Columns.Add(nameof(SubjectPo.f_content), typeof(string));
                    udt.Columns.Add(nameof(SubjectPo.f_creatorId), typeof(string));
                    udt.Columns.Add(nameof(SubjectPo.f_createdAt), typeof(DateTime));
                    udt.Columns.Add(nameof(SubjectPo.f_messageCount), typeof(string));
                    foreach (var inputSubject in input)
                    {
                        var dr = udt.NewRow();
                        dr[nameof(SubjectPo.f_id)] = inputSubject.Id;
                        dr[nameof(SubjectPo.f_title)] = inputSubject.Title;
                        dr[nameof(SubjectPo.f_content)] = inputSubject.Content;
                        dr[nameof(SubjectPo.f_creatorId)] = inputSubject.CreatorId;
                        dr[nameof(SubjectPo.f_createdAt)] = inputSubject.CreatedAt;
                        dr[nameof(SubjectPo.f_messageCount)] = inputSubject.MessageCount;
                        udt.Rows.Add(dr);
                    }
                    var result = cn.Query<SubjectPo>(
                        "pro_subjectBatchUpsert",
                        new
                        {
                            BatchSubjects = udt.AsTableValuedParameter("dbo.type_batchUpsertSubject")
                        },
                        commandType: CommandType.StoredProcedure)
                        .ToList();
                    if (result == null || !result.Any())
                        return (null, null);

                    // 轉換 Po -> Domain Object
                    var subject = result.Select(s => new Subject
                    {
                        Id = s.f_id,
                        Title = s.f_title,
                        Content = s.f_content,
                        CreatorId = s.f_creatorId,
                        CreatedAt = s.f_createdAt,
                        MessageCount = s.f_messageCount,
                    }).ToList();

                    return (null, subject);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }

        public (Exception exception, Subject subject) Delete(int subjectId)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_subjectDelete",
                        new
                        {
                            f_id = subjectId,
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (new Exception("刪除失敗"), null);

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

        public (Exception exception, Subject subject) GetById(int subjectId)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_subjectGetById",
                        new
                        {
                            f_id = subjectId
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (null, null);

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

        public (Exception exception, List<Subject> subjects) GetByIds(List<int> subjectIds)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var udt = new DataTable();
                    udt.Columns.Add(nameof(SubjectPo.f_id), typeof(string));
                    foreach (var subjectId in subjectIds)
                    {
                        var dr = udt.NewRow();
                        dr[nameof(SubjectPo.f_id)] = subjectIds;
                        udt.Rows.Add(dr);
                    }
                    var result = cn.Query<SubjectPo>(
                        "pro_subjectGetByIds",
                        new
                        {
                            SubjectIds = udt.AsTableValuedParameter("dbo.type_batchGetByIdsSubject")
                        },
                        commandType: CommandType.StoredProcedure
                        ).ToList();
                    if (result == null || !result.Any())
                        return (null, null);
                    var subjects = result.Select(po => new Subject
                    {
                        Id = po.f_id,
                        Title = po.f_title,
                        Content = po.f_content,
                        CreatorId = po.f_creatorId,
                        CreatedAt = po.f_createdAt,
                        MessageCount = po.f_messageCount,
                    }).ToList();

                    return (null, subjects);
                }
            }
            catch (Exception ex) 
            {
                return (ex, null);
            }
        }

        public (Exception exception, Subject subject) Query(QuerySubjectRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_subjectGetById",
                        new
                        {
                            f_id = req.SubjectId,
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (null, null);

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

        public (Exception exception, Subject subject) Upsert(Subject input)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_subjectUpsert",
                        new
                        {
                            f_id = input.Id,
                            f_title = input.Title,
                            f_content = input.Content,
                            f_creatorId = input.CreatorId,
                            f_messageCount = input.MessageCount
                        },
                        commandType: CommandType.StoredProcedure);
                    if (result == null) return (null, null);

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
