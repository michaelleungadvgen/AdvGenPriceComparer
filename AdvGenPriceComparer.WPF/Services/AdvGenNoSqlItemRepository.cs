using System.Net;
using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// HTTP client implementation of IItemRepository for AdvGenNoSQLServer
/// </summary>
public class AdvGenNoSqlItemRepository : IItemRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private const string ApiEndpoint = "/api/v1/items";

    public AdvGenNoSqlItemRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public string Add(Item item)
    {
        try
        {
            var response = _provider.PostWithRetryAsync(ApiEndpoint, item).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result?.Data != null)
                {
                    _logger.LogInfo($"Item added successfully: {result.Data.Id}");
                    return result.Data.Id ?? string.Empty;
                }
            }
            
            _logger.LogWarning($"Failed to add item: {response?.StatusCode}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding item: {ex.Message}", ex);
            return string.Empty;
        }
    }

    public bool Update(Item item)
    {
        try
        {
            var response = _provider.PutWithRetryAsync($"{ApiEndpoint}/{item.Id}", item).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Item updated successfully: {item.Id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to update item: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating item: {ex.Message}", ex);
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
                _logger.LogInfo($"Item deleted successfully: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to delete item: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting item: {ex.Message}", ex);
            return false;
        }
    }

    public bool SoftDelete(string id)
    {
        try
        {
            // Try soft delete endpoint first, fall back to regular delete
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/{id}/soft-delete", new { }).GetAwaiter().GetResult();
            
            if (response?.StatusCode == HttpStatusCode.NotFound)
            {
                // Endpoint not available, use regular delete
                return Delete(id);
            }
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Item soft-deleted successfully: {id}");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error soft-deleting item: {ex.Message}", ex);
            return false;
        }
    }

    public Item? GetById(string id)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/{id}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting item by ID: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<Item> GetAll()
    {
        try
        {
            var response = _provider.GetWithRetryAsync(ApiEndpoint).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all items: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<Item> SearchByName(string name)
    {
        try
        {
            var encodedName = Uri.EscapeDataString(name);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/search?name={encodedName}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching items by name: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<Item> GetByCategory(string category)
    {
        try
        {
            var encodedCategory = Uri.EscapeDataString(category);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?category={encodedCategory}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting items by category: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<Item> GetByBrand(string brand)
    {
        try
        {
            var encodedBrand = Uri.EscapeDataString(brand);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?brand={encodedBrand}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting items by brand: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<Item> GetByBarcode(string barcode)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/barcode/{barcode}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data != null ? new[] { result.Data } : Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting items by barcode: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<string> GetAllCategories()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/categories").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<string>();
            }
            
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all categories: {ex.Message}", ex);
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetAllBrands()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/brands").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<string>();
            }
            
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all brands: {ex.Message}", ex);
            return Enumerable.Empty<string>();
        }
    }

    public int GetTotalCount()
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
            _logger.LogError($"Error getting total count: {ex.Message}", ex);
            return 0;
        }
    }

    public IEnumerable<Item> GetRecentlyAdded(int count = 10)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/recent?count={count}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting recently added items: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<Item> GetRecentlyUpdated(int count = 10)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/recently-updated?count={count}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Item>();
            }
            
            return Enumerable.Empty<Item>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting recently updated items: {ex.Message}", ex);
            return Enumerable.Empty<Item>();
        }
    }
}

/// <summary>
/// API response wrapper for single item
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

/// <summary>
/// API response wrapper for list of items
/// </summary>
public class ApiListResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<T>? Data { get; set; }
    public int TotalCount { get; set; }
}
