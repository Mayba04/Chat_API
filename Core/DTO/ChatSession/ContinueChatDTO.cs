using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.ChatSession
{
    public class ContinueChatDTO
    {
        public int ChatSessionId { get; set; }
        public string Prompt { get; set; }
    }
}
