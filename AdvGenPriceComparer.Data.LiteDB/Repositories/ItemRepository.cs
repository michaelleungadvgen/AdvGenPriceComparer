using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly DatabaseService _database;

    public ItemRepository(DatabaseService database)
    {
        _database = database;
    }

    public string Add(Item item)
    {
        item.DateAdded = DateTime.UtcNow;
        item.LastUpdated = DateTime.UtcNow;
        
        var entity = ItemEntity.FromItem(item);
        var insertedId = _database.Items.Insert(entity);
        return insertedId.ToString();
    }

    public bool Update(Item item)
    {
        item.LastUpdated = DateTime.UtcNow;
        var entity = ItemEntity.FromItem(item);
        return _database.Items.Update(entity);
    }

    public bool Delete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        return _database.Items.Delete(objectId);
    }

    public bool SoftDelete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        
        var entity = _database.Items.FindById(objectId);
        if (entity == null) return false;
        
        entity.IsActive = false;
        entity.LastUpdated = DateTime.UtcNow;
        return _database.Items.Update(entity);
    }

    public Item? GetById(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return null;
        
        var entity = _database.Items.FindById(objectId);
        return entity?.ToItem();
    }

    public IEnumerable<Item> GetAll()
    {
        return _database.Items.FindAll().Where(x => x.IsActive).Select(x => x.ToItem());
    }

    public IEnumerable<Item> SearchByName(string name)
    {
        return _database.Items
            .Find(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase) && x.IsActive)
            .Select(x => x.ToItem());
    }

    public IEnumerable<Item> GetByCategory(string category)
    {
        return _database.Items
            .Find(x => x.Category == category && x.IsActive)
            .Select(x => x.ToItem());
    }

    public IEnumerable<Item> GetByBrand(string brand)
    {
        return _database.Items
            .Find(x => x.Brand == brand && x.IsActive)
            .Select(x => x.ToItem());
    }

    public IEnumerable<Item> GetByBarcode(string barcode)
    {
        return _database.Items
            .Find(x => x.Barcode == barcode && x.IsActive)
            .Select(x => x.ToItem());
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
            .Take(count)
            .Select(x => x.ToItem());
    }

    public IEnumerable<Item> GetRecentlyUpdated(int count = 10)
    {
        return _database.Items
            .Find(x => x.IsActive)
            .OrderByDescending(x => x.LastUpdated)
            .Take(count)
            .Select(x => x.ToItem());
    }
}