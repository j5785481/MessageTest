using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    public class Message
    {
        public int SubjectId { get; set; }

        public string Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
