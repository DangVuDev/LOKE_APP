using Core.Model.Aplication;
using Core.Model.DTO.Model;
using Core.Model.DTO.Response;
using Core.Model.Settings;
using Core.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class AuthService<TUser>(
        UserManager<TUser> userManager,
        SignInManager<TUser> signInManager,
        IOptions<JwtSettings> options) : IAuthService<TUser> where TUser : BaseUser, new()
    {
        private readonly UserManager<TUser> _userManager = userManager;
        private readonly SignInManager<TUser> _signInManager = signInManager;
        private readonly JwtSettings _options = options.Value;

        // Đơn giản lưu refresh token vào memory (demo),
        // thực tế có thể lưu vào Mongo/Redis
        private static readonly Dictionary<string, string> RefreshTokens = [];

        public async Task<BaseServiceResponse<AuthResponse>> RegisterAsync(string email, string password)
        {
            // check email trùng
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return BaseServiceResponse<AuthResponse>.Fail("Email already exists.");
            }

            var userId = GenerateId(email);

            var user = new TUser
            {
                Id = userId,
                UserName = userId,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BaseServiceResponse<AuthResponse>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            var tokens = GenerateTokens(user);
            return BaseServiceResponse<AuthResponse>.Ok(tokens, "User registered successfully.");
        }

        public async Task<BaseServiceResponse<AuthResponse>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BaseServiceResponse<AuthResponse>.Fail("Invalid email or password.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return BaseServiceResponse<AuthResponse>.Fail("Invalid email or password.");
            }

            var tokens = GenerateTokens(user);
            return BaseServiceResponse<AuthResponse>.Ok(tokens, "Login successful.");
        }

        public Task<BaseServiceResponse<AuthResponse>> LoginWithRefreshTokenAsync(string refreshToken)
        {
            if (!RefreshTokens.TryGetValue(refreshToken, out var userName))
            {
                return Task.FromResult(BaseServiceResponse<AuthResponse>.Fail("Invalid refresh token."));
            }
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
            
            if(user == null)
            {
                return Task.FromResult(BaseServiceResponse<AuthResponse>.Fail("User not found."));
            }   

            var tokens = GenerateTokens(user);
            return Task.FromResult(BaseServiceResponse<AuthResponse>.Ok(tokens, "Login with refresh token successful."));
        }


        private string GenerateId(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống", nameof(email));

            var localPart = email.Split('@')[0]; // lấy phần trước @

            var normalized = new string(localPart
                .ToLower()
                .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                .ToArray())
                .Replace(" ", "");

            return $"@{normalized}";
        }

        private AuthResponse GenerateTokens(TUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            RefreshTokens[refreshToken] = user.UserName!;

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo,
                UserName = user.UserName!,
                Email = user.Email!
            };
        }
    }
}
