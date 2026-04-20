using BairlyMN.Domain.Enums;

namespace BairlyMN.Data.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }          // ← нэмэх
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}