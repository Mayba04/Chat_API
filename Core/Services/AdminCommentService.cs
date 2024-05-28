using AutoMapper;
using Core.DTO.AdminComment;
using Core.DTO.ChatSession;
using Core.DTO.Message;
using Core.Entities;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class AdminCommentService : IAdminCommentService
    {
        private readonly IRepository<AdminComment> _adminCommentRepository;
        private readonly IRepository<Message> _messageRepository;
        private readonly IChatService _chatSession;
        private readonly IMapper _mapper;

        public AdminCommentService(IRepository<AdminComment> adminCommentRepository, IRepository<Message> messageRepository, IMapper mapper, IChatService chatSessionRepository)
        {
            _adminCommentRepository = adminCommentRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
            _chatSession = chatSessionRepository;
        }

        public async Task AddAdminCommentAsync(int messageId, CreateAdminCommentDTO commentDTO)
        {
           
            var message = await _messageRepository.GetByID(messageId);
            if (message == null || message.Role != "assistant")
            {
                throw new KeyNotFoundException($"Message with ID {messageId} not found or it is not a bot message.");
            }
            var chatSession = await _chatSession.GetChatSessionByIdAsync(message.ChatSessionId);
            var chat = new UpdateChatSessionDTO
            {
                Id = chatSession.Id,
                ChatId = chatSession.ChatId,
                SessionVerificationByAdmin = false
            };

            var adminComment = new AdminComment
            {
                MessageId = messageId,
                Comment = commentDTO.Comment,
                CreatedAt = DateTime.UtcNow,
                AdminId = commentDTO.AdminId
            };

            message.AdminComment = true;
            await _chatSession.UpdateChatSessionAsync(chat);
            await _messageRepository.Update(message);
            await _adminCommentRepository.Insert(adminComment);
            await _adminCommentRepository.Save();
            await _messageRepository.Save();
        }

        public async Task DeleteAdminCommentAsync(int commentId)
        {
            var comment = await _adminCommentRepository.GetByID(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException($"Admin comment with ID {commentId} not found.");
            }

            var Message = await _messageRepository.GetByID(comment.MessageId);

            if (Message == null)
            {
                throw new KeyNotFoundException($"Message with ID {comment.MessageId} not found.");
            }

            Message.AdminComment = false;
            await _messageRepository.Update(Message);
            await _messageRepository.Save();
            await _adminCommentRepository.Delete(comment);
            await _adminCommentRepository.Save();
        }

        public async Task<AdminCommentDTO> GetAdminCommentByIdAsync(int commentId)
        {
            var comment = await _adminCommentRepository.GetByID(commentId);
            if (comment == null)
            {
                throw new KeyNotFoundException($"Admin comment with ID {commentId} not found.");
            }

            return _mapper.Map<AdminCommentDTO>(comment);
        }

        public async Task<IEnumerable<AdminCommentDTO>> GetAdminCommentsForMessageAsync(int messageId)
        {
            var comments = await _adminCommentRepository.GetListBySpec(new AdminCommentSpecification.GetCommentsByMessageId(messageId));
            return _mapper.Map<IEnumerable<AdminCommentDTO>>(comments);
        }

        public async Task<IEnumerable<MessageDTO>> GetPendingBotMessagesAsync()
        {
            var messages = await _messageRepository.GetListBySpec(new MessageSpecification.GetPendingBotMessages());
            return _mapper.Map<IEnumerable<MessageDTO>>(messages);
        }

        public async Task UpdateAdminCommentAsync(UpdateAdminCommentDTO commentDTO)
        {
            var comment = await _adminCommentRepository.GetByID(commentDTO.Id);
            if (comment == null)
            {
                throw new KeyNotFoundException($"Admin comment with ID {commentDTO.Id} not found.");
            }

            comment.Comment = commentDTO.Comment;
            await _adminCommentRepository.Update(comment);
            await _adminCommentRepository.Save();
        }
    }

}
