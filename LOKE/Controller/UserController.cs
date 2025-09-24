using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto.ApplicationDto;
using LOKE.Models.Model.ApplicationModel;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController(IUserService<ApplicationUser> userService) : CoreController
    {
        private readonly IUserService<ApplicationUser> _userService = userService;

        // Cập nhật thông tin người dùng
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] ApplicationUserUpdateRequestDto request)
        {
            var requester = HttpContext.Request.GetInfoRequester();

            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            if (request == null)
                return BadRequest("Request body cannot be null.");

            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest("UserName is required.");

            // Nếu muốn chỉ cho phép update chính bản thân
            if (request.UserName != requester.UserName)
                return Forbid("Cannot update other user's profile.");

            var getUserResponse = await _userService.GetUserByIdAsync(request.UserName);
            if (!getUserResponse.IsSuccess || getUserResponse.Data == null)
                return NotFound(getUserResponse.Message ?? "User not found.");

            var userToUpdate = getUserResponse.Data;

            // Áp thông tin mới
            if (!string.IsNullOrEmpty(request.Email)) userToUpdate.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Name)) userToUpdate.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Bio)) userToUpdate.Bio = request.Bio;
            if (!string.IsNullOrEmpty(request.Hometown)) userToUpdate.Hometown = request.Hometown;
            if (!string.IsNullOrEmpty(request.Education)) userToUpdate.Education = request.Education;
            if (!string.IsNullOrEmpty(request.Job)) userToUpdate.Job = request.Job;
            if (!string.IsNullOrEmpty(request.Company)) userToUpdate.Company = request.Company;
            if (!string.IsNullOrEmpty(request.Status)) userToUpdate.Status = request.Status;
            if (!string.IsNullOrEmpty(request.Interests)) userToUpdate.Interests = request.Interests;
            if (!string.IsNullOrEmpty(request.ProfileImageUrl)) userToUpdate.ProfileImageUrl = request.ProfileImageUrl;
            if (request.Contacts != null) userToUpdate.Contacts = request.Contacts;

            var updateResponse = await _userService.UpdateUserAsync(userToUpdate);
            return updateResponse.IsSuccess
                ? Ok(updateResponse.Data)
                : BadRequest(updateResponse.Message);
        }

        // Xóa người dùng
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            // Nếu muốn chỉ cho phép xóa chính bản thân hoặc admin
            if (userId != requester.UserName /* && !requester.IsAdmin */)
                return Forbid("Cannot delete other user's account.");

            var result = await _userService.DeleteUserAsync(userId);
            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Message);
        }

        // Lấy người dùng theo Id
        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var result = await _userService.GetUserByIdAsync(userId);
            return result.IsSuccess && result.Data != null
                ? Ok(result.Data.ToDto())
                : NotFound(result.Message ?? "User not found.");
        }

        // Lấy người dùng theo Email
        [HttpGet("by-email")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var result = await _userService.GetUserByEmailAsync(email);
            return result.IsSuccess && result.Data != null
                ? Ok(result.Data.ToDto())
                : NotFound(result.Message ?? "User not found.");
        }

        // Lấy tất cả người dùng
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            var result = await _userService.GetAllUsersAsync();
            return result.IsSuccess && result.Data != null
                ? Ok(result.Data.ToDtoList())
                : BadRequest(result.Message);
        }

        // Gán role cho người dùng
        [HttpPost("{userId}/assign-role")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            if (request == null || string.IsNullOrWhiteSpace(request.RoleName))
                return BadRequest("RoleName is required.");

            // Có thể check requester quyền admin ở đây nếu muốn

            var result = await _userService.AssignRoleAsync(userId, request.RoleName);
            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Message);
        }
    }

    public class AssignRoleRequest
    {
        public string RoleName { get; set; } = null!;
    }
}
