using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;

namespace ConnectHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;
        private readonly IWebHostEnvironment _environment;

        public FilesController(FileUploadService fileUploadService, IWebHostEnvironment environment)
        {
            _fileUploadService = fileUploadService;
            _environment = environment;
        }

        [HttpGet("{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = GetContentType(fileName);
            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, contentType);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            using (var stream = file.OpenReadStream())
            {
                if (!await _fileUploadService.ValidateFileAsync(stream, file.ContentType))
                {
                    return BadRequest("Invalid file type or size");
                }

                stream.Position = 0;
                var fileUrl = await _fileUploadService.UploadFileAsync(stream, file.FileName, file.ContentType);
                return Ok(new { url = fileUrl });
            }
        }

        private string GetContentType(string fileName)
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
    }
}
