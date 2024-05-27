using Core.DTO.AdminComment;
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
    public class AdminCommentController : ControllerBase
    {
        private readonly IAdminCommentService _adminCommentService;

        public AdminCommentController(IAdminCommentService adminCommentService)
        {
            _adminCommentService = adminCommentService;
        }

        [HttpPost]
        public async Task<ActionResult> AddAdminComment([FromBody] CreateAdminCommentDTO commentDTO)
        {
            try
            {
                await _adminCommentService.AddAdminCommentAsync(commentDTO.MessageId, commentDTO);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{commentId}")]
        public async Task<ActionResult<AdminCommentDTO>> GetAdminCommentById(int commentId)
        {
            try
            {
                var comment = await _adminCommentService.GetAdminCommentsForMessageAsync(commentId);
                return Ok(comment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateAdminComment([FromBody] UpdateAdminCommentDTO commentDTO)
        {
            try
            {
                await _adminCommentService.UpdateAdminCommentAsync(commentDTO);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<ActionResult> DeleteAdminComment(int commentId)
        {
            try
            {
                await _adminCommentService.DeleteAdminCommentAsync(commentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("pending-messages")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetPendingBotMessages()
        {
            var messages = await _adminCommentService.GetPendingBotMessagesAsync();
            return Ok(messages);
        }
    }
}
