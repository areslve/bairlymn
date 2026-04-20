using BairlyMN.Data.Entities;

namespace BairlyMN.Services.Interfaces;

public interface IChatService
{
    Task<Conversation> StartOrGetConversationAsync(string initiatorId, string participantId, int? listingId = null);
    Task<Conversation?> GetConversationByIdAsync(int conversationId, string requestingUserId);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId);
    Task<ChatMessage> SendMessageAsync(int conversationId, string senderId,
        string content, string? imagePath = null);   // ← imagePath нэмэх
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int page = 1, int pageSize = 50);
    Task MarkConversationAsReadAsync(int conversationId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
}