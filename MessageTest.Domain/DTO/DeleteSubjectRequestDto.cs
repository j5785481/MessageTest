using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Domain.DTO
{
    public class DeleteSubjectRequestDto
    {
        public string UserId { get; set; }
        public int Id { get; set; }
    }
}
