using Core.Model.Aplication;
using Core.Model.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IAuthService<TUser> where TUser : BaseUser
    {
        Task<BaseServiceResponse<AuthResponse>> RegisterAsync(string email, string password);
        Task<BaseServiceResponse<AuthResponse>> LoginAsync(string email, string password);
        Task<BaseServiceResponse<AuthResponse>> LoginWithRefreshTokenAsync(string refreshToken);
    }
}
