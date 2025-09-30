using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Settings
{
    public class RealtimeOptions
    {
        public bool UseRedis { get; set; } = false;
        public string? RedisConnectionString { get; set; }
        public string HubPath { get; set; } = "/hubs/app";
    }
}
