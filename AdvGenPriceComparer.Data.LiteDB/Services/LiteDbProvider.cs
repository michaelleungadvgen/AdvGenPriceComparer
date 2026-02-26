using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

/// <summary>
/// LiteDB implementation of IDatabaseProvider
/// </summary>
public class LiteDbProvider : IDatabaseProvider
{
    private DatabaseService? _database;
    private bool _disposed = false;

    public string ProviderName => "LiteDB";

    public bool IsConnected => _database != null;

    public IItemRepository Items { get; private set; } = null!;
    public IPlaceRepository Places { get; private set; } = null!;
    public IPriceRecordRepository PriceRecords { get; private set; } = null!;
    public IAlertRepository Alerts { get; private set; } = null!;

    public Task<bool> ConnectAsync(DatabaseConnectionSettings settings)
    {
        try
        {
            // Close existing connection if any
            _database?.Dispose();

            _database = new DatabaseService(settings.LiteDbPath);
            
            // Initialize repositories
            Items = new ItemRepository(_database);
            Places = new PlaceRepository(_database);
            PriceRecords = new PriceRecordRepository(_database);
            Alerts = new AlertRepository(_database);
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LiteDbProvider connection failed: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task DisconnectAsync()
    {
        _database?.Dispose();
        _database = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _database?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
