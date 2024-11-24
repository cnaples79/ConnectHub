using System;

namespace ConnectHub.Shared.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsOnline { get; set; }
    }
}
