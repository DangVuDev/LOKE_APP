using Core.Controller;
using Core.Extention;
using Core.Service.Interfaces;
using LOKE.Models.Dto;
using LOKE.Models.Model;
using LOKE.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/posts")]
    [Authorize]
    public class PostController(
        IBaseService<UserPostModel> postService,
        IBaseService<FriendModel> friendService
    ) : CoreController
    {
        private readonly IBaseService<UserPostModel> _postService = postService;
        private readonly IBaseService<FriendModel> _friendService = friendService;

        // ✅ Check API status
        [HttpHead]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("Posts API is alive");

        // 🧩 1️⃣ Tạo bài đăng mới
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] PostRequest postRequest)
        {
            if (postRequest == null)
                return BadRequest("Post cannot be null.");
            if (string.IsNullOrWhiteSpace(postRequest.Content) && string.IsNullOrWhiteSpace(postRequest.ImageUrl))
                return BadRequest("Post must have content or image.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token invalid.");

            var userId = requester.UserName!;
            var existingUserPosts = await _postService.GetOneByFilterAsync(e => e.UserId == userId);

            UserPostModel userPostData;

            // Nếu chưa có record UserPostModel, tạo mới
            if (!existingUserPosts.IsSuccess || existingUserPosts.Data == null)
            {
                var createResp = await _postService.CreateAsync(new UserPostModel
                {
                    UserId = userId,
                    Accesser = Accesser.Everyone,
                    Posts = []
                });

                if (!createResp.IsSuccess || createResp.Data == null)
                    return BadRequest(createResp.Message);

                userPostData = createResp.Data;
            }
            else
            {
                userPostData = existingUserPosts.Data;
            }

            var newPost = new PostModel
            {
                UserId = userId,
                Content = postRequest.Content,
                ImageUrl = postRequest.ImageUrl
            };

            userPostData.Posts.Add(newPost);

            var response = await _postService.UpdateAsync(userPostData.Id!, userPostData);
            return response.IsSuccess ? Ok(newPost.ToDto()) : BadRequest(response.Message);
        }

        // 🧩 2️⃣ Lấy bài đăng của 1 user
        // 🧩 2️⃣ Lấy bài đăng của 1 user (tùy chọn có token + hỗ trợ phân trang)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByUser(
            string? userId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            // 1️⃣ Lấy userId mục tiêu
            var targetUserId = userId?.Trim();
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                // Nếu không có userId → thử lấy từ token
                var requesterAuto = HttpContext.Request.GetInfoRequester();
                if (requesterAuto == null)
                    return Unauthorized("Missing userId and no token provided.");
                targetUserId = requesterAuto.UserName!;
            }

            // 2️⃣ Lấy bài đăng của user đó
            var targetPostResp = await _postService.GetOneByFilterAsync(e => e.UserId == targetUserId);
            if (!targetPostResp.IsSuccess || targetPostResp.Data == null)
                return NotFound("User post not found.");

            var targetUserPost = targetPostResp.Data;

            // 3️⃣ Nếu bài đăng là công khai → ai cũng xem được
            if (targetUserPost.Accesser == Accesser.Everyone)
            {
                var pagedPosts = targetUserPost.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();

                return Ok(pagedPosts.ToDtoList());
            }

            // 4️⃣ Nếu không công khai → cần token
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Ok(new List<object>()); // chưa đăng nhập mà bài không công khai → trả về []

            var currentUserId = requester.UserName!;
            bool canView = false;

            switch (targetUserPost.Accesser)
            {
                case Accesser.OwnerOnly:
                    // chỉ chính chủ xem được
                    canView = (currentUserId == targetUserId);
                    break;

                case Accesser.FriendOnly:
                    // chính chủ hoặc bạn bè accepted mới xem được
                    if (currentUserId == targetUserId)
                    {
                        canView = true;
                    }
                    else
                    {
                        var friendsResp = await _friendService.GetAllAsync();
                        if (friendsResp.IsSuccess && friendsResp.Data != null)
                        {
                            canView = friendsResp.Data.Any(f =>
                                f.Status == "accepted" &&
                                ((f.UserId == currentUserId && f.FriendUserId == targetUserId) ||
                                 (f.UserId == targetUserId && f.FriendUserId == currentUserId))
                            );
                        }
                    }
                    break;
            }

            // 5️⃣ Kết quả cuối
            if (!canView)
                return Ok(new List<object>()); // Không đủ quyền → trả về []

            // 6️⃣ Trả về danh sách bài viết theo trang
            var filteredPosts = targetUserPost.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return Ok(filteredPosts.ToDtoList());
        }


        // 🧩 3️⃣ Xóa bài đăng (có OwnerPostId và PostId)
        [HttpDelete]
        public async Task<IActionResult> DeletePost([FromBody] DeletePostRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OwnerPostId) || string.IsNullOrWhiteSpace(request.PostId))
                return BadRequest("Invalid delete request.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token invalid.");

            var ownerResp = await _postService.GetOneByFilterAsync(e => e.UserId == request.OwnerPostId);
            if (!ownerResp.IsSuccess || ownerResp.Data == null)
                return NotFound("Owner not found.");

            int removedCount = ownerResp.Data.Posts.RemoveAll(p => p.Id == request.PostId);
            if (removedCount == 0)
                return NotFound("Post not found.");

            var updateResp = await _postService.UpdateAsync(ownerResp.Data.Id!, ownerResp.Data);
            return updateResp.IsSuccess ? NoContent() : BadRequest(updateResp.Message);
        }

        // 🧩 4️⃣ Thêm comment
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token invalid.");

            var ownerResp = await _postService.GetOneByFilterAsync( e => e.UserId == request.OwnerPostId);
            if (!ownerResp.IsSuccess || ownerResp.Data == null)
                return NotFound("Post owner not found.");

            var targetPost = ownerResp.Data.Posts.FirstOrDefault(p => p.Id == request.PostId);
            if (targetPost == null)
                return NotFound("Post not found.");

            var newComment = new CommentModel
            {
                UserId = requester.UserName!,
                Content = request.Content
            };

            targetPost.Comments.Add(newComment);

            var updateResp = await _postService.UpdateAsync(ownerResp.Data.Id!, ownerResp.Data);
            return updateResp.IsSuccess ? Ok(newComment.ToDto()) : BadRequest(updateResp.Message);
        }

        // 🧩 5️⃣ Like công khai
        [HttpPost("like")]
        public async Task<IActionResult> LikePost([FromBody] LikeRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var ownerResp = await _postService.GetOneByFilterAsync(e => e.UserId == request.OwnerPostId);
            if (!ownerResp.IsSuccess || ownerResp.Data == null)
                return NotFound("Owner not found.");

            var post = ownerResp.Data.Posts.FirstOrDefault(p => p.Id == request.PostId);
            if (post == null)
                return NotFound("Post not found.");

            post.Likes += 1;

            var updateResp = await _postService.UpdateAsync(ownerResp.Data.Id!, ownerResp.Data);
            return updateResp.IsSuccess
                ? Ok(new { message = "Liked successfully", likes = post.Likes })
                : BadRequest(updateResp.Message);
        }

        // 🧩 6️⃣ Like ẩn danh
        [HttpPost("secret-like")]
        public async Task<IActionResult> SecretLikePost([FromBody] SecretLikeRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var ownerResp = await _postService.GetOneByFilterAsync(e => e.UserId == request.OwnerPostId);
            if (!ownerResp.IsSuccess || ownerResp.Data == null)
                return NotFound("Owner not found.");

            var post = ownerResp.Data.Posts.FirstOrDefault(p => p.Id == request.PostId);
            if (post == null)
                return NotFound("Post not found.");

            post.SecretLikes += 1;

            var updateResp = await _postService.UpdateAsync(ownerResp.Data.Id!, ownerResp.Data);
            return updateResp.IsSuccess
                ? Ok(new { message = "Secret like added", secret_like = post.SecretLikes })
                : BadRequest(updateResp.Message);
        }

        // 🧩 7️⃣ Lấy feed
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized("Token invalid.");

            string userId = requester.UserName!;

            var friendsResp = await _friendService.GetAllAsync();
            if (!friendsResp.IsSuccess || friendsResp.Data == null)
                return BadRequest(friendsResp.Message);

            var friendIds = friendsResp.Data
                .Where(f => f.Status == "accepted" && (f.UserId == userId || f.FriendUserId == userId))
                .Select(f => f.UserId == userId ? f.FriendUserId : f.UserId)
                .Distinct()
                .Append(userId)
                .ToHashSet();

            var postsResp = await _postService.GetAllAsync();
            if (!postsResp.IsSuccess || postsResp.Data == null)
                return BadRequest(postsResp.Message);

            var feedPosts = postsResp.Data
                .Where(up => friendIds.Contains(up.UserId))
                .SelectMany(up => up.Posts.Select(p => new
                {
                    Post = p,
                    OwnerId = up.UserId,
                    up.Accesser
                }))
                .Where(x =>
                    x.Accesser == Accesser.Everyone ||
                    (x.Accesser == Accesser.FriendOnly && x.OwnerId != userId) ||
                    (x.OwnerId == userId))
                .Select(x =>
                {
                    x.Post.UserId = x.OwnerId;
                    return x.Post;
                })
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToList();

            Shuffle(feedPosts);
            return Ok(feedPosts.ToDtoList());
        }

        // 🔄 Fisher–Yates shuffle
        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
