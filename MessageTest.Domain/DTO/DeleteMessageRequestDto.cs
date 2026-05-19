using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class DeleteMessageRequestDto
    {
        public string UserId { get; set; }
        public Guid MessageId { get; set; }
        public int SubjectId { get; set; }
    }
}
