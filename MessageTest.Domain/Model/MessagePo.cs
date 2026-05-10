using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    public class MessagePo
    {
        public string f_sujectId { get; set; }

        public string f_id { get; set; }

        public string f_content { get; set; }

        public string f_userId { get; set; }

        public DateTime f_createdAt { get; set; }
    }
}
