using Core.Model.Base;
using Microsoft.VisualBasic;

namespace LOKE.Models.Model
{
    public class ConversationModel : BaseModel
    {
        public string UserAId { get; set; } = null!;
        public string UserBId { get; set; } = null!;
        public List<MessageContent> Messages { get; set; } = [];
    }

    public class MessageContent : BaseModel
    {
        public string SenderId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Type { get; set; } = "text";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    
}
