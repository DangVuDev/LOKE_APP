using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Settings
{
    public class DeviceNotificationMessage
    {
        public string DeviceToken { get; set; } = string.Empty; // Token từ app mobile (iOS/Android)
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public IDictionary<string, string>? Data { get; set; } // Optional payload (deep link, extra info…)
    }
}
