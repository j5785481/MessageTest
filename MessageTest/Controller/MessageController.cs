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
        private readonly ILifetimeScope lifetimeScope;
        private readonly ILogger logger = LogManager.GetLogger("MessageTest")
            .WithProperty("Type", nameof(MessageController));

        public MessageController(IMessagePoRepository messagePoRepository, ILifetimeScope lifetimeScope)
        {
            this.messagePoRepository = messagePoRepository;
            this.lifetimeScope = lifetimeScope;
        }

        [HttpPost]
        public HttpResponseMessage PostMessage([FromBody] AddMessageRequestDto input)
        {
            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var addResult = this.messagePoRepository.Add(input);

                if (addResult.exception != null)
                {
                    throw addResult.exception;
                }
                result.Content = new StringContent(JsonConvert.SerializeObject(new AddMessageResponseDto
                {
                    Status = AddMessageStatus.Success,
                    Message = addResult.message
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
                var deleteResult = this.messagePoRepository.Delete(input);

                if (deleteResult.exception != null)
                {
                    throw deleteResult.exception;
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
