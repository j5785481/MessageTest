using System;
using System.Collections.Generic;

namespace MessageTest.Domain.Model
{
    public class Subject
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string CreatorId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int MessageCount { get; set; }
    }
}
