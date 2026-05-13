using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace MessageTest.Controller
{
    public class SubjectController : ApiController
    {
        private readonly ISubjectRepository subjectRepository;
        private readonly ILifetimeScope lifetimeScope;

        public SubjectController(ISubjectRepository subjectRepository, ILifetimeScope lifetimeScope)
        {
            this.subjectRepository = subjectRepository;
            this.lifetimeScope = lifetimeScope;
        }

        [HttpPost]
        public HttpResponseMessage PostSubject([FromBody] AddSubjectRequestDto input)
        {
            try
            {
                var addResult = this.subjectRepository.Add(input);

                if (addResult.exception != null)
                {
                    throw addResult.exception;
                }
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(JsonConvert.SerializeObject(new AddSubjectResponseDto
                {
                    Status = AddSubjectStatus.Success,
                    Subject = addResult.subject
                }));
                return result;
            }
            catch (Exception ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpDelete]
        public HttpResponseMessage DeleteSubject([FromBody] DeleteSubjectRequestDto input)
        {
            try
            {
                var deleteResult = this.subjectRepository.Delete(input);

                if (deleteResult.exception != null)
                {
                    throw deleteResult.exception;
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
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage QueryMessageCount([FromBody] QueryMessageCountRequestDto input)
        {
            try
            {
                var queryResult = this.subjectRepository.Query(input);

                if (queryResult.exception != null)
                {
                    throw queryResult.exception;
                }
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(JsonConvert.SerializeObject(new QueryMessageCountResponseDto
                {
                    Status = QueryMessageCountStatus.Success,
                    Subject = queryResult.subject,
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
