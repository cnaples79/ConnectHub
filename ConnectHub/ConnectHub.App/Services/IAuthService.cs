using ConnectHub.Shared.DTOs;

namespace ConnectHub.App.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<bool> LogoutAsync();
        bool IsAuthenticated { get; }
        string? Token { get; set; }
        string? UserId { get; set; }
    }
}
