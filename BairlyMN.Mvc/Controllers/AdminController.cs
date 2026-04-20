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

    // UserManager-г _userManager нэрээр ашигладаг (agent section-д)
    private UserManager<ApplicationUser> _userManager => _users;

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

    // ══════════════════════════════════════════════════════════════
    // AGENT MANAGEMENT
    // ══════════════════════════════════════════════════════════════

    [HttpGet("/Admin/Agents")]
    public async Task<IActionResult> Agents()
    {
        var agents = await _db.Users
            .Where(u => u.IsAgent)
            .Include(u => u.AgentSubscriptions
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.EndsAt))
            .OrderByDescending(u => u.AgentUntil)
            .ToListAsync();

        var requests = await _db.AgentRequests
            .Where(r => r.Status == AgentRequestStatus.Pending)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.PendingRequests = requests;
        return View(agents);
    }

    // ── GET ──────────────────────────────────────────────────────
    [HttpGet("/Admin/GrantAgent/{userId}")]
    public async Task<IActionResult> GrantAgent(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest();

        var user = await _db.Users
            .Include(u => u.AgentSubscriptions
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.EndsAt))
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        ViewBag.User = user;
        return View();
    }

    // ── POST ─────────────────────────────────────────────────────
    [HttpPost("/Admin/GrantAgent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantAgent(string userId, int months, string? note)
    {
        // Хамгаалалт
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "Хэрэглэгчийн ID байхгүй байна.";
            return RedirectToAction(nameof(Agents));
        }

        if (months <= 0)
        {
            TempData["Error"] = "Хугацаа сонгоно уу.";
            return RedirectToAction(nameof(Agents));
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Хэрэглэгч олдсонгүй.";
            return RedirectToAction(nameof(Agents));
        }

        var adminId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(adminId))
        {
            TempData["Error"] = "Admin ID олдсонгүй.";
            return RedirectToAction(nameof(Agents));
        }

        try
        {
            var now = DateTime.UtcNow;
            var baseDate = (user.AgentUntil.HasValue && user.AgentUntil.Value > now)
                ? user.AgentUntil.Value
                : now;
            var endsAt = baseDate.AddMonths(months);

            // Subscription бүртгэх
            _db.AgentSubscriptions.Add(new AgentSubscription
            {
                UserId = userId,
                GrantedByAdminId = adminId,
                StartsAt = now,
                EndsAt = endsAt,
                Note = note,
                IsActive = true,
                GrantedAt = now
            });

            // User-г agent болгох
            user.IsAgent = true;
            user.AgentUntil = endsAt;

            // Хүлээгдэж буй хүсэлтийг зөвшөөрсөн болгох
            var pendingRequest = await _db.AgentRequests
                .Where(r => r.UserId == userId
                    && r.Status == AgentRequestStatus.Pending)
                .FirstOrDefaultAsync();

            if (pendingRequest != null)
            {
                pendingRequest.Status = AgentRequestStatus.Approved;
                pendingRequest.ReviewedByAdminId = adminId;
                pendingRequest.ReviewedAt = now;
                pendingRequest.AdminNote = note;
            }

            // Chat мессеж илгээх (conversation байгаа бол)
            var conv = await _db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.InitiatorId == userId && c.ParticipantId == adminId) ||
                    (c.InitiatorId == adminId && c.ParticipantId == userId));

            if (conv != null)
            {
                var msg = $"✅ Таны агент эрх идэвхжлээ!\n\n" +
                          $"📅 Хугацаа: {endsAt:yyyy-MM-dd} хүртэл ({months} сар)\n" +
                          $"🏷️ Таны профайл болон зарууд дээр \"Агент\" тэмдэг харагдах болно.";

                _db.ChatMessages.Add(new ChatMessage
                {
                    ConversationId = conv.Id,
                    SenderId = adminId,
                    Content = msg,
                    SentAt = now,
                    Status = BairlyMN.Domain.Enums.MessageStatus.Sent
                });
                conv.LastMessageAt = now;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{user.DisplayName}-д {months} сарын агент эрх олгогдлоо!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Алдаа гарлаа: {ex.Message}";
        }

        return RedirectToAction(nameof(Agents));
    }
    // 2. AdminController.cs — RejectAgentRequest action нэмэх
    //    (Татгалзах товч ажиллахын тулд)
    // ══════════════════════════════════════════════════════════════

    [HttpPost("/Admin/RejectAgentRequest/{requestId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAgentRequest(int requestId)
    {
        var request = await _db.AgentRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return NotFound();

        var adminId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        request.Status = AgentRequestStatus.Rejected;
        request.ReviewedByAdminId = adminId;
        request.ReviewedAt = now;

        // Хэрэглэгчид мэдэгдэл илгээх
        var conv = await _db.Conversations
            .FirstOrDefaultAsync(c =>
                (c.InitiatorId == request.UserId && c.ParticipantId == adminId) ||
                (c.InitiatorId == adminId && c.ParticipantId == request.UserId));

        if (conv != null)
        {
            var msg = "❌ Таны агент болох хүсэлт энэ удаад зөвшөөрөгдсөнгүй.\n\n" +
                      "Дэлгэрэнгүй мэдэх бол бидэнтэй холбогдоно уу.";

            _db.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conv.Id,
                SenderId = adminId,
                Content = msg,
                SentAt = now,
                Status = BairlyMN.Domain.Enums.MessageStatus.Sent
            });
            conv.LastMessageAt = now;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "Хүсэлт татгалзагдлаа.";

        // Чат руу буцах
        if (conv != null)
            return RedirectToAction("Conversation", "Chat", new { id = conv.Id });

        return RedirectToAction(nameof(Agents));
    }

    [HttpPost("/Admin/RevokeAgent/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAgent(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var subs = await _db.AgentSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var sub in subs)
        {
            sub.IsActive = false;
            sub.EndsAt = DateTime.UtcNow;
        }

        user.IsAgent = false;
        user.AgentUntil = null;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"{user.DisplayName}-ийн агент эрх хасагдлаа.";
        return RedirectToAction(nameof(Agents));
    }

    [HttpPost("/Admin/ExpireAgents")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpireAgents()
    {
        var now = DateTime.UtcNow;
        var expired = await _db.Users
            .Where(u => u.IsAgent && u.AgentUntil.HasValue && u.AgentUntil.Value < now)
            .ToListAsync();

        foreach (var u in expired)
        {
            u.IsAgent = false;
            u.AgentUntil = null;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{expired.Count} агентын эрх дуусгавар боллоо.";
        return RedirectToAction(nameof(Agents));
    }
}