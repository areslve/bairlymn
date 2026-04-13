using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

public class ListingsController : Controller
{
    private readonly IListingService _listingService;
    private readonly IFileUploadService _fileUpload;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ListingsController(
        IListingService listingService,
        IFileUploadService fileUpload,
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _listingService = listingService;
        _fileUpload = fileUpload;
        _db = db;
        _userManager = userManager;
    }

    // GET /Listings
    public async Task<IActionResult> Index(ListingFilterViewModel filter)
    {
        filter.Page ??= 1;
        filter.PageSize ??= 12;

        var (items, total) = await _listingService.GetPagedListingsAsync(
            filter.Page.Value, filter.PageSize.Value,
            filter.Search, filter.PropertyType, filter.TransactionType,
            filter.LocationNodeId, filter.MinPrice, filter.MaxPrice,
            filter.CategoryId, filter.MinArea, filter.MaxArea,
            filter.MinRooms, filter.MaxRooms);

        // Идэвхтэй баннеруудыг авах
        var banners = await _db.Banners
            .Where(b => b.IsActive
                && (b.StartsAt == null || b.StartsAt <= DateTime.UtcNow)
                && (b.EndsAt == null || b.EndsAt >= DateTime.UtcNow))
            .OrderBy(b => b.SortOrder)
            .ToListAsync();

        ViewBag.Banners = banners;

        return View(new ListingIndexViewModel
        {
            Listings = items,
            Filter = filter,
            Pagination = new PaginationViewModel
            {
                CurrentPage = filter.Page.Value,
                PageSize = filter.PageSize.Value,
                TotalCount = total
            },
            Categories = await GetCategoriesAsync(),
            Locations = await GetLocationsAsync()
        });
    }

    // GET /Listings/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null) return NotFound();

        await _listingService.IncrementViewCountAsync(id);

        var userId = _userManager.GetUserId(User);
        var isFavorited = userId != null
            && await _listingService.IsFavoritedByUserAsync(id, userId);

        var (related, _) = await _listingService.GetPagedListingsAsync(
            page: 1, pageSize: 4,
            propertyType: listing.PropertyType,
            locationNodeId: listing.LocationNodeId,
            transactionType: listing.TransactionType);

        var relatedFiltered = related
            .Where(r => r.Id != id)
            .Take(3)
            .ToList();

        ViewBag.RelatedListings = relatedFiltered;

