using CloudinaryDotNet;
using Core.Service.DeployService;
using Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    public static class FileStorageConfig
    {
        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Cloudinary");
            var account = new Account
            (
                config["CloudName"],
                config["ApiKey"],
                config["ApiSecret"]
            );
            var cloudinary = new Cloudinary(account);
            services.AddSingleton(cloudinary);
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

            return services;
        }
    }
}
