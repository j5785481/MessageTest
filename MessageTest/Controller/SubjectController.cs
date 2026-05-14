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
        private readonly ISubjectPoRepository subjectPoRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly ILifetimeScope lifetimeScope;

        public SubjectController(ISubjectPoRepository subjectPoRepository, ISubjectRepository subjectRepository, ILifetimeScope lifetimeScope)
        {
            this.subjectPoRepository = subjectPoRepository;
            this.subjectRepository = subjectRepository;
            this.lifetimeScope = lifetimeScope;
        }

        [HttpPost]
        public HttpResponseMessage PostSubject([FromBody] AddSubjectRequestDto input)
        {
            try
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
                var deleteResult = this.subjectPoRepository.Delete(input);

                if (deleteResult.exception != null)
                {
                    throw deleteResult.exception;
                }
                var deleteException = this.subjectRepository.Delete(deleteResult.subject.Id);
                if (deleteException != null)
                {
                    throw deleteException;
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
        public HttpResponseMessage QueryMessageCount([FromBody] QuerySubjectRequestDto input)
        {
            try
            {
                Subject finalSubject = null;
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var getByIdResult = this.subjectRepository.GetById(input.SubjectId);
                if (getByIdResult.exception != null)
                {
                    throw getByIdResult.exception;
                }
                if (getByIdResult.subject != null)
                {
                    finalSubject = getByIdResult.subject;
                }
                else
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                    {
                        Status = QuerySubjectStatus.Success,
                        Subject = getByIdResult.subject,
                    }));
                    var queryResult = this.subjectPoRepository.Query(input);

                    if (queryResult.exception != null)
                    {
                        throw queryResult.exception;
                    }
                    if (queryResult.subject != null) 
                    {
                        finalSubject = queryResult.subject;
                        var saveException = this.subjectRepository.Save(finalSubject);
                        if (saveException != null)
                        {
                            throw saveException;
                        }
                    }
                }
                // 檢查最終是否拿到資料
                if (finalSubject == null)
                {
                    result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                    {
                        Status = QuerySubjectStatus.NoHaveSubject,
                        Subject = finalSubject
                    }));
                    return result;
                }
                result.Content = new StringContent(JsonConvert.SerializeObject(new QuerySubjectResponseDto
                {
                    Status = QuerySubjectStatus.Success,
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
