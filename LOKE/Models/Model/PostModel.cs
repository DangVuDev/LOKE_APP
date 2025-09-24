using Core.Model.Base;

namespace LOKE.Models.Model
{
    public class PostModel : BaseModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Likes { get; set; } = 0;
        public int SecretLikes { get; set; } = 0;
        public List<CommentModel> Comments { get; set; } = [];
    }

    public class CommentModel : BaseModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
