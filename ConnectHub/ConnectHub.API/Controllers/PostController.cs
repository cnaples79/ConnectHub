using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;
using ConnectHub.Shared.Models;

namespace ConnectHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly FileUploadService _fileUploadService;

        public PostController(PostService postService, FileUploadService fileUploadService)
        {
            _postService = postService;
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            string? imageUrl = null;
            if (dto.Image != null)
            {
                var isValid = await _fileUploadService.ValidateFileAsync(dto.Image.OpenReadStream(), dto.Image.ContentType);
                if (!isValid)
                    return BadRequest("Invalid file type or size");

                imageUrl = await _fileUploadService.UploadFileAsync(
                    dto.Image.OpenReadStream(),
                    dto.Image.FileName,
                    dto.Image.ContentType
                );
            }

            var post = await _postService.CreatePostAsync(
                userId,
                dto.Content,
                imageUrl,
                dto.Latitude,
                dto.Longitude
            );

            return Ok(post);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var posts = await _postService.GetFeedAsync(userId, skip, take);
            return Ok(posts);
        }

        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> AddComment(string postId, [FromBody] AddCommentDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _postService.AddCommentAsync(postId, userId, dto.Content);
            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(string postId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _postService.LikePostAsync(postId, userId);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyPosts([FromQuery] double latitude, [FromQuery] double longitude)
        {
            var posts = await _postService.GetNearbyPostsAsync(latitude, longitude);
            return Ok(posts);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] string query)
        {
            var posts = await _postService.SearchPostsAsync(query);
            return Ok(posts);
        }
    }

    public class CreatePostDto
    {
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class AddCommentDto
    {
        public string Content { get; set; }
    }
}
