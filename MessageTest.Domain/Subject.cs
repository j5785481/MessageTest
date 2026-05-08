using System;
using System.Collections.Generic;
using ProtoBuf;

namespace MessageTest.Domain
{
    [ProtoContract]
    public class Subject
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Content { get; set; } = "";

        public int CreatorId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
