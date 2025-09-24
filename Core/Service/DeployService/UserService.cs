using Core.Model.Aplication;
using Core.Model.DTO.Response;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class UserService<TUser, TRole> : IUserService<TUser>
    where TUser : BaseUser
    where TRole : BaseRole
    {
        private readonly UserManager<TUser> _userManager;
        private readonly RoleManager<TRole> _roleManager;

        public UserService(UserManager<TUser> userManager, RoleManager<TRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<BaseServiceResponse<TUser>> UpdateUserAsync(TUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BaseServiceResponse<TUser>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );

            return BaseServiceResponse<TUser>.Ok(user, "User updated successfully.");
        }

        public async Task<BaseServiceResponse<bool>> DeleteUserAsync(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userId);
            if (user == null)
                return BaseServiceResponse<bool>.Fail("User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BaseServiceResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

            return BaseServiceResponse<bool>.Ok(true, "User deleted successfully.");
        }

        public Task<BaseServiceResponse<TUser?>> GetUserByIdAsync(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userId);
            return Task.FromResult(user == null
                ? BaseServiceResponse<TUser?>.Fail("User not found.")
                : BaseServiceResponse<TUser?>.Ok(user));
        }

        public Task<BaseServiceResponse<TUser?>> GetUserByEmailAsync(string email)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Email == email);
            return Task.FromResult(user == null
                ? BaseServiceResponse<TUser?>.Fail("User not found.")
                : BaseServiceResponse<TUser?>.Ok(user));
        }

        public Task<BaseServiceResponse<IEnumerable<TUser>>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            return Task.FromResult(BaseServiceResponse<IEnumerable<TUser>>.Ok(users));
        }

        public async Task<BaseServiceResponse<bool>> AssignRoleAsync(string userId, string roleName)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userId);
            if (user == null)
                return BaseServiceResponse<bool>.Fail("User not found.");

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = Activator.CreateInstance<TRole>();
                role.Name = roleName;
                var roleResult = await _roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                    return BaseServiceResponse<bool>.Fail("Failed to create role.");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BaseServiceResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

            return BaseServiceResponse<bool>.Ok(true, "Role assigned successfully.");
        }
    }

}
