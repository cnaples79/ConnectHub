using System.ComponentModel.DataAnnotations;

namespace ConnectHub.Shared.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public UserDto Sender { get; set; }
        public UserDto Receiver { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ChatThreadDto
    {
        public UserDto OtherUser { get; set; }
        public ChatMessageDto LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}
