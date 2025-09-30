using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IConnectionStore
    {
        Task AddConnectionAsync(string userId, string connectionId);
        Task RemoveConnectionAsync(string userId, string connectionId);
        Task<IReadOnlyList<string>> GetConnectionsAsync(string userId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<IReadOnlyList<string>> GetOnlineUsersAsync();
    }
}
