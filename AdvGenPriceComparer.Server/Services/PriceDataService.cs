using AdvGenPriceComparer.Server.Data;
using AdvGenPriceComparer.Server.Hubs;
using AdvGenPriceComparer.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Service for managing shared price data
/// </summary>
public class PriceDataService : IPriceDataService
{
    private readonly PriceDataContext _context;
    private readonly INotificationService _notificationService;

    public PriceDataService(PriceDataContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<SharedItem>> GetItemsAsync(ItemFilter? filter = null, int page = 1, int pageSize = 100)
    {
        var query = _context.Items.AsNoTracking().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(i => i.Category == filter.Category);

            if (!string.IsNullOrEmpty(filter.Brand))
                query = query.Where(i => i.Brand == filter.Brand);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(i => i.Name.Contains(filter.SearchQuery) || 
                                         (i.Brand != null && i.Brand.Contains(filter.SearchQuery)));
        }

        return await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SharedItem?> GetItemByIdAsync(int id)
    {
        return await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<SharedItem?> GetItemByProductIdAsync(string productId)
    {
        return await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<IEnumerable<SharedItem>> SearchItemsAsync(string query, int limit = 20)
    {
        return await _context.Items
            .AsNoTracking()
            .Where(i => i.Name.Contains(query) || 
                       (i.Brand != null && i.Brand.Contains(query)) ||
                       (i.Barcode != null && i.Barcode == query))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<SharedItem> UpsertItemAsync(SharedItem item)
    {
        var existing = await _context.Items
            .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

        if (existing != null)
        {
            // Update existing
            existing.Name = item.Name;
            existing.Brand = item.Brand;
            existing.Category = item.Category;
            existing.Description = item.Description;
            existing.Barcode = item.Barcode;
            existing.Unit = item.Unit;
            existing.Size = item.Size;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Create new
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }

    public async Task<IEnumerable<SharedPlace>> GetPlacesAsync(PlaceFilter? filter = null, int page = 1, int pageSize = 100)
    {
        var query = _context.Places.AsNoTracking().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Chain))
                query = query.Where(p => p.Chain == filter.Chain);

            if (!string.IsNullOrEmpty(filter.State))
                query = query.Where(p => p.State == filter.State);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(p => p.Name.Contains(filter.SearchQuery) || 
                                         (p.Suburb != null && p.Suburb.Contains(filter.SearchQuery)));
        }

        return await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SharedPlace?> GetPlaceByIdAsync(int id)
    {
        return await _context.Places
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<SharedPlace> UpsertPlaceAsync(SharedPlace place)
    {
        var existing = await _context.Places
            .FirstOrDefaultAsync(p => p.StoreId == place.StoreId);

        if (existing != null)
        {
            // Update existing
            existing.Name = place.Name;
            existing.Chain = place.Chain;
            existing.Address = place.Address;
            existing.Suburb = place.Suburb;
            existing.State = place.State;
            existing.Postcode = place.Postcode;
            existing.Country = place.Country;
            existing.Latitude = place.Latitude;
            existing.Longitude = place.Longitude;
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Create new
            _context.Places.Add(place);
            await _context.SaveChangesAsync();
            return place;
        }
    }

    public async Task<IEnumerable<SharedPriceRecord>> GetPriceRecordsAsync(PriceFilter? filter = null, int page = 1, int pageSize = 100)
    {
        var query = _context.PriceRecords
            .AsNoTracking()
            .Include(p => p.Item)
            .Include(p => p.Place)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.ItemId.HasValue)
                query = query.Where(p => p.ItemId == filter.ItemId.Value);

            if (filter.PlaceId.HasValue)
                query = query.Where(p => p.PlaceId == filter.PlaceId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(p => p.DateRecorded >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(p => p.DateRecorded <= filter.ToDate.Value);

            if (filter.IsCurrent.HasValue)
                query = query.Where(p => p.IsCurrent == filter.IsCurrent.Value);

            if (filter.MinDiscount.HasValue)
                query = query.Where(p => p.OriginalPrice.HasValue && 
                    (p.OriginalPrice - p.Price) / p.OriginalPrice >= filter.MinDiscount.Value);
        }

        return await query
            .OrderByDescending(p => p.DateRecorded)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SharedPriceRecord?> GetCurrentPriceAsync(int itemId, int placeId)
    {
        return await _context.PriceRecords
            .AsNoTracking()
            .Include(p => p.Item)
            .Include(p => p.Place)
            .Where(p => p.ItemId == itemId && p.PlaceId == placeId && p.IsCurrent)
            .OrderByDescending(p => p.DateRecorded)
            .FirstOrDefaultAsync();
    }

    public async Task<SharedPriceRecord> CreatePriceRecordAsync(SharedPriceRecord record)
    {
        // Mark any existing current price as not current
        var existingCurrent = await _context.PriceRecords
            .Where(p => p.ItemId == record.ItemId && p.PlaceId == record.PlaceId && p.IsCurrent)
            .ToListAsync();

        foreach (var existing in existingCurrent)
        {
            existing.IsCurrent = false;
        }

        // Add new record
        record.DateRecorded = DateTime.UtcNow;
        record.IsCurrent = true;
        _context.PriceRecords.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<IEnumerable<SharedPriceRecord>> GetLatestDealsAsync(int limit = 50)
    {
        return await _context.PriceRecords
            .AsNoTracking()
            .Include(p => p.Item)
            .Include(p => p.Place)
            .Where(p => p.IsCurrent && p.OriginalPrice.HasValue && p.OriginalPrice > p.Price)
            .OrderByDescending(p => (p.OriginalPrice - p.Price) / p.OriginalPrice)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<SharedPriceRecord>> ComparePricesAsync(int itemId)
    {
        return await _context.PriceRecords
            .AsNoTracking()
            .Include(p => p.Item)
            .Include(p => p.Place)
            .Where(p => p.ItemId == itemId && p.IsCurrent)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<IEnumerable<SharedPriceRecord>> GetPriceHistoryAsync(int itemId, int? placeId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.PriceRecords
            .AsNoTracking()
            .Include(p => p.Item)
            .Include(p => p.Place)
            .Where(p => p.ItemId == itemId)
            .AsQueryable();

        if (placeId.HasValue)
            query = query.Where(p => p.PlaceId == placeId.Value);

        if (from.HasValue)
            query = query.Where(p => p.DateRecorded >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.DateRecorded <= to.Value);

        return await query
            .OrderBy(p => p.DateRecorded)
            .ToListAsync();
    }

    public async Task<UploadResult> UploadDataAsync(DataUploadRequest request, int apiKeyId)
    {
        var result = new UploadResult { Success = true };
        var session = new UploadSession
        {
            ApiKeyId = apiKeyId,
            ClientVersion = request.ClientVersion,
            IsSuccess = true
        };

        try
        {
            // Process places first (needed for price records)
            var placeIdMap = new Dictionary<string, int>();
            foreach (var place in request.Places)
            {
                var savedPlace = await UpsertPlaceAsync(place);
                placeIdMap[place.StoreId] = savedPlace.Id;
                result.PlacesUploaded++;
            }

            // Process items
            var itemIdMap = new Dictionary<string, int>();
            foreach (var item in request.Items)
            {
                var savedItem = await UpsertItemAsync(item);
                itemIdMap[item.ProductId] = savedItem.Id;
                result.ItemsUploaded++;
            }

            // Process price records
            foreach (var record in request.PriceRecords)
            {
                if (itemIdMap.TryGetValue(record.ItemId.ToString(), out int itemId) &&
                    placeIdMap.TryGetValue(record.PlaceId.ToString(), out int placeId))
                {
                    record.ItemId = itemId;
                    record.PlaceId = placeId;
                    await CreatePriceRecordAsync(record);
                    result.PricesUploaded++;
                }
            }

            await _context.UploadSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            // Send real-time notification about new data
            await _notificationService.NotifyDataUploadedAsync(
                result.ItemsUploaded, 
                result.PlacesUploaded, 
                result.PricesUploaded);

            // Send notifications for new deals (items with discounts)
            foreach (var record in request.PriceRecords.Where(r => r.OriginalPrice.HasValue && r.OriginalPrice > r.Price))
            {
                var item = request.Items.FirstOrDefault(i => i.Id == record.ItemId);
                var place = request.Places.FirstOrDefault(p => p.Id == record.PlaceId);

                if (item != null && place != null)
                {
                    await _notificationService.NotifyNewDealAsync(new NewDealNotification
                    {
                        ItemId = record.ItemId,
                        ItemName = item.Name,
                        Brand = item.Brand,
                        Category = item.Category,
                        PlaceId = record.PlaceId,
                        PlaceName = place.Name,
                        Price = record.Price,
                        OriginalPrice = record.OriginalPrice,
                        Savings = record.OriginalPrice - record.Price,
                        DealStartDate = DateTime.UtcNow
                    });
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            session.IsSuccess = false;
            session.ErrorMessage = ex.Message;
            await _context.UploadSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        return result;
    }

    public async Task<ServerStats> GetServerStatsAsync()
    {
        return new ServerStats
        {
            TotalItems = await _context.Items.CountAsync(),
            TotalPlaces = await _context.Places.CountAsync(),
            TotalPriceRecords = await _context.PriceRecords.CountAsync(),
            CurrentPrices = await _context.PriceRecords.CountAsync(p => p.IsCurrent),
            ApiKeysActive = await _context.ApiKeys.CountAsync(k => k.IsActive)
        };
    }
}
