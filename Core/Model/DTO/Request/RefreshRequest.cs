using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Request
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
