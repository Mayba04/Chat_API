using Core.DTO.ChatSession;
using Core.DTO.User;
using Core.Entities;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Services.ChatService;

namespace Core.Interfaces
{
    public interface IChatService
    {
        Task<string> SendMessageToOpenAI(string prompt, string token);
        Task<string> ContinueChatSessionAsync(int chatSessionId, string prompt, string token);
        Task<string> SendMessageToAssistant(string prompt, string token);
        Task<string> ContinueMessageToAssistant(string prompt, int chatSessionId, string token);
        Task<string> ContinueDialogWithThread(string prompt, string threadId);
        Task<bool> CheckIfThreadExists(string threadId);
        //Task<List<object>> GetThreadMessages(string threadId);
        Task<List<Messagethread>> GetThreadMessages(string threadId);

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
