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
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(int userId)
        {
            try
            {
                var currentUserId = GetUserId();
                var profile = await _userService.GetUserProfileAsync(userId, currentUserId);
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("follow/{userId}")]
        public async Task<ActionResult> FollowUser(int userId)
        {
            try
            {
                var currentUserId = GetUserId();
                await _userService.FollowUserAsync(userId, currentUserId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("follow/{userId}")]
        public async Task<ActionResult> UnfollowUser(int userId)
        {
            try
            {
                var currentUserId = GetUserId();
                await _userService.UnfollowUserAsync(userId, currentUserId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetUserId();
                var users = await _userService.SearchUsersAsync(query, currentUserId, page, pageSize);
                return Ok(users);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                var userId = GetUserId();
                var profile = await _userService.UpdateProfileAsync(userId, updateProfileDto);
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID claim");
            }
            return userId;
        }
    }
}
