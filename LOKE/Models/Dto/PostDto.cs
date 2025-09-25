namespace LOKE.Models.Dto
{
    public class CommentDto
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class PostDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public int SecretLikes { get; set; }
        public List<CommentDto> Comments { get; set; } = [];
    }
}
