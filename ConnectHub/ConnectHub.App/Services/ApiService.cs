using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using ConnectHub.Shared.Models;
using ConnectHub.Shared.DTOs;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Mime;

namespace ConnectHub.App.Services
{
    public class ApiService : IApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public string? Token 
        { 
            get => _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            set
            {
                if (string.IsNullOrEmpty(value))
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                else
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
            }
        }

        public ApiService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration["ApiBaseUrl"] ?? "https://localhost:5001/")
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Debug.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Exception.Message}");
                    });
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { email, password };
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("api/auth/login", content));

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                    var token = result["token"];
                    Token = token;
                    return token;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                throw new HttpRequestException($"Login failed: {responseContent}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex}");
                throw;
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, string confirmPassword)
        {
            try
            {
                var registerData = new { 
                    Username = username, 
                    Email = email, 
                    Password = password,
                    ConfirmPassword = confirmPassword
                };
                
                var json = JsonConvert.SerializeObject(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("api/auth/register", content));
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException(errorContent);
                }
                
                throw new HttpRequestException($"Registration failed: {errorContent}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex}");
                throw;
            }
        }

        public async Task<List<Post>> GetFeedAsync(int skip, int take)
        {
            var response = await _httpClient.GetAsync($"api/posts?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>(_jsonOptions) ?? new List<Post>();
        }

        public async Task<Post> CreatePostAsync(string content, Stream? imageStream = null, string? fileName = null, double? latitude = null, double? longitude = null)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(content), "Content");

            if (imageStream != null && fileName != null)
            {
                var imageContent = new StreamContent(imageStream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                formData.Add(imageContent, "Image", fileName);
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                formData.Add(new StringContent(latitude.Value.ToString()), "Latitude");
                formData.Add(new StringContent(longitude.Value.ToString()), "Longitude");
            }

            var response = await _httpClient.PostAsync("api/posts", formData);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Post>(_jsonOptions) ?? throw new Exception("Failed to create post");
        }

        public async Task<bool> LikePostAsync(int postId)
        {
            var response = await _httpClient.PostAsync($"api/posts/{postId}/like", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnlikePostAsync(int postId)
        {
            var response = await _httpClient.DeleteAsync($"api/posts/{postId}/like");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Comment>> GetCommentsAsync(int postId)
        {
            var response = await _httpClient.GetAsync($"api/posts/{postId}/comments");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Comment>>(_jsonOptions) ?? new List<Comment>();
        }

        public async Task<Comment> AddCommentAsync(int postId, string content)
        {
            var comment = new { Content = content };
            var response = await _httpClient.PostAsJsonAsync($"api/posts/{postId}/comments", comment);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Comment>(_jsonOptions) ?? throw new Exception("Failed to add comment");
        }

        public async Task<List<Post>> SearchPostsAsync(string query)
        {
            var response = await _httpClient.GetAsync($"api/posts/search?q={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>(_jsonOptions) ?? new List<Post>();
        }

        public async Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude)
        {
            var response = await _httpClient.GetAsync($"api/posts/nearby?latitude={latitude}&longitude={longitude}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>(_jsonOptions) ?? new List<Post>();
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/chat/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ChatMessage>>(_jsonOptions) ?? new List<ChatMessage>();
        }

        public async Task SendMessageAsync(int receiverId, string content)
        {
            var message = new { ReceiverId = receiverId, Content = content };
            var response = await _httpClient.PostAsJsonAsync("api/chat", message);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> ReportPostAsync(int postId)
        {
            var response = await _httpClient.PostAsync($"api/posts/{postId}/report", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<User> GetUserProfileAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>(_jsonOptions) ?? throw new Exception("Failed to get user profile");
        }

        public async Task UpdateProfileAsync(string username, Stream? profilePictureStream = null)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(username), "Username");

            if (profilePictureStream != null)
            {
                var imageContent = new StreamContent(profilePictureStream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                formData.Add(imageContent, "ProfilePicture", "profile.jpg");
            }

            var response = await _httpClient.PutAsync("api/users/profile", formData);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var response = await _httpClient.PostAsync($"api/auth/logout/{userId}", null);
            if (response.IsSuccessStatusCode)
            {
                Token = null;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
