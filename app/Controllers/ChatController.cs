using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using app.Dtos.Chat;
using app.Interface;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;

        public ChatController(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new UnauthorizedAccessException("User is not authenticated.");
            return Guid.Parse(userId);
        }

        // POST /api/chat/direct
        [HttpPost("direct")]
        public async Task<IActionResult> SendDirectMessage(SendDirectMessageDto dto)
        {
            try
            {
                var result = await _chatRepo.SendDirectMessageAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/chat/direct/{userId}?page=1
        [HttpGet("direct/{userId}")]
        public async Task<IActionResult> GetDirectMessages(Guid userId, int page = 1)
        {
            try
            {
                var result = await _chatRepo.GetDirectMessagesAsync(GetCurrentUserId(), userId, page);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/chat/group/{groupId}
        [HttpPost("group/{groupId}")]
        public async Task<IActionResult> SendGroupMessage(Guid groupId, SendGroupMessageDto dto)
        {
            try
            {
                var result = await _chatRepo.SendGroupMessageAsync(dto, groupId, GetCurrentUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/chat/group/{groupId}?page=1
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupMessages(Guid groupId, int page = 1)
        {
            try
            {
                var result = await _chatRepo.GetGroupMessagesAsync(groupId, page);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/chat/conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var result = await _chatRepo.GetConversationsAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}