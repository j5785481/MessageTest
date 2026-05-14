using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.DTO
{
    public class QuerySubjectResponseDto
    {
        public QuerySubjectStatus Status { get; set; }
        public Subject Subject { get; set; }
    }
}
