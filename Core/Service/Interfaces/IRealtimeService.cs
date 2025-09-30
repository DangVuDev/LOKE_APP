using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IRealtimeService
    {
        Task SendToUserAsync(string userId, string fromUserId, string message);
        Task BroadcastAsync(string fromUserId, string message);
        Task SendToGroupAsync(string groupName, string fromUserId, string message);

        Task AddConnectionAsync(string userId, string connectionId);
        Task RemoveConnectionAsync(string userId, string connectionId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<IReadOnlyList<string>> GetConnectionsAsync(string userId);
        Task<IReadOnlyList<string>> GetOnlineUsersAsync();

        Task NotifyUserAsync(string userId, string eventName, object? payload);

    }

}
