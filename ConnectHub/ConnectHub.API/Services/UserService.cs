using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.API.Services
{
    public class UserService : IUserService
    {
        private readonly ConnectHubContext _context;
        private readonly FileUploadService _fileUploadService;

        public UserService(ConnectHubContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                throw new InvalidOperationException("User not found");
            return await MapToUserDto(user);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new InvalidOperationException("User not found");
            return await MapToUserDto(user);
        }

        public async Task<List<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(await MapToUserDto(user));
            }

            return userDtos;
        }

        public async Task<bool> FollowUserAsync(string userId, string targetUserId)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            var targetUser = await _context.Users.FindAsync(int.Parse(targetUserId));

            if (user == null || targetUser == null)
                throw new InvalidOperationException("User not found");

            var follow = new UserFollow
            {
                FollowerId = int.Parse(userId),
                FollowingId = int.Parse(targetUserId)
            };

            _context.UserFollows.Add(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowUserAsync(string userId, string targetUserId)
        {
            var follow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == int.Parse(userId) && f.FollowingId == int.Parse(targetUserId));

            if (follow == null)
                return false;

            _context.UserFollows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserDto>> GetFollowersAsync(string userId, int page = 1, int pageSize = 10)
        {
            var followers = await _context.UserFollows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == int.Parse(userId))
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var follower in followers)
            {
                userDtos.Add(await MapToUserDto(follower));
            }

            return userDtos;
        }

        public async Task<List<UserDto>> GetFollowingAsync(string userId, int page = 1, int pageSize = 10)
        {
            var following = await _context.UserFollows
                .Include(f => f.Following)
                .Where(f => f.FollowerId == int.Parse(userId))
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Following)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var followedUser in following)
            {
                userDtos.Add(await MapToUserDto(followedUser));
            }

            return userDtos;
        }

        public async Task<UserDto> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
                throw new InvalidOperationException("User not found");

            return await MapToUserDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.Bio = updateProfileDto.Bio;
            if (!string.IsNullOrEmpty(updateProfileDto.ProfileImageUrl))
            {
                user.ProfileImageUrl = updateProfileDto.ProfileImageUrl;
            }

            await _context.SaveChangesAsync();
            return await MapToUserDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Update basic info
            user.Username = updateDto.Username ?? user.Username;
            user.Bio = updateDto.Bio ?? user.Bio;

            // Handle profile image update
            if (updateDto.ProfileImageData != null)
            {
                var contentType = updateDto.ProfileImageContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    throw new ArgumentException("File content type is required");
                }

                var fileName = updateDto.ProfileImageFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name is required");
                }

                // Validate and upload the file
                using var stream = new MemoryStream(updateDto.ProfileImageData);
                var isValid = await _fileUploadService.ValidateFileAsync(stream, contentType);
                if (!isValid)
                {
                    throw new ArgumentException("Invalid file type or size");
                }

                stream.Position = 0;
                var imageUrl = await _fileUploadService.UploadFileAsync(stream, fileName, contentType);

                user.ProfileImageUrl = imageUrl;
            }

            user.LastActive = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await MapToUserDto(user);
        }

        public async Task<bool> ValidateUserAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == int.Parse(userId));
        }

        private async Task<UserDto> MapToUserDto(User user)
        {
            var followersCount = await _context.UserFollows.CountAsync(f => f.FollowingId == user.Id);
            var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == user.Id);

            return new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                CreatedAt = user.CreatedAt,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };
        }
    }
}
