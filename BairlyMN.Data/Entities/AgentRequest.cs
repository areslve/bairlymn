

namespace BairlyMN.Data.Entities;

public enum AgentRequestStatus
{
    Pending = 0,   // Хүлээгдэж байна
    Approved = 1,   // Зөвшөөрсөн
    Rejected = 2    // Татгалзсан
}

public class AgentRequest
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public AgentRequestStatus Status { get; set; } = AgentRequestStatus.Pending;

    // Хэрэглэгчийн мессеж
    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Admin хариу өгсөн
    public string? ReviewedByAdminId { get; set; }
    public ApplicationUser? ReviewedByAdmin { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNote { get; set; }

    // Холбогдох conversation (admin-тай чат)
    public int? ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
}