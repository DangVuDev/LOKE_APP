using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class CloudinaryFileStorageService(Cloudinary cloudinary) : IFileStorageService
    {
        private readonly Cloudinary _cloudinary = cloudinary;

        public async Task<string> UploadAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = Guid.NewGuid().ToString()
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                return result.SecureUrl.ToString();

            throw new Exception($"Upload failed: {result.Error?.Message}");
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }

        public Task<string> GetDownloadUrlAsync(string publicId)
        {
            var url = _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
            return Task.FromResult(url);
        }
    }

}
