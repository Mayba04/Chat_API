using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.AdminComment
{
    public class CreateAdminCommentDTO
    {
        public int MessageId { get; set; }
        public string Comment { get; set; }
        public int AdminId { get; set; }
    }
}
