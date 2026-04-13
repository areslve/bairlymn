using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IListingService _listings;
    private readonly IFileUploadService _fileUpload;
    private readonly UserManager<ApplicationUser> _users;

    public AdminController(
        ApplicationDbContext db,
        IListingService listings,
        IFileUploadService fileUpload,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _listings = listings;
        _fileUpload = fileUpload;
        _users = users;
    }

    // ── DASHBOARD ──────────────────────────────────────────────────
    [HttpGet("/Admin")]
    [HttpGet("/Admin/Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _listings.GetAdminStatsAsync();

        var recentListings = await _db.Listings
            .Include(l => l.User)
            .Include(l => l.Images)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentUsers = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(new AdminDashboardViewModel
        {
            Stats = stats,
            RecentListings = recentListings,
            RecentUsers = recentUsers
        });
    }

    // ── LISTINGS ───────────────────────────────────────────────────
    [HttpGet("/Admin/Listings")]
    public async Task<IActionResult> Listings(string? search, int page = 1)
    {
        const int size = 20;
        var q = _db.Listings
            .Include(l => l.User)
            .Include(l => l.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(l => l.Title.Contains(search) ||
                             l.User.Email!.Contains(search));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return View(new AdminListingsViewModel
        {
            Listings = items,
            Search = search,
            Pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = size,
                TotalCount = total
            }
        });
    }

    [HttpPost("/Admin/ToggleListingStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleListingStatus(int id)
    {
        var l = await _db.Listings.FindAsync(id);
        if (l == null) return NotFound();
        l.Status = l.Status == ListingStatus.Active
            ? ListingStatus.Inactive
            : ListingStatus.Active;
        await _db.SaveChangesAsync();
        return Json(new { status = l.Status.ToString() });
    }

    [HttpPost("/Admin/DeleteListing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteListing(int id)
    {
        var l = await _db.Listings
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (l == null) return NotFound();

        foreach (var img in l.Images)
            _fileUpload.DeleteFile(img.ImagePath);

        _db.Listings.Remove(l);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Зар устгагдлаа.";
        return RedirectToAction(nameof(Listings));
    }

    // ══════════════════════════════════════════════════════════════
    // BANNER CRUD
    // ══════════════════════════════════════════════════════════════

    [HttpGet("/Admin/Banners")]
    public async Task<IActionResult> Banners()
    {
        var banners = await _db.Banners
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(banners);
    }

    [HttpGet("/Admin/Banners/Create")]
    public IActionResult CreateBanner()
        => View(new BannerCreateViewModel());

    [HttpPost("/Admin/Banners/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBanner(BannerCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.ImageFile == null || vm.ImageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Зураг заавал оруулна уу");
            return View(vm);
        }

        if (!_fileUpload.IsValidImageFile(vm.ImageFile))
        {
            ModelState.AddModelError("ImageFile", "Зөвхөн .jpg .png .webp зургийн файл зөвшөөрнө");
            return View(vm);
        }

        var imagePath = await _fileUpload.UploadBannerAsync(vm.ImageFile);

        _db.Banners.Add(new Banner
        {
            Title = vm.Title,
            Subtitle = vm.Subtitle,
            LinkUrl = vm.LinkUrl,
            ImagePath = imagePath,
            IsActive = vm.IsActive,
            SortOrder = vm.SortOrder,
            Height = (int)vm.Height,
            StartsAt = vm.StartsAt?.ToUniversalTime(),
            EndsAt = vm.EndsAt?.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Баннер амжилттай нэмэгдлээ!";
        return RedirectToAction(nameof(Banners));
    }

    [HttpGet("/Admin/Banners/Edit/{id:int}")]
    public async Task<IActionResult> EditBanner(int id)
    {
        var b = await _db.Banners.FindAsync(id);
        if (b == null) return NotFound();

        return View(new BannerEditViewModel
        {
            Id = id,
            Title = b.Title,
            Subtitle = b.Subtitle,
            LinkUrl = b.LinkUrl,
            IsActive = b.IsActive,
            SortOrder = b.SortOrder,
            Height = (BannerHeight)(b.Height == 0 ? 120 : b.Height),
            StartsAt = b.StartsAt?.ToLocalTime(),
            EndsAt = b.EndsAt?.ToLocalTime(),
            CurrentImagePath = b.ImagePath
        });
    }

    [HttpPost("/Admin/Banners/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBanner(int id, BannerEditViewModel vm)
    {
        ModelState.Remove("ImageFile");
        if (!ModelState.IsValid) return View(vm);

        var b = await _db.Banners.FindAsync(id);
        if (b == null) return NotFound();

        b.Title = vm.Title;
        b.Subtitle = vm.Subtitle;
        b.LinkUrl = vm.LinkUrl;
        b.IsActive = vm.IsActive;
        b.SortOrder = vm.SortOrder;
        b.Height = (int)vm.Height;
        b.StartsAt = vm.StartsAt?.ToUniversalTime();
        b.EndsAt = vm.EndsAt?.ToUniversalTime();

        if (vm.ImageFile != null && vm.ImageFile.Length > 0)
        {
            if (!_fileUpload.IsValidImageFile(vm.ImageFile))
            {
                ModelState.AddModelError("ImageFile", "Зөвхөн .jpg .png .webp зургийн файл зөвшөөрнө");
                return View(vm);
            }
            if (!string.IsNullOrEmpty(b.ImagePath))
                _fileUpload.DeleteFile(b.ImagePath);

            b.ImagePath = await _fileUpload.UploadBannerAsync(vm.ImageFile);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Баннер шинэчлэгдлээ!";
        return RedirectToAction(nameof(Banners));
    }

    [HttpPost("/Admin/Banners/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var b = await _db.Banners.FindAsync(id);
        if (b == null) return NotFound();

        if (!string.IsNullOrEmpty(b.ImagePath))
            _fileUpload.DeleteFile(b.ImagePath);

        _db.Banners.Remove(b);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Баннер устгагдлаа.";
        return RedirectToAction(nameof(Banners));
    }

    // ── USERS ──────────────────────────────────────────────────────
    [HttpGet("/Admin/Users")]
    public async Task<IActionResult> Users(string? search, int page = 1)
    {
        const int size = 30;

        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u =>
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)) ||
                (u.Email != null && u.Email.Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        // Get roles for users
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in items)
            userRoles[u.Id] = await _users.GetRolesAsync(u);

        ViewBag.UserRoles = userRoles;
        ViewBag.Search = search;
        ViewBag.Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            PageSize = size,
            TotalCount = total
        };

        return View(items);
    }
}