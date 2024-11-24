using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.API.Services
{
    public class UserService
    {
        private readonly ConnectHubContext _context;
        private readonly FileUploadService _fileUploadService;

        public UserService(ConnectHubContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId, int currentUserId)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Comments)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.LikedBy)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            var recentPosts = user.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    User = MapToUserDto(user),
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    LocationName = p.LocationName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikesCount = p.LikedBy.Count,
                    CommentsCount = p.Comments.Count,
                    IsLikedByCurrentUser = p.LikedBy.Any(u => u.Id == currentUserId),
                    Comments = p.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(3)
                        .Select(c => MapToCommentDto(c, currentUserId))
                        .ToList()
                })
                .ToList();

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                CreatedAt = user.CreatedAt,
                LastActive = user.LastActive,
                IsOnline = user.IsOnline,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                IsFollowing = user.Followers.Any(u => u.Id == currentUserId),
                RecentPosts = recentPosts
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.Bio = updateProfileDto.Bio;
            user.ProfileImageUrl = updateProfileDto.ProfileImageUrl;
            user.LastActive = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetUserProfileAsync(userId, userId);
        }

        public async Task FollowUserAsync(int userId, int currentUserId)
        {
            if (userId == currentUserId)
                throw new InvalidOperationException("You cannot follow yourself");

            var userToFollow = await _context.Users
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToFollow == null)
                throw new InvalidOperationException("User not found");

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (!userToFollow.Followers.Any(u => u.Id == currentUserId))
            {
                userToFollow.Followers.Add(currentUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnfollowUserAsync(int userId, int currentUserId)
        {
            var userToUnfollow = await _context.Users
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToUnfollow == null)
                throw new InvalidOperationException("User not found");

            var follower = userToUnfollow.Followers.FirstOrDefault(u => u.Id == currentUserId);
            if (follower != null)
            {
                userToUnfollow.Followers.Remove(follower);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserDto>> SearchUsersAsync(string query, int currentUserId, int page = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .OrderByDescending(u => u.LastActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return users.Select(u => MapToUserDto(u)).ToList();
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                CreatedAt = user.CreatedAt,
                LastActive = user.LastActive,
                IsOnline = user.IsOnline,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0
            };
        }

        private static CommentDto MapToCommentDto(Comment comment, int currentUserId)
        {
            return new CommentDto
            {
                Id = comment.Id,
                User = MapToUserDto(comment.User),
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                LikesCount = comment.LikedBy?.Count ?? 0,
                IsLikedByCurrentUser = comment.LikedBy?.Any(u => u.Id == currentUserId) ?? false
            };
        }
    }
}
