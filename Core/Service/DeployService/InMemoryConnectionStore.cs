using Core.Service.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class InMemoryConnectionStore : IConnectionStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _map = new();

        public Task AddConnectionAsync(string userId, string connectionId)
        {
            var set = _map.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
            set[connectionId] = 0;
            return Task.CompletedTask;
        }

        public Task RemoveConnectionAsync(string userId, string connectionId)
        {
            if (_map.TryGetValue(userId, out var set))
            {
                set.TryRemove(connectionId, out _);
                if (set.IsEmpty)
                    _map.TryRemove(userId, out _);
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetConnectionsAsync(string userId)
        {
            if (_map.TryGetValue(userId, out var set))
                return Task.FromResult<IReadOnlyList<string>>(set.Keys.ToList());

            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        public Task<bool> IsUserOnlineAsync(string userId) =>
            Task.FromResult(_map.ContainsKey(userId));

        public Task<IReadOnlyList<string>> GetOnlineUsersAsync() =>
            Task.FromResult<IReadOnlyList<string>>([.. _map.Keys]);
    }
}
