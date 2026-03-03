using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// HTTP client implementation of IPlaceRepository for AdvGenNoSQLServer
/// </summary>
public class AdvGenNoSqlPlaceRepository : IPlaceRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private const string ApiEndpoint = "/api/v1/places";

    public AdvGenNoSqlPlaceRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public string Add(Place place)
    {
        try
        {
            var response = _provider.PostWithRetryAsync(ApiEndpoint, place).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result?.Data != null)
                {
                    _logger.LogInfo($"Place added successfully: {result.Data.Id}");
                    return result.Data.Id ?? string.Empty;
                }
            }
            
            _logger.LogWarning($"Failed to add place: {response?.StatusCode}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding place: {ex.Message}", ex);
            return string.Empty;
        }
    }

    public bool Update(Place place)
    {
        try
        {
            var response = _provider.PutWithRetryAsync($"{ApiEndpoint}/{place.Id}", place).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Place updated successfully: {place.Id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to update place: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating place: {ex.Message}", ex);
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
                _logger.LogInfo($"Place deleted successfully: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to delete place: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting place: {ex.Message}", ex);
            return false;
        }
    }

    public bool SoftDelete(string id)
    {
        try
        {
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/{id}/soft-delete", new { }).GetAwaiter().GetResult();
            
            if (response?.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Delete(id);
            }
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Place soft-deleted successfully: {id}");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error soft-deleting place: {ex.Message}", ex);
            return false;
        }
    }

    public Place? GetById(string id)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/{id}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting place by ID: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<Place> GetAll()
    {
        try
        {
            var response = _provider.GetWithRetryAsync(ApiEndpoint).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all places: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<Place> SearchByName(string name)
    {
        try
        {
            var encodedName = Uri.EscapeDataString(name);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/search?name={encodedName}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching places by name: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<Place> GetByChain(string chain)
    {
        try
        {
            var encodedChain = Uri.EscapeDataString(chain);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?chain={encodedChain}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting places by chain: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<Place> GetBySuburb(string suburb)
    {
        try
        {
            var encodedSuburb = Uri.EscapeDataString(suburb);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?suburb={encodedSuburb}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting places by suburb: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<Place> GetByState(string state)
    {
        try
        {
            var encodedState = Uri.EscapeDataString(state);
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?state={encodedState}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting places by state: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<Place> GetByLocation(double latitude, double longitude, double radiusKm)
    {
        try
        {
            var response = _provider.GetWithRetryAsync(
                $"{ApiEndpoint}/nearby?lat={latitude}&lng={longitude}&radius={radiusKm}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<Place>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<Place>();
            }
            
            return Enumerable.Empty<Place>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting places by location: {ex.Message}", ex);
            return Enumerable.Empty<Place>();
        }
    }

    public IEnumerable<string> GetAllChains()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/chains").GetAwaiter().GetResult();
            
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
            _logger.LogError($"Error getting all chains: {ex.Message}", ex);
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetAllSuburbs()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/suburbs").GetAwaiter().GetResult();
            
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
            _logger.LogError($"Error getting all suburbs: {ex.Message}", ex);
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetAllStates()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/states").GetAwaiter().GetResult();
            
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
            _logger.LogError($"Error getting all states: {ex.Message}", ex);
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

    public Dictionary<string, int> GetChainCounts()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/chain-counts").GetAwaiter().GetResult();
            
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
            _logger.LogError($"Error getting chain counts: {ex.Message}", ex);
            return new Dictionary<string, int>();
        }
    }
}
