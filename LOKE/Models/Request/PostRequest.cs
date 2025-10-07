namespace LOKE.Models.Request
{
    // 🧩 Tạo bài đăng mới
    public class PostRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    // 💬 Thêm comment vào bài đăng
    public class CommentRequest
    {
        /// <summary>Id của người sở hữu bài đăng (UserPostModel.Id hoặc UserId)</summary>
        public string OwnerPostId { get; set; } = string.Empty;

        /// <summary>Id của bài đăng cần comment (Post.Id)</summary>
        public string PostId { get; set; } = string.Empty;

        /// <summary>Nội dung comment</summary>
        public string Content { get; set; } = string.Empty;
    }

    // ❤️ Like công khai
    public class LikeRequest
    {
        /// <summary>Id của người sở hữu bài đăng</summary>
        public string OwnerPostId { get; set; } = string.Empty;

        /// <summary>Id của bài đăng cần like</summary>
        public string PostId { get; set; } = string.Empty;
    }

    // 🕵️‍♂️ Like ẩn danh
    public class SecretLikeRequest
    {
        public string OwnerPostId { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
    }

    // 🗑️ Xóa bài đăng
    public class DeletePostRequest
    {
        public string OwnerPostId { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
    }
}
