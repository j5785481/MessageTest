using MessageTest.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class IntegrateMessageRequestDto
    {
        public int SubjectId { get; set; }
        public List<Message> Messages { get; set; }
    }
}
