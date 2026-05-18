using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using app.Dtos.Group;
using app.Interface;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
    [Route("api/Group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _groupRepo;

        public GroupController(IGroupRepository groupRepo)
        {
            _groupRepo = groupRepo;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new UnauthorizedAccessException("User is not authenticated.");
            return Guid.Parse(userId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto dto)
        {
            try
            {
                var result = await _groupRepo.CreateGroupAsync(dto, GetCurrentUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });  // 👈 401
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyGroups()
        {
            try
            {
                var result = await _groupRepo.GetMyGroupsAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });  // 👈 401
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetGroupMembers(Guid id)
        {
            try
            {
                var result = await _groupRepo.GetGroupMembersAsync(id);
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

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(Guid id, AddMemberDto dto)
        {
            try
            {
                await _groupRepo.AddMemberAsync(id, dto, GetCurrentUserId());
                return Ok(new { message = "Member added successfully." });
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

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            try
            {
                await _groupRepo.RemoveMemberAsync(id, userId, GetCurrentUserId());
                return Ok(new { message = "Member removed successfully." });
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