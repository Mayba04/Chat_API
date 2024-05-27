using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.ChatSession
{
    public class ChatSessionDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool SessionVerificationByAdmin { get; set; }
    }
}
