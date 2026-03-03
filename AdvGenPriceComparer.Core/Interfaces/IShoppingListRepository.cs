using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IShoppingListRepository
{
    string Add(ShoppingList shoppingList);
    bool Update(ShoppingList shoppingList);
    bool Delete(string id);
    ShoppingList? GetById(string id);
    IEnumerable<ShoppingList> GetAll();
    IEnumerable<ShoppingList> GetActive();
    IEnumerable<ShoppingList> GetFavorites();
    IEnumerable<ShoppingList> SearchByName(string name);
    int GetTotalCount();
}
