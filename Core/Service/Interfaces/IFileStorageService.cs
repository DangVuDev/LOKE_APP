using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder = "uploads");
        Task<bool> DeleteAsync(string publicId);
        Task<string> GetDownloadUrlAsync(string publicId);
    }
}
