using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class AddMessageRequestDto
    {
        public string UserId { get; set; }
        public int SubjectId { get; set; }
        public string Content { get; set; }
        public long ClientTimeStamp { get; set; }
        public long CreateTimeStamp { get; set; }
    }
}
