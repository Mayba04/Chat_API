﻿using Core.DTO.ChatSession;
using Core.DTO.User;
using Core.Interfaces;
using Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace Chat_API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly HttpClient _httpClient;

        public ChatController(IChatService chatService)
        {
            _httpClient = new HttpClient();
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAssistant([FromBody] string prompt)
        {
            var token = await HttpContext.GetTokenAsync("access_token");


            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("Prompt is required");
            }

            try
            {
                var response = await _chatService.SendMessageToAssistant(prompt, token);
                return Ok(new { Response = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }

           
        }

        [HttpPost("continue/ask")]
        public async Task<IActionResult> ContinueChatAskSession([FromBody] ContinueChatDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Prompt))
            {
                return BadRequest("Prompt is required");
            }

            var token = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing");
            }

            try
            {
                var response = await _chatService.ContinueMessageToAssistant( dto.Prompt, dto.ChatSessionId, token);
                return Ok(new { Response = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string prompt)
        {
            // Читаємо токен з аутентифікаційних заголовків
            var token = await HttpContext.GetTokenAsync("access_token");
           

            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("Prompt is required");
            }

            try
            {
                var response = await _chatService.SendMessageToOpenAI(prompt, token);
                return Ok(new { Response = response });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("continue")]
        public async Task<IActionResult> ContinueChatSession([FromBody] ContinueChatDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Prompt))
            {
                return BadRequest("Prompt is required");
            }

            var token = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing");
            }

            try
            {
                var response = await _chatService.ContinueChatSessionAsync(dto.ChatSessionId, dto.Prompt, token);
                return Ok(new { Response = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateChatSession([FromBody] CreateChatSessionDTO sessionDto)
        {
            try
            {
                var session = await _chatService.CreateChatSessionAsync(sessionDto);
                return Ok(session);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatSession(int id)
        {
            try
            {
                var session = await _chatService.GetChatSessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound($"Chat session with id {id} not found.");
                }
                return Ok(session);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatSession(int id)
        {
            try
            {
                await _chatService.DeleteChatSessionAsync(id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateChatSession([FromBody] UpdateChatSessionDTO sessionDto)
        {
            try
            {
                await _chatService.UpdateChatSessionAsync(sessionDto);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllChatSessions()
        {
            try
            {
                var sessions = await _chatService.GetAllChatSessionsAsync();
                return Ok(sessions);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("user/{userId}/chats")]
        public async Task<IActionResult> GetUserChats(int userId)
        {
            try
            {
                var sessions = await _chatService.GetChatSessionsByUserIdAsync(userId);
                if (sessions == null || !sessions.Any())
                {
                    return NotFound($"No chat sessions found for user with id {userId}.");
                }
                return Ok(sessions);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("pending-verification-sessions")]
        public async Task<IActionResult> GetPendingVerificationSessions()
        {
            var sessions = await _chatService.GetPendingVerificationSessionsAsync();
            return Ok(sessions);
        }

        [HttpGet("chat-sessions-with-admin-comments")]
        public async Task<IActionResult> GetChatSessionsWithAdminComments()
        {
            try
            {
                var sessions = await _chatService.GetChatSessionsWithAdminCommentsAsync();
                if (sessions == null)
                {
                    return NotFound("No chat sessions with admin comments found.");
                }
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error" + ex.Message);
            }
        }

        [HttpGet("threadIdCheck")]
        public async Task<IActionResult> CheckThreadId(string threadId)
        {
            if (string.IsNullOrWhiteSpace(threadId))
            {
                return BadRequest("Thread ID is required.");
            }

            bool exists = await _chatService.CheckIfThreadExists(threadId);

            if (exists)
            {
                return Ok(new { ThreadExists = true });
            }
            else
            {
                return NotFound(new { ThreadExists = false });
            }
        }

        [HttpGet("getThreadContext")]
        public async Task<IActionResult> GetThreadContext(string threadId)
        {
            var threadExists = await _chatService.CheckIfThreadExists(threadId);
            if (!threadExists)
            {
                return NotFound(new { Message = "Thread does not exist." });
            }

            var threadMessages = await _chatService.GetThreadMessages(threadId);
            if (threadMessages == null)
            {
                return NotFound(new { Message = "Failed to retrieve thread messages." });
            }

            return Ok(threadMessages);
        }

        [HttpPost("continueThread")]
        public async Task<IActionResult> ContinueThread(string prompt, string threadId)
        {
            if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(threadId))
            {
                return BadRequest("Prompt and ThreadId are required.");
            }

            var response = await _chatService.ContinueDialogWithThread(prompt, threadId);
            return Ok(response);
        }


    }
}
