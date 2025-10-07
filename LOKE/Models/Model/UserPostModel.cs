using Core.Model.Base;

namespace LOKE.Models.Model
{
    public class UserPostModel : BaseModel
    {
        public string UserId { get; set; } = string.Empty;
        public Accesser Accesser { get; set; } = Accesser.Everyone;
        public List<PostModel> Posts { get; set; } = [];
    }

    public enum Accesser
    {
        OwnerOnly = 0,
        FriendOnly = 1,
        Everyone = 2
    }
}
