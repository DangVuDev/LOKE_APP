using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Response
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
