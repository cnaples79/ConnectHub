using System.Net.Http.Headers;
using System.Net.Http.Json;
using ConnectHub.Shared.Models;
using Newtonsoft.Json;

namespace ConnectHub.App.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IPreferences _preferences;

        public ApiService(IPreferences preferences)
        {
            _preferences = preferences;
            _baseUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:5000" // Android Emulator
                : "http://localhost:5000"; // iOS Simulator or Local Debug

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            // Add token if exists
            var token = _preferences.Get("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { email, password });
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _preferences.Set("auth_token", result.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            
            return result.Token;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new { username, email, password });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Post>> GetFeedAsync(int skip, int take)
        {
            var response = await _httpClient.GetAsync($"/api/post/feed?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>();
        }

        public async Task<Post> CreatePostAsync(string content, Stream? imageStream = null, string? fileName = null, double? latitude = null, double? longitude = null)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(content), "content");
            
            if (latitude.HasValue)
                form.Add(new StringContent(latitude.Value.ToString()), "latitude");
            
            if (longitude.HasValue)
                form.Add(new StringContent(longitude.Value.ToString()), "longitude");

            if (imageStream != null && fileName != null)
            {
                var imageContent = new StreamContent(imageStream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                form.Add(imageContent, "image", fileName);
            }

            var response = await _httpClient.PostAsync("/api/post", form);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Post>();
        }

        public async Task<bool> LikePostAsync(string postId)
        {
            var response = await _httpClient.PostAsync($"/api/post/{postId}/like", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<Post> AddCommentAsync(string postId, string content)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/post/{postId}/comment", new { content });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Post>();
        }

        public async Task<List<Post>> SearchPostsAsync(string query)
        {
            var response = await _httpClient.GetAsync($"/api/post/search?query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>();
        }

        public async Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude)
        {
            var response = await _httpClient.GetAsync($"/api/post/nearby?latitude={latitude}&longitude={longitude}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>();
        }

        public async Task<List<PrivateMessage>> GetChatHistoryAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<PrivateMessage>>();
        }

        public async Task SendMessageAsync(string receiverId, string content)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/chat/send", new { receiverId, content });
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> ReportPostAsync(string postId)
        {
            var response = await _httpClient.PostAsync($"/api/post/{postId}/report", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<User> GetUserProfileAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/user/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task UpdateProfileAsync(string username, Stream? profilePictureStream = null)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(username), "username");

            if (profilePictureStream != null)
            {
                var imageContent = new StreamContent(profilePictureStream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "profilePicture", "profile.jpg");
            }

            var response = await _httpClient.PutAsync("/api/user/profile", form);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> LogoutAsync()
        {
            var response = await _httpClient.PostAsync("/api/auth/logout", null);
            if (response.IsSuccessStatusCode)
            {
                _preferences.Remove("auth_token");
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return true;
            }
            return false;
        }

        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private class LoginResponse
        {
            public string Token { get; set; }
        }
    }
}
