using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Reflection;

namespace BairlyMN.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarPath { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> ConversationsAsInitiator { get; set; } = new List<Conversation>();
    public ICollection<Conversation> ConversationsAsParticipant { get; set; } = new List<Conversation>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => string.IsNullOrWhiteSpace(FullName)
        ? (UserName ?? Email ?? "Хэрэглэгч") : FullName;
}