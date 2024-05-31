using Core.DTO.ChatSession;
using Core.DTO.User;
using Core.Entities;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IChatService
    {
        Task<string> SendMessageToOpenAI(string prompt, string token);
        Task<string> ContinueChatSessionAsync(int chatSessionId, string prompt, string token);
        Task<string> SendMessageToAssistant(string prompt);

        Task<ChatSession> CreateChatSessionAsync(CreateChatSessionDTO session);
        Task<ChatSessionDTO> GetChatSessionByIdAsync(int sessionId);
        Task<IEnumerable<ChatSessionDTO>> GetAllChatSessionsAsync();
        Task UpdateChatSessionAsync(UpdateChatSessionDTO session);
        Task DeleteChatSessionAsync(int sessionId);
        Task<ChatSessionDTO> GetLastChatSessionAsync();
        Task<IEnumerable<ChatSessionDTO>> GetChatSessionsByUserIdAsync(int userId);
        Task<IEnumerable<ChatSessionDTO>> GetPendingVerificationSessionsAsync();
        Task<IEnumerable<ChatSessionDTO>> GetChatSessionsWithAdminCommentsAsync();

    }
}
