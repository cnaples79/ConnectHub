using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using System.Collections.Generic;

namespace ConnectHub.API.Services
{
    public class AuthService
    {
        private readonly ConnectHubContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ConnectHubContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                IsOnline = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            user.IsOnline = true;
            user.LastActive = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                User = MapToUserDto(user)
            };
        }

        public async Task LogoutUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastActive = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7"));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                IsOnline = user.IsOnline,
                LastActive = user.LastActive,
                CreatedAt = user.CreatedAt,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0
            };
        }
    }
}
