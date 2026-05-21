using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MessageTest.Hubs;
using MongoDB.Driver;
using Newtonsoft.Json;
using NLog;

namespace MessageTest.Controller
{
    public class SubjectController : ApiController
    {
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly ISubjectCacheRepository subjectCacheRepository;
        private readonly IMessagePoRepository messagePoRepository;
        private readonly IMessageRepository messageRepository;
        private readonly ISubjectColdDownRepository subjectColdDownRepository;
        private readonly ILifetimeScope lifetimeScope;
        private readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(SubjectController));

        public SubjectController(ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository, 
            ISubjectCacheRepository subjectCacheRepository, ISubjectColdDownRepository subjectColdDownRepository,
            IMessagePoRepository messagePoRepository, IMessageRepository messageRepository,
            ILifetimeScope lifetimeScope)
        {
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
            this.subjectCacheRepository = subjectCacheRepository;
            this.subjectColdDownRepository = subjectColdDownRepository;
            this.messagePoRepository = messagePoRepository;
            this.messageRepository = messageRepository;
            this.lifetimeScope = lifetimeScope;
        }

        [HttpPost]
        public HttpResponseMessage PostSubject([FromBody] AddSubjectRequestDto input)
        {
            try
            {
                var coldDownResult = this.subjectColdDownRepository.TryLock(input.UserId);
                if (coldDownResult.ex != null) 
                {
                    throw coldDownResult.ex;
                }
                if (coldDownResult.ok)
                {
                    var addResult = this.subjectPoRepository.Add(input);

                    if (addResult.exception != null)
                    {
                        throw addResult.exception;
                    }
                    var saveException = this.subjectRepository.Save(addResult.subject);
                    if (saveException != null)
                    {
                        throw saveException;
                    }
                    var result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StringContent(JsonConvert.SerializeObject(new AddSubjectResponseDto
                    {
                        Status = AddSubjectStatus.Success,
                        Subject = addResult.subject
                    }));
                    return result;
                }
                else
                {
                    var result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StringContent(JsonConvert.SerializeObject(new AddSubjectResponseDto
                    {
                        Status = AddSubjectStatus.AddSujectColdDown,
                        Subject = null
                    }));
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"add subject failed {JsonConvert.SerializeObject(input)}");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpDelete]
        public HttpResponseMessage DeleteSubject([FromUri] int subjectId)
        {
            try
            {
                var getByIdResult = this.messageRepository.GetById(subjectId);
                if(getByIdResult.exception == null)
                {
                    logger.Info($"DeleteSubject messageRepository.GetById expection{JsonConvert.SerializeObject(getByIdResult.exception)}");
                }
                List<DeleteMessageRequestDto> deleteMessageReqDtos = getByIdResult.messages
                    .Select(msg => new DeleteMessageRequestDto
                    {
                        UserId = msg.UserId,
                        MessageId = msg.Id,
                        SubjectId = msg.SubjectId
                    })
                    .ToList();
                var batchDeleteResult = this.messagePoRepository.BatchDelete(deleteMessageReqDtos);
                if (batchDeleteResult.exception != null)
                {
                    logger.Info($"DeleteSubject messagePoRepository.BatchDelete expection{JsonConvert.SerializeObject(batchDeleteResult.exception)}");
                }
                List<string> messageIds = batchDeleteResult.messages
                    .Select(msg => msg.Id).ToList();
                var batchDeleteExpection = this.messageRepository.BatchDelete(messageIds);
                if(batchDeleteExpection != null)
                {
                    logger.Info($"DeleteSubject messageRepository.BatchDelete expection{JsonConvert.SerializeObject(batchDeleteExpection)}");
                }
                var deleteResult = this.subjectPoRepository.Delete(subjectId);

                if (deleteResult.exception != null)
                {
                    throw deleteResult.exception;
                }
                var deleteException = this.subjectRepository.Delete(deleteResult.subject.Id);
                if (deleteException != null)
                {
                    throw deleteException;
                }
                var removeResult = this.subjectCacheRepository.Remove(deleteResult.subject.Id);
                if (removeResult.ex != null)
                {
                    throw removeResult.ex;
                }
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(JsonConvert.SerializeObject(new DeleteSubjectResponseDto
                {
                    Status = DeleteSubjectStatus.Success,
                    Subject = deleteResult.subject
                }));
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"delete subject failed {JsonConvert.SerializeObject(subjectId)}");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage QuerySubject([FromBody] QuerySubjectRequestDto input)
        {
            try
            {
                Subject finalSubject = null;
                var result = new HttpResponseMessage(HttpStatusCode.OK);

                //針對Redis進行查詢
                var findByIdResult = this.subjectCacheRepository.FindInSubjectId(input.SubjectId);
                if (findByIdResult.ex != null)
                {
                    throw findByIdResult.ex;
                }
                if (findByIdResult.subject != null)
                {
                    logger.Trace($"redis query have subject {JsonConvert.SerializeObject(findByIdResult.subject)}");
                    finalSubject = findByIdResult.subject;
                    result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                    {
                        Status = QuerySubjectStatus.Success,
                        Subject = finalSubject
                    }));
                    return result;
                }
                
                //針對Mongo進行查詢
                var getByIdResult = this.subjectRepository.GetById(input.SubjectId);
                if (getByIdResult.exception != null)
                {
                    throw getByIdResult.exception;
                }
                if (getByIdResult.subject != null)
                {
                    logger.Trace($"mongo query have subject {JsonConvert.SerializeObject(getByIdResult.subject)}");
                    finalSubject = getByIdResult.subject;
                    var setException = this.subjectCacheRepository.Set(finalSubject);
                    if (setException != null)
                    {
                        throw setException;
                    }
                    logger.Trace($"redis add subject {JsonConvert.SerializeObject(getByIdResult.subject)}");
                    result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                    {
                        Status = QuerySubjectStatus.Success,
                        Subject = finalSubject
                    }));
                    return result;
                }

                //針對MMSQL進行查詢
                var queryResult = this.subjectPoRepository.Query(input);
                if (queryResult.exception != null)
                {
                    throw queryResult.exception;
                }
                if (queryResult.subject != null)
                {
                    logger.Trace($"mmsql query have subject {JsonConvert.SerializeObject(queryResult.subject)}");
                    finalSubject = queryResult.subject;
                    var saveException = this.subjectRepository.Save(finalSubject);
                    if (saveException != null)
                    {
                        throw saveException;
                    }
                    logger.Trace($"mongo add subject {JsonConvert.SerializeObject(queryResult.subject)}");
                    var setException = this.subjectCacheRepository.Set(finalSubject);
                    if (setException != null)
                    {
                        throw setException;
                    }
                    logger.Trace($"redis add subject {JsonConvert.SerializeObject(queryResult.subject)}");
                    result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                    {
                        Status = QuerySubjectStatus.Success,
                        Subject = finalSubject
                    }));
                    return result;
                }
                logger.Info($"no this subject data {JsonConvert.SerializeObject(input)}");
                // 上述都沒有取得資料
                result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                {
                    Status = QuerySubjectStatus.NoHaveSubject,
                    Subject = finalSubject
                }));
                return result;
            }
            catch (Exception ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
