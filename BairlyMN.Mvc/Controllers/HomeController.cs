using BairlyMN.Data;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly IListingService _listings;
    private readonly ApplicationDbContext _db;

    public HomeController(IListingService listings, ApplicationDbContext db)
    {
        _listings = listings;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var (latest, _) = await _listings.GetPagedListingsAsync(1, 8);

        var banners = await _db.Banners
            .Where(b => b.IsActive
                && (b.StartsAt == null || b.StartsAt <= DateTime.UtcNow)
                && (b.EndsAt == null || b.EndsAt >= DateTime.UtcNow))
            .OrderBy(b => b.SortOrder)
            .ToListAsync();

        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return View(new HomeViewModel
        {
            LatestListings = latest,
            ActiveBanners = banners,
            Categories = categories
        });
    }

    [Route("Home/Error/{statusCode?}")]
    public IActionResult Error(int? statusCode = null)
    {
        if (statusCode == 404)
            return View("NotFound");
        return View("Error");
    }
}