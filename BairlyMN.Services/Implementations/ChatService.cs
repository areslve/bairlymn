using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;
using BairlyMN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Services.Implementations;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;

    public ChatService(ApplicationDbContext db) => _db = db;

    public async Task<Conversation> StartOrGetConversationAsync(
        string initiatorId, string participantId, int? listingId = null)
    {
        var existing = await _db.Conversations
            .Include(c => c.Initiator)
            .Include(c => c.Participant)
            .FirstOrDefaultAsync(c =>
                c.ListingId == listingId &&
                ((c.InitiatorId == initiatorId && c.ParticipantId == participantId) ||
                 (c.InitiatorId == participantId && c.ParticipantId == initiatorId)));

        if (existing != null) return existing;

        var conversation = new Conversation
        {
            InitiatorId = initiatorId,
            ParticipantId = participantId,
            ListingId = listingId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        return await _db.Conversations
            .Include(c => c.Initiator)
            .Include(c => c.Participant)
            .Include(c => c.Listing)
            .FirstAsync(c => c.Id == conversation.Id);
    }

    public async Task<Conversation?> GetConversationByIdAsync(int conversationId, string requestingUserId)
    {
        var conv = await _db.Conversations
            .Include(c => c.Initiator)
            .Include(c => c.Participant)
            .Include(c => c.Listing).ThenInclude(l => l!.Images)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conv == null) return null;
        if (conv.InitiatorId != requestingUserId && conv.ParticipantId != requestingUserId)
            return null;

        return conv;
    }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId)
        => await _db.Conversations
            .AsNoTracking()
            .Where(c => c.InitiatorId == userId || c.ParticipantId == userId)
            .Include(c => c.Initiator)
            .Include(c => c.Participant)
            .Include(c => c.Listing).ThenInclude(l => l!.Images)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

    public async Task<ChatMessage> SendMessageAsync(int conversationId, string senderId, string content)
    {
        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow,
            Status = MessageStatus.Sent
        };

        _db.ChatMessages.Add(message);

        await _db.Conversations
            .Where(c => c.Id == conversationId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LastMessageAt, DateTime.UtcNow));

        await _db.SaveChangesAsync();

        return await _db.ChatMessages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        int conversationId, int page = 1, int pageSize = 50)
        => await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task MarkConversationAsReadAsync(int conversationId, string userId)
        => await _db.ChatMessages
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId != userId
                     && m.Status != MessageStatus.Read)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, MessageStatus.Read)
                .SetProperty(m => m.ReadAt, DateTime.UtcNow));

    public async Task<int> GetUnreadCountAsync(string userId)
        => await _db.ChatMessages
            .CountAsync(m =>
                (m.Conversation.ParticipantId == userId || m.Conversation.InitiatorId == userId)
                && m.SenderId != userId
                && m.Status != MessageStatus.Read);
}