using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;
using ConnectHub.Shared.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ConnectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostController> _logger;
        private readonly IUserService _userService;

        public PostController(IPostService postService, ILogger<PostController> logger, IUserService userService)
        {
            _postService = postService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PostDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PostDto>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 50)
                {
                    return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1 and pageSize must be between 1 and 50." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var posts = await _postService.GetFeedAsync(userId, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while retrieving posts" });
            }
        }

        [HttpGet("{postId}")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPost(int postId)
        {
            try
            {
                var post = await _postService.GetPostDtoAsync(postId.ToString());
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<PostDto>> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid post data", errors = ModelState });
                }

                var post = await _postService.CreatePostAsync(createPostDto, userId);
                _logger.LogInformation("Post created successfully for user {UserId}", userId);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid post data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while creating the post" });
            }
        }

        [HttpPut("{postId}")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> UpdatePost(int postId, UpdatePostDto updatePostDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var post = await _postService.UpdatePostAsync(postId.ToString(), userId, updatePostDto);
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{postId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePost(int postId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var post = await _postService.GetPostDtoAsync(postId.ToString());
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }

                if (post.User.Id != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to delete post {PostId} owned by another user", userId, postId);
                    return Forbid();
                }

                await _postService.DeletePostAsync(postId.ToString(), userId);
                _logger.LogInformation("Post {PostId} deleted successfully by user {UserId}", postId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId} for user {UserId}", postId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while deleting the post" });
            }
        }

        [HttpPost("{postId}/comments")]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> AddComment(int postId, CreateCommentDto commentDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var post = await _postService.AddCommentAsync(postId.ToString(), userId, commentDto);
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{postId}/like")]
        public async Task<ActionResult> LikePost(int postId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var post = await _postService.GetPostDtoAsync(postId.ToString());
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }

                await _postService.LikePostAsync(postId.ToString(), userId);
                _logger.LogInformation("Post {PostId} liked by user {UserId}", postId, userId);
                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post {PostId} for user {UserId}", postId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while liking the post" });
            }
        }

        [HttpDelete("{postId}/like")]
        public async Task<ActionResult> UnlikePost(int postId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var post = await _postService.GetPostDtoAsync(postId.ToString());
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }

                await _postService.UnlikePostAsync(postId.ToString(), userId);
                _logger.LogInformation("Post {PostId} unliked by user {UserId}", postId, userId);
                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post {PostId} for user {UserId}", postId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while unliking the post" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<PostDto>>> SearchPosts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 50)
                {
                    return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1 and pageSize must be between 1 and 50." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var posts = await _postService.SearchPostsAsync(query, userId, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while searching posts" });
            }
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<List<PostDto>>> GetNearbyPosts(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radius,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var posts = await _postService.GetNearbyPostsAsync(latitude, longitude, radius, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }
    }
}
