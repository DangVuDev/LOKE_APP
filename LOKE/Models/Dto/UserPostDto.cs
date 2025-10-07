using LOKE.Models.Model;

namespace LOKE.Models.Dto
{
    public class UserPostDto
    {
        public string UserId { get; set; } = string.Empty;
        public Accesser Accesser { get; set; } = Accesser.Everyone;
        public List<PostDto> Posts { get; set; } = [];
    }
}
