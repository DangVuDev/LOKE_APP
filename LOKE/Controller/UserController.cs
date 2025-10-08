using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto.ApplicationDto;
using LOKE.Models.Model.ApplicationModel;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/user")]
    [Authorize]
    public class UserController(IUserService<ApplicationUser> userService) : CoreController
    {
        private readonly IUserService<ApplicationUser> _userService = userService;

        // Cập nhật thông tin người dùng
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] ApplicationUserUpdateRequestDto request)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            if (request == null)
                return BadRequest("Request body cannot be null.");

            if (string.IsNullOrWhiteSpace(requester.UserName))
                return BadRequest("UserName is required.");

            // Nếu muốn chỉ cho phép update chính bản thân
            if (requester.UserName != requester.UserName)
                return Forbid("Cannot update other user's profile.");

            var getUserResponse = await _userService.GetUserByIdAsync(requester.UserName);
            if (!getUserResponse.IsSuccess || getUserResponse.Data == null)
                return NotFound(getUserResponse.Message ?? "User not found.");

            var userToUpdate = getUserResponse.Data;

            // Áp thông tin mới
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
                ? Ok(updateResponse.Data.ToDto())
                : BadRequest(updateResponse.Message);
        }

        // Xóa người dùng
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

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

        // Lấy người dùng theo token
        [HttpGet]
        public async Task<IActionResult> GetUserByToken()
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var result = await _userService.GetUserByIdAsync(requester.UserName!);
            return result.IsSuccess && result.Data != null
                ? Ok(result.Data.ToDto())
                : NotFound(result.Message ?? "User not found.");
        }

        // Lấy người dùng theo Id
        [HttpGet("by-id")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("UserId cannot be empty.");

            var result = await _userService.GetUserByIdAsync(id);
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
            if (requester == null)
                return Unauthorized("Token imvaliable.");

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
            if (requester == null)
                return Unauthorized("Token imvaliable.");

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

        [HttpGet("ui/by-id")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUIById([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("UserId cannot be empty.");

            // Giả định _userService.GetUserByIdAsync trả về một đối tượng có IsSuccess, Message và Data (chứa ApplicationUserDto)
            var result = await _userService.GetUserByIdAsync(id);

            if (result.IsSuccess && result.Data != null)
            {
                // Lấy dữ liệu người dùng (Giả sử result.Data là ApplicationUserDto hoặc có thể chuyển đổi sang)
                var user = result.Data;

                var sb = new StringBuilder();

                // --- 1. HEADER (Bao gồm CDN Tailwind, Style và Config mới) ---
                sb.Append($@"
                    <!DOCTYPE html>
                    <html lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Profile Cá Nhân - {user.Name}</title>
                        <script src='https://cdn.tailwindcss.com'></script>
                        <script>
                            tailwind.config = {{
                                theme: {{
                                    extend: {{
                                        colors: {{
                                            'brand-main': '#10b981', /* Màu Emerald */
                                            'bg-light': '#f5f7f9', /* Nền Rất nhẹ */
                                            'card-bg': '#ffffff', /* Nền Card Trắng */
                                            'bio-bg': '#f0fff4', /* Nền Bio nhẹ nhàng */
                                        }},
                                    }}
                                }}
                            }}
                        </script>
                        <style>
                            /* CSS cho hiệu ứng Soft UI */
                            .card {{ transition: all 0.3s ease-in-out; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); }}
                            .card:hover {{ transform: translateY(-3px); box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1); }}
                            .bio-box {{ border-left: 5px solid #10b981; background-color: #f0fff4; }}
                            .contact-link {{ transition: transform 0.2s, box-shadow 0.2s; }}
                            .contact-link:hover {{ transform: scale(1.02); box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1); }}
                        </style>
                    </head>
                    <body class='bg-bg-light min-h-screen py-12 px-4'>

                        <div class='max-w-4xl mx-auto space-y-8'>
                    
                            <div class='card bg-card-bg rounded-3xl overflow-hidden p-8'>
                        
                                <div class='flex flex-col sm:flex-row items-center sm:items-start pb-6 mb-6 border-b border-gray-100'>
                            
                                    <div class='relative w-32 h-32 sm:w-40 sm:h-40 mb-6 sm:mb-0 sm:mr-8'>
                                        <img class='w-full h-full rounded-full object-cover border-4 border-gray-100 shadow-xl' 
                                            src='{(string.IsNullOrEmpty(user.ProfileImageUrl) ? "https://via.placeholder.com/160/10b981/ffffff?text=AVT" : user.ProfileImageUrl)}' alt='Ảnh đại diện'>
                                        <span class='absolute bottom-2 right-2 w-5 h-5 bg-brand-main rounded-full border-2 border-white'></span>
                                    </div>
                            
                                    <div class='text-center sm:text-left pt-2'>
                                        <h1 class='text-4xl sm:text-5xl font-extrabold text-gray-900'>{user.Name}</h1> 
                                        <p class='text-xl text-brand-main font-semibold mt-2'>{(string.IsNullOrEmpty(user.Job) ? "Chưa rõ nghề nghiệp" : user.Job)}</p> 
                                        <p class='text-gray-500 mt-3 flex items-center justify-center sm:justify-start text-lg'>
                                            <svg class='w-5 h-5 mr-2 text-gray-400' fill='none' stroke='currentColor' viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z'></path></svg>
                                            {user.Email}
                                        </p>
                                    </div>
                                </div>

                                <div class='bio-box p-5 rounded-xl text-gray-700'>
                                    <h2 class='text-lg font-bold text-gray-800 mb-2'>Tóm tắt cá nhân</h2>
                                    <p class='leading-relaxed'>
                                        {user.Bio ?? "Chưa có thông tin mô tả chi tiết về bản thân."}
                                    </p>
                                </div>
                            </div>

                            <div class='card bg-card-bg p-8 rounded-3xl'>
                                <h2 class='text-2xl font-bold text-gray-800 border-b-2 border-brand-main/20 pb-4 mb-8 flex items-center'>
                                    <svg class='w-6 h-6 mr-2 text-brand-main' fill='none' stroke='currentColor' viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2'></path></svg>
                                    Chi Tiết Cá Nhân
                                </h2>

                                <div class='grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-8'>
                            
                                    {(string.IsNullOrEmpty(user.Hometown) ? "" : $@"
                                        <div class='detail-item'>
                                            <strong class='block text-sm font-medium text-gray-500 mb-1'>Quê quán</strong>
                                            <span class='text-xl font-semibold text-gray-800'>{user.Hometown}</span>
                                        </div>")}
                            
                                    {(string.IsNullOrEmpty(user.Education) ? "" : $@"
                                        <div class='detail-item'>
                                            <strong class='block text-sm font-medium text-gray-500 mb-1'>Học vấn</strong>
                                            <span class='text-xl font-semibold text-gray-800'>{user.Education}</span>
                                        </div>")}
                            
                                    {(string.IsNullOrEmpty(user.Company) ? "" : $@"
                                        <div class='detail-item'>
                                            <strong class='block text-sm font-medium text-gray-500 mb-1'>Công ty</strong>
                                            <span class='text-xl font-semibold text-gray-800'>{user.Company}</span>
                                        </div>")}

                                    {(string.IsNullOrEmpty(user.Status) ? "" : $@"
                                        <div class='detail-item'>
                                            <strong class='block text-sm font-medium text-gray-500 mb-1'>Trạng thái</strong>
                                            <span class='text-xl font-semibold text-gray-800'>{user.Status}</span>
                                        </div>")}

                                    {(string.IsNullOrEmpty(user.Interests) ? "" : $@"
                                        <div class='detail-item md:col-span-2'>
                                            <strong class='block text-sm font-medium text-gray-500 mb-2'>Sở thích</strong>
                                            <div class='flex flex-wrap gap-3'>
                                                {string.Join("", (user.Interests ?? "").Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(i => $@"<span class='px-4 py-1 bg-brand-main/10 text-brand-main text-sm font-medium rounded-full hover:bg-brand-main/20 transition duration-200 cursor-pointer'>{i.Trim()}</span>"))}
                                            </div>
                                        </div>")}
                                </div>
                            </div>
                    
                            {(user.Contacts!.Count != 0 ? $@"
                                <div class='card bg-card-bg p-8 rounded-3xl'>
                                    <h2 class='text-2xl font-bold text-gray-800 border-b-2 border-brand-main/20 pb-4 mb-8 flex items-center'>
                                        <svg class='w-6 h-6 mr-2 text-brand-main' fill='none' stroke='currentColor' viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z'></path></svg>
                                        Kết Nối
                                    </h2>
                                    <div class='flex flex-wrap gap-5'>
                            " : "")}
                        ");

                    // LẶP QUA DANH SÁCH CONTACTS VÀ TẠO THẺ DƯỚI DẠNG CARD NHỎ
                    foreach (var contact in user.Contacts)
                    {
                        // 1. XÁC ĐỊNH MÀU SẮC DỰA TRÊN LOẠI LIÊN HỆ (Để sử dụng cho Icon)
                        // Tôi sẽ giữ màu này cho icon hoặc tag nội bộ, nhưng màu nền chính sẽ là màu trắng.
                        string baseColor = contact.Type.ToLower() switch
                        {
                            "facebook" => "#1877f2",
                            "email" => "#d93025",
                            "zalo" => "#0084ff",
                            "linkedin" => "#0077b5",
                            "phone" => "#25d366", // WhatsApp/Green
                            _ => "#10b981" // Mặc định là Emerald
                        };

                        string hrefAttribute = contact.IsLink ? $"href='{contact.Value}' target='_blank'" : "";
                        string tagName = contact.IsLink ? "a" : "span";
                        string content = contact.IsLink ? contact.DisplayName : $"{contact.DisplayName}: {contact.Value}";

                        // Icon
                        string iconSrc = string.IsNullOrEmpty(contact.IconUrl)
                            ? $"https://via.placeholder.com/20/{baseColor.TrimStart('#')}/ffffff?text={contact.Type.Substring(0, 1).ToUpper()}"
                            : contact.IconUrl;

                        // 2. CLASS MỚI: CONTACT LÀ CARD (NỀN TRẮNG, CHỮ ĐEN, HIỆU ỨNG TƯƠNG TỰ CARD LỚN)
                        string newClass = $@"
                            contact-link 
                            flex-1 min-w-[200px] max-w-full 
                            flex items-center 
                            px-5 py-3 
                            bg-white text-gray-800 
                            font-semibold 
                            rounded-xl 
                            border-t border-b border-gray-100 
                            shadow-md 
                            hover:shadow-lg hover:border-brand-main 
                            transition duration-300
                        ";

                                        // Thêm style cho icon để thể hiện loại liên hệ
                                        string iconStyle = $"style='background-color: {baseColor}; border-radius: 9999px; padding: 2px; box-shadow: 0 1px 3px rgba(0,0,0,0.2);'";

                                        sb.Append($@"
                            <{tagName} class='{newClass.Replace("\n", "").Trim()}' {hrefAttribute}>
                                <span class='flex items-center justify-center w-8 h-8 mr-3' {iconStyle}>
                                    <img class='w-5 h-5' src='{iconSrc}' alt='{contact.Type} Icon'>
                                </span>
                                <span class='truncate'>{content}</span>
                            </{tagName}>
                        ");
                                }

                                if (user.Contacts.Any())
                                    {
                                        sb.Append("</div></div>"); // Đóng div flex-wrap và CARD 3
                                    }

                                    // --- 3. FOOTER CỦA HTML ---
                                    sb.Append(@"
                                    </div>
                                </body>
                                </html>
                            ");

                    // Trả về HTML đã nhúng dữ liệu
                    return Content(sb.ToString(), "text/html", Encoding.UTF8);
                }
                else
                {
                    // Trả về HTML 404
                    string notFoundHtml = $@"
                <html lang='vi'><head><script src='https://cdn.tailwindcss.com'></script></head>
                    <body class='bg-gray-100 flex items-center justify-center h-screen'>
                        <div class='text-center p-10 bg-white rounded-xl shadow-lg'>
                            <h1 class='text-5xl font-bold text-red-600 mb-4'>LỖI 404</h1>
                            <p class='text-xl text-gray-700'>{result.Message ?? "Người dùng với ID này không tồn tại."}</p>
                        </div>
                    </body>
                </html>";
                    return Content(notFoundHtml, "text/html", Encoding.UTF8);
                }
            }
        }

    public class AssignRoleRequest
    {
        public string RoleName { get; set; } = null!;
    }
}
