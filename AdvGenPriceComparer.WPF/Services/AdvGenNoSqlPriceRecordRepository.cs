using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// HTTP client implementation of IPriceRecordRepository for AdvGenNoSQLServer
/// </summary>
public class AdvGenNoSqlPriceRecordRepository : IPriceRecordRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private const string ApiEndpoint = "/api/v1/prices";

    public AdvGenNoSqlPriceRecordRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public string Add(PriceRecord priceRecord)
    {
        try
        {
            var response = _provider.PostWithRetryAsync(ApiEndpoint, priceRecord).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result?.Data != null)
                {
                    _logger.LogInfo($"Price record added successfully: {result.Data.Id}");
                    return result.Data.Id ?? string.Empty;
                }
            }
            
            _logger.LogWarning($"Failed to add price record: {response?.StatusCode}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding price record: {ex.Message}", ex);
            return string.Empty;
        }
    }

    public bool Update(PriceRecord priceRecord)
    {
        try
        {
            var response = _provider.PutWithRetryAsync($"{ApiEndpoint}/{priceRecord.Id}", priceRecord).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Price record updated successfully: {priceRecord.Id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to update price record: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating price record: {ex.Message}", ex);
            return false;
        }
    }

    public bool Delete(string id)
    {
        try
        {
            var response = _provider.DeleteWithRetryAsync($"{ApiEndpoint}/{id}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Price record deleted successfully: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to delete price record: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting price record: {ex.Message}", ex);
            return false;
        }
    }

    public PriceRecord? GetById(string id)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/{id}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price record by ID: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<PriceRecord> GetAll()
    {
        try
        {
            var response = _provider.GetWithRetryAsync(ApiEndpoint).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all price records: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<PriceRecord> GetByItem(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?itemId={itemId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price records by item: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<PriceRecord> GetByPlace(string placeId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?placeId={placeId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price records by place: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<PriceRecord> GetByItemAndPlace(string itemId, string placeId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?itemId={itemId}&placeId={placeId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price records by item and place: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public PriceRecord? GetLatestPrice(string itemId, string placeId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/latest?itemId={itemId}&placeId={placeId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting latest price: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<PriceRecord> GetCurrentSales()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/sales").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting current sales: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = $"{ApiEndpoint}/history?itemId={itemId}";
            if (fromDate.HasValue)
                query += $"&from={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue)
                query += $"&to={toDate.Value:yyyy-MM-dd}";

            var response = _provider.GetWithRetryAsync(query).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price history: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public decimal? GetLowestPrice(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/stats?itemId={itemId}&stat=lowest").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<decimal>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting lowest price: {ex.Message}", ex);
            return null;
        }
    }

    public decimal? GetHighestPrice(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/stats?itemId={itemId}&stat=highest").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<decimal>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting highest price: {ex.Message}", ex);
            return null;
        }
    }

    public decimal? GetAveragePrice(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/stats?itemId={itemId}&stat=average").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<decimal>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting average price: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<PriceRecord> GetBestDeals(int count = 10)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/best-deals?count={count}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting best deals: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<PriceRecord> ComparePrices(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/compare?itemId={itemId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error comparing prices: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public int GetTotalRecordsCount()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/count").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<int>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? 0;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting total records count: {ex.Message}", ex);
            return 0;
        }
    }

    public int GetRecordsCountThisWeek()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/count?period=this-week").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<int>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? 0;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting records count this week: {ex.Message}", ex);
            return 0;
        }
    }

    public int GetSaleRecordsCount()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/count?salesOnly=true").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<int>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? 0;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting sale records count: {ex.Message}", ex);
            return 0;
        }
    }

    public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/recent?count={count}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<PriceRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<PriceRecord>();
            }
            
            return Enumerable.Empty<PriceRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting recent price updates: {ex.Message}", ex);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public Dictionary<string, int> GetPriceRecordsBySource()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/by-source").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Dictionary<string, int>>>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? new Dictionary<string, int>();
            }
            
            return new Dictionary<string, int>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price records by source: {ex.Message}", ex);
            return new Dictionary<string, int>();
        }
    }
}
