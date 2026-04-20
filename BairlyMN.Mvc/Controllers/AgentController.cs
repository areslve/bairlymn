// BairlyMN.Mvc/Controllers/AgentController.cs

using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

[Authorize]
public class AgentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUpload;

    public AgentController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUpload)
    {
        _db = db;
        _userManager = userManager;
        _fileUpload = fileUpload;
    }

    // GET /Agent/Pricing — нийтэд нээлттэй
    [AllowAnonymous]
    public IActionResult Pricing() => View();

    // POST /Agent/RequestAgent — хэрэглэгч хүсэлт илгээнэ
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestAgent(
        string? message,
        IFormFile? receiptImage)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        // Аль хэдийн agent бол буцаах
        if (user.HasActiveAgent)
        {
            TempData["Info"] = "Та аль хэдийн идэвхтэй агент эрхтэй байна.";
            return RedirectToAction("Index", "Profile");
        }

        // Аль хэдийн хүлээгдэж буй хүсэлт байгаа эсэх
        var existing = await _db.AgentRequests
            .FirstOrDefaultAsync(r => r.UserId == user.Id
                && r.Status == AgentRequestStatus.Pending);

        if (existing != null)
        {
            TempData["Info"] = "Таны хүсэлт хүлээгдэж байна. Admin тантай холбогдох болно.";
            return RedirectToAction("Index", "Profile");
        }

        // Гүйлгээний зураг хадгалах
        string? receiptPath = null;
        if (receiptImage != null && _fileUpload.IsValidImageFile(receiptImage))
        {
            try
            {
                receiptPath = await _fileUpload.UploadListingImageAsync(receiptImage);
            }
            catch
            {
                // Зураг upload амжилтгүй болсон ч хүсэлтийг цуцлахгүй
                receiptPath = null;
            }
        }

        // Admin-ийг олох
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins.FirstOrDefault();

        // Admin-тай conversation үүсгэх
        Conversation? conv = null;
        if (admin != null)
        {
            conv = await _db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.InitiatorId == user.Id && c.ParticipantId == admin.Id) ||
                    (c.InitiatorId == admin.Id && c.ParticipantId == user.Id));

            if (conv == null)
            {
                conv = new Conversation
                {
                    InitiatorId = user.Id,
                    ParticipantId = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }

            // Чатын мессеж
            var msgText = string.IsNullOrWhiteSpace(message)
                ? "Сайн байна уу! Би агент болохыг хүсч байна. Дансны дугаарт төлбөр шилжүүлсний дараа баталгаажуулна уу.\n\n💳 KhanBank: 5909252474 (Badrakh)"
                : $"{message}\n\n💳 KhanBank: 5909252474 (Badrakh)";

            // Зургийн холбоос мессежэд нэмэх
            if (receiptPath != null)
                msgText += $"\n\n🧾 Гүйлгээний баримт: {receiptPath}";

            _db.ChatMessages.Add(new ChatMessage
            {
                ConversationId = conv.Id,
                SenderId = user.Id,
                Content = msgText,
                SentAt = DateTime.UtcNow,
            });
        }

        // AgentRequest үүсгэх
        var request = new AgentRequest
        {
            UserId = user.Id,
            Message = string.IsNullOrWhiteSpace(message) ? receiptPath : message,
            Status = AgentRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ConversationId = conv?.Id,
        };
        _db.AgentRequests.Add(request);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Хүсэлт амжилттай илгээгдлээ! Admin тантай удахгүй холбогдох болно.";

        if (conv != null)
            return RedirectToAction("Index", "Chat", new { conversationId = conv.Id });

        return RedirectToAction("Index", "Profile");
    }
}