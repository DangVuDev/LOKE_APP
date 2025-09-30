using Core.Hubs;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Core.Service.DeployService
{
    public class RealtimeService : IRealtimeService
    {
        private readonly IHubContext<AppHub, IRealtimeClient> _hubContext;
        private readonly IConnectionStore _connectionStore;

        public RealtimeService(IHubContext<AppHub, IRealtimeClient> hubContext, IConnectionStore connectionStore)
        {
            _hubContext = hubContext;
            _connectionStore = connectionStore;
        }

        public async Task SendToUserAsync(string userId, string fromUserId, string message)
        {
            var connections = await _connectionStore.GetConnectionsAsync(userId);
            foreach (var conn in connections)
                await _hubContext.Clients.Client(conn).ReceiveMessage(fromUserId, message);
        }

        public async Task BroadcastAsync(string fromUserId, string message)
            => await _hubContext.Clients.All.ReceiveMessage(fromUserId, message);

        public async Task SendToGroupAsync(string groupName, string fromUserId, string message)
            => await _hubContext.Clients.Group(groupName).ReceiveMessage(fromUserId, message);

        public Task AddConnectionAsync(string userId, string connectionId)
            => _connectionStore.AddConnectionAsync(userId, connectionId);

        public Task RemoveConnectionAsync(string userId, string connectionId)
            => _connectionStore.RemoveConnectionAsync(userId, connectionId);

        public Task<bool> IsUserOnlineAsync(string userId)
            => _connectionStore.IsUserOnlineAsync(userId);

        public Task<IReadOnlyList<string>> GetConnectionsAsync(string userId)
            => _connectionStore.GetConnectionsAsync(userId);

        public Task<IReadOnlyList<string>> GetOnlineUsersAsync()
            => _connectionStore.GetOnlineUsersAsync();

        public async Task NotifyUserAsync(string userId, string eventName, object? payload)
        {
            var connections = await _connectionStore.GetConnectionsAsync(userId);
            foreach (var conn in connections)
                await _hubContext.Clients.Client(conn).ReceiveNotification(eventName, payload);
        }
    }
}
