using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class ShoppingListRepository : IShoppingListRepository
{
    private readonly ILiteDatabase _database;
    private readonly ILiteCollection<ShoppingList> _collection;

    public ShoppingListRepository(ILiteDatabase database)
    {
        _database = database;
        _collection = _database.GetCollection<ShoppingList>("shopping_lists");
        
        // Create indexes for common queries
        _collection.EnsureIndex(x => x.Name);
        _collection.EnsureIndex(x => x.IsActive);
        _collection.EnsureIndex(x => x.IsFavorite);
        _collection.EnsureIndex(x => x.CreatedDate);
    }

    public string Add(ShoppingList shoppingList)
    {
        if (string.IsNullOrEmpty(shoppingList.Id))
        {
            shoppingList.Id = Guid.NewGuid().ToString();
        }
        
        shoppingList.CreatedDate = DateTime.UtcNow;
        shoppingList.LastModifiedDate = DateTime.UtcNow;
        
        return _collection.Insert(shoppingList);
    }

    public bool Update(ShoppingList shoppingList)
    {
        shoppingList.LastModifiedDate = DateTime.UtcNow;
        return _collection.Update(shoppingList);
    }

    public bool Delete(string id)
    {
        return _collection.Delete(id);
    }

    public ShoppingList? GetById(string id)
    {
        return _collection.FindById(id);
    }

    public IEnumerable<ShoppingList> GetAll()
    {
        return _collection.FindAll().OrderByDescending(x => x.LastModifiedDate);
    }

    public IEnumerable<ShoppingList> GetActive()
    {
        return _collection.Find(x => x.IsActive)
            .OrderByDescending(x => x.LastModifiedDate);
    }

    public IEnumerable<ShoppingList> GetFavorites()
    {
        return _collection.Find(x => x.IsFavorite && x.IsActive)
            .OrderByDescending(x => x.LastModifiedDate);
    }

    public IEnumerable<ShoppingList> SearchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GetAll();
            
        var searchTerm = name.ToLowerInvariant();
        return _collection.Find(x => x.Name.ToLower().Contains(searchTerm))
            .OrderByDescending(x => x.LastModifiedDate);
    }

    public int GetTotalCount()
    {
        return _collection.Count();
    }
}
