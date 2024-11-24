using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;
using ConnectHub.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
        public async Task<ActionResult<PostDto>> UpdatePost(int id, [FromForm] UpdatePostDto updatePostDto)
        {
            try
            {
                var userId = GetUserId();
                var post = await _postService.UpdatePostAsync(id, updatePostDto, userId);
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePost(int id)
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
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

        [HttpPost("{postId}/like")]
        public async Task<ActionResult> LikePost(int postId)
        {
            try
            {
                var userId = GetUserId();
                await _postService.LikePostAsync(postId, userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{postId}/like")]
        public async Task<ActionResult> UnlikePost(int postId)
        {
            try
            {
                var userId = GetUserId();
                await _postService.UnlikePostAsync(postId, userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<PostDto>>> SearchPosts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var posts = await _postService.SearchPostsAsync(query, userId, page, pageSize);
                return Ok(posts);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<List<PostDto>>> GetNearbyPosts([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var posts = await _postService.GetNearbyPostsAsync(latitude, longitude, userId, page, pageSize);
                return Ok(posts);
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
