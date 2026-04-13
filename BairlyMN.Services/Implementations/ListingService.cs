using BairlyMN.Data;
using BairlyMN.Data.Entities;
using BairlyMN.Domain.Enums;
using BairlyMN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Services.Implementations;

public class ListingService : IListingService
{
    private readonly ApplicationDbContext _db;

    public ListingService(ApplicationDbContext db) => _db = db;

    public async Task<(IEnumerable<Listing> Items, int TotalCount)> GetPagedListingsAsync(
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
        int? maxRooms = null)
    {
        var query = _db.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active)
            .Include(l => l.Images)
            .Include(l => l.User)
            .Include(l => l.LocationNode)
            .Include(l => l.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                l.Title.Contains(search) ||
                l.Description.Contains(search) ||
                (l.Address != null && l.Address.Contains(search)));

        if (propertyType.HasValue)
            query = query.Where(l => l.PropertyType == propertyType.Value);

        if (transactionType.HasValue)
            query = query.Where(l => l.TransactionType == transactionType.Value);

        if (locationNodeId.HasValue)
        {
            // Include all child nodes of selected location
            var childIds = await GetAllChildIdsAsync(locationNodeId.Value);
            childIds.Add(locationNodeId.Value);
            query = query.Where(l => l.LocationNodeId != null && childIds.Contains(l.LocationNodeId.Value));
        }

        if (minPrice.HasValue)
            query = query.Where(l => l.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(l => l.Price <= maxPrice.Value);

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId.Value);

        if (minArea.HasValue)
            query = query.Where(l => l.Area >= minArea.Value);

        if (maxArea.HasValue)
            query = query.Where(l => l.Area <= maxArea.Value);

        if (minRooms.HasValue)
            query = query.Where(l =>
                l.ApartmentDetails != null && l.ApartmentDetails.Rooms >= minRooms.Value ||
                l.HouseDetails != null && l.HouseDetails.Rooms >= minRooms.Value);

        if (maxRooms.HasValue)
            query = query.Where(l =>
                l.ApartmentDetails != null && l.ApartmentDetails.Rooms <= maxRooms.Value ||
                l.HouseDetails != null && l.HouseDetails.Rooms <= maxRooms.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.IsPromoted)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    private async Task<List<int>> GetAllChildIdsAsync(int parentId)
    {
        var result = new List<int>();
        var children = await _db.LocationNodes
            .Where(l => l.ParentId == parentId)
            .Select(l => l.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            result.Add(childId);
            var grandChildren = await GetAllChildIdsAsync(childId);
            result.AddRange(grandChildren);
        }

        return result;
    }

    public async Task<Listing?> GetListingByIdAsync(int id, bool includeDetails = true)
    {
        var query = _db.Listings
            .Include(l => l.Images)
            .Include(l => l.User)
            .Include(l => l.LocationNode)
            .Include(l => l.Category)
            .AsQueryable();

        if (includeDetails)
            query = query
                .Include(l => l.ApartmentDetails)
                .Include(l => l.HouseDetails)
                .Include(l => l.LandDetails);

        return await query.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<int> CreateListingAsync(Listing listing)
    {
        listing.CreatedAt = DateTime.UtcNow;
        listing.UpdatedAt = DateTime.UtcNow;
        _db.Listings.Add(listing);
        await _db.SaveChangesAsync();
        return listing.Id;
    }

    public async Task UpdateListingAsync(Listing listing)
    {
        listing.UpdatedAt = DateTime.UtcNow;
        _db.Listings.Update(listing);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteListingAsync(int id)
    {
        var listing = await _db.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing != null)
        {
            _db.Listings.Remove(listing);
            await _db.SaveChangesAsync();
        }
    }

    public async Task IncrementViewCountAsync(int id)
    {
        await _db.Listings
            .Where(l => l.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(l => l.ViewCount, l => l.ViewCount + 1));
    }

    public async Task<bool> ToggleFavoriteAsync(int listingId, string userId)
    {
        var existing = await _db.Favorites
            .FirstOrDefaultAsync(f => f.ListingId == listingId && f.UserId == userId);

        if (existing != null)
        {
            _db.Favorites.Remove(existing);
            await _db.SaveChangesAsync();
            return false;
        }

        _db.Favorites.Add(new Favorite { ListingId = listingId, UserId = userId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFavoritedByUserAsync(int listingId, string userId)
        => await _db.Favorites.AnyAsync(f =>
            f.ListingId == listingId && f.UserId == userId);

    public async Task<IEnumerable<Listing>> GetUserListingsAsync(string userId)
        => await _db.Listings
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Include(l => l.Images)
            .Include(l => l.LocationNode)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Listing>> GetFavoriteListingsAsync(string userId)
        => await _db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Include(f => f.Listing).ThenInclude(l => l.Images)
            .Include(f => f.Listing).ThenInclude(l => l.User)
            .Include(f => f.Listing).ThenInclude(l => l.LocationNode)
            .Select(f => f.Listing)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<Dictionary<string, int>> GetAdminStatsAsync()
    {
        return new Dictionary<string, int>
        {
            ["TotalListings"] = await _db.Listings.CountAsync(),
            ["ActiveListings"] = await _db.Listings.CountAsync(l => l.Status == ListingStatus.Active),
            ["TotalUsers"] = await _db.Users.CountAsync(),
            ["TotalMessages"] = await _db.ChatMessages.CountAsync(),
            ["TotalBanners"] = await _db.Banners.CountAsync()
        };
    }
}