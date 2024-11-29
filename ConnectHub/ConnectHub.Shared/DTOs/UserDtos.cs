using System.ComponentModel.DataAnnotations;

namespace ConnectHub.Shared.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
    }

    public class UserProfileDto : UserDto 
    {
        public bool IsFollowing { get; set; }
        public List<PostDto> RecentPosts { get; set; } = new();
    }

    public class UpdateProfileDto
    {
        [StringLength(200)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}
