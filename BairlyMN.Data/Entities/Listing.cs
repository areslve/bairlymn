using BairlyMN.Domain.Enums;
using Microsoft.VisualBasic;

namespace BairlyMN.Data.Entities;

public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PropertyType PropertyType { get; set; }
    public TransactionType TransactionType { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public decimal? Area { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsPromoted { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? LocationNodeId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Category? Category { get; set; }
    public LocationNode? LocationNode { get; set; }
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ApartmentDetails? ApartmentDetails { get; set; }
    public HouseDetails? HouseDetails { get; set; }
    public LandDetails? LandDetails { get; set; }
}