using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Mvc.Hubs;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chat;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _fileUpload;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(
        IChatService chat,
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IFileUploadService fileUpload,
        IHubContext<ChatHub> hubContext)
    {
        _chat = chat;
        _users = users;
        _db = db;
        _fileUpload = fileUpload;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        var convs = await _chat.GetUserConversationsAsync(uid);
        if (convs.Any())
            return RedirectToAction(nameof(Conversation), new { id = convs.First().Id });

        return View(new ChatViewModel { Conversations = convs, CurrentUserId = uid });
    }

    public async Task<IActionResult> Conversation(int id)
    {
        var uid = _users.GetUserId(User)!;
        var conv = await _chat.GetConversationByIdAsync(id, uid);
        if (conv == null) return NotFound();

        await _chat.MarkConversationAsReadAsync(id, uid);

        if (User.IsInRole("Admin"))
        {
            var otherUserId = conv.InitiatorId == uid
                ? conv.ParticipantId : conv.InitiatorId;

            ViewBag.PendingAgentRequest = await _db.AgentRequests
                .Where(r => r.UserId == otherUserId
                    && r.Status == AgentRequestStatus.Pending)
                .FirstOrDefaultAsync();
        }

        return View(new ChatViewModel
        {
            Conversations = await _chat.GetUserConversationsAsync(uid),
            ActiveConversation = conv,
            Messages = await _chat.GetMessagesAsync(id),
            CurrentUserId = uid,
            UnreadCount = await _chat.GetUnreadCountAsync(uid)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(string participantId, int? listingId = null)
    {
        var uid = _users.GetUserId(User)!;
        if (uid == participantId) return RedirectToAction("Index", "Home");

        var conv = await _chat.StartOrGetConversationAsync(uid, participantId, listingId);
        return RedirectToAction(nameof(Conversation), new { id = conv.Id });
    }

    public async Task<IActionResult> UnreadCount()
    {
        var uid = _users.GetUserId(User)!;
        return Json(new { count = await _chat.GetUnreadCountAsync(uid) });
    }

    // ── Зураг + текст илгээх ──────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendWithImage(
        int conversationId, string? content, IFormFile? image)
    {
        var uid = _users.GetUserId(User)!;
        var conv = await _chat.GetConversationByIdAsync(conversationId, uid);
        if (conv == null) return Forbid();

        string? imgPath = null;
        if (image != null && _fileUpload.IsValidImageFile(image))
            imgPath = await _fileUpload.UploadListingImageAsync(image);

        var msg = await _chat.SendMessageAsync(
            conversationId, uid, content ?? "", imgPath);

        var dto = new ChatMessageDto
        {
            Id = msg.Id,
            ConversationId = conversationId,
            SenderId = uid,
            SenderName = msg.Sender.DisplayName,
            SenderAvatar = msg.Sender.AvatarPath ?? "/img/default-avatar.svg",
            Content = msg.Content ?? "",
            ImageUrl = imgPath,
            SentAt = msg.SentAt.ToString("HH:mm")
        };

        await _hubContext.Clients
            .Group($"conv_{conversationId}")
            .SendAsync("ReceiveMessage", dto);

        return Ok(dto);
    }
}