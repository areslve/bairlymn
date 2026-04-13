using BairlyMN.Data.Entities;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BairlyMN.Mvc.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chat;
    private readonly UserManager<ApplicationUser> _users;

    public ChatController(IChatService chat, UserManager<ApplicationUser> users)
    {
        _chat = chat; _users = users;
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
}