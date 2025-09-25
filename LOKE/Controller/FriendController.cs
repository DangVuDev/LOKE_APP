using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto;
using LOKE.Models.Model;
using LOKE.Models.Model.ApplicationModel;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/friends")]
    [Authorize]
    public class FriendController(
        IBaseService<FriendModel> friendService,
        IUserService<ApplicationUser> userService
    ) : CoreController
    {
        private readonly IBaseService<FriendModel> _friendService = friendService;
        private readonly IUserService<ApplicationUser> _userService = userService;

        [HttpHead]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("Hello Friends API");

        // Gửi lời mời kết bạn
        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Request body cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.FriendUserId))
                return BadRequest("UserId and FriendUserId are required.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            if (requester.UserName == dto.FriendUserId)
                return BadRequest("Cannot send friend request to yourself.");

            var friend = new FriendModel
            {
                UserId = requester.UserName!,
                FriendUserId = dto.FriendUserId,
                Status = "pending"
            };

            var response = await _friendService.CreateAsync(friend);
            return response.IsSuccess
                ? Ok(response.Data.ToDto())
                : BadRequest(response.Message);
        }

        // Chấp nhận lời mời
        [HttpPut("accept")]
        public async Task<IActionResult> Accept(IdOnlyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return BadRequest("Id cannot be empty.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var response = await _friendService.GetByIdAsync(request.Id);
            if (!response.IsSuccess || response.Data == null)
                return NotFound(response.Message ?? "Friend request not found.");

            if (response.Data.Status == "accepted")
                return BadRequest("Friend request already accepted.");

            response.Data.Status = "accepted";
            var updateResponse = await _friendService.UpdateAsync(request.Id, response.Data);

            return updateResponse.IsSuccess
                ? Ok(updateResponse.Data)
                : BadRequest(updateResponse.Message);
        }

        // Từ chối lời mời
        [HttpPut("reject")]
        public async Task<IActionResult> Reject(IdOnlyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return BadRequest("Id cannot be empty.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var response = await _friendService.GetByIdAsync(request.Id);
            if (!response.IsSuccess || response.Data == null)
                return NotFound(response.Message ?? "Friend request not found.");

            var deleteResponse = await _friendService.DeleteAsync(request.Id);
            return deleteResponse.IsSuccess
                ? Ok(deleteResponse.Message)
                : BadRequest(deleteResponse.Message);
        }

        // Hủy kết bạn
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove(IdOnlyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return BadRequest("Id cannot be empty.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var response = await _friendService.DeleteAsync(request.Id);
            return response.IsSuccess
                ? NoContent()
                : BadRequest(response.Message);
        }

        // Lấy danh sách bạn bè
        [HttpGet("list")]
        public async Task<IActionResult> GetFriends()
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var allFriendsResp = await _friendService.GetAllAsync();
            if (!allFriendsResp.IsSuccess || allFriendsResp.Data == null)
                return BadRequest(allFriendsResp.Message);

            var relatedFriends = allFriendsResp.Data
                .Where(f => (f.UserId == requester.UserName && f.Status == "accepted") || f.FriendUserId == requester.UserName)
                .ToList();

            var result = new List<FriendDto>();

            foreach (var link in relatedFriends)
            {
                var otherUserId = link.UserId == requester.UserName ? link.FriendUserId : link.UserId;
                var userRes = await _userService.GetUserByIdAsync(otherUserId);

                if (userRes.IsSuccess && userRes.Data != null)
                {
                    result.Add(new FriendDto
                    {
                        Id = link.Id,
                        FriendUserId = otherUserId,
                        Status = link.Status,
                        UserId = requester.UserName,
                        Name = userRes.Data.Name! ?? userRes.Data.UserName!,
                        ProfileImageUrl = userRes.Data.ProfileImageUrl ?? ""
                    });
                }
            }

            return Ok(result);
        }

        // Lấy danh sách pending request
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token imvaliable.");

            var allFriendsResp = await _friendService.GetAllAsync();
            if (!allFriendsResp.IsSuccess || allFriendsResp.Data == null)
                return BadRequest(allFriendsResp.Message);

            var pending = allFriendsResp.Data
                .Where(f => f.FriendUserId == requester.UserName && f.Status == "pending")
                .ToList();

            var result = new List<FriendDto>();

            foreach (var link in pending)
            {
                var senderRes = await _userService.GetUserByIdAsync(link.UserId);
                if (senderRes.IsSuccess && senderRes.Data != null)
                {
                    result.Add(new FriendDto
                    {
                        FriendUserId = link.Id,
                        Status = link.Status,
                        UserId = senderRes.Data.Id,
                        Name = senderRes.Data.Name! ?? senderRes.Data.UserName!,
                        ProfileImageUrl = senderRes.Data.ProfileImageUrl ?? ""
                    });
                }
            }

            return Ok(result);
        }

        
    }
}
