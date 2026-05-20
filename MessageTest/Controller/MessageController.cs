using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Repository;
using Newtonsoft.Json;
using NLog;

namespace MessageTest.Controller
{
    public class MessageController : ApiController
    {
        private readonly IMessagePoRepository messagePoRepository;
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ILifetimeScope lifetimeScope;
        private readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(MessageController));

        public MessageController(IMessagePoRepository messagePoRepository, ISubjectPoRepository subjectPoRepository, ILifetimeScope lifetimeScope)
        {
            this.messagePoRepository = messagePoRepository;
            this.subjectPoRepository = subjectPoRepository;
            this.lifetimeScope = lifetimeScope;
        }

        [HttpPost]
        public HttpResponseMessage PostMessage([FromBody] AddMessageRequestDto input)
        {
            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var querySubject = this.subjectPoRepository.GetById(input.SubjectId);
                if (querySubject.exception != null) 
                {
                    throw querySubject.exception;
                }
                if (querySubject.subject == null)
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new AddMessageResponseDto
                    {
                        Status = AddMessageStatus.QueryHasNotSubject,
                        Message = null
                    }));
                }

                var addMessage = this.messagePoRepository.Add(input);

                if (addMessage.exception != null)
                {
                    throw addMessage.exception;
                }
                if (addMessage.message == null)
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new AddMessageResponseDto
                    {
                        Status = AddMessageStatus.Fail,
                        Message = null
                    }));
                }
                querySubject.subject.MessageCount = querySubject.subject.MessageCount + 1;
                var upsertSubject = this.subjectPoRepository.Upsert(querySubject.subject);
                if (upsertSubject.exception != null) 
                {
                    logger.Info($"PostMessage subjectPoRepository.Upsert expcetion{JsonConvert.SerializeObject(upsertSubject.exception)}");
                }
                if(upsertSubject.subject == null)
                {
                    logger.Info($"PostMessage subjectPoRepository.Upsert failed{JsonConvert.SerializeObject(input)}");
                }
                result.Content = new StringContent(JsonConvert.SerializeObject(new AddMessageResponseDto
                {
                    Status = AddMessageStatus.Success,
                    Message = addMessage.message
                }));
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"add message failed {JsonConvert.SerializeObject(input)}");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteMessage([FromUri] DeleteMessageRequestDto input)
        {
            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var queryByAccount = this.messagePoRepository.GetByAccount(input.UserId);
                if (queryByAccount.exception != null)
                {
                    throw queryByAccount.exception;
                }
                bool isExists = false;
                if (queryByAccount.messages != null)
                {
                    isExists = queryByAccount.messages.Any(m => m.Id == input.MessageId);
                }
                if (queryByAccount.messages == null || !isExists)
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new DeleteMessageResponseDto
                    {
                        Status = DeleteMessageStatus.UserHasNotMessage,
                        Message = null
                    }));
                    return result;
                }
                var deleteResult = this.messagePoRepository.Delete(input);

                if (deleteResult.exception != null)
                {
                    throw deleteResult.exception;
                }
                if (deleteResult.message == null)
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new DeleteMessageResponseDto
                    {
                        Status = DeleteMessageStatus.Fail,
                        Message = null
                    }));
                }
                var queryById = this.subjectPoRepository.GetById(input.SubjectId);
                if (queryById.exception != null)
                {
                    logger.Info($"DeleteMessage subjectPoRepository.GetById expcetion{JsonConvert.SerializeObject(queryById.exception)}");
                }
                if (queryById.subject == null)
                {
                    logger.Info($"DeleteMessage subjectPoRepository.GetById fail{JsonConvert.SerializeObject(input)}");
                }
                queryById.subject.MessageCount = queryById.subject.MessageCount - 1;
                if (queryById.subject.MessageCount > 0) queryById.subject.MessageCount = 0;
                var upsertSubject = this.subjectPoRepository.Upsert(queryById.subject);
                if (upsertSubject.exception != null)
                {
                    logger.Info($"DeleteMessage subjectPoRepository.Upsert expcetion{JsonConvert.SerializeObject(upsertSubject.exception)}");
                }
                if (upsertSubject.subject == null)
                {
                    logger.Info($"DeleteMessage subjectPoRepository.Upsert failed{JsonConvert.SerializeObject(input)}");
                }
                result.Content = new StringContent(JsonConvert.SerializeObject(new DeleteMessageResponseDto
                {
                    Status = DeleteMessageStatus.Success,
                    Message = deleteResult.message
                }));
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"delete message failed {JsonConvert.SerializeObject(input)}");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage QueryMessage([FromBody] QueryMessageRequestDto input)
        {
            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var queryResult = this.messagePoRepository.Query(input);

                if (queryResult.exception != null)
                {
                    throw queryResult.exception;
                }
                result.Content = new StringContent(JsonConvert.SerializeObject(new QueryMessageResponseDto
                {
                    TotalCount = queryResult.messages.Count,
                    Items = queryResult.messages,
                    Request = input
                }));
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"query message failed {JsonConvert.SerializeObject(input)}");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
