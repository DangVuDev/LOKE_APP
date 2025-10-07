using Core.Controller;
using Core.Extention;
using Core.Model.DTO.Request;
using Core.Model.DTO.Response;
using Core.Service.Interfaces;
using LOKE.Models.Model;
using LOKE.Models.Model.ApplicationModel;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController(IAuthService<ApplicationUser> authService) : CoreController
    {
        private readonly IAuthService<ApplicationUser> _authService = authService;

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            if (request == null) return BadRequest("Invalid request.");
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and Password are required.");
            if (!request.Email.IsEmail())
                return BadRequest("Invalid email format.");

            var createUserResult = await _authService.RegisterAsync(request.Email, request.Password);
            if (!createUserResult.IsSuccess)
                return BadRequest(createUserResult.Message);
            

            return Ok(createUserResult.Data);
        }

        /// <summary>
        /// Đăng nhập bằng email và password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            if (request == null) return BadRequest("Invalid request.");
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and Password are required.");

            var result = await _authService.LoginAsync(request.Email, request.Password);
            if (!result.IsSuccess)
                return Unauthorized(result.Message);

            return Ok(result.Data);
        }

        /// <summary>
        /// Đăng nhập lại bằng Refresh Token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required.");

            var result = await _authService.LoginWithRefreshTokenAsync(request.RefreshToken);
            if (!result.IsSuccess)
                return Unauthorized(result.Message);

            return Ok(result.Data);
        }
    }
}
