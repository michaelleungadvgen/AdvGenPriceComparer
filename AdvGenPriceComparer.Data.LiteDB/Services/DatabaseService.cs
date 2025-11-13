using LiteDB;
using AdvGenPriceComparer.Data.LiteDB.Entities;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

public class DatabaseService : IDisposable
{
    private readonly LiteDatabase _database;
    private bool _disposed = false;

    public DatabaseService(string connectionString = "GroceryPrices.db")
    {
        try
        {
            // Create a new mapper instance instead of modifying global
            var mapper = new BsonMapper();

            // Configure ObjectId serialization and field mappings for entity models
            mapper.Entity<ItemEntity>()
                .Id(x => x.Id)
                .Field(x => x.Name, "name")
                .Field(x => x.Description, "description")
                .Field(x => x.Brand, "brand")
                .Field(x => x.Category, "category")
                .Field(x => x.SubCategory, "subCategory")
                .Field(x => x.Barcode, "barcode")
                .Field(x => x.PackageSize, "packageSize")
                .Field(x => x.Unit, "unit")
                .Field(x => x.Weight, "weight")
                .Field(x => x.Volume, "volume")
                .Field(x => x.ImageUrl, "imageUrl")
                .Field(x => x.NutritionalInfo, "nutritionalInfo")
                .Field(x => x.Allergens, "allergens")
                .Field(x => x.DietaryFlags, "dietaryFlags")
                .Field(x => x.Tags, "tags")
                .Field(x => x.IsActive, "isActive")
                .Field(x => x.DateAdded, "dateAdded")
                .Field(x => x.LastUpdated, "lastUpdated")
                .Field(x => x.ExtraInformation, "extraInfo");

            mapper.Entity<PlaceEntity>()
                .Id(x => x.Id);

            mapper.Entity<PriceRecordEntity>()
                .Id(x => x.Id);

            mapper.Entity<AlertEntity>()
                .Id(x => x.Id);

            _database = new LiteDatabase(connectionString, mapper);

            // Create indexes for better performance
            CreateIndexes();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex}");
            throw;
        }
    }

    public ILiteCollection<ItemEntity> Items => _database.GetCollection<ItemEntity>("items");
    public ILiteCollection<PlaceEntity> Places => _database.GetCollection<PlaceEntity>("places");
    public ILiteCollection<PriceRecordEntity> PriceRecords => _database.GetCollection<PriceRecordEntity>("price_records");
    public ILiteCollection<AlertEntity> Alerts => _database.GetCollection<AlertEntity>("alerts");

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

        // Alert indexes
        Alerts.EnsureIndex(x => x.ItemId);
        Alerts.EnsureIndex(x => x.PlaceId);
        Alerts.EnsureIndex(x => x.IsActive);
        Alerts.EnsureIndex(x => x.IsRead);
        Alerts.EnsureIndex(x => x.IsDismissed);
        Alerts.EnsureIndex(x => x.LastTriggered);
        Alerts.EnsureIndex(x => x.DateCreated);
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