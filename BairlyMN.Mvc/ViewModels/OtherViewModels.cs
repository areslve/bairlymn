using BairlyMN.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BairlyMN.Mvc.ViewModels;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public IEnumerable<Listing> Listings { get; set; } = Enumerable.Empty<Listing>();
    public IEnumerable<Listing> Favorites { get; set; } = Enumerable.Empty<Listing>();
    public IEnumerable<Conversation> Conversations { get; set; } = Enumerable.Empty<Conversation>();
}

public class EditProfileViewModel
{
    [MaxLength(100)] public string? FirstName { get; set; }
    [MaxLength(100)] public string? LastName { get; set; }
    [MaxLength(500)] public string? Bio { get; set; }
    [Phone] public string? PhoneNumber { get; set; }
    public IFormFile? AvatarFile { get; set; }
    public string? CurrentAvatarPath { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalListings { get; set; }
    public int TotalBanners { get; set; }
    public int ActiveListings { get; set; }
    public int ActiveBanners { get; set; }
    public int TotalChats { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalViews { get; set; }

    public Dictionary<string, int> Stats { get; set; } = new();

    public IEnumerable<Listing> RecentListings { get; set; } = Enumerable.Empty<Listing>();
    public IEnumerable<ApplicationUser> RecentUsers { get; set; } = Enumerable.Empty<ApplicationUser>();
}

public class AdminListingsViewModel
{
    public IEnumerable<Listing> Listings { get; set; } = Enumerable.Empty<Listing>();
    public PaginationViewModel Pagination { get; set; } = new();
    public string? Search { get; set; }
}

public class ChatViewModel
{
    public IEnumerable<Conversation> Conversations { get; set; } = Enumerable.Empty<Conversation>();
    public Conversation? ActiveConversation { get; set; }
    public IEnumerable<ChatMessage> Messages { get; set; } = Enumerable.Empty<ChatMessage>();
    public string CurrentUserId { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }          // ← нэмэх
    public string SentAt { get; set; } = string.Empty;
}

public class HomeViewModel
{
    public IEnumerable<Listing> FeaturedListings { get; set; } = Enumerable.Empty<Listing>();
    public IEnumerable<Listing> LatestListings { get; set; } = Enumerable.Empty<Listing>();
    public IEnumerable<Banner> ActiveBanners { get; set; } = Enumerable.Empty<Banner>();
    public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();
}