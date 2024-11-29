using ConnectHub.Shared.DTOs;

namespace ConnectHub.API.Services
{
    public interface IPostService
    {
        Task<List<PostDto>> GetFeedAsync(string userId, int page = 1, int pageSize = 10);
        Task<PostDto> CreatePostAsync(CreatePostDto postDto, string userId);
        Task<bool> LikePostAsync(string postId, string userId);
        Task<bool> UnlikePostAsync(string postId, string userId);
        Task<List<PostDto>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 10);
        Task<bool> DeletePostAsync(string postId, string userId);
        Task<PostDto> GetPostByIdAsync(string postId);
        Task<PostDto> UpdatePostAsync(string postId, string userId, UpdatePostDto updatePostDto);
        Task<PostDto> AddCommentAsync(string postId, string userId, CreateCommentDto commentDto);
        Task<List<PostDto>> SearchPostsAsync(string query, string userId, int page = 1, int pageSize = 10);
        Task<List<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, double radius, int page = 1, int pageSize = 10);
        Task<PostDto> GetPostDtoAsync(string postId);
    }
}
