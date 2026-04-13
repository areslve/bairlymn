namespace BairlyMN.Data.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int? ListingId { get; set; }
    public string InitiatorId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public Listing? Listing { get; set; }
    public ApplicationUser Initiator { get; set; } = null!;
    public ApplicationUser Participant { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}