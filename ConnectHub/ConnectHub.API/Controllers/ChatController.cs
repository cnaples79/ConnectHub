using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;
using ConnectHub.Shared.DTOs;
using System.Security.Claims;

namespace ConnectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("threads")]
        public async Task<ActionResult<List<ChatThreadDto>>> GetChatThreads()
        {
            var userId = GetUserId();
            var threads = await _chatService.GetChatThreadsAsync(userId);
            return Ok(threads);
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetChatHistory(
            int otherUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var messages = await _chatService.GetChatHistoryAsync(userId, otherUserId, page, pageSize);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageDto messageDto)
        {
            try
            {
                var userId = GetUserId();
                var message = await _chatService.SendMessageAsync(messageDto, userId);
                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("mark-read/{otherUserId}")]
        public async Task<IActionResult> MarkMessagesAsRead(int otherUserId)
        {
            var userId = GetUserId();
            await _chatService.MarkMessagesAsReadAsync(userId, otherUserId);
            return Ok();
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}
