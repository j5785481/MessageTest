using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageTest.Domain.Model;

namespace MessageTest.Domain.DTO
{
    public class DeleteMessageResponseDto
    {
        public DeleteMessageStatus Status { get; set; }
        public Message Message { get; set; }
    }
}
