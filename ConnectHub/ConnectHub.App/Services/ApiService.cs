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
using Microsoft.Extensions.Configuration;
using System.Net.Mime;
using System.Text.Json;

namespace ConnectHub.App.Services
{
    public class ApiService : IApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly string _logFilePath;

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
            Debug.WriteLine("Initializing ApiService...");
            _configuration = configuration;
            var apiBaseUrl = _configuration["ApiBaseUrl"];
            Debug.WriteLine($"API Base URL from configuration: {apiBaseUrl}");
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl ?? "http://localhost:5000/")
            };
            Debug.WriteLine($"HttpClient BaseAddress set to: {_httpClient.BaseAddress}");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var logDirectory = Path.Combine(documentsPath, "ConnectHub", "Logs");
                _logFilePath = Path.Combine(logDirectory, "connecthub_login.log");
                Debug.WriteLine($"Log file will be created at: {_logFilePath}");

                // Create the directory if it doesn't exist
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                    Debug.WriteLine($"Created log directory: {logDirectory}");
                }
                
                // Test write to the log file
                LogToFile("ApiService initialized");
                Debug.WriteLine("Successfully wrote initial log message");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing log file: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Debug.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Exception.Message}");
                        LogToFile($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Exception.Message}");
                    });
        }

        private void LogToFile(string message)
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logMessage);
                Debug.WriteLine($"Successfully logged to file: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                Debug.WriteLine($"Attempted to write to: {_logFilePath}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            Debug.WriteLine("=== Login Attempt Started ===");
            Debug.WriteLine($"Base Address: {_httpClient.BaseAddress}");
            
            try
            {
                LogToFile("=== Login Attempt Started ===");
                LogToFile($"Base Address: {_httpClient.BaseAddress}");
                
                var loginData = new { email, password };
                var json = JsonConvert.SerializeObject(loginData);
                Debug.WriteLine("Login data serialized (email hidden for security)");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                LogToFile("Sending login request...");
                Debug.WriteLine("Sending login request...");
                
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
                    {
                        Content = content
                    };
                    Debug.WriteLine($"Request URI: {request.RequestUri}");
                    LogToFile($"Request URI: {request.RequestUri}");
                    
                    var resp = await _httpClient.SendAsync(request);
                    Debug.WriteLine($"Response Status Code: {resp.StatusCode}");
                    LogToFile($"Response Status Code: {resp.StatusCode}");
                    return resp;
                });

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Received response content");
                LogToFile($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<AuthResponseDto>(responseContent);
                        if (result?.Token != null)
                        {
                            Debug.WriteLine("Successfully parsed authentication response");
                            LogToFile("Successfully parsed authentication response");
                            Token = result.Token;
                            return result.Token;
                        }
                        else
                        {
                            Debug.WriteLine("ERROR: Token was null in response");
                            LogToFile("ERROR: Token was null in response");
                            throw new InvalidOperationException("Invalid response format: Token was null");
                        }
                    }
                    catch (Newtonsoft.Json.JsonException ex)
                    {
                        Debug.WriteLine($"ERROR: Failed to parse response: {ex.Message}");
                        LogToFile($"ERROR: Failed to parse response: {ex.Message}");
                        throw new InvalidOperationException($"Failed to parse login response: {ex.Message}");
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("ERROR: Unauthorized: Invalid credentials");
                    LogToFile("ERROR: Unauthorized: Invalid credentials");
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                Debug.WriteLine($"ERROR: Unexpected response: {response.StatusCode}");
                LogToFile($"ERROR: Unexpected response: {response.StatusCode}");
                throw new HttpRequestException($"Login failed with status code {response.StatusCode}: {responseContent}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: Login exception: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                LogToFile($"ERROR: Login exception: {ex.GetType().Name}: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    LogToFile($"Inner exception: {ex.InnerException.Message}");
                    LogToFile($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
            finally
            {
                Debug.WriteLine("=== Login Attempt Completed ===");
                LogToFile("=== Login Attempt Completed ===");
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
                LogToFile($"ERROR: Registration error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PostDto>> GetFeedAsync(int page, int pageSize)
        {
            try
            {
                Debug.WriteLine($"Getting feed for page {page}, size {pageSize}");
                var skip = (page - 1) * pageSize;
                var response = await _httpClient.GetAsync($"api/posts?skip={skip}&take={pageSize}");
                response.EnsureSuccessStatusCode();
                
                var posts = await response.Content.ReadFromJsonAsync<List<Post>>(_jsonOptions) ?? new List<Post>();
                return posts.Select(p => new PostDto
                {
                    Id = p.Id.ToString(),
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    IsLikedByCurrentUser = p.IsLikedByCurrentUser,
                    LocationName = p.LocationName,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ImageUrl = p.ImageUrl,
                    User = p.User != null ? new UserDto 
                    {
                        Id = p.User.Id.ToString(),
                        Username = p.User.Username,
                        Email = p.User.Email,
                        ProfileImageUrl = p.User.ProfileImageUrl,
                        Bio = p.User.Bio,
                        CreatedAt = p.User.CreatedAt,
                        FollowersCount = p.User.Followers.Count,
                        FollowingCount = p.User.Following.Count,
                        IsFollowedByCurrentUser = false // This should be set based on the actual relationship
                    } : null
                }).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting feed: {ex.Message}");
                throw;
            }
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

        public async Task<List<CommentDto>> GetCommentsAsync(int postId)
        {
            try
            {
                Debug.WriteLine($"Getting comments for post {postId}...");
                var response = await _httpClient.GetAsync($"api/posts/{postId}/comments");
                response.EnsureSuccessStatusCode();
                var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>(_jsonOptions);
                return comments ?? new List<CommentDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting comments: {ex.Message}");
                throw;
            }
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

        public async Task<List<ChatMessage>> GetChatHistoryAsync()
        {
            Debug.WriteLine("Getting chat history...");
            try
            {
                var response = await _httpClient.GetAsync($"/api/chat/history");
                response.EnsureSuccessStatusCode();
                var messages = await response.Content.ReadFromJsonAsync<List<ChatMessage>>();
                return messages ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting chat history: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessageAsync(string message, int receiverId)
        {
            Debug.WriteLine("Sending message...");
            try
            {
                var content = new { Content = message, ReceiverId = receiverId };
                var response = await _httpClient.PostAsJsonAsync("/api/chat/send", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                throw;
            }
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

        public async Task<List<PostDto>> GetUserPostsAsync(int userId, int page, int pageSize)
        {
            try
            {
                Debug.WriteLine($"Getting posts for user {userId}...");
                var response = await _httpClient.GetAsync($"api/users/{userId}/posts?page={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();
                var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>(_jsonOptions);
                return posts ?? new List<PostDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user posts: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
