using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForumMessageSystem.Persistent.Core;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;
using MongoDB.Driver;

namespace MessageTest.Persistent.Mongo
{
    public class SubjectRepository : BaseMongoRepository, ISubjectRepository
    {
        public SubjectRepository(MongoClient mongoClient, string dataBaseName) : base(mongoClient, dataBaseName)
        {
        }

        public (Exception exception, Subject subject) Add(AddSubjectRequestDto req)
        {
            throw new NotImplementedException();
        }
    }
}
