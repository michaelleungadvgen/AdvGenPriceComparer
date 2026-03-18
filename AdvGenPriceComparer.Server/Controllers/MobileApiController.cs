using AdvGenPriceComparer.Server.Data;
using AdvGenPriceComparer.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvGenPriceComparer.Server.Controllers;

/// <summary>
/// Mobile Companion App API Controller
/// Provides optimized endpoints for mobile clients
/// </summary>
[ApiController]
[Route("api/mobile")]
public class MobileApiController : ControllerBase
{
    private readonly PriceDataContext _context;
    private readonly ILogger<MobileApiController> _logger;

    public MobileApiController(PriceDataContext context, ILogger<MobileApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard summary for mobile app
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<MobileApiResponse<MobileDashboardSummary>>> GetDashboard()
    {
        try
        {
            var totalItems = await _context.Items.CountAsync();
            var totalStores = await _context.Places.CountAsync();
            
            // Count active deals (items with special prices)
            var now = DateTime.UtcNow;
            var activeDeals = await _context.PriceRecords
                .Where(p => p.IsCurrent && p.SpecialType != null && p.ValidUntil > now)
                .Select(p => p.ItemId)
                .Distinct()
                .CountAsync();

            // Get recent price updates (last 24 hours)
            var yesterday = now.AddDays(-1);
            var recentUpdates = await _context.PriceRecords
                .Where(p => p.DateRecorded > yesterday)
                .OrderByDescending(p => p.DateRecorded)
                .Take(5)
                .Select(p => new MobilePriceUpdate
                {
                    ItemId = p.ItemId,
                    ItemName = p.Item != null ? p.Item.Name : "Unknown",
                    StoreName = p.Place != null ? p.Place.Name : "Unknown",
                    OldPrice = p.OriginalPrice ?? p.Price,
                    NewPrice = p.Price,
                    ChangePercent = p.OriginalPrice.HasValue 
                        ? (double)((p.Price - p.OriginalPrice.Value) / p.OriginalPrice.Value * 100)
                        : 0,
                    Timestamp = p.DateRecorded
                })
                .ToListAsync();

            // Get best deals today
            var bestDeals = await _context.PriceRecords
                .Where(p => p.IsCurrent && p.SpecialType != null && p.OriginalPrice.HasValue && p.ValidUntil > now)
                .OrderByDescending(p => (p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value)
                .Take(5)
                .Select(p => new MobileDeal
                {
                    Id = p.Id,
                    ItemId = p.ItemId,
                    ItemName = p.Item != null ? p.Item.Name : "Unknown",
                    Brand = p.Item != null ? p.Item.Brand : null,
                    Size = p.Item != null ? p.Item.Size : null,
                    StoreName = p.Place != null ? p.Place.Name : "Unknown",
                    Chain = p.Place != null ? p.Place.Chain : null,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Savings = p.OriginalPrice.HasValue ? p.OriginalPrice.Value - p.Price : null,
                    SavingsPercent = p.OriginalPrice.HasValue 
                        ? (double)((p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value * 100)
                        : 0,
                    SpecialType = p.SpecialType,
                    ValidUntil = p.ValidUntil,
                    Category = p.Item != null ? p.Item.Category : null
                })
                .ToListAsync();

            // Calculate average savings
            var averageSavings = bestDeals.Any() 
                ? bestDeals.Average(d => d.SavingsPercent) 
                : 0;

            var summary = new MobileDashboardSummary
            {
                TotalItems = totalItems,
                TotalStores = totalStores,
                ActiveDeals = activeDeals,
                AverageSavings = Math.Round(averageSavings, 1),
                RecentUpdates = recentUpdates,
                BestDealsToday = bestDeals
            };

            return Ok(new MobileApiResponse<MobileDashboardSummary> { Data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return StatusCode(500, new MobileApiError { Error = "Failed to get dashboard summary" });
        }
    }

    /// <summary>
    /// Quick price check by item name or barcode
    /// </summary>
    [HttpGet("price-check")]
    public async Task<ActionResult<MobileApiResponse<MobilePriceCheckResult>>> PriceCheck(
        [FromQuery] string? query,
        [FromQuery] string? barcode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new MobileApiError { Error = "Either 'query' or 'barcode' parameter is required" });
            }

            SharedItem? item = null;

            // Search by barcode first if provided
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Barcode == barcode);
            }

            // Fall back to name search
            if (item == null && !string.IsNullOrWhiteSpace(query))
            {
                var searchLower = query.ToLower();
                item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Name.ToLower().Contains(searchLower) ||
                        (i.Brand != null && i.Brand.ToLower().Contains(searchLower)));
            }

            if (item == null)
            {
                return Ok(new MobileApiResponse<MobilePriceCheckResult> 
                { 
                    Data = new MobilePriceCheckResult 
                    { 
                        ItemName = query ?? barcode ?? "Unknown",
                        Prices = new List<MobileStorePrice>()
                    } 
                });
            }

            var now = DateTime.UtcNow;
            var prices = await _context.PriceRecords
                .Where(p => p.ItemId == item.Id && p.IsCurrent && (p.ValidUntil == null || p.ValidUntil > now))
                .Include(p => p.Place)
                .OrderBy(p => p.Price)
                .Select(p => new MobileStorePrice
                {
                    StoreId = p.PlaceId,
                    StoreName = p.Place != null ? p.Place.Name : "Unknown",
                    Chain = p.Place != null ? p.Place.Chain : null,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    SpecialType = p.SpecialType,
                    ValidUntil = p.ValidUntil
                })
                .ToListAsync();

            MobileBestPrice? bestPrice = null;
            if (prices.Any())
            {
                var cheapest = prices.First();
                bestPrice = new MobileBestPrice
                {
                    StoreName = cheapest.StoreName,
                    Price = cheapest.Price,
                    Savings = cheapest.OriginalPrice.HasValue 
                        ? cheapest.OriginalPrice.Value - cheapest.Price 
                        : null
                };
            }

            var result = new MobilePriceCheckResult
            {
                ItemId = item.Id,
                ItemName = item.Name,
                Brand = item.Brand,
                Category = item.Category,
                Barcode = item.Barcode,
                Prices = prices,
                BestPrice = bestPrice
            };

            return Ok(new MobileApiResponse<MobilePriceCheckResult> { Data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing price check");
            return StatusCode(500, new MobileApiError { Error = "Failed to perform price check" });
        }
    }

    /// <summary>
    /// Find nearby stores based on user location
    /// </summary>
    [HttpGet("nearby-stores")]
    public async Task<ActionResult<MobileApiResponse<List<MobileNearbyStore>>>> GetNearbyStores(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 10,
        [FromQuery] string? chain = null)
    {
        try
        {
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
            {
                return BadRequest(new MobileApiError { Error = "Invalid latitude or longitude" });
            }

            // Get all places with coordinates
            var placesQuery = _context.Places
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue);

            if (!string.IsNullOrWhiteSpace(chain))
            {
                placesQuery = placesQuery.Where(p => p.Chain == chain);
            }

            var places = await placesQuery.ToListAsync();

            // Calculate distances and filter
            var nearbyStores = places
                .Select(p => new
                {
                    Place = p,
                    Distance = CalculateDistance(lat, lng, p.Latitude!.Value, p.Longitude!.Value)
                })
                .Where(x => x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .Take(20)
                .Select(x => new MobileNearbyStore
                {
                    Id = x.Place.Id,
                    Name = x.Place.Name,
                    Chain = x.Place.Chain,
                    Address = x.Place.Address,
                    Suburb = x.Place.Suburb,
                    State = x.Place.State,
                    Postcode = x.Place.Postcode,
                    Latitude = x.Place.Latitude,
                    Longitude = x.Place.Longitude,
                    Distance = Math.Round(x.Distance, 2),
                    Bearing = CalculateBearing(lat, lng, x.Place.Latitude.Value, x.Place.Longitude.Value),
                    CurrentDeals = _context.PriceRecords
                        .Count(p => p.PlaceId == x.Place.Id && p.IsCurrent && p.SpecialType != null)
                })
                .ToList();

            return Ok(new MobileApiResponse<List<MobileNearbyStore>> { Data = nearbyStores });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby stores");
            return StatusCode(500, new MobileApiError { Error = "Failed to get nearby stores" });
        }
    }

    /// <summary>
    /// Get compact items list optimized for mobile
    /// </summary>
    [HttpGet("items")]
    public async Task<ActionResult<MobileApiResponse<List<MobileCompactItem>>>> GetCompactItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null)
    {
        try
        {
            pageSize = Math.Min(pageSize, 100);
            
            var query = _context.Items.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(searchLower) ||
                    (i.Brand != null && i.Brand.ToLower().Contains(searchLower)) ||
                    (i.Barcode != null && i.Barcode.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new MobileCompactItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Brand = i.Brand,
                    Category = i.Category,
                    Barcode = i.Barcode,
                    BestPrice = _context.PriceRecords
                        .Where(p => p.ItemId == i.Id && p.IsCurrent)
                        .OrderBy(p => p.Price)
                        .Select(p => (decimal?)p.Price)
                        .FirstOrDefault(),
                    BestStore = _context.PriceRecords
                        .Where(p => p.ItemId == i.Id && p.IsCurrent)
                        .OrderBy(p => p.Price)
                        .Select(p => p.Place != null ? p.Place.Name : null)
                        .FirstOrDefault(),
                    AvgPrice = _context.PriceRecords
                        .Where(p => p.ItemId == i.Id && p.IsCurrent)
                        .Average(p => (decimal?)p.Price)
                })
                .ToListAsync();

            return Ok(new MobileApiResponse<List<MobileCompactItem>> 
            { 
                Data = items,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compact items");
            return StatusCode(500, new MobileApiError { Error = "Failed to get items" });
        }
    }

    /// <summary>
    /// Lookup item by barcode
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<MobileApiResponse<MobileBarcodeResult>>> LookupBarcode(string barcode)
    {
        try
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Barcode == barcode);

            if (item == null)
            {
                return Ok(new MobileApiResponse<MobileBarcodeResult>
                {
                    Data = new MobileBarcodeResult { Found = false }
                });
            }

            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            var prices = await _context.PriceRecords
                .Where(p => p.ItemId == item.Id && p.IsCurrent && (p.ValidUntil == null || p.ValidUntil > now))
                .Include(p => p.Place)
                .OrderBy(p => p.Price)
                .Select(p => new MobileStorePrice
                {
                    StoreId = p.PlaceId,
                    StoreName = p.Place != null ? p.Place.Name : "Unknown",
                    Chain = p.Place != null ? p.Place.Chain : null,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    SpecialType = p.SpecialType,
                    ValidUntil = p.ValidUntil
                })
                .ToListAsync();

            var bestDeal = prices.Any() ? new MobileBestPrice
            {
                StoreName = prices.First().StoreName,
                Price = prices.First().Price,
                Savings = prices.First().OriginalPrice.HasValue 
                    ? prices.First().OriginalPrice.Value - prices.First().Price 
                    : null
            } : null;

            var priceHistory = await _context.PriceRecords
                .Where(p => p.ItemId == item.Id && p.DateRecorded > thirtyDaysAgo)
                .GroupBy(p => 1)
                .Select(g => new MobilePriceHistorySummary
                {
                    Average30Day = g.Average(p => p.Price),
                    Lowest30Day = g.Min(p => p.Price),
                    Highest30Day = g.Max(p => p.Price)
                })
                .FirstOrDefaultAsync();

            var result = new MobileBarcodeResult
            {
                Found = true,
                Item = new MobileCompactItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Brand = item.Brand,
                    Category = item.Category,
                    Barcode = item.Barcode,
                    Size = item.Size
                },
                Prices = prices,
                BestDeal = bestDeal,
                PriceHistory = priceHistory
            };

