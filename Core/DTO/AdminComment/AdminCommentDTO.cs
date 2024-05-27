using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.AdminComment
{
    public class AdminCommentDTO
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AdminId { get; set; }
    }
}
