using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Service for managing shopping lists and their items
/// </summary>
public interface IShoppingListService
{
    // Shopping List CRUD
    ShoppingList CreateShoppingList(string name, string? description = null);
    bool UpdateShoppingList(ShoppingList shoppingList);
    bool DeleteShoppingList(string id);
    ShoppingList? GetShoppingList(string id);
    IEnumerable<ShoppingList> GetAllShoppingLists();
    IEnumerable<ShoppingList> GetFavoriteShoppingLists();
    
    // Shopping List Items
    void AddItemToList(string listId, ShoppingListItem item);
    void RemoveItemFromList(string listId, string itemId);
    void UpdateItemInList(string listId, ShoppingListItem item);
    void ToggleItemChecked(string listId, string itemId);
    void ClearCheckedItems(string listId);
    
    // List Operations
    void DuplicateShoppingList(string sourceId, string newName);
    void ClearShoppingList(string listId);
    void MarkListAsFavorite(string listId, bool isFavorite);
    
    // Search and Filter
    IEnumerable<ShoppingList> SearchShoppingLists(string searchTerm);
    IEnumerable<ShoppingListItem> GetPendingItems(string listId);
    IEnumerable<ShoppingListItem> GetCheckedItems(string listId);
    
    // Import/Export
    string ExportShoppingListToMarkdown(string listId);
    ShoppingList? ImportShoppingListFromMarkdown(string markdownContent, string name);
}
