using AdvGenPriceComparer.Server.Data;
using AdvGenPriceComparer.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Service for API key management
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly PriceDataContext _context;

    public ApiKeyService(PriceDataContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiKey?> ValidateKeyAsync(string apiKey)
    {
        var keyHash = HashKey(apiKey);
        var key = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive);

        if (key != null)
        {
            key.LastUsedAt = DateTime.UtcNow;
            key.TotalRequests++;
            await _context.SaveChangesAsync();
        }

        return key;
    }

    /// <inheritdoc />
    public async Task<(ApiKey Key, string PlainKey)> GenerateKeyAsync(string name, int rateLimit = 100)
    {
        var plainKey = GenerateRandomKey();
        var keyHash = HashKey(plainKey);

        var apiKey = new ApiKey
        {
            Name = name,
            KeyHash = keyHash,
            RateLimit = rateLimit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        return (apiKey, plainKey);
    }

    /// <inheritdoc />
    public async Task<bool> RevokeKeyAsync(int id)
    {
        var key = await _context.ApiKeys.FindAsync(id);
        if (key == null) return false;

        key.IsActive = false;
        key.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApiKey>> GetAllKeysAsync()
    {
        return await _context.ApiKeys
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateKeyUsageAsync(int keyId)
    {
        var key = await _context.ApiKeys.FindAsync(keyId);
        if (key != null)
        {
            key.LastUsedAt = DateTime.UtcNow;
            key.TotalRequests++;
            await _context.SaveChangesAsync();
        }
    }

    private static string GenerateRandomKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return "ag_" + Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes);
    }
}
