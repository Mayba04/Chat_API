using Core.DTO.Message;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDTO messageDto)
        {
            try
            {
                var message = await _messageService.CreateMessageAsync(messageDto);
                return Ok(message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            try
            {
                var message = await _messageService.GetMessageByIdAsync(id);
                if (message == null)
                {
                    return NotFound($"Message with id {id} not found.");
                }
                return Ok(message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetMessagesBySessionId(int sessionId)
        {
            try
            {
                var messages = await _messageService.GetMessagesBySessionIdAsync(sessionId);
                return Ok(messages);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateMessage([FromBody] UpdateMessageDTO messageDto)
        {
            try
            {
                await _messageService.UpdateMessageAsync(messageDto);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                await _messageService.DeleteMessageAsync(id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
