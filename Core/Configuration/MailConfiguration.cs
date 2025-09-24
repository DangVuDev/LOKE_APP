using Core.Model.Settings;
using Core.Service.DeployService;
using Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    internal static class MailConfiguration
    {
        public static IServiceCollection AddMailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<IMailService, MailService>();
            return services;
        }
    }
}
