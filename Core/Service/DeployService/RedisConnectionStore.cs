using Core.Service.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Core.Service.DeployService
{
    public class RedisConnectionStore : IConnectionStore, IDisposable
    {
        private readonly ConnectionMultiplexer _mux;
        private readonly IDatabase _db;
        private readonly string _prefix;

        public RedisConnectionStore(string connectionString, string prefix = "presence:")
        {
            _mux = ConnectionMultiplexer.Connect(connectionString);
            _db = _mux.GetDatabase();
            _prefix = prefix;
        }

        private string Key(string userId) => $"{_prefix}{userId}";

        public async Task AddConnectionAsync(string userId, string connectionId)
            => await _db.SetAddAsync(Key(userId), connectionId);

        public async Task RemoveConnectionAsync(string userId, string connectionId)
            => await _db.SetRemoveAsync(Key(userId), connectionId);

        public async Task<IReadOnlyList<string>> GetConnectionsAsync(string userId)
        {
            var members = await _db.SetMembersAsync(Key(userId));
            return [.. members.Select(m => (string)m!)];
        }

        public async Task<bool> IsUserOnlineAsync(string userId) =>
            (await _db.SetLengthAsync(Key(userId))) > 0;

        public async Task<IReadOnlyList<string>> GetOnlineUsersAsync()
        {
            // Warning: KEYS is not recommended in production with huge datasets
            var server = _mux.GetServer(_mux.GetEndPoints().First());
            return [.. server.Keys(pattern: $"{_prefix}*").Select(k => k.ToString().Replace(_prefix, ""))];
        }

        public void Dispose()
        {
            _mux.Dispose();
        }
    }
}
