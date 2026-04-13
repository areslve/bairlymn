namespace BairlyMN.Data.Entities;

public class LandDetails
{
    public int Id { get; set; }
    public int ListingId { get; set; }

    public string? ZoningType { get; set; }
    public bool HasRoadAccess { get; set; }
    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasSewer { get; set; }
    public bool HasGas { get; set; }           // Хий
    public bool HasFence { get; set; }         // Хашаа
    public string? CadastralNumber { get; set; }
    public string? LandUsePurpose { get; set; }
    public string? LandOwnershipType { get; set; } // Эрхийн төрөл (Эзэмших/Өмчлөх)
    public decimal? LandSlope { get; set; }    // Налуу хэмжээ (%)

    public Listing Listing { get; set; } = null!;
}