namespace LOKE.Models.Dto
{
    public class FriendDto
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string FriendUserId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ProfileImageUrl { get; set; } = default!;
        public string Status { get; set; } = default!;
    }
}
