using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ConnectHub.API.Services
{
    public class PostService : IPostService
    {
        private readonly ConnectHubContext _context;
        private readonly FileUploadService _fileUploadService;
        private const int MaxContentLength = 500;
        private const int MaxCommentLength = 200;

        public PostService(ConnectHubContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(createPostDto.Content))
                throw new InvalidOperationException("Content cannot be empty");

            if (createPostDto.Content.Length > MaxContentLength)
                throw new InvalidOperationException($"Content cannot exceed {MaxContentLength} characters");

            string imageUrl = null;
            if (createPostDto.Image != null)
            {
                try
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error uploading image: {ex}");
                    throw new InvalidOperationException("Failed to upload image", ex);
                }
            }

            var post = new Post
            {
                UserId = int.Parse(userId),
                Content = createPostDto.Content.Trim(),
                ImageUrl = imageUrl,
                Latitude = createPostDto.Latitude,
                Longitude = createPostDto.Longitude,
                LocationName = createPostDto.LocationName?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                return await GetPostByIdAsync(post.Id.ToString());
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await _fileUploadService.DeleteFileAsync(imageUrl);
                }
                Debug.WriteLine($"Error creating post: {ex}");
                throw new InvalidOperationException("Failed to create post", ex);
            }
        }

        public async Task<bool> DeletePostAsync(string postId, string userId)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != int.Parse(userId))
                throw new UnauthorizedAccessException("You can only delete your own posts");

            try
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    await _fileUploadService.DeleteFileAsync(post.ImageUrl);
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting post: {ex}");
                throw new InvalidOperationException("Failed to delete post", ex);
            }
        }

        public async Task<List<PostDto>> GetFeedAsync(string userId, int page = 1, int pageSize = 10)
        {
            var userIdInt = int.Parse(userId);
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => p.UserId == userIdInt || 
                    _context.UserFollows.Any(f => f.FollowerId == userIdInt && f.FollowingId == p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await MapToPostDtoAsync(post));
            }

            return postDtos;
        }

        public async Task<PostDto> GetPostByIdAsync(string postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            return await MapToPostDtoAsync(post);
        }

        public async Task<PostDto> GetPostDtoAsync(string postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            return await MapToPostDtoAsync(post);
        }

        public async Task<List<PostDto>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 10)
        {
            var userIdInt = int.Parse(userId);
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => p.UserId == userIdInt)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await MapToPostDtoAsync(post));
            }

            return postDtos;
        }

        public async Task<bool> LikePostAsync(string postId, string userId)
        {
            var post = await _context.Posts
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            var userIdInt = int.Parse(userId);
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == post.Id && pl.UserId == userIdInt);

            if (existingLike != null)
                return false;

            var like = new PostLike
            {
                PostId = post.Id,
                UserId = userIdInt,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(string postId, string userId)
        {
            var post = await _context.Posts
                .Include(p => p.LikedBy)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            var userIdInt = int.Parse(userId);
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == post.Id && pl.UserId == userIdInt);

            if (existingLike == null)
                return false;

            _context.PostLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PostDto> UpdatePostAsync(string postId, string userId, UpdatePostDto updatePostDto)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != int.Parse(userId))
                throw new UnauthorizedAccessException("You can only update your own posts");

            post.Content = updatePostDto.Content;
            post.LocationName = updatePostDto.LocationName;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToPostDtoAsync(post);
        }

        public async Task<PostDto> AddCommentAsync(string postId, string userId, CreateCommentDto commentDto)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == int.Parse(postId));

            if (post == null)
                throw new InvalidOperationException("Post not found");

            var comment = new Comment
            {
                PostId = post.Id,
                UserId = int.Parse(userId),
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            post.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return await MapToPostDtoAsync(post);
        }

        public async Task<List<PostDto>> SearchPostsAsync(string query, string userId, int page = 1, int pageSize = 10)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => (p.Content != null && p.Content.Contains(query)) || 
                           (p.LocationName != null && p.LocationName.Contains(query)))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postDtos = new List<PostDto>();
            foreach (var post in posts)
            {
                postDtos.Add(await MapToPostDtoAsync(post));
            }

            return postDtos;
        }

        public async Task<List<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, double radius, int page = 1, int pageSize = 10)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.LikedBy)
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Filter posts by distance
            var nearbyPosts = posts.Where(p => 
            {
                if (!p.Latitude.HasValue || !p.Longitude.HasValue)
                    return false;
                    
                var distance = CalculateDistance(latitude, longitude, p.Latitude.Value, p.Longitude.Value);
                return distance <= radius;
            }).ToList();

            var postDtos = new List<PostDto>();
            foreach (var post in nearbyPosts)
            {
                postDtos.Add(await MapToPostDtoAsync(post));
            }

            return postDtos;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private async Task<PostDto> MapToPostDtoAsync(Post post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var user = await _context.Users.FindAsync(post.UserId);
            if (user == null)
                throw new InvalidOperationException("Post user not found");

            var likesCount = await _context.PostLikes.CountAsync(pl => pl.PostId == post.Id);
            var commentsCount = await _context.Comments.CountAsync(c => c.PostId == post.Id);

            return new PostDto
            {
                Id = post.Id.ToString(),
                Content = post.Content ?? "",
                ImageUrl = post.ImageUrl ?? "",
                LocationName = post.LocationName ?? "",
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                User = MapToUserDto(user),
                LikesCount = likesCount,
                CommentsCount = commentsCount
            };
        }

        private static UserDto MapToUserDto(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                Bio = user.Bio ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                CreatedAt = user.CreatedAt,
                FollowersCount = 0,
                FollowingCount = 0
            };
        }
    }
}
