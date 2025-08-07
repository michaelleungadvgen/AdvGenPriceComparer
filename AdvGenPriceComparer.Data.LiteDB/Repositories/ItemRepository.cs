using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class ItemRepository
{
    private readonly DatabaseService _database;

    public ItemRepository(DatabaseService database)
    {
        _database = database;
    }

    public ObjectId Add(Item item)
    {
        item.DateAdded = DateTime.UtcNow;
        item.LastUpdated = DateTime.UtcNow;
        return _database.Items.Insert(item);
    }

    public bool Update(Item item)
    {
        item.LastUpdated = DateTime.UtcNow;
        return _database.Items.Update(item);
    }

    public bool Delete(ObjectId id)
    {
        return _database.Items.Delete(id);
    }

    public bool SoftDelete(ObjectId id)
    {
        var item = _database.Items.FindById(id);
        if (item == null) return false;
        
        item.IsActive = false;
        item.LastUpdated = DateTime.UtcNow;
        return _database.Items.Update(item);
    }

    public Item? GetById(ObjectId id)
    {
        return _database.Items.FindById(id);
    }

    public IEnumerable<Item> GetAll()
    {
        return _database.Items.FindAll().Where(x => x.IsActive);
    }

    public IEnumerable<Item> SearchByName(string name)
    {
        return _database.Items
            .Find(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase) && x.IsActive);
    }

    public IEnumerable<Item> GetByCategory(string category)
    {
        return _database.Items
            .Find(x => x.Category == category && x.IsActive);
    }

    public IEnumerable<Item> GetByBrand(string brand)
    {
        return _database.Items
            .Find(x => x.Brand == brand && x.IsActive);
    }

    public IEnumerable<Item> GetByBarcode(string barcode)
    {
        return _database.Items
            .Find(x => x.Barcode == barcode && x.IsActive);
    }

    public IEnumerable<string> GetAllCategories()
    {
        return _database.Items
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.Category))
            .Select(x => x.Category!)
            .Distinct()
            .OrderBy(x => x);
    }

    public IEnumerable<string> GetAllBrands()
    {
        return _database.Items
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.Brand))
            .Select(x => x.Brand!)
            .Distinct()
            .OrderBy(x => x);
    }

    public int GetTotalCount()
    {
        return _database.Items.Count(x => x.IsActive);
    }

    public IEnumerable<Item> GetRecentlyAdded(int count = 10)
    {
        return _database.Items
            .Find(x => x.IsActive)
            .OrderByDescending(x => x.DateAdded)
            .Take(count);
    }

    public IEnumerable<Item> GetRecentlyUpdated(int count = 10)
    {
        return _database.Items
            .Find(x => x.IsActive)
            .OrderByDescending(x => x.LastUpdated)
            .Take(count);
    }
}