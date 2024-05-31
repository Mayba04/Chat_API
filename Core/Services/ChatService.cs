using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Core.Interfaces;
using Core.Entities;
using Core.DTO.ChatSession;
using Core.DTO.User;
using Core.Entities.Identity;
using AutoMapper;
using Core.Specifications;
using Core.DTO.Role;
using Core.DTO.Message;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class ChatService: IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _assistantId;
        private readonly IRepository<ChatSession> _chatSessionRepository;
        private readonly IMapper _mapper;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMessageService _messageService;

        public ChatService(IMessageService messageService ,IJwtTokenService jwtTokenService,IConfiguration configuration, IRepository<ChatSession> chatSessionRepository, IMapper mapper)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["ApiKey"];
            _assistantId = configuration["AssistantId"];
            _chatSessionRepository = chatSessionRepository;
            _mapper = mapper;
            _jwtTokenService = jwtTokenService;
            _messageService = messageService;
        }

        public async Task<string> SendMessageToOpenAI(string prompt, string token)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var user = await _jwtTokenService.GetTokenDes(token);

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    var chatId = jsonResponse.id;
                    var message = jsonResponse.choices[0].message.content;
                    var name = prompt.Length > 20 ? prompt.Substring(0, 20) + "." : prompt;

                    var chatSessionDTO = new CreateChatSessionDTO
                    {
                        UserId = user.Id,
                        Created = DateTime.Now,
                        ChatId = chatId,
                        Name = name,
                        SessionVerificationByAdmin = true,
                    };
                    var chatSession = await CreateChatSessionAsync(chatSessionDTO);
                    var IdLastCreateSession = await GetLastChatSessionAsync();
                    var MessageUserDTO = new CreateMessageDTO
                    {
                        ChatSessionId = IdLastCreateSession.Id,
                        SenderId = user.Id,
                        Content = prompt,
                        Timestamp = DateTime.Now,
                        Role = "user",
                        AdminComment = false

                    };
                    await _messageService.CreateMessageAsync(MessageUserDTO);
                    var MessagBotDTO = new CreateMessageDTO
                    {
                        ChatSessionId = IdLastCreateSession.Id,
                        SenderId = user.Id,
                        Content = message,
                        Timestamp = DateTime.Now,
                        Role = "assistant",
                        AdminComment = false
                    };
                    await _messageService.CreateMessageAsync(MessagBotDTO);

                    return $"ChatSesion: {IdLastCreateSession.Id},Chat ID: {chatId}, Message: {message}";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode}, Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception occurred: {ex.Message}";
            }
        }

        public async Task<string> ContinueChatSessionAsync(int chatSessionId, string prompt, string token)
        {
            var messages = await _messageService.GetMessagesBySessionIdAsync(chatSessionId);

            var history = messages.Select(m => new { role = m.Role, content = m.Content }).ToList();
            history.Add(new { role = "user", content = prompt });

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = history.ToArray()
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var user = await _jwtTokenService.GetTokenDes(token);

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    var message = jsonResponse.choices[0].message.content;

                    var chatSession = await GetChatSessionByIdAsync(chatSessionId);
                    var chat = new UpdateChatSessionDTO
                    {
                        Id = chatSession.Id,
                        ChatId = chatSession.ChatId,
                        SessionVerificationByAdmin = true
                    };

                    await UpdateChatSessionAsync(chat);

                    // Запис нового повідомлення від користувача
                    var userMessageDTO = new CreateMessageDTO
                    {
                        ChatSessionId = chatSessionId,
                        SenderId = user.Id,
                        Content = prompt,
                        Timestamp = DateTime.Now,
                        Role = "user",
                        AdminComment = false
                    };
                    await _messageService.CreateMessageAsync(userMessageDTO);

                    // Запис повідомлення від асистента
                    var botMessageDTO = new CreateMessageDTO
                    {
                        ChatSessionId = chatSessionId,
                        SenderId = user.Id, // або ID асистента, якщо є
                        Content = message,
                        Timestamp = DateTime.Now,
                        Role = "assistant",
                        AdminComment = false
                    };
                    await _messageService.CreateMessageAsync(botMessageDTO);

                    return $"Message: {message}";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode}, Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception occurred: {ex.Message}";
            }
        }

        public async Task<ChatSession> CreateChatSessionAsync(CreateChatSessionDTO session)
        {
            var newSession = _mapper.Map<ChatSession>(session);
            // Перетворення локального часу на UTC
            newSession.Created = session.Created.ToUniversalTime();
            await _chatSessionRepository.Insert(newSession);
            await _chatSessionRepository.Save();
            return newSession;
        }


        public async Task<ChatSessionDTO> GetChatSessionByIdAsync(int sessionId)
        {
            var session = await _chatSessionRepository.GetByID(sessionId);
            if (session == null)
            {
                throw new Exception("Session not found");
            }

            var mapped = _mapper.Map<ChatSessionDTO>(session);
            return mapped;
        }

        public async Task<IEnumerable<ChatSessionDTO>> GetAllChatSessionsAsync()
        {
            try
            {
                var ChatSessionDTO = _mapper.Map<List<ChatSessionDTO>>(await _chatSessionRepository.GetAll());
              
                return ChatSessionDTO;
            }
            catch (Exception ex)
            {
                throw new Exception("ChatSessions not found (" + ex.Message+")");
            }
        }

        public async Task UpdateChatSessionAsync(UpdateChatSessionDTO sessionDTO)
        {
            var session = await _chatSessionRepository.GetByID(sessionDTO.Id);
            if (session == null)
            {
                throw new KeyNotFoundException($"ChatSession with ID {sessionDTO.Id} not found.");
            }

            session.ChatId = sessionDTO.ChatId;
            session.SessionVerificationByAdmin = sessionDTO.SessionVerificationByAdmin;

            await _chatSessionRepository.Update(session);
            await _chatSessionRepository.Save();
        }

        public async Task DeleteChatSessionAsync(int sessionId)
        {
            var session = await _chatSessionRepository.GetByID(sessionId);
            if (session == null)
            {
                throw new KeyNotFoundException($"ChatSession with ID {sessionId} not found.");
            }

            await _chatSessionRepository.Delete(session);
            await _chatSessionRepository.Save();
        }

        public async Task<ChatSessionDTO> GetLastChatSessionAsync()
        {
            return _mapper.Map<ChatSessionDTO>(await _chatSessionRepository.GetItemBySpec(new ChatSessionSpecification.GetLastChatSession())); ;
        }

        public async Task<IEnumerable<ChatSessionDTO>> GetChatSessionsByUserIdAsync(int userId)
        {
            return _mapper.Map<List<ChatSessionDTO>>(await _chatSessionRepository.GetListBySpec(new ChatSessionSpecification.GetChatSesionUserId(userId)));
        }

        public async Task<IEnumerable<ChatSessionDTO>> GetPendingVerificationSessionsAsync()
        {
            var sessions = await _chatSessionRepository.GetListBySpec(new ChatSessionSpecification.GetPendingVerificationSessions());
            return _mapper.Map<IEnumerable<ChatSessionDTO>>(sessions);
        }

        public async Task<IEnumerable<ChatSessionDTO>> GetChatSessionsWithAdminCommentsAsync()
        {
            var sessions = await _chatSessionRepository.GetListBySpec(new ChatSessionSpecification.GetChatSessionsWithAdminComments());
            return _mapper.Map<IEnumerable<ChatSessionDTO>>(sessions);
        }
        //v1
        //public async Task<string> SendMessageToAssistant(string prompt)
        //{
        //    var createThreadUrl = "https://api.openai.com/v1/threads";
        //    var addMessageUrlTemplate = "https://api.openai.com/v1/threads/{0}/messages";
        //    var createRunUrlTemplate = "https://api.openai.com/v1/threads/{0}/runs";
        //    var retrieveRunUrlTemplate = "https://api.openai.com/v1/threads/{0}/runs/{1}";
        //    var listMessagesUrlTemplate = "https://api.openai.com/v1/threads/{0}/messages";

        //    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        //    _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        //    _httpClient.DefaultRequestHeaders.Add("OpenAI-Assistant-ID", _assistantId);

        //    try
        //    {
        //        // Створення потоку
        //        var threadRequestBody = new { };
        //        var threadHttpContent = new StringContent(JsonConvert.SerializeObject(threadRequestBody), Encoding.UTF8, "application/json");
        //        var threadResponse = await _httpClient.PostAsync(createThreadUrl, threadHttpContent);

        //        if (!threadResponse.IsSuccessStatusCode)
        //        {
        //            var errorContent = await threadResponse.Content.ReadAsStringAsync();
        //            return $"Error creating thread: {threadResponse.StatusCode}, Details: {errorContent}";
        //        }

        //        var threadResponseContent = await threadResponse.Content.ReadAsStringAsync();
        //        var threadJsonResponse = JsonConvert.DeserializeObject<dynamic>(threadResponseContent);
        //        string threadId = threadJsonResponse.id;

        //        // Додавання повідомлення до потоку
        //        var addMessageUrl = string.Format(addMessageUrlTemplate, threadId);
        //        var messageRequestBody = new
        //        {
        //            role = "user",
        //            content = prompt
        //        };

        //        var messageHttpContent = new StringContent(JsonConvert.SerializeObject(messageRequestBody), Encoding.UTF8, "application/json");
        //        var messageResponse = await _httpClient.PostAsync(addMessageUrl, messageHttpContent);

        //        if (!messageResponse.IsSuccessStatusCode)
        //        {
        //            var errorContent = await messageResponse.Content.ReadAsStringAsync();
        //            return $"Error sending message: {messageResponse.StatusCode}, Details: {errorContent}";
        //        }

        //        // Запуск виконання
        //        var createRunUrl = string.Format(createRunUrlTemplate, threadId);
        //        var runRequestBody = new
        //        {
        //            assistant_id = _assistantId
        //        };

        //        var runHttpContent = new StringContent(JsonConvert.SerializeObject(runRequestBody), Encoding.UTF8, "application/json");
        //        var runResponse = await _httpClient.PostAsync(createRunUrl, runHttpContent);

        //        if (!runResponse.IsSuccessStatusCode)
        //        {
        //            var errorContent = await runResponse.Content.ReadAsStringAsync();
        //            return $"Error creating run: {runResponse.StatusCode}, Details: {errorContent}";
        //        }

        //        var runResponseContent = await runResponse.Content.ReadAsStringAsync();
        //        var runJsonResponse = JsonConvert.DeserializeObject<dynamic>(runResponseContent);
        //        string runId = runJsonResponse.id;

        //        // Очікування завершення виконання
        //        var retrieveRunUrl = string.Format(retrieveRunUrlTemplate, threadId, runId);
        //        dynamic runStatus;
        //        do
        //        {
        //            await Task.Delay(1500); // Зачекайте 1.5 секунди перед повторною перевіркою
        //            var runStatusResponse = await _httpClient.GetAsync(retrieveRunUrl);
        //            var runStatusContent = await runStatusResponse.Content.ReadAsStringAsync();
        //            runStatus = JsonConvert.DeserializeObject<dynamic>(runStatusContent);
        //        } while (runStatus.status != "completed" && runStatus.status != "failed");

        //        if (runStatus.status == "failed")
        //        {
        //            return "Run failed.";
        //        }

        //        // Отримання відповідей асистента
        //        var listMessagesUrl = string.Format(listMessagesUrlTemplate, threadId);
        //        var messagesResponse = await _httpClient.GetAsync(listMessagesUrl);

        //        if (!messagesResponse.IsSuccessStatusCode)
        //        {
        //            var errorContent = await messagesResponse.Content.ReadAsStringAsync();
        //            return $"Error retrieving messages: {messagesResponse.StatusCode}, Details: {errorContent}";
        //        }

        //        var messagesResponseContent = await messagesResponse.Content.ReadAsStringAsync();
        //        var messagesJsonResponse = JsonConvert.DeserializeObject<dynamic>(messagesResponseContent);

        //        // Додавання діагностики
        //        if (messagesJsonResponse.messages == null)
        //        {
        //            return $"Error: 'messages' is null. Full response: {messagesResponseContent}";
        //        }

        //        var messageContent = messagesJsonResponse.messages[0].content.ToString();
        //        return messageContent;
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Exception occurred: {ex.Message}";
        //    }
        //}





        //v2
        public async Task<string> SendMessageToAssistant(string prompt)
        {
            var createThreadUrl = "https://api.openai.com/v1/threads";
            var addMessageUrlTemplate = "https://api.openai.com/v1/threads/{0}/messages";
            var createRunUrlTemplate = "https://api.openai.com/v1/threads/{0}/runs";
            var retrieveRunUrlTemplate = "https://api.openai.com/v1/threads/{0}/runs/{1}";
            var listMessagesUrlTemplate = "https://api.openai.com/v1/threads/{0}/messages";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Assistant-ID", _assistantId);

            try
            {
                // Створення потоку
                var threadRequestBody = new { };
                var threadHttpContent = new StringContent(JsonConvert.SerializeObject(threadRequestBody), Encoding.UTF8, "application/json");
                var threadResponse = await _httpClient.PostAsync(createThreadUrl, threadHttpContent);

                if (!threadResponse.IsSuccessStatusCode)
                {
                    var errorContent = await threadResponse.Content.ReadAsStringAsync();
                    return $"Error creating thread: {threadResponse.StatusCode}, Details: {errorContent}";
                }

                var threadResponseContent = await threadResponse.Content.ReadAsStringAsync();
                var threadJsonResponse = JObject.Parse(threadResponseContent);
                string threadId = threadJsonResponse["id"].ToString();

                // Додавання повідомлення до потоку
                var addMessageUrl = string.Format(addMessageUrlTemplate, threadId);
                var messageRequestBody = new
                {
                    role = "user",
                    content = new[]
                    {
                        new {
                        type = "text",
                        text = prompt
                        }
                    }
                };

                var messageHttpContent = new StringContent(JsonConvert.SerializeObject(messageRequestBody), Encoding.UTF8, "application/json");
                var messageResponse = await _httpClient.PostAsync(addMessageUrl, messageHttpContent);

                if (!messageResponse.IsSuccessStatusCode)
                {
                    var errorContent = await messageResponse.Content.ReadAsStringAsync();
                    return $"Error sending message: {messageResponse.StatusCode}, Details: {errorContent}";
                }

                // Запуск виконання
                var createRunUrl = string.Format(createRunUrlTemplate, threadId);
                var runRequestBody = new
                {
                    assistant_id = _assistantId
                };

                var runHttpContent = new StringContent(JsonConvert.SerializeObject(runRequestBody), Encoding.UTF8, "application/json");
                var runResponse = await _httpClient.PostAsync(createRunUrl, runHttpContent);

                if (!runResponse.IsSuccessStatusCode)
                {
                    var errorContent = await runResponse.Content.ReadAsStringAsync();
                    return $"Error creating run: {runResponse.StatusCode}, Details: {errorContent}";
                }

                var runResponseContent = await runResponse.Content.ReadAsStringAsync();
                var runJsonResponse = JObject.Parse(runResponseContent);
                string runId = runJsonResponse["id"].ToString();

                // Очікування завершення виконання
                var retrieveRunUrl = string.Format(retrieveRunUrlTemplate, threadId, runId);
                JObject runStatus;
                do
                {
                    await Task.Delay(1500); // Зачекайте 1.5 секунди перед повторною перевіркою
                    var runStatusResponse = await _httpClient.GetAsync(retrieveRunUrl);
                    var runStatusContent = await runStatusResponse.Content.ReadAsStringAsync();
                    runStatus = JObject.Parse(runStatusContent);
                } while (runStatus["status"].ToString() != "completed" && runStatus["status"].ToString() != "failed");

                if (runStatus["status"].ToString() == "failed")
                {
                    return "Run failed.";
                }

                // Отримання відповідей асистента
                var listMessagesUrl = string.Format(listMessagesUrlTemplate, threadId);
                var messagesResponse = await _httpClient.GetAsync(listMessagesUrl);

                if (!messagesResponse.IsSuccessStatusCode)
                {
                    var errorContent = await messagesResponse.Content.ReadAsStringAsync();
                    return $"Error retrieving messages: {messagesResponse.StatusCode}, Details: {errorContent}";
                }

                var messagesResponseContent = await messagesResponse.Content.ReadAsStringAsync();
                var messagesJsonResponse = JObject.Parse(messagesResponseContent);

                // Діагностика для перевірки структури відповіді
                if (messagesJsonResponse["data"] == null || !messagesJsonResponse["data"].Any())
                {
                    return $"Error: 'data' is null or empty. Full response: {messagesResponseContent}";
                }

                var assistantMessage = messagesJsonResponse["data"].FirstOrDefault(msg => msg["role"].ToString() == "assistant");
                if (assistantMessage == null)
                {
                    return $"Error: No assistant message found. Full response: {messagesResponseContent}";
                }

                var messageContent = assistantMessage["content"][0]["text"]["value"].ToString();
                return messageContent;
            }
            catch (Exception ex)
            {
                return $"Exception occurred: {ex.Message}";
            }
        }




    }
}
