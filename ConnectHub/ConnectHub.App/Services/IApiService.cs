using ConnectHub.Shared.Models;

namespace ConnectHub.App.Services
{
    public interface IApiService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task<List<Post>> GetFeedAsync(int skip, int take);
        Task<Post> CreatePostAsync(string content, Stream? imageStream = null, string? fileName = null, double? latitude = null, double? longitude = null);
        Task<bool> LikePostAsync(string postId);
        Task<Post> AddCommentAsync(string postId, string content);
        Task<List<Post>> SearchPostsAsync(string query);
        Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude);
        Task<List<PrivateMessage>> GetChatHistoryAsync(string userId);
        Task SendMessageAsync(string receiverId, string content);
        Task<bool> ReportPostAsync(string postId);
        Task<User> GetUserProfileAsync(string userId);
        Task UpdateProfileAsync(string username, Stream? profilePictureStream = null);
        Task<bool> LogoutAsync();
    }
}
