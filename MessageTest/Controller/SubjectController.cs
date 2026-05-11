using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Repository;
using MessageTest.Hubs;
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
        public HttpResponseMessage PostSubject([FromBody] AddSubjectRequestDto subject)
        {
            try
            {
                var addResult = this.subjectRepository.Add(subject);

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
    }
}
