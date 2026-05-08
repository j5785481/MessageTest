using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.Model
{
    class Message
    {
        public int SujectId { get; set; }

        public int Id { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
