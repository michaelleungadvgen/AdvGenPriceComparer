using LiteDB;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

public class DatabaseService : IDisposable
{
    private readonly LiteDatabase _database;
    private bool _disposed = false;

    public DatabaseService(string connectionString = "GroceryPrices.db")
    {
        var mapper = BsonMapper.Global;
        
        // Configure ObjectId serialization for the models
        mapper.Entity<Item>()
            .Id(x => x.Id);
            
        mapper.Entity<Place>()
            .Id(x => x.Id);
            
        mapper.Entity<PriceRecord>()
            .Id(x => x.Id);

        _database = new LiteDatabase(connectionString);
        
        // Create indexes for better performance
        CreateIndexes();
    }

    public ILiteCollection<Item> Items => _database.GetCollection<Item>("items");
    public ILiteCollection<Place> Places => _database.GetCollection<Place>("places");
    public ILiteCollection<PriceRecord> PriceRecords => _database.GetCollection<PriceRecord>("price_records");

    private void CreateIndexes()
    {
        // Item indexes
        Items.EnsureIndex(x => x.Name);
        Items.EnsureIndex(x => x.Brand);
        Items.EnsureIndex(x => x.Category);
        Items.EnsureIndex(x => x.Barcode);
        Items.EnsureIndex(x => x.IsActive);

        // Place indexes
        Places.EnsureIndex(x => x.Name);
        Places.EnsureIndex(x => x.Chain);
        Places.EnsureIndex(x => x.Suburb);
        Places.EnsureIndex(x => x.State);
        Places.EnsureIndex(x => x.IsActive);

        // PriceRecord indexes
        PriceRecords.EnsureIndex(x => x.ItemId);
        PriceRecords.EnsureIndex(x => x.PlaceId);
        PriceRecords.EnsureIndex(x => x.DateRecorded);
        PriceRecords.EnsureIndex(x => x.Price);
        PriceRecords.EnsureIndex(x => x.IsOnSale);
        PriceRecords.EnsureIndex(x => x.ValidFrom);
        PriceRecords.EnsureIndex(x => x.ValidTo);
    }

    public void BackupDatabase(string backupPath)
    {
        _database.Rebuild();
        var dbPath = GetDatabasePath();
        if (!string.IsNullOrEmpty(dbPath))
        {
            File.Copy(dbPath, backupPath, overwrite: true);
        }
    }

    public void OptimizeDatabase()
    {
        _database.Rebuild();
    }

    private string? GetDatabasePath()
    {
        // Get the database file path using reflection since Filename property might not be available
        var connectionString = _database.GetType().GetProperty("ConnectionString")?.GetValue(_database)?.ToString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Extract filename from connection string
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("Filename=", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Substring("Filename=".Length);
                }
            }
        }
        return null;
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