using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConnectHub.Shared.DTOs
{
    public class PostDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public UserDto User { get; set; }
        public List<CommentDto>? Comments { get; set; }
    }

    public class CreatePostDto
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters")]
        public string Content { get; set; }

        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".gif" })]
        [MaxFileSize(10 * 1024 * 1024)] // 10MB
        public IFormFile? Image { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double? Longitude { get; set; }

        [StringLength(100, ErrorMessage = "Location name cannot exceed 100 characters")]
        public string? LocationName { get; set; }
    }

    public class UpdatePostDto
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters")]
        public string Content { get; set; }

        [StringLength(100, ErrorMessage = "Location name cannot exceed 100 characters")]
        public string? LocationName { get; set; }
    }

    public class CommentDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto User { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class CreateCommentDto
    {
        public string PostId { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 200 characters")]
        public string Content { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult($"File extension {extension} is not allowed. Allowed extensions: {string.Join(", ", _extensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"File size cannot exceed {_maxFileSize / (1024 * 1024)}MB");
                }
            }

            return ValidationResult.Success;
        }
    }
}
