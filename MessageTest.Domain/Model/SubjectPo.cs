using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    public class SubjectPo
    {
        public string f_id { get; set; }

        public string f_title { get; set; }

        public string f_content { get; set; }

        public int f_creatorId { get; set; }

        public DateTime f_createdAt { get; set; }

        public int f_messageCount { get; set; }
    }
}
