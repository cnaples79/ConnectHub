using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConnectHub.Shared.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> LikedBy { get; set; } = new List<PostLike>();
    }
}
