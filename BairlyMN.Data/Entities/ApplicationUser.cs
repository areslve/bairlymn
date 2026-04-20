// BairlyMN.Data/Entities/ApplicationUser.cs

using Microsoft.AspNetCore.Identity;

namespace BairlyMN.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarPath { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Agent ─────────────────────────────────────────────────────
    // Одоогийн идэвхтэй subscription байгаа эсэх (хурдан шалгахад)
    public bool IsAgent { get; set; } = false;
    public DateTime? AgentUntil { get; set; }

    // ── Navigation ────────────────────────────────────────────────
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> ConversationsAsInitiator { get; set; } = new List<Conversation>();
    public ICollection<Conversation> ConversationsAsParticipant { get; set; } = new List<Conversation>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public ICollection<AgentSubscription> AgentSubscriptions { get; set; } = new List<AgentSubscription>();
    public ICollection<AgentRequest> AgentRequests { get; set; } = new List<AgentRequest>();

    // ── Computed ──────────────────────────────────────────────────
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => string.IsNullOrWhiteSpace(FullName)
        ? (UserName ?? Email ?? "Хэрэглэгч") : FullName;

    // Одоо agent эрхтэй эсэхийг шалгах
    public bool HasActiveAgent => IsAgent && AgentUntil.HasValue && AgentUntil.Value >= DateTime.UtcNow;
}