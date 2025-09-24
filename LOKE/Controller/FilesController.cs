using Core.Service.Interfaces;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
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
}
