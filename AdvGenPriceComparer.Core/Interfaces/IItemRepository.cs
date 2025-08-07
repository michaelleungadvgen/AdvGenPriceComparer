using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IItemRepository
{
    ObjectId Add(Item item);
    bool Update(Item item);
    bool Delete(ObjectId id);
    bool SoftDelete(ObjectId id);
    Item? GetById(ObjectId id);
    IEnumerable<Item> GetAll();
    IEnumerable<Item> SearchByName(string name);
    IEnumerable<Item> GetByCategory(string category);
    IEnumerable<Item> GetByBrand(string brand);
    IEnumerable<Item> GetByBarcode(string barcode);
    IEnumerable<string> GetAllCategories();
    IEnumerable<string> GetAllBrands();
    int GetTotalCount();
    IEnumerable<Item> GetRecentlyAdded(int count = 10);
    IEnumerable<Item> GetRecentlyUpdated(int count = 10);
}