namespace LOKE.Models.Request
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; } = default!;
        public string Folder { get; set; } = "uploads";
    }
}
