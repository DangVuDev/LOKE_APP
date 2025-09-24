using Core.Model.Base;

namespace LOKE.Models.Model
{
    public class FriendModel : BaseModel
    {
        public string UserId { get; set; } = default!;
        public string FriendUserId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ProfileImageUrl { get; set; } = default!;
        public string Status { get; set; } = "pending"; // pending, accepted, rejected
    }
}
