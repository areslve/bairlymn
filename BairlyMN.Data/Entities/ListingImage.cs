namespace BairlyMN.Data.Entities;

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Listing Listing { get; set; } = null!;
}