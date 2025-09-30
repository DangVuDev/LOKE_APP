using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IRealtimeClient
    {
        Task ReceiveNotification(string eventName, object? payload);
        Task ReceiveMessage(string fromUserId, string message);
        Task PresenceUpdated(string userId, bool isOnline);
    }
}
