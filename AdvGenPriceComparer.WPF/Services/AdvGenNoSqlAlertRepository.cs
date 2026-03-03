using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// HTTP client implementation of IAlertRepository for AdvGenNoSQLServer
/// </summary>
public class AdvGenNoSqlAlertRepository : IAlertRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private const string ApiEndpoint = "/api/v1/alerts";

    public AdvGenNoSqlAlertRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public string Add(AlertLogicEntity alert)
    {
        try
        {
            var response = _provider.PostWithRetryAsync(ApiEndpoint, alert).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result?.Data != null)
                {
                    _logger.LogInfo($"Alert added successfully: {result.Data.Id}");
                    return result.Data.Id ?? string.Empty;
                }
            }
            
            _logger.LogWarning($"Failed to add alert: {response?.StatusCode}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding alert: {ex.Message}", ex);
            return string.Empty;
        }
    }

    public bool Update(AlertLogicEntity alert)
    {
        try
        {
            var response = _provider.PutWithRetryAsync($"{ApiEndpoint}/{alert.Id}", alert).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Alert updated successfully: {alert.Id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to update alert: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating alert: {ex.Message}", ex);
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
                _logger.LogInfo($"Alert deleted successfully: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to delete alert: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting alert: {ex.Message}", ex);
            return false;
        }
    }

    public AlertLogicEntity? GetById(string id)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/{id}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting alert by ID: {ex.Message}", ex);
            return null;
        }
    }

    public IEnumerable<AlertLogicEntity> GetAll()
    {
        try
        {
            var response = _provider.GetWithRetryAsync(ApiEndpoint).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all alerts: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public IEnumerable<AlertLogicEntity> GetActiveAlerts()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?active=true").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting active alerts: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public IEnumerable<AlertLogicEntity> GetUnreadAlerts()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?read=false").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting unread alerts: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public IEnumerable<AlertLogicEntity> GetAlertsByItem(string itemId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?itemId={itemId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting alerts by item: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public IEnumerable<AlertLogicEntity> GetAlertsByPlace(string placeId)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}?placeId={placeId}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting alerts by place: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public IEnumerable<AlertLogicEntity> GetTriggeredAlerts(DateTime since)
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/triggered?since={since:yyyy-MM-ddTHH:mm:ssZ}").GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonSerializer.Deserialize<ApiListResponse<AlertLogicEntity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? Enumerable.Empty<AlertLogicEntity>();
            }
            
            return Enumerable.Empty<AlertLogicEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting triggered alerts: {ex.Message}", ex);
            return Enumerable.Empty<AlertLogicEntity>();
        }
    }

    public int GetUnreadCount()
    {
        try
        {
            var response = _provider.GetWithRetryAsync($"{ApiEndpoint}/count?read=false").GetAwaiter().GetResult();
            
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
            _logger.LogError($"Error getting unread count: {ex.Message}", ex);
            return 0;
        }
    }

    public bool MarkAsRead(string id)
    {
        try
        {
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/{id}/mark-read", new { }).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Alert marked as read: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to mark alert as read: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking alert as read: {ex.Message}", ex);
            return false;
        }
    }

    public bool MarkAllAsRead()
    {
        try
        {
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/mark-all-read", new { }).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo("All alerts marked as read");
                return true;
            }
            
            _logger.LogWarning($"Failed to mark all alerts as read: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking all alerts as read: {ex.Message}", ex);
            return false;
        }
    }

    public bool Dismiss(string id)
    {
        try
        {
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/{id}/dismiss", new { }).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo($"Alert dismissed: {id}");
                return true;
            }
            
            _logger.LogWarning($"Failed to dismiss alert: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error dismissing alert: {ex.Message}", ex);
            return false;
        }
    }

    public bool DismissAllRead()
    {
        try
        {
            var response = _provider.PostWithRetryAsync($"{ApiEndpoint}/dismiss-all-read", new { }).GetAwaiter().GetResult();
            
            if (response?.IsSuccessStatusCode == true)
            {
                _logger.LogInfo("All read alerts dismissed");
                return true;
            }
            
            _logger.LogWarning($"Failed to dismiss all read alerts: {response?.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error dismissing all read alerts: {ex.Message}", ex);
            return false;
        }
    }
}
