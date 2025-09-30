using Core.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Controller
{
    [ApiController]
    [Route("api/realtime")]
    public class RealtimeController(IRealtimeService realtime) : ControllerBase
    {
        private readonly IRealtimeService _realtime = realtime;

        [HttpGet("online-users")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var users = await _realtime.GetOnlineUsersAsync();
            return Ok(users);
        }
    }
}
