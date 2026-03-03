using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// AdvGenNoSQLServer implementation of IDatabaseProvider
/// Provides HTTP client-based access to remote NoSQL server
/// </summary>
public class AdvGenNoSqlProvider : IDatabaseProvider
{
    private readonly ILoggerService _logger;
    private HttpClient? _httpClient;
    private bool _disposed = false;
    private DatabaseConnectionSettings? _settings;

    public string ProviderName => "AdvGenNoSQLServer";

    public bool IsConnected { get; private set; }

    public IItemRepository Items { get; private set; } = null!;
    public IPlaceRepository Places { get; private set; } = null!;
    public IPriceRecordRepository PriceRecords { get; private set; } = null!;
    public IAlertRepository Alerts { get; private set; } = null!;

    public AdvGenNoSqlProvider(ILoggerService logger)
    {
        _logger = logger;
        // Initialize with null repositories until connected
        Items = new AdvGenNoSqlItemRepository(this, logger);
        Places = new AdvGenNoSqlPlaceRepository(this, logger);
        PriceRecords = new AdvGenNoSqlPriceRecordRepository(this, logger);
        Alerts = new AdvGenNoSqlAlertRepository(this, logger);
    }

    public async Task<bool> ConnectAsync(DatabaseConnectionSettings settings)
    {
        try
        {
            _logger.LogInfo($"Connecting to AdvGenNoSQLServer at {settings.ServerHost}:{settings.ServerPort}");
            
            _settings = settings;
            
            // Dispose existing client if any
            _httpClient?.Dispose();
            
            // Create new HttpClient with configuration
            _httpClient = new HttpClient();
            
            var scheme = settings.UseSsl ? "https" : "http";
            var baseUrl = $"{scheme}://{settings.ServerHost}:{settings.ServerPort}";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.ConnectionTimeout);
            
            // Set default headers
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Database-Name", settings.DatabaseName);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Test connection with health check
            var healthResult = await TestConnectionAsync();
            
            if (healthResult)
            {
                IsConnected = true;
                
                // Re-initialize repositories with connected client
                Items = new AdvGenNoSqlItemRepository(this, _logger);
                Places = new AdvGenNoSqlPlaceRepository(this, _logger);
                PriceRecords = new AdvGenNoSqlPriceRecordRepository(this, _logger);
                Alerts = new AdvGenNoSqlAlertRepository(this, _logger);
                
                _logger.LogInfo("Successfully connected to AdvGenNoSQLServer");
                return true;
            }
            else
            {
                _logger.LogWarning("Health check failed for AdvGenNoSQLServer");
                IsConnected = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to connect to AdvGenNoSQLServer: {ex.Message}", ex);
            IsConnected = false;
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        _logger.LogInfo("Disconnecting from AdvGenNoSQLServer");
        
        _httpClient?.Dispose();
        _httpClient = null;
        IsConnected = false;
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Test connection to the server
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        if (_httpClient == null)
        {
            return false;
        }

        try
        {
            // Try multiple endpoints for health check
            string[] healthEndpoints = { "/api/health", "/health", "/api/v1/health", "/" };
            
            foreach (var endpoint in healthEndpoints)
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug($"Health check succeeded at endpoint: {endpoint}");
                        return true;
                    }
                }
                catch
                {
                    // Try next endpoint
                    continue;
                }
            }
            
            // If no health endpoint worked, try a simple GET to verify server is reachable
            var testResponse = await _httpClient.GetAsync("");
            return testResponse.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Health check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the configured HttpClient for repository use
    /// </summary>
    internal HttpClient? GetHttpClient()
    {
        return _httpClient;
    }

    /// <summary>
    /// Get the current connection settings
    /// </summary>
    internal DatabaseConnectionSettings? GetSettings()
    {
        return _settings;
    }

    /// <summary>
    /// Send HTTP GET request with retry logic
    /// </summary>
    internal async Task<HttpResponseMessage?> GetWithRetryAsync(string endpoint, int maxRetries = 3)
    {
        if (_httpClient == null || !IsConnected)
        {
            throw new InvalidOperationException("Provider is not connected");
        }

        int retryCount = _settings?.RetryCount ?? maxRetries;
        
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                // If successful or client error (4xx), don't retry
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                // Server error - retry after delay
                if (attempt < retryCount - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
                }
            }
            catch (HttpRequestException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
        
        return null;
    }

    /// <summary>
    /// Send HTTP POST request with retry logic
    /// </summary>
    internal async Task<HttpResponseMessage?> PostWithRetryAsync<T>(string endpoint, T data, int maxRetries = 3)
    {
        if (_httpClient == null || !IsConnected)
        {
            throw new InvalidOperationException("Provider is not connected");
        }

        int retryCount = _settings?.RetryCount ?? maxRetries;
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                if (attempt < retryCount - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
                }
            }
            catch (HttpRequestException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
        
        return null;
    }

    /// <summary>
    /// Send HTTP PUT request with retry logic
    /// </summary>
    internal async Task<HttpResponseMessage?> PutWithRetryAsync<T>(string endpoint, T data, int maxRetries = 3)
    {
        if (_httpClient == null || !IsConnected)
        {
            throw new InvalidOperationException("Provider is not connected");
        }

        int retryCount = _settings?.RetryCount ?? maxRetries;
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                var response = await _httpClient.PutAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                if (attempt < retryCount - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
                }
            }
            catch (HttpRequestException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
        
        return null;
    }

    /// <summary>
    /// Send HTTP DELETE request with retry logic
    /// </summary>
    internal async Task<HttpResponseMessage?> DeleteWithRetryAsync(string endpoint, int maxRetries = 3)
    {
        if (_httpClient == null || !IsConnected)
        {
            throw new InvalidOperationException("Provider is not connected");
        }

        int retryCount = _settings?.RetryCount ?? maxRetries;
        
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                if (attempt < retryCount - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
                }
            }
            catch (HttpRequestException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < retryCount - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
            }
        }
        
        return null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
