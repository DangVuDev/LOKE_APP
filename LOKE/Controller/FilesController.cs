using Core.Controller;
using Core.Service.Interfaces;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/image")]
    public class FilesController : CoreController
    {
        private readonly IFileStorageService _fileStorageService;

        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Upload([FromForm] FileUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File is required");

            var url = await _fileStorageService.UploadAsync(request.File, request.Folder);
            return Ok(new { url });
        }

        [HttpPost("upload-base64")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> UploadBase64([FromBody] Base64UploadRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64))
                return BadRequest("Base64 string is required");

            try
            {
                // Chuyển base64 -> byte[]
                var fileBytes = Convert.FromBase64String(request.Base64);

                // Tạo IFormFile tạm để dùng lại service UploadAsync
                var stream = new MemoryStream(fileBytes);
                var file = new FormFile(stream, 0, fileBytes.Length, "file", request.FileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                var url = await _fileStorageService.UploadAsync(file, request.Folder);

                return Ok(new { url });
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
        }

        [HttpDelete("delete/{publicId}")]
        public async Task<IActionResult> Delete(string publicId)
        {
            var result = await _fileStorageService.DeleteAsync(publicId);
            if (!result) return BadRequest("Delete failed");

            return Ok(new { success = true });
        }

        [HttpGet("download/{publicId}")]
        public async Task<IActionResult> GetDownloadUrl(string publicId)
        {
            var url = await _fileStorageService.GetDownloadUrlAsync(publicId);
            return Ok(new { url });
        }
    }
    public class Base64UploadRequest
    {
        public string Base64 { get; set; } = string.Empty; // Chuỗi base64
        public string FileName { get; set; } = "image.png"; // Tên file muốn lưu
        public string Folder { get; set; } = "uploads"; // Thư mục lưu
    }
}
