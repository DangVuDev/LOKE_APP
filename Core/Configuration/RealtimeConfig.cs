using Core.Hubs;
using Core.Model.Settings;
using Core.Service.DeployService;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    public static class RealtimeConfig
    {
        public static IServiceCollection AddRealtime(this IServiceCollection services, Action<RealtimeOptions>? configure = null)
        {
            var options = new RealtimeOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);

            if (options.UseRedis && !string.IsNullOrEmpty(options.RedisConnectionString))
                services.AddSingleton<IConnectionStore>(_ => new RedisConnectionStore(options.RedisConnectionString!));
            else
                services.AddSingleton<IConnectionStore, InMemoryConnectionStore>();

            services.AddSingleton<IRealtimeService, RealtimeService>();
            services.AddSignalR()
                    .AddJsonProtocol();

            if (options.UseRedis && !string.IsNullOrEmpty(options.RedisConnectionString))
                services.AddSignalR().AddStackExchangeRedis(options.RedisConnectionString);

            return services;
        }

        public static IEndpointRouteBuilder MapRealtimeHub(this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<RealtimeOptions>();
            endpoints.MapHub<AppHub>(options.HubPath);
            return endpoints;
        }
    }
}
