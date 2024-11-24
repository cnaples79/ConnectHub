using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ConnectHub.Shared.DTOs
{
    public class CreatePostDto
    {
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
        public IFormFile Image { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationName { get; set; }
    }

    public class UpdatePostDto
    {
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
        public string LocationName { get; set; }
    }

    public class PostDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

    public class CreateCommentDto
    {
        public int PostId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}
