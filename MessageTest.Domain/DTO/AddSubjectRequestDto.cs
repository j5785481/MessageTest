using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class AddSubjectRequestDto
    {
        public string UserId { get; set; }
        public string SubjectTitle { get; set; }
        public string SubjectContent { get; set; }
        public long ClientTimeStamp { get; set; }
        public long CreateTimeStamp { get; set; }
    }
}
