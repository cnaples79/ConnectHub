using ConnectHub.Shared.DTOs;
using Microsoft.Maui.Storage;

namespace ConnectHub.App.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IPreferences _preferences;

        public AuthService(IApiService apiService, IPreferences preferences)
        {
            _apiService = apiService;
            _preferences = preferences;
            Token = _preferences.Get<string>("token", null);
            UserId = _preferences.Get<string>("userId", null);
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
        public string? Token { get; set; }
        public string? UserId { get; set; }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var token = await _apiService.LoginAsync(loginDto.Email, loginDto.Password);
            if (!string.IsNullOrEmpty(token))
            {
                Token = token;
                _preferences.Set("token", token);
                return token;
            }
            return string.Empty;
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            return await _apiService.RegisterAsync(registerDto.Username, registerDto.Email, registerDto.Password, registerDto.ConfirmPassword);
        }

        public async Task<bool> LogoutAsync()
        {
            if (!string.IsNullOrEmpty(UserId) && int.TryParse(UserId, out int userId))
            {
                var result = await _apiService.LogoutAsync(userId);
                if (result)
                {
                    Token = null;
                    UserId = null;
                    _preferences.Remove("token");
                    _preferences.Remove("userId");
                    return true;
                }
            }
            return false;
        }
    }
}
