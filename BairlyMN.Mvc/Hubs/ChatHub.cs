using BairlyMN.Services.Interfaces;
using BairlyMN.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BairlyMN.Mvc.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chat;
    private static readonly Dictionary<string, HashSet<string>> _conns = new();
    private static readonly object _lock = new();

    public ChatHub(IChatService chat) => _chat = chat;

    public override Task OnConnectedAsync()
    {
        var uid = Uid();
        lock (_lock)
        {
            if (!_conns.ContainsKey(uid)) _conns[uid] = new();
            _conns[uid].Add(Context.ConnectionId);
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? ex)
    {
        var uid = Uid();
        lock (_lock)
        {
            if (_conns.TryGetValue(uid, out var set))
            {
                set.Remove(Context.ConnectionId);
                if (set.Count == 0) _conns.Remove(uid);
            }
        }
        return base.OnDisconnectedAsync(ex);
    }

    public async Task JoinConversation(int conversationId)
    {
        var conv = await _chat.GetConversationByIdAsync(conversationId, Uid());
        if (conv == null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");

    public async Task SendMessage(int conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        var uid = Uid();
        var conv = await _chat.GetConversationByIdAsync(conversationId, uid);
        if (conv == null) return;

        var msg = await _chat.SendMessageAsync(conversationId, uid, content);

        var dto = new ChatMessageDto
        {
            Id = msg.Id,
            ConversationId = conversationId,
            SenderId = uid,
            SenderName = msg.Sender.DisplayName,
            SenderAvatar = msg.Sender.AvatarPath ?? "/img/default-avatar.svg",
            Content = msg.Content,
            SentAt = msg.SentAt.ToString("HH:mm")
        };

        await Clients.Group($"conv_{conversationId}").SendAsync("ReceiveMessage", dto);

        var otherId = conv.InitiatorId == uid ? conv.ParticipantId : conv.InitiatorId;
        await NotifyUser(otherId, "ConversationUpdated", conversationId);
    }

    public async Task MarkAsRead(int conversationId)
    {
        var uid = Uid();
        await _chat.MarkConversationAsReadAsync(conversationId, uid);
        await Clients.Group($"conv_{conversationId}").SendAsync("MessagesRead", conversationId, uid);
    }

    private string Uid()
        => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new HubException("Нэвтрэх шаардлагатай");

    private async Task NotifyUser(string userId, string method, object payload)
    {
        IReadOnlyList<string>? ids;
        lock (_lock) { ids = _conns.TryGetValue(userId, out var s) ? s.ToList() : null; }
        if (ids?.Count > 0)
            await Clients.Clients(ids).SendAsync(method, payload);
    }
}