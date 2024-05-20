using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Message
{
    public class UpdateMessageDTO
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string Role { get; set; }
        public bool AdminComment { get; set; }
    }
}
