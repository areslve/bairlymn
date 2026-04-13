namespace BairlyMN.Data.Entities;

public class HouseDetails
{
    public int Id { get; set; }
    public int ListingId { get; set; }

    public int? Rooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floors { get; set; }
    public decimal? LandArea { get; set; }
    public int? BuildYear { get; set; }
    public string? Condition { get; set; }
    public string? BuildingType { get; set; }  // Барилгын төрөл
    public string? RoofType { get; set; }      // Дээврийн төрөл
    public string? HeatingType { get; set; }   // Халаалтын төрөл
    public string? FloorMaterial { get; set; }

    public bool HasGarage { get; set; }
    public bool HasGarden { get; set; }
    public bool HasFence { get; set; }
    public bool HasBasement { get; set; }      // Подвал
    public bool HasSauna { get; set; }         // Ус халаах/Сауна
    public bool IsFurnished { get; set; }
    public bool HasAircon { get; set; }

    public Listing Listing { get; set; } = null!;
}