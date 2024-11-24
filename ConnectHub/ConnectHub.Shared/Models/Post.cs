using System;
using System.Collections.Generic;

namespace ConnectHub.Shared.Models
{
    public class Post
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int LikeCount { get; set; }
    }
}
