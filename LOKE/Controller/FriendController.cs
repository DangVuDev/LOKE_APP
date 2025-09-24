using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto;
using LOKE.Models.Model;
using LOKE.Models.Model.ApplicationModel;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/friends")]
    public class FriendController(
        IBaseService<FriendModel> friendService,
        IUserService<ApplicationUser> userService
    ) : CoreController
    {
        private readonly IBaseService<FriendModel> _friendService = friendService;
        private readonly IUserService<ApplicationUser> _userService = userService;

        [HttpHead]
        public IActionResult Ping() => Ok("Hello Friends API");

        // Gửi lời mời kết bạn
        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Request body cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.FriendUserId))
                return BadRequest("UserId and FriendUserId are required.");

            if (dto.UserId == dto.FriendUserId)
                return BadRequest("Cannot send friend request to yourself.");

            var friend = new FriendModel
            {
                UserId = dto.UserId,
                FriendUserId = dto.FriendUserId,
                Status = "pending"
            };

            var response = await _friendService.CreateAsync(friend);
            return response.IsSuccess
                ? Ok(response.Data.ToDto())
                : BadRequest(response.Message);
        }

        // Chấp nhận lời mời
        [HttpPut("accept/{id}")]
        public async Task<IActionResult> Accept(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id cannot be empty.");

            var response = await _friendService.GetByIdAsync(id);
            if (!response.IsSuccess || response.Data == null)
                return NotFound(response.Message ?? "Friend request not found.");

            if (response.Data.Status == "accepted")
                return BadRequest("Friend request already accepted.");

            response.Data.Status = "accepted";
            var updateResponse = await _friendService.UpdateAsync(id, response.Data);

            return updateResponse.IsSuccess
                ? Ok(updateResponse.Data)
                : BadRequest(updateResponse.Message);
        }

        // Từ chối lời mời
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id cannot be empty.");

            var response = await _friendService.GetByIdAsync(id);
            if (!response.IsSuccess || response.Data == null)
                return NotFound(response.Message ?? "Friend request not found.");

            var deleteResponse = await _friendService.DeleteAsync(id);
            return deleteResponse.IsSuccess
                ? Ok(deleteResponse.Message)
                : BadRequest(deleteResponse.Message);
        }

        // Hủy kết bạn
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id cannot be empty.");

            var response = await _friendService.DeleteAsync(id);
            return response.IsSuccess
                ? NoContent()
                : BadRequest(response.Message);
        }

        // Lấy danh sách bạn bè
        [HttpGet("list/{userId}")]
        public async Task<IActionResult> GetFriends(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var allFriendsResp = await _friendService.GetAllAsync();
            if (!allFriendsResp.IsSuccess || allFriendsResp.Data == null)
                return BadRequest(allFriendsResp.Message);

            var relatedFriends = allFriendsResp.Data
                .Where(f => (f.UserId == userId && f.Status == "accepted") || f.FriendUserId == userId)
                .ToList();

            var result = new List<FriendDto>();

            foreach (var link in relatedFriends)
            {
                var otherUserId = link.UserId == userId ? link.FriendUserId : link.UserId;
                var userRes = await _userService.GetUserByIdAsync(otherUserId);

                if (userRes.IsSuccess && userRes.Data != null)
                {
                    result.Add(new FriendDto
                    {
                        Id = link.Id,
                        FriendUserId = otherUserId,
                        Status = link.Status,
                        UserId = userId,
                        Name = userRes.Data.Name! ?? userRes.Data.UserName!,
                        ProfileImageUrl = userRes.Data.ProfileImageUrl ?? ""
                    });
                }
            }

            return Ok(result);
        }

        // Lấy danh sách pending request
        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var allFriendsResp = await _friendService.GetAllAsync();
            if (!allFriendsResp.IsSuccess || allFriendsResp.Data == null)
                return BadRequest(allFriendsResp.Message);

            var pending = allFriendsResp.Data
                .Where(f => f.FriendUserId == userId && f.Status == "pending")
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
