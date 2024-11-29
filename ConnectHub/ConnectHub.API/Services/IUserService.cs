using ConnectHub.Shared.DTOs;

namespace ConnectHub.API.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateDto);
        Task<bool> ValidateUserAsync(string userId);
        Task<List<UserDto>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task<bool> FollowUserAsync(string userId, string targetUserId);
        Task<bool> UnfollowUserAsync(string userId, string targetUserId);
        Task<List<UserDto>> GetFollowersAsync(string userId, int page, int pageSize);
        Task<List<UserDto>> GetFollowingAsync(string userId, int page, int pageSize);
        Task<UserDto> GetUserProfileAsync(string userId);
        Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto);
    }
}