        return View(new ListingDetailViewModel
        {
            Listing = listing,
            IsFavorited = isFavorited,
            IsOwner = userId == listing.UserId,
            IsAdmin = User.IsInRole("Admin")
        });
    }

    // GET /Listings/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        return View(new ListingCreateViewModel
        {
            Categories = await GetCategoriesAsync(),
            Locations = await GetLocationsAsync()
        });
    }

    // POST /Listings/Create
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ListingCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategoriesAsync();
            vm.Locations = await GetLocationsAsync();
            return View(vm);
        }

        var userId = _userManager.GetUserId(User)!;

        var listing = new Listing
        {
            Title = vm.Title,
            Description = vm.Description,
            Price = vm.Price,
            PropertyType = vm.PropertyType,
            TransactionType = vm.TransactionType,
            Area = vm.Area,
            Address = vm.Address,
            CategoryId = vm.CategoryId,
            LocationNodeId = vm.LocationNodeId,
            UserId = userId,
            Status = ListingStatus.Active
        };

        MapPropertyDetails(listing, vm);

        var id = await _listingService.CreateListingAsync(listing);

        if (vm.Images?.Count > 0)
            await SaveImagesAsync(id, vm.Images);

        TempData["Success"] = "Зар амжилттай нэмэгдлээ!";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET /Listings/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        var vm = new ListingEditViewModel
        {
            Id = id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            PropertyType = listing.PropertyType,
            TransactionType = listing.TransactionType,
            Area = listing.Area,
            Address = listing.Address,
            CategoryId = listing.CategoryId,
            LocationNodeId = listing.LocationNodeId,
            ExistingImages = listing.Images.OrderBy(i => i.SortOrder).ToList(),
            Categories = await GetCategoriesAsync(),
            Locations = await GetLocationsAsync()
        };

        if (listing.ApartmentDetails is { } ad)
            vm.ApartmentDetails = MapFromEntity(ad);

        if (listing.HouseDetails is { } hd)
            vm.HouseDetailsVM = MapFromEntity(hd);

        if (listing.LandDetails is { } ld)
            vm.LandDetailsVM = MapFromEntity(ld);

        return View(vm);
    }

    // POST /Listings/Edit/5
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ListingEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategoriesAsync();
            vm.Locations = await GetLocationsAsync();
            vm.ExistingImages = await _db.ListingImages
                .Where(i => i.ListingId == id)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
            return View(vm);
        }

        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        listing.Title = vm.Title;
        listing.Description = vm.Description;
        listing.Price = vm.Price;
        listing.PropertyType = vm.PropertyType;
        listing.TransactionType = vm.TransactionType;
        listing.Area = vm.Area;
        listing.Address = vm.Address;
        listing.CategoryId = vm.CategoryId;
        listing.LocationNodeId = vm.LocationNodeId;

        if (vm.ImageIdsToDelete?.Count > 0)
        {
            var toDelete = await _db.ListingImages
                .Where(i => vm.ImageIdsToDelete.Contains(i.Id) && i.ListingId == id)
                .ToListAsync();
            foreach (var img in toDelete)
            {
                _fileUpload.DeleteFile(img.ImagePath);
                _db.ListingImages.Remove(img);
            }
        }

        if (vm.Images?.Count > 0)
            await SaveImagesAsync(id, vm.Images);

        if (listing.ApartmentDetails != null && listing.PropertyType != PropertyType.Apartment)
        { _db.ApartmentDetails.Remove(listing.ApartmentDetails); listing.ApartmentDetails = null; }
        if (listing.HouseDetails != null && listing.PropertyType != PropertyType.House)
        { _db.HouseDetails.Remove(listing.HouseDetails); listing.HouseDetails = null; }
        if (listing.LandDetails != null && listing.PropertyType != PropertyType.Land)
        { _db.LandDetails.Remove(listing.LandDetails); listing.LandDetails = null; }

        MapPropertyDetails(listing, vm);

        await _listingService.UpdateListingAsync(listing);

        TempData["Success"] = "Зар амжилттай шинэчлэгдлээ!";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Listings/Delete/5
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (listing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        foreach (var img in listing.Images)
            _fileUpload.DeleteFile(img.ImagePath);

        await _listingService.DeleteListingAsync(id);
        TempData["Success"] = "Зар устгагдлаа.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Listings/ToggleFavorite/5
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        var added = await _listingService.ToggleFavoriteAsync(id, uid);
        return Json(new
        {
            added,
            message = added ? "Хадгалалтад нэмэгдлээ" : "Хадгалалтаас хасагдлаа"
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task SaveImagesAsync(int listingId, List<IFormFile> files)
    {
        int order = await _db.ListingImages.CountAsync(i => i.ListingId == listingId);
        foreach (var f in files)
        {
            if (!_fileUpload.IsValidImageFile(f)) continue;
            var path = await _fileUpload.UploadListingImageAsync(f);
            _db.ListingImages.Add(new ListingImage
            {
                ListingId = listingId,
                ImagePath = path,
                IsPrimary = order == 0,
                SortOrder = order++
            });
        }
        await _db.SaveChangesAsync();
    }

    private static void MapPropertyDetails(Listing listing, ListingCreateViewModel vm)
    {
        switch (vm.PropertyType)
        {
            case PropertyType.Apartment when vm.ApartmentDetails != null:
                var ad = vm.ApartmentDetails;
                if (listing.ApartmentDetails == null)
                    listing.ApartmentDetails = new ApartmentDetails { ListingId = listing.Id };

                var apt = listing.ApartmentDetails;
                apt.Floor = ad.Floor;
                apt.TotalFloors = ad.TotalFloors;
                apt.Rooms = ad.Rooms;
                apt.Bathrooms = ad.Bathrooms;
                apt.LivingRooms = ad.LivingRooms;
                apt.KitchenArea = ad.KitchenArea;
                apt.LivingArea = ad.LivingArea;
                apt.BalconyArea = ad.BalconyArea;
                apt.BuildYear = ad.BuildYear;
                apt.Condition = ad.Condition;
                apt.BuildingType = ad.BuildingType;
                apt.WindowType = ad.WindowType;
                apt.FloorMaterial = ad.FloorMaterial;
                apt.HeatingType = ad.HeatingType;
                apt.DoorsCount = ad.DoorsCount;
                apt.HasBalcony = ad.HasBalcony;
                apt.HasParking = ad.HasParking;
                apt.HasElevator = ad.HasElevator;
                apt.HasStorage = ad.HasStorage;
                apt.HasSecurity = ad.HasSecurity;
                apt.IsFurnished = ad.IsFurnished;
                apt.HasAircon = ad.HasAircon;
                apt.AllowsPets = ad.AllowsPets;
                apt.AllowsSmoking = ad.AllowsSmoking;
                apt.MinRentMonths = ad.MinRentMonths;
                apt.IsSharedApartment = ad.IsSharedApartment;
                apt.AvailableRooms = ad.AvailableRooms;
                break;

            case PropertyType.House when vm.HouseDetailsVM != null:
                var hd = vm.HouseDetailsVM;
                if (listing.HouseDetails == null)
                    listing.HouseDetails = new HouseDetails { ListingId = listing.Id };

                var house = listing.HouseDetails;
                house.Rooms = hd.Rooms;
                house.Bathrooms = hd.Bathrooms;
                house.Floors = hd.Floors;
                house.LandArea = hd.LandArea;
                house.BuildYear = hd.BuildYear;
                house.Condition = hd.Condition;
                house.BuildingType = hd.BuildingType;
                house.RoofType = hd.RoofType;
                house.HeatingType = hd.HeatingType;
                house.FloorMaterial = hd.FloorMaterial;
                house.HasGarage = hd.HasGarage;
                house.HasGarden = hd.HasGarden;
                house.HasFence = hd.HasFence;
                house.HasBasement = hd.HasBasement;
                house.HasSauna = hd.HasSauna;
                house.IsFurnished = hd.IsFurnished;
                house.HasAircon = hd.HasAircon;
                break;

            case PropertyType.Land when vm.LandDetailsVM != null:
                var ld = vm.LandDetailsVM;
                if (listing.LandDetails == null)
                    listing.LandDetails = new LandDetails { ListingId = listing.Id };

                var land = listing.LandDetails;
                land.ZoningType = ld.ZoningType;
                land.HasRoadAccess = ld.HasRoadAccess;
                land.HasElectricity = ld.HasElectricity;
                land.HasWater = ld.HasWater;
                land.HasSewer = ld.HasSewer;
                land.HasGas = ld.HasGas;
                land.HasFence = ld.HasFence;
                land.CadastralNumber = ld.CadastralNumber;
                land.LandUsePurpose = ld.LandUsePurpose;
                land.LandOwnershipType = ld.LandOwnershipType;
                land.LandSlope = ld.LandSlope;
                break;
        }
    }

    private static ApartmentDetailsViewModel MapFromEntity(ApartmentDetails e) => new()
    {
        Floor = e.Floor,
        TotalFloors = e.TotalFloors,
        Rooms = e.Rooms,
        Bathrooms = e.Bathrooms,
        LivingRooms = e.LivingRooms,
        KitchenArea = e.KitchenArea,
        LivingArea = e.LivingArea,
        BalconyArea = e.BalconyArea,
        BuildYear = e.BuildYear,
        Condition = e.Condition,
        BuildingType = e.BuildingType,
        WindowType = e.WindowType,
        FloorMaterial = e.FloorMaterial,
        HeatingType = e.HeatingType,
        DoorsCount = e.DoorsCount,
        HasBalcony = e.HasBalcony,
        HasParking = e.HasParking,
        HasElevator = e.HasElevator,
        HasStorage = e.HasStorage,
        HasSecurity = e.HasSecurity,
        IsFurnished = e.IsFurnished,
        HasAircon = e.HasAircon,
        AllowsPets = e.AllowsPets,
        AllowsSmoking = e.AllowsSmoking,
        MinRentMonths = e.MinRentMonths,
        IsSharedApartment = e.IsSharedApartment,
        AvailableRooms = e.AvailableRooms
    };

    private static HouseDetailsViewModel MapFromEntity(HouseDetails e) => new()
    {
        Rooms = e.Rooms,
        Bathrooms = e.Bathrooms,
        Floors = e.Floors,
        LandArea = e.LandArea,
        BuildYear = e.BuildYear,
        Condition = e.Condition,
        BuildingType = e.BuildingType,
        RoofType = e.RoofType,
        HeatingType = e.HeatingType,
        FloorMaterial = e.FloorMaterial,
        HasGarage = e.HasGarage,
        HasGarden = e.HasGarden,
        HasFence = e.HasFence,
        HasBasement = e.HasBasement,
        HasSauna = e.HasSauna,
        IsFurnished = e.IsFurnished,
        HasAircon = e.HasAircon
    };

    private static LandDetailsViewModel MapFromEntity(LandDetails e) => new()
    {
        ZoningType = e.ZoningType,
        HasRoadAccess = e.HasRoadAccess,
        HasElectricity = e.HasElectricity,
        HasWater = e.HasWater,
        HasSewer = e.HasSewer,
        HasGas = e.HasGas,
        HasFence = e.HasFence,
        CadastralNumber = e.CadastralNumber,
        LandUsePurpose = e.LandUsePurpose,
        LandOwnershipType = e.LandOwnershipType,
        LandSlope = e.LandSlope
    };

    private async Task<IEnumerable<SelectListItem>> GetCategoriesAsync()
        => (await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync())
           .Select(c => new SelectListItem(c.Name, c.Id.ToString()));

    private async Task<IEnumerable<SelectListItem>> GetLocationsAsync()
        => (await _db.LocationNodes
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync())
           .Select(l => new SelectListItem(l.Name, l.Id.ToString()));
}