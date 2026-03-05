using AdvGenPriceComparer.Server.Models;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Service interface for API key management
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Validate an API key and return the key info if valid
    /// </summary>
    Task<ApiKey?> ValidateKeyAsync(string apiKey);

    /// <summary>
    /// Generate a new API key
    /// </summary>
    Task<(ApiKey Key, string PlainKey)> GenerateKeyAsync(string name, int rateLimit = 100);

    /// <summary>
    /// Revoke an API key
    /// </summary>
    Task<bool> RevokeKeyAsync(int id);

    /// <summary>
    /// Get all API keys
    /// </summary>
    Task<IEnumerable<ApiKey>> GetAllKeysAsync();

    /// <summary>
    /// Update key usage statistics
    /// </summary>
    Task UpdateKeyUsageAsync(int keyId);
}
