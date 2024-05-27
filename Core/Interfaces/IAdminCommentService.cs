using Core.DTO.AdminComment;
using Core.DTO.Message;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAdminCommentService
    {
        Task AddAdminCommentAsync(int messageId, CreateAdminCommentDTO commentDTO);
        Task<AdminCommentDTO> GetAdminCommentByIdAsync(int commentId);
        Task<IEnumerable<AdminCommentDTO>> GetAdminCommentsForMessageAsync(int messageId);
        Task<IEnumerable<MessageDTO>> GetPendingBotMessagesAsync();
        Task UpdateAdminCommentAsync(UpdateAdminCommentDTO commentDTO);
        Task DeleteAdminCommentAsync(int commentId);
    }

}
