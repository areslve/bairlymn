using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;

namespace BairlyMN.Services.Interfaces;

public interface IListingService
{
    Task<(IEnumerable<Listing> Items, int TotalCount)> GetPagedListingsAsync(
        int page, int pageSize,
        string? search = null,
        PropertyType? propertyType = null,
        TransactionType? transactionType = null,
        int? locationNodeId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? categoryId = null,
        decimal? minArea = null,
        decimal? maxArea = null,
        int? minRooms = null,
        int? maxRooms = null);

    Task<Listing?> GetListingByIdAsync(int id, bool includeDetails = true);
    Task<int> CreateListingAsync(Listing listing);
    Task UpdateListingAsync(Listing listing);
    Task DeleteListingAsync(int id);
    Task IncrementViewCountAsync(int id);
    Task<bool> ToggleFavoriteAsync(int listingId, string userId);
    Task<bool> IsFavoritedByUserAsync(int listingId, string userId);
    Task<IEnumerable<Listing>> GetUserListingsAsync(string userId);
    Task<IEnumerable<Listing>> GetFavoriteListingsAsync(string userId);
    Task<Dictionary<string, int>> GetAdminStatsAsync();
}