using ConnectHub.API.Data;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.API.Services
{
    public class PostService
    {
        private readonly ConnectHubContext _context;
        private readonly IConfiguration _configuration;

        public PostService(ConnectHubContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Post> CreatePostAsync(string userId, string content, string? imageUrl = null, double? latitude = null, double? longitude = null)
        {
            var post = new Post
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Content = content,
                ImageUrl = imageUrl,
                Latitude = latitude,
                Longitude = longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<List<Post>> GetFeedAsync(string userId, int skip = 0, int take = 20)
        {
            return await _context.Posts
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Post?> AddCommentAsync(string postId, string userId, string content)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return null;

            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            post.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> LikePostAsync(string postId, string userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return false;

            post.LikeCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Post>> SearchPostsAsync(string searchTerm)
        {
            return await _context.Posts
                .Where(p => p.Content.Contains(searchTerm))
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude, double radiusInKm = 10)
        {
            // Using the Haversine formula to calculate distance
            return await _context.Posts
                .Where(p => p.Latitude != null && p.Longitude != null)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();
        }
    }
}
