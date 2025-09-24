using Core.Model.Aplication;
using Core.Model.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interfaces
{
    public interface IUserService<TUser> where TUser : BaseUser
    {
        Task<BaseServiceResponse<TUser>> UpdateUserAsync(TUser user);
        Task<BaseServiceResponse<bool>> DeleteUserAsync(string userId);
        Task<BaseServiceResponse<TUser?>> GetUserByIdAsync(string userId);
        Task<BaseServiceResponse<TUser?>> GetUserByEmailAsync(string email);
        Task<BaseServiceResponse<IEnumerable<TUser>>> GetAllUsersAsync();
        Task<BaseServiceResponse<bool>> AssignRoleAsync(string userId, string roleName);
    }
}
