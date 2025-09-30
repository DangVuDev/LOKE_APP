namespace LOKE.Models.Dto
{
    public class ConversationDto
    {
        public string Id { get; set; } = null!;
        public string UserA { get; set; } = null!;
        public string UserB { get; set; } = null!;
        public List<MessageContentDto> Messages { get; set; } = [];
    }

    public class CreateConversationDto
    {
        public string OtherId { get; set; } = null!;
    }


    public class MessageContentDto
    {
        public string Id { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Type { get; set; } = "text";
        public DateTime SentAt { get; set; }
    }

    public class CreateMessageDto
    {
        public string ConversationId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Type { get; set; } = "text";
    }
}
