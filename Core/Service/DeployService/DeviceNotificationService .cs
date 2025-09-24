using Core.Model.Settings;
using Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Core.Service.DeployService
{
    public class DeviceNotificationService : IDeviceNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverKey;

        public DeviceNotificationService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _serverKey = configuration["Fcm:ServerKey"]
                ?? throw new Exception("FCM ServerKey not found in config.");
        }

        public async Task SendToDeviceAsync(DeviceNotificationMessage message)
        {
            var payload = new
            {
                to = message.DeviceToken,
                notification = new
                {
                    title = message.Title,
                    body = message.Body
                },
                data = message.Data
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            request.Headers.TryAddWithoutValidation("Authorization", $"key={_serverKey}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"FCM error: {response.StatusCode}, {error}");
            }
        }
    }
}
