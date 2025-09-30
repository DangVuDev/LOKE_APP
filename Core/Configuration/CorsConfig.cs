using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    public static class CorsConfig
    {
        public static IServiceCollection AddCorsService(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true) // cho phép mọi origin
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // bắt buộc khi dùng SignalR + token
                });
            });
            return services;
        }
    }



}