            return Ok(new MobileApiResponse<MobileBarcodeResult> { Data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up barcode");
            return StatusCode(500, new MobileApiError { Error = "Failed to lookup barcode" });
        }
    }

    /// <summary>
    /// Get deal feed for mobile
    /// </summary>
    [HttpGet("deals")]
    public async Task<ActionResult<MobileApiResponse<List<MobileDeal>>>> GetDeals(
        [FromQuery] int limit = 20,
        [FromQuery] string? category = null,
        [FromQuery] int? storeId = null)
    {
        try
        {
            limit = Math.Min(limit, 100);
            var now = DateTime.UtcNow;

            var query = _context.PriceRecords
                .Where(p => p.IsCurrent && p.SpecialType != null && p.ValidUntil > now)
                .Include(p => p.Item)
                .Include(p => p.Place)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Item != null && p.Item.Category == category);
            }

            if (storeId.HasValue)
            {
                query = query.Where(p => p.PlaceId == storeId.Value);
            }

            var deals = await query
                .OrderByDescending(p => (p.OriginalPrice.HasValue ? (p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value : 0))
                .Take(limit)
                .Select(p => new MobileDeal
                {
                    Id = p.Id,
                    ItemId = p.ItemId,
                    ItemName = p.Item != null ? p.Item.Name : "Unknown",
                    Brand = p.Item != null ? p.Item.Brand : null,
                    Size = p.Item != null ? p.Item.Size : null,
                    StoreName = p.Place != null ? p.Place.Name : "Unknown",
                    Chain = p.Place != null ? p.Place.Chain : null,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Savings = p.OriginalPrice.HasValue ? p.OriginalPrice.Value - p.Price : null,
                    SavingsPercent = p.OriginalPrice.HasValue 
                        ? (double)((p.OriginalPrice.Value - p.Price) / p.OriginalPrice.Value * 100)
                        : 0,
                    SpecialType = p.SpecialType,
                    ValidUntil = p.ValidUntil,
                    Category = p.Item != null ? p.Item.Category : null
                })
                .ToListAsync();

            return Ok(new MobileApiResponse<List<MobileDeal>> { Data = deals });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deals");
            return StatusCode(500, new MobileApiError { Error = "Failed to get deals" });
        }
    }

    #region Shopping Lists (In-Memory for Demo)

    private static readonly Dictionary<string, MobileShoppingList> _shoppingLists = new();

    /// <summary>
    /// Get all shopping lists
    /// </summary>
    [HttpGet("shopping-lists")]
    public Task<ActionResult<MobileApiResponse<List<MobileShoppingList>>>> GetShoppingLists()
    {
        var lists = _shoppingLists.Values.ToList();
        return Task.FromResult<ActionResult<MobileApiResponse<List<MobileShoppingList>>>>(
            Ok(new MobileApiResponse<List<MobileShoppingList>> { Data = lists }));
    }

    /// <summary>
    /// Create or update a shopping list
    /// </summary>
    [HttpPost("shopping-lists")]
    public Task<ActionResult<MobileApiResponse<MobileShoppingList>>> SaveShoppingList([FromBody] MobileShoppingListRequest request)
    {
        var listId = request.Id ?? Guid.NewGuid().ToString();
        
        var list = new MobileShoppingList
        {
            Id = listId,
            Name = request.Name,
            Items = request.Items ?? new List<MobileShoppingListItem>(),
            ItemCount = request.Items?.Count ?? 0,
            CompletedCount = request.Items?.Count(i => i.IsChecked) ?? 0,
            LastModified = DateTime.UtcNow
        };

        _shoppingLists[listId] = list;

        return Task.FromResult<ActionResult<MobileApiResponse<MobileShoppingList>>>(
            Ok(new MobileApiResponse<MobileShoppingList> { Data = list }));
    }

    /// <summary>
    /// Delete a shopping list
    /// </summary>
    [HttpDelete("shopping-lists/{listId}")]
    public Task<ActionResult<MobileApiResponse<object>>> DeleteShoppingList(string listId)
    {
        _shoppingLists.Remove(listId);
        
        return Task.FromResult<ActionResult<MobileApiResponse<object>>>(
            Ok(new MobileApiResponse<object> { Message = "Shopping list deleted" }));
    }

    /// <summary>
    /// Sync shopping lists
    /// </summary>
    [HttpPost("shopping-lists/sync")]
    public Task<ActionResult<MobileApiResponse<MobileSyncResponse>>> SyncShoppingLists([FromBody] MobileSyncRequest request)
    {
        var response = new MobileSyncResponse
        {
            ServerTime = DateTime.UtcNow,
            ListsToUpdate = new List<MobileSyncAction>(),
            ListsToDelete = new List<string>()
        };

        foreach (var serverList in _shoppingLists.Values)
        {
            var clientList = request.Lists?.FirstOrDefault(l => l.Id == serverList.Id);
            
            if (clientList == null)
            {
                // New list on server, send to client
                response.ListsToUpdate.Add(new MobileSyncAction
                {
                    Id = serverList.Id,
                    Action = "update",
                    Data = serverList
                });
            }
            else if (serverList.LastModified > clientList.LastModified)
            {
                // Server has newer version
                response.ListsToUpdate.Add(new MobileSyncAction
                {
                    Id = serverList.Id,
                    Action = "update",
                    Data = serverList
                });
            }
        }

        // Find lists deleted on server
        if (request.Lists != null)
        {
            foreach (var clientList in request.Lists)
            {
                if (!_shoppingLists.ContainsKey(clientList.Id))
                {
                    response.ListsToDelete.Add(clientList.Id);
                }
            }
        }

        return Task.FromResult<ActionResult<MobileApiResponse<MobileSyncResponse>>>(
            Ok(new MobileApiResponse<MobileSyncResponse> { Data = response }));
    }

    #endregion

    #region Price Alerts (In-Memory for Demo)

    private static readonly Dictionary<string, MobilePriceAlert> _priceAlerts = new();

    /// <summary>
    /// Get all price alerts
    /// </summary>
    [HttpGet("price-alerts")]
    public async Task<ActionResult<MobileApiResponse<List<MobilePriceAlert>>>> GetPriceAlerts()
    {
        var alerts = _priceAlerts.Values.ToList();
        
        // Enrich with current prices
        foreach (var alert in alerts)
        {
            var currentPrice = await _context.PriceRecords
                .Where(p => p.ItemId == alert.ItemId && p.IsCurrent)
                .OrderBy(p => p.Price)
                .Select(p => (decimal?)p.Price)
                .FirstOrDefaultAsync();
            
            alert.CurrentPrice = currentPrice;
        }

        return Ok(new MobileApiResponse<List<MobilePriceAlert>> { Data = alerts });
    }

    /// <summary>
    /// Create a price alert
    /// </summary>
    [HttpPost("price-alerts")]
    public async Task<ActionResult<MobileApiResponse<MobilePriceAlert>>> CreatePriceAlert([FromBody] MobilePriceAlertRequest request)
    {
        var item = await _context.Items.FindAsync(request.ItemId);
        if (item == null)
        {
            return NotFound(new MobileApiError { Error = "Item not found" });
        }

        var currentPrice = await _context.PriceRecords
            .Where(p => p.ItemId == request.ItemId && p.IsCurrent)
            .OrderBy(p => p.Price)
            .Select(p => (decimal?)p.Price)
            .FirstOrDefaultAsync();

        var alert = new MobilePriceAlert
        {
            Id = Guid.NewGuid().ToString(),
            ItemId = request.ItemId,
            ItemName = item.Name,
            TargetPrice = request.TargetPrice,
            CurrentPrice = currentPrice,
            Condition = request.Condition,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _priceAlerts[alert.Id] = alert;

        return Ok(new MobileApiResponse<MobilePriceAlert> { Data = alert });
    }

    /// <summary>
    /// Delete a price alert
    /// </summary>
    [HttpDelete("price-alerts/{alertId}")]
    public Task<ActionResult<MobileApiResponse<object>>> DeletePriceAlert(string alertId)
    {
        _priceAlerts.Remove(alertId);
        
        return Task.FromResult<ActionResult<MobileApiResponse<object>>>(
            Ok(new MobileApiResponse<object> { Message = "Price alert deleted" }));
    }

    #endregion

    #region Push Notifications (Stub)

    /// <summary>
    /// Register device for push notifications
    /// </summary>
    [HttpPost("push-register")]
    public Task<ActionResult<MobileApiResponse<object>>> RegisterPush([FromBody] MobilePushRegistrationRequest request)
    {
        // In a real implementation, this would store the device token
        _logger.LogInformation("Registering device {DeviceId} for push notifications", request.DeviceId);
        
        return Task.FromResult<ActionResult<MobileApiResponse<object>>>(
            Ok(new MobileApiResponse<object> { Message = "Device registered successfully" }));
    }

    /// <summary>
    /// Unregister device from push notifications
    /// </summary>
    [HttpPost("push-unregister")]
    public Task<ActionResult<MobileApiResponse<object>>> UnregisterPush([FromBody] MobilePushUnregistrationRequest request)
    {
        _logger.LogInformation("Unregistering device token from push notifications");
        
        return Task.FromResult<ActionResult<MobileApiResponse<object>>>(
            Ok(new MobileApiResponse<object> { Message = "Device unregistered successfully" }));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate distance between two coordinates using Haversine formula
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    /// <summary>
    /// Calculate bearing from point 1 to point 2
    /// </summary>
    private static string CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRad(lon2 - lon1);
        var lat1Rad = ToRad(lat1);
        var lat2Rad = ToRad(lat2);

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearing = (Math.Atan2(y, x) * 180 / Math.PI + 360) % 360;

        // Convert to cardinal direction
        var directions = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
        var index = (int)Math.Round(bearing / 45);
        return directions[index];
    }

    #endregion
}
