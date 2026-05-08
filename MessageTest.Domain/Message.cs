using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain
{
    class Message
    {
        public SubjectID SujectId { get; set; }

        public MessageID MessageId { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }
    }
}
