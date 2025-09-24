using Core.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IDeviceNotificationService
    {
        Task SendToDeviceAsync(DeviceNotificationMessage message);
    }
}
