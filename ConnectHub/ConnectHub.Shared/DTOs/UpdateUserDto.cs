using Microsoft.AspNetCore.Http;

namespace ConnectHub.Shared.DTOs
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public byte[]? ProfileImageData { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? ProfileImageContentType { get; set; }
    }
}
