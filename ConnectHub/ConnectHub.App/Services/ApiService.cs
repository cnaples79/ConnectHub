using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Diagnostics;
using ConnectHub.Shared.Models;
using Newtonsoft.Json;

namespace ConnectHub.App.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IPreferences _preferences;

        public string? Token
        {
            get => _preferences.Get<string>("auth_token", null);
            set
            {
                if (value == null)
                    _preferences.Remove("auth_token");
                else
                    _preferences.Set("auth_token", value);
                
                if (value != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
        }

        public ApiService(IPreferences preferences)
        {
            _preferences = preferences;
            _baseUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:5000" // Android Emulator
                : "http://localhost:5000"; // iOS Simulator or Local Debug

            Debug.WriteLine($"Initializing ApiService with base URL: {_baseUrl}");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            // Add token if exists
            var token = _preferences.Get<string>("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                Token = token;
                Debug.WriteLine("Token found and set in HttpClient headers");
            }
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Login failed: {errorContent}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token == null)
                {
                    throw new HttpRequestException("Invalid response from server");
                }
                
                Token = result.Token;
                
                return result.Token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
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
                
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerData);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Registration failed: {errorContent}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex}");
                throw;
            }
        }

        public async Task<List<Post>> GetFeedAsync(int skip, int take)
        {
            try
            {
                // Convert skip/take to page/pageSize
                int page = (skip / take) + 1;
                int pageSize = take;
                
                Debug.WriteLine($"Getting feed with page={page}, pageSize={pageSize}");
                
                // Ensure token is set
                if (string.IsNullOrEmpty(Token))
                {
                    Debug.WriteLine("No auth token found for feed request");
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    Debug.WriteLine("Added token to request headers");
                }

                var response = await _httpClient.GetAsync($"/api/post?page={page}&pageSize={pageSize}");
                Debug.WriteLine($"Feed API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Feed API error: {error}");
                    throw new HttpRequestException($"Failed to load feed: {error}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Feed API response content: {content}");
                
                var posts = JsonConvert.DeserializeObject<List<Post>>(content);
                Debug.WriteLine($"Deserialized {posts?.Count ?? 0} posts");
                return posts ?? new List<Post>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetFeedAsync error: {ex}");
                throw;
            }
        }

        public async Task<Post> CreatePostAsync(string content, Stream? imageStream = null, string? fileName = null, double? latitude = null, double? longitude = null)
        {
            try
            {
                // Ensure token is set
                if (string.IsNullOrEmpty(Token))
                {
                    Debug.WriteLine("No auth token found for create post request");
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    Debug.WriteLine("Added token to request headers");
                }

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(content), "Content"); // Changed to match DTO property name

                if (latitude.HasValue)
                    form.Add(new StringContent(latitude.Value.ToString()), "Latitude");
                
                if (longitude.HasValue)
                    form.Add(new StringContent(longitude.Value.ToString()), "Longitude");

                if (imageStream != null && fileName != null)
                {
                    var imageContent = new StreamContent(imageStream);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                    form.Add(imageContent, "Image", fileName);
                }

                Debug.WriteLine("Sending create post request...");
                var response = await _httpClient.PostAsync("/api/post", form);
                Debug.WriteLine($"Create post response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Create post error: {errorContent}");
                    throw new HttpRequestException($"Failed to create post: {errorContent}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Create post response: {responseContent}");
                
                var post = JsonConvert.DeserializeObject<Post>(responseContent);
                if (post == null)
                {
                    throw new HttpRequestException("Invalid response from server");
                }
                return post;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreatePostAsync error: {ex}");
                throw;
            }
        }

        public async Task<bool> LikePostAsync(int postId)
        {
            var response = await _httpClient.PostAsync($"/api/posts/{postId}/like", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnlikePostAsync(int postId)
        {
            var response = await _httpClient.DeleteAsync($"/api/posts/{postId}/like");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Comment>> GetCommentsAsync(int postId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/post/{postId}/comments");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Comment>>() ?? new List<Comment>();
                }
                throw new HttpRequestException($"Error getting comments: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting comments: {ex}");
                throw;
            }
        }

        public async Task<Comment> AddCommentAsync(int postId, string content)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/post/{postId}/comments", new { Content = content });
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Comment>();
                }
                throw new HttpRequestException($"Error adding comment: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding comment: {ex}");
                throw;
            }
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/{userId}/history");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ChatMessage>>();
        }

        public async Task SendMessageAsync(int receiverId, string content)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/chat/{receiverId}/send", new { content });
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> ReportPostAsync(int postId)
        {
            var response = await _httpClient.PostAsync($"/api/posts/{postId}/report", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<User> GetUserProfileAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}/profile");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var response = await _httpClient.PostAsync($"/api/auth/logout/{userId}", null);
            if (response.IsSuccessStatusCode)
            {
                Token = null;
                return true;
            }
            return false;
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

        public async Task<List<Post>> SearchPostsAsync(string query)
        {
            var response = await _httpClient.GetAsync($"/api/posts/search?query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>();
        }

        public async Task<List<Post>> GetNearbyPostsAsync(double latitude, double longitude)
        {
            var response = await _httpClient.GetAsync($"/api/posts/nearby?latitude={latitude}&longitude={longitude}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Post>>();
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
