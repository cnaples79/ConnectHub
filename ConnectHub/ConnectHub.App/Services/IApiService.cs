using ConnectHub.Shared.Models;
using ConnectHub.Shared.DTOs;

namespace ConnectHub.App.Services
{
    public interface IApiService
    {
        string? Token { get; set; }
        Task<string> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string username, string email, string password, string confirmPassword);
        Task<bool> LogoutAsync(int userId);
        Task<List<PostDto>> GetFeedAsync(int page, int pageSize);
        Task<Post> CreatePostAsync(string content, Stream? imageStream = null, string? fileName = null, double? latitude = null, double? longitude = null);
        Task<bool> LikePostAsync(int postId);
        Task<bool> UnlikePostAsync(int postId);
        Task<List<Comment>> GetCommentsAsync(int postId);
        Task<Comment> AddCommentAsync(int postId, string content);
        Task<List<Post>> SearchPostsAsync(string query);
        Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude);
        Task<List<ChatMessage>> GetChatHistoryAsync(int userId);
        Task SendMessageAsync(int receiverId, string content);
        Task<bool> ReportPostAsync(int postId);
        Task<User> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(string username, Stream? profilePictureStream = null);
    }
}
