using Core.Service.DeployService;
using Core.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
  //  "Fcm": {
  //  "ServerKey": "YOUR_FCM_SERVER_KEY"
  //}
internal static class NotificationConfig
    {
        public static IServiceCollection AddDeviceNotification(this IServiceCollection services)
        {
            services.AddHttpClient(); // cần cho FCM
            services.AddScoped<IDeviceNotificationService, DeviceNotificationService>();
            return services;
        }
    }
}
