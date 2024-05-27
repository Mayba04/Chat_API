using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public virtual ChatSession ChatSession { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string Role { get; set; }
        public bool AdminComment { get; set; }
        public virtual AdminComment AdminCommentDetail { get; set; }
    }
}
