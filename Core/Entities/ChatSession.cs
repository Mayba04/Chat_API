using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ChatSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserEntity? User { get; set; }
        public DateTime Created { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Message>? Messages { get; set; }
    }
}
