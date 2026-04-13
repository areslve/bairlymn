using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BairlyMN.Mvc.ViewModels;

// ── Index / Filter ────────────────────────────────────────────────────────────
public class ListingIndexViewModel
{
    public IEnumerable<Listing> Listings { get; set; } = Enumerable.Empty<Listing>();
    public ListingFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class ListingFilterViewModel
{
    public string? Search { get; set; }
    public PropertyType? PropertyType { get; set; }
    public TransactionType? TransactionType { get; set; }
    public int? LocationNodeId { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public int? MinRooms { get; set; }
    public int? MaxRooms { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 12;
}

public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

// ── Create / Edit ─────────────────────────────────────────────────────────────
public class ListingCreateViewModel
{
    [Required(ErrorMessage = "Гарчиг оруулна уу")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Тайлбар оруулна уу")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Үнэ оруулна уу")]
    [Range(0, 9_999_999_999, ErrorMessage = "Үнэ буруу байна")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Үл хөдлөхийн төрөл сонгоно уу")]
    public PropertyType PropertyType { get; set; }

    [Required(ErrorMessage = "Гүйлгээний төрөл сонгоно уу")]
    public TransactionType TransactionType { get; set; }

    [Range(0, 100000, ErrorMessage = "Талбай буруу байна")]
    public decimal? Area { get; set; }

    public string? Address { get; set; }
    public int? CategoryId { get; set; }
    public int? LocationNodeId { get; set; }

    public ApartmentDetailsViewModel? ApartmentDetails { get; set; }
    public HouseDetailsViewModel? HouseDetailsVM { get; set; }
    public LandDetailsViewModel? LandDetailsVM { get; set; }

    public List<IFormFile>? Images { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class ListingEditViewModel : ListingCreateViewModel
{
    public int Id { get; set; }
    public List<ListingImage> ExistingImages { get; set; } = new();
    public List<int> ImageIdsToDelete { get; set; } = new();
}

public class ListingDetailViewModel
{
    public Listing Listing { get; set; } = null!;
    public bool IsFavorited { get; set; }
    public bool IsOwner { get; set; }
    public bool IsAdmin { get; set; }
}

// ── Property Detail ViewModels ────────────────────────────────────────────────
public class ApartmentDetailsViewModel
{
    // Байршил
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }

    // Өрөөнүүд
    public int? Rooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? LivingRooms { get; set; }

    // Талбайнууд
    public decimal? KitchenArea { get; set; }
    public decimal? LivingArea { get; set; }
    public decimal? BalconyArea { get; set; }

    // Барилга
    public int? BuildYear { get; set; }
    public string? Condition { get; set; }
    public string? BuildingType { get; set; }

    // Дотоод
    public string? WindowType { get; set; }
    public string? FloorMaterial { get; set; }
    public string? HeatingType { get; set; }
    public int? DoorsCount { get; set; }

    // Тохиромж
    public bool HasBalcony { get; set; }
    public bool HasParking { get; set; }
    public bool HasElevator { get; set; }
    public bool HasStorage { get; set; }
    public bool HasSecurity { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasAircon { get; set; }

    // Түрээс
    public bool? AllowsPets { get; set; }
    public bool? AllowsSmoking { get; set; }
    public int? MinRentMonths { get; set; }

    // Хуваалцсан
    public bool IsSharedApartment { get; set; }
    public int? AvailableRooms { get; set; }
}

public class HouseDetailsViewModel
{
    public int? Rooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floors { get; set; }
    public decimal? LandArea { get; set; }
    public int? BuildYear { get; set; }
    public string? Condition { get; set; }
    public string? BuildingType { get; set; }
    public string? RoofType { get; set; }
    public string? HeatingType { get; set; }
    public string? FloorMaterial { get; set; }
    public bool HasGarage { get; set; }
    public bool HasGarden { get; set; }
    public bool HasFence { get; set; }
    public bool HasBasement { get; set; }
    public bool HasSauna { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasAircon { get; set; }
}

public class LandDetailsViewModel
{
    public string? ZoningType { get; set; }
    public bool HasRoadAccess { get; set; }
    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasSewer { get; set; }
    public bool HasGas { get; set; }
    public bool HasInternet { get; set; }
    public bool HasFence { get; set; }
    public string? CadastralNumber { get; set; }
    public string? LandUsePurpose { get; set; }
    public string? LandOwnershipType { get; set; }
    public decimal? LandSlope { get; set; }
}