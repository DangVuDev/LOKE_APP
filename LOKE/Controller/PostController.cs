using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto;
using LOKE.Models.Model;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/posts")]
    [Authorize]
    public class PostController(
        IBaseService<PostModel> postService,
        IBaseService<FriendModel> friendService
    ) : CoreController
    {
        private readonly IBaseService<PostModel> _postService = postService;
        private readonly IBaseService<FriendModel> _friendService = friendService;

        [HttpHead]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("Posts API is alive with ci/cd");

        // Tạo bài đăng mới
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] PostRequest postRequest)
        {
            if (postRequest == null)
                return BadRequest("Post cannot be null.");

            if (string.IsNullOrWhiteSpace(postRequest.Content) && string.IsNullOrWhiteSpace(postRequest.ImageUrl))
                return BadRequest("Post must have content or image.");

            // Lấy UserId từ token
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            var postModel = new PostModel
            {
                UserId = requester.UserName!,
                Content = postRequest.Content,
                ImageUrl = postRequest.ImageUrl,
            };

            var response = await _postService.CreateAsync(postModel);
            return response.IsSuccess ? Ok(response.Data.ToDto()) : BadRequest(response.Message);
        }

        // Lấy bài đăng theo Id
        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return BadRequest("PostId is required.");

            var response = await _postService.GetByIdAsync(postId);
            return response.IsSuccess && response.Data != null
                ? Ok(response.Data.ToDto())
                : NotFound(response.Message ?? "Post not found.");
        }

        // Lấy tất cả bài đăng (skip/limit)
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts([FromQuery] int skip = 0, [FromQuery] int limit = 100)
        {
            var response = await _postService.GetAllAsync(skip, limit);
            return response.IsSuccess && response.Data != null
                ? Ok(response.Data.ToDtoList())
                : BadRequest(response.Message);
        }

      

        // Xóa bài đăng
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return BadRequest("PostId is required.");

            var getResponse = await _postService.GetByIdAsync(postId);
            if (!getResponse.IsSuccess || getResponse.Data == null)
                return NotFound(getResponse.Message ?? "Post not found.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            if (getResponse.Data.UserId != requester.UserName)
                return Forbid("Cannot delete others' post.");

            var response = await _postService.DeleteAsync(postId);
            return response.IsSuccess ? NoContent() : BadRequest(response.Message);
        }

        // Thêm comment cho bài đăng
        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest commentRequest)
        {
            if (commentRequest == null) 
                return BadRequest("Request is null");
            if (string.IsNullOrWhiteSpace(commentRequest.PostId))
                return BadRequest("PostId is required.");
            if (string.IsNullOrWhiteSpace(commentRequest.Content))
                return BadRequest("Comment content is required.");

            var postResp = await _postService.GetByIdAsync(commentRequest.PostId);
            if (!postResp.IsSuccess || postResp.Data == null)
                return NotFound(postResp.Message ?? "Post not found.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            var comment = new CommentModel
            {
                UserId = requester.UserName!,
                Content = commentRequest.Content
            };

            postResp.Data.Comments.Add(comment);
            var updateResp = await _postService.UpdateAsync(commentRequest.PostId, postResp.Data);

            return updateResp.IsSuccess ? Ok(updateResp.Message) : BadRequest(updateResp.Message);
        }

        // Thích bài đăng
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return BadRequest("PostId is required.");

            var postResp = await _postService.GetByIdAsync(postId);
            if (!postResp.IsSuccess || postResp.Data == null)
                return NotFound(postResp.Message ?? "Post not found.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            postResp.Data.Likes += 1;
            var updateResp = await _postService.UpdateAsync(postId, postResp.Data);

            return updateResp.IsSuccess ? Ok(updateResp.Message) : BadRequest(updateResp.Message);
        }

        // Thích ẩn danh
        [HttpPost("{postId}/secret-like")]
        public async Task<IActionResult> SecretLikePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return BadRequest("PostId is required.");

            var postResp = await _postService.GetByIdAsync(postId);
            if (!postResp.IsSuccess || postResp.Data == null)
                return NotFound(postResp.Message ?? "Post not found.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");

            postResp.Data.SecretLikes += 1;
            var updateResp = await _postService.UpdateAsync(postId, postResp.Data);

            return updateResp.IsSuccess ? Ok(updateResp.Message) : BadRequest(updateResp.Message);
        }

        // Lấy feed bài đăng
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed(
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 10)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester.IsExpired)
                return Unauthorized("Token expired.");
            // Lấy danh sách bạn bè accepted
            var friendsResp = await _friendService.GetAllAsync();
            if (!friendsResp.IsSuccess || friendsResp.Data == null)
                return BadRequest(friendsResp.Message);

            string userId = requester.UserName!;

            var friendIds = friendsResp.Data
                .Where(f => f.Status == "accepted" && (f.UserId == userId || f.FriendUserId == userId))
                .Select(f => f.UserId == userId ? f.FriendUserId : f.UserId)
                .ToList();

            friendIds.Add(userId);

            var postsResp = await _postService.GetAllAsync();
            if (!postsResp.IsSuccess || postsResp.Data == null)
                return BadRequest(postsResp.Message);

            var feedPosts = postsResp.Data
                .Where(p => friendIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var rnd = new Random();
            feedPosts = feedPosts.OrderBy(x => rnd.Next()).ToList();

            var pagedPosts = feedPosts.Skip(skip).Take(limit).ToList();
            return Ok(pagedPosts.ToDtoList());
        }
    }
}
