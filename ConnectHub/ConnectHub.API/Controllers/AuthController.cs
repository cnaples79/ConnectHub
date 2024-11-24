using Microsoft.AspNetCore.Mvc;
using ConnectHub.API.Services;
using ConnectHub.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace ConnectHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            var user = await _authService.RegisterUserAsync(
                registrationDto.Username, 
                registrationDto.Email, 
                registrationDto.Password
            );

            if (user == null)
                return BadRequest("Email already exists");

            return Ok(new { Message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var token = await _authService.AuthenticateUserAsync(
                loginDto.Email, 
                loginDto.Password
            );

            if (token == null)
                return Unauthorized("Invalid email or password");

            return Ok(new { Token = token });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _authService.LogoutUserAsync(userId);
            return Ok(new { Message = "Logged out successfully" });
        }
    }

    // DTOs for input validation
    public class UserRegistrationDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
