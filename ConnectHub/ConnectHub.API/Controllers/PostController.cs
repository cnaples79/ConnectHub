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
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PostDto>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var posts = await _postService.GetFeedAsync(userId, page, pageSize);
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            try
            {
                var userId = GetUserId();
                var post = await _postService.GetPostDtoAsync(id, userId);
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            try
            {
                var userId = GetUserId();
                var post = await _postService.CreatePostAsync(createPostDto, userId);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> UpdatePost(int id, UpdatePostDto updatePostDto)
        {
            try
            {
                var userId = GetUserId();
                var post = await _postService.UpdatePostAsync(id, updatePostDto, userId);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var userId = GetUserId();
                await _postService.DeletePostAsync(id, userId);
                return NoContent();
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

        [HttpPost("{postId}/comments")]
        public async Task<ActionResult<CommentDto>> AddComment(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            try
            {
                var userId = GetUserId();
                createCommentDto.PostId = postId;
                var comment = await _postService.AddCommentAsync(createCommentDto, userId);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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

    public class CreatePostDto
    {
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class UpdatePostDto
    {
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; }
    }
}
