using AutoMapper;
using Core.DTO.Message;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IMapper _mapper;

        public MessageService(IRepository<Message> messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<Message> CreateMessageAsync(CreateMessageDTO messageDTO)
        {
            var message = _mapper.Map<Message>(messageDTO);
            message.Timestamp = messageDTO.Timestamp.ToUniversalTime();
            await _messageRepository.Insert(message);
            await _messageRepository.Save();
            return message;
        }

        public async Task<MessageDTO> GetMessageByIdAsync(int messageId)
        {
            var message = await _messageRepository.GetByID(messageId);
            return _mapper.Map<MessageDTO>(message);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessagesBySessionIdAsync(int sessionId)
        {
            var messages = await _messageRepository.GetListBySpec(new MessageSpecification.GetMessagesBySessionId(sessionId));
            return _mapper.Map<IEnumerable<MessageDTO>>(messages);
        }

        public async Task UpdateMessageAsync(UpdateMessageDTO messageDTO)
        {
            var message = await _messageRepository.GetByID(messageDTO.Id);
            if (message == null)
            {
                throw new KeyNotFoundException($"Message with ID {messageDTO.Id} not found.");
            }
            message.Content = messageDTO.Content;
            await _messageRepository.Update(message);
            await _messageRepository.Save();
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            var message = await _messageRepository.GetByID(messageId);
            if (message == null)
            {
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");
            }
            await _messageRepository.Delete(message);
            await _messageRepository.Save();
        }


    }
}
