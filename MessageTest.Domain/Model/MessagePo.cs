using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    public class MessagePo
    {
        public int f_subjectId { get; set; }

        public string f_id { get; set; }

        public string f_content { get; set; }

        public string f_userId { get; set; }

        public int f_floor {  get; set; }

        public DateTime f_createdAt { get; set; }
    }
}
