namespace BairlyMN.Data.Entities;

public class Banner
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int Height { get; set; } = 120;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}