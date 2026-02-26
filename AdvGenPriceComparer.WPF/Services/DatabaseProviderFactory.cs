using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Factory for creating the appropriate database provider based on settings
/// </summary>
public class DatabaseProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsService _settings;
    private readonly ILoggerService _logger;

    public DatabaseProviderFactory(IServiceProvider serviceProvider, ISettingsService settings, ILoggerService logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
    }

    public async Task<IDatabaseProvider> CreateProviderAsync()
    {
        IDatabaseProvider provider;
        
        var settings = new DatabaseConnectionSettings
        {
            ProviderType = _settings.DatabaseProviderType,
            LiteDbPath = _settings.LiteDbPath,
            ServerHost = _settings.ServerHost,
            ServerPort = _settings.ServerPort,
            ApiKey = _settings.ApiKey,
            DatabaseName = _settings.DatabaseName,
            UseSsl = _settings.UseSsl,
            ConnectionTimeout = _settings.ConnectionTimeout,
            RetryCount = _settings.RetryCount
        };

        _logger.LogInfo($"Creating database provider: {settings.ProviderType}");

        // For now, always use LiteDB provider (AdvGenNoSQLServer provider removed due to API incompatibility)
        provider = new LiteDbProvider();

        bool connected = await provider.ConnectAsync(settings);
        if (!connected)
        {
            _logger.LogWarning($"Failed to connect to {settings.ProviderType}, falling back to LiteDB");
            if (settings.ProviderType != DatabaseProviderType.LiteDB)
            {
                provider = new LiteDbProvider();
                settings.ProviderType = DatabaseProviderType.LiteDB;
                await provider.ConnectAsync(settings);
            }
        }

        return provider;
    }
}
