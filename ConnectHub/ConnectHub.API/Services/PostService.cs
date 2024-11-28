using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ConnectHub.API.Services
{
    public class PostService
    {
        private readonly ConnectHubContext _context;
        private readonly FileUploadService _fileUploadService;

        public PostService(ConnectHubContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId)
        {
            string imageUrl = null;
            if (createPostDto.Image != null)
            {
                using var stream = createPostDto.Image.OpenReadStream();
                if (await _fileUploadService.ValidateFileAsync(stream, createPostDto.Image.ContentType))
                {
                    stream.Position = 0;
                    imageUrl = await _fileUploadService.UploadFileAsync(stream, createPostDto.Image.FileName, createPostDto.Image.ContentType);
                }
                else
                {
                    throw new InvalidOperationException("Invalid file type or size");
                }
            }

            var post = new Post
            {
                UserId = userId,
                Content = createPostDto.Content,
                ImageUrl = imageUrl,
                Latitude = createPostDto.Latitude,
                Longitude = createPostDto.Longitude,
                LocationName = createPostDto.LocationName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return await GetPostDtoAsync(post.Id, userId);
        }

        public async Task<PostDto> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own posts");

            post.Content = updatePostDto.Content;
            post.LocationName = updatePostDto.LocationName;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetPostDtoAsync(postId, userId);
        }

        public async Task DeletePostAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own posts");

            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                await _fileUploadService.DeleteFileAsync(post.ImageUrl);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<PostDto> GetPostDtoAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            return new PostDto
            {
                Id = post.Id,
                User = MapToUserDto(post.User),
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                LocationName = post.LocationName,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                LikesCount = post.LikedBy?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0,
                IsLikedByCurrentUser = post.LikedBy?.Any(u => u.Id == userId) ?? false,
                Comments = post.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => MapToCommentDto(c, userId))
                    .ToList() ?? new List<CommentDto>()
            };
        }

        public async Task<List<PostDto>> GetFeedAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.LikedBy)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return posts.Select(p => new PostDto
                {
                    Id = p.Id,
                    User = MapToUserDto(p.User),
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    LocationName = p.LocationName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikesCount = p.LikedBy?.Count ?? 0,
                    CommentsCount = p.Comments?.Count ?? 0,
                    IsLikedByCurrentUser = p.LikedBy?.Any(u => u.Id == userId) ?? false,
                    Comments = p.Comments?
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => MapToCommentDto(c, userId))
                        .ToList() ?? new List<CommentDto>()
                }).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetFeedAsync error: {ex}");
                throw;
            }
        }

        public async Task<CommentDto> AddCommentAsync(CreateCommentDto createCommentDto, int userId)
        {
            var post = await _context.Posts.FindAsync(createCommentDto.PostId);
            if (post == null)
                throw new InvalidOperationException("Post not found");

            var comment = new Comment
            {
                PostId = createCommentDto.PostId,
                UserId = userId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return await GetCommentDtoAsync(comment.Id, userId);
        }

        public async Task<CommentDto> GetCommentDtoAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.LikedBy)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            return MapToCommentDto(comment, userId);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                LastActive = user.LastActive,
                IsOnline = user.IsOnline,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0
            };
        }

        private CommentDto MapToCommentDto(Comment comment, int currentUserId)
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

        public async Task LikePostAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (!post.LikedBy.Any(u => u.Id == userId))
            {
                var user = await _context.Users.FindAsync(userId);
                post.LikedBy.Add(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnlikePostAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            var user = post.LikedBy.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                post.LikedBy.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PostDto>> SearchPostsAsync(string query, int userId, int page = 1, int pageSize = 10)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => p.Content.Contains(query) || p.LocationName.Contains(query))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts.Select(p => new PostDto
            {
                Id = p.Id,
                User = MapToUserDto(p.User),
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LocationName = p.LocationName,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                LikesCount = p.LikedBy?.Count ?? 0,
                CommentsCount = p.Comments?.Count ?? 0,
                IsLikedByCurrentUser = p.LikedBy?.Any(u => u.Id == userId) ?? false,
                Comments = p.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .Select(c => MapToCommentDto(c, userId))
                    .ToList() ?? new List<CommentDto>()
            }).ToList();
        }

        public async Task<List<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, int userId, int page = 1, int pageSize = 10)
        {
            const double radius = 10; // 10km radius
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => p.Latitude != null && p.Longitude != null)
                .OrderBy(p => 
                    Math.Acos(Math.Sin(latitude * Math.PI / 180) * Math.Sin(p.Latitude.Value * Math.PI / 180) +
                    Math.Cos(latitude * Math.PI / 180) * Math.Cos(p.Latitude.Value * Math.PI / 180) *
                    Math.Cos((p.Longitude.Value - longitude) * Math.PI / 180)) * 6371)
                .Where(p => 
                    Math.Acos(Math.Sin(latitude * Math.PI / 180) * Math.Sin(p.Latitude.Value * Math.PI / 180) +
                    Math.Cos(latitude * Math.PI / 180) * Math.Cos(p.Latitude.Value * Math.PI / 180) *
                    Math.Cos((p.Longitude.Value - longitude) * Math.PI / 180)) * 6371 <= radius)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts.Select(p => new PostDto
            {
                Id = p.Id,
                User = MapToUserDto(p.User),
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                LocationName = p.LocationName,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                LikesCount = p.LikedBy?.Count ?? 0,
                CommentsCount = p.Comments?.Count ?? 0,
                IsLikedByCurrentUser = p.LikedBy?.Any(u => u.Id == userId) ?? false,
                Comments = p.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .Select(c => MapToCommentDto(c, userId))
                    .ToList() ?? new List<CommentDto>()
            }).ToList();
        }
    }
}
