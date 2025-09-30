using Core.Extention;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Core.Hubs
{
    public class AppHub(IRealtimeService realtime) : Hub<IRealtimeClient>
    {
        private readonly IRealtimeService _realtime = realtime;

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var requester = httpContext?.Request.GetInfoRequester();

            if (requester != null && !requester.IsExpired && !string.IsNullOrEmpty(requester.UserName))
            {
                await _realtime.AddConnectionAsync(requester.UserName!, Context.ConnectionId);

                // thông báo toàn hệ thống rằng user này online
                await Clients.All.PresenceUpdated(requester.UserName!, true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var requester = httpContext?.Request.GetInfoRequester();

            if (requester != null && !string.IsNullOrEmpty(requester.UserName))
            {
                await _realtime.RemoveConnectionAsync(requester.UserName!, Context.ConnectionId);

                if (!await _realtime.IsUserOnlineAsync(requester.UserName!))
                {
                    await Clients.All.PresenceUpdated(requester.UserName!, false);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Gửi message tới 1 user cụ thể (theo format MessageContent của FE)
        /// </summary>
        public async Task SendMessage(string toUserId, string content)
        {
            var httpContext = Context.GetHttpContext();
            var requester = httpContext?.Request.GetInfoRequester();

            if (requester == null || requester.IsExpired || string.IsNullOrEmpty(requester.UserName))
                return;

            var connections = await _realtime.GetConnectionsAsync(toUserId);
            foreach (var connId in connections)
            {
                // push object JSON về client
                await Clients.Client(connId).ReceiveMessage(requester.UserName!, content);
            }
        }

        /// <summary>
        /// Gửi thông báo (event + payload) tới 1 user
        /// </summary>
        public async Task SendNotification(string toUserId, string eventName, object? payload)
        {
            var connections = await _realtime.GetConnectionsAsync(toUserId);
            foreach (var connId in connections)
            {
                await Clients.Client(connId).ReceiveNotification(eventName, payload);
            }
        }

        /// <summary>
        /// Gửi broadcast toàn hệ thống
        /// </summary>
        public async Task BroadcastMessage(string message)
        {
            var httpContext = Context.GetHttpContext();
            var requester = httpContext?.Request.GetInfoRequester();

            if (requester == null || requester.IsExpired || string.IsNullOrEmpty(requester.UserName))
                return;

            await Clients.All.ReceiveMessage(requester.UserName!, message);
        }
    }
}
