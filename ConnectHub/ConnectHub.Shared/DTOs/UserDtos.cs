using System.ComponentModel.DataAnnotations;

namespace ConnectHub.Shared.DTOs
{
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
