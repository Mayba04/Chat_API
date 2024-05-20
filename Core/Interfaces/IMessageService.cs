using Core.DTO.ChatSession;
using Core.DTO.Message;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IMessageService
    {
        Task<Message> CreateMessageAsync(CreateMessageDTO message);
        Task<MessageDTO> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<MessageDTO>> GetMessagesBySessionIdAsync(int sessionId);
        Task UpdateMessageAsync(UpdateMessageDTO message);
        Task DeleteMessageAsync(int messageId);
    }
}
