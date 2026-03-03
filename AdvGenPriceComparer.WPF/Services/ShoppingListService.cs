using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using System.Text;

namespace AdvGenPriceComparer.WPF.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository;
    private readonly ILoggerService _logger;

    public ShoppingListService(IShoppingListRepository shoppingListRepository, ILoggerService logger)
    {
        _shoppingListRepository = shoppingListRepository;
        _logger = logger;
    }

    public ShoppingList CreateShoppingList(string name, string? description = null)
    {
        var list = new ShoppingList
        {
            Name = name,
            Description = description,
            Items = new List<ShoppingListItem>(),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            IsActive = true
        };

        var id = _shoppingListRepository.Add(list);
        _logger.LogInfo($"Created shopping list '{name}' with ID {id}");
        
        return list;
    }

    public bool UpdateShoppingList(ShoppingList shoppingList)
    {
        shoppingList.MarkAsUpdated();
        var result = _shoppingListRepository.Update(shoppingList);
        _logger.LogInfo($"Updated shopping list '{shoppingList.Name}' - Success: {result}");
        return result;
    }

    public bool DeleteShoppingList(string id)
    {
        var result = _shoppingListRepository.Delete(id);
        _logger.LogInfo($"Deleted shopping list with ID {id} - Success: {result}");
        return result;
    }

    public ShoppingList? GetShoppingList(string id)
    {
        return _shoppingListRepository.GetById(id);
    }

    public IEnumerable<ShoppingList> GetAllShoppingLists()
    {
        return _shoppingListRepository.GetAll();
    }

    public IEnumerable<ShoppingList> GetFavoriteShoppingLists()
    {
        return _shoppingListRepository.GetFavorites();
    }

    public void AddItemToList(string listId, ShoppingListItem item)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null)
        {
            _logger.LogError($"Shopping list {listId} not found", new Exception("List not found"));
            throw new InvalidOperationException($"Shopping list {listId} not found");
        }

        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        list.AddItem(item);
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Added item '{item.Name}' to shopping list '{list.Name}'");
    }

    public void RemoveItemFromList(string listId, string itemId)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        list.RemoveItem(itemId);
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Removed item {itemId} from shopping list '{list.Name}'");
    }

    public void UpdateItemInList(string listId, ShoppingListItem item)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        var existingItem = list.Items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem == null) return;

        var index = list.Items.IndexOf(existingItem);
        list.Items[index] = item;
        list.MarkAsUpdated();
        
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Updated item '{item.Name}' in shopping list '{list.Name}'");
    }

    public void ToggleItemChecked(string listId, string itemId)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        var item = list.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return;

        item.ToggleChecked();
        list.MarkAsUpdated();
        
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Toggled item '{item.Name}' checked state to {item.IsChecked} in shopping list '{list.Name}'");
    }

    public void ClearCheckedItems(string listId)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        list.ClearCheckedItems();
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Cleared checked items from shopping list '{list.Name}'");
    }

    public void DuplicateShoppingList(string sourceId, string newName)
    {
        var source = _shoppingListRepository.GetById(sourceId);
        if (source == null)
        {
            throw new InvalidOperationException($"Source shopping list {sourceId} not found");
        }

        var newList = new ShoppingList
        {
            Name = newName,
            Description = source.Description,
            IsActive = true,
            Items = source.Items.Select(i => new ShoppingListItem
            {
                Name = i.Name,
                Brand = i.Brand,
                Category = i.Category,
                Notes = i.Notes,
                Quantity = i.Quantity,
                Unit = i.Unit,
                EstimatedPrice = i.EstimatedPrice,
                Priority = i.Priority,
                IsChecked = false,
                AddedDate = DateTime.UtcNow
            }).ToList()
        };

        _shoppingListRepository.Add(newList);
        _logger.LogInfo($"Duplicated shopping list '{source.Name}' to '{newName}'");
    }

    public void ClearShoppingList(string listId)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        list.ClearAllItems();
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Cleared all items from shopping list '{list.Name}'");
    }

    public void MarkListAsFavorite(string listId, bool isFavorite)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null) return;

        list.IsFavorite = isFavorite;
        list.MarkAsUpdated();
        _shoppingListRepository.Update(list);
        _logger.LogInfo($"Marked shopping list '{list.Name}' as {(isFavorite ? "favorite" : "not favorite")}");
    }

    public IEnumerable<ShoppingList> SearchShoppingLists(string searchTerm)
    {
        return _shoppingListRepository.SearchByName(searchTerm);
    }

    public IEnumerable<ShoppingListItem> GetPendingItems(string listId)
    {
        var list = _shoppingListRepository.GetById(listId);
        return list?.Items.Where(i => !i.IsChecked) ?? Enumerable.Empty<ShoppingListItem>();
    }

    public IEnumerable<ShoppingListItem> GetCheckedItems(string listId)
    {
        var list = _shoppingListRepository.GetById(listId);
        return list?.Items.Where(i => i.IsChecked) ?? Enumerable.Empty<ShoppingListItem>();
    }

    public string ExportShoppingListToMarkdown(string listId)
    {
        var list = _shoppingListRepository.GetById(listId);
        if (list == null)
        {
            throw new InvalidOperationException($"Shopping list {listId} not found");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# {list.Name}");
        
        if (!string.IsNullOrEmpty(list.Description))
        {
            sb.AppendLine($"\n{list.Description}");
        }
        
        sb.AppendLine($"\n**Created:** {list.CreatedDate:yyyy-MM-dd}");
        sb.AppendLine($"**Items:** {list.CheckedItems}/{list.TotalItems} completed");
        
        if (list.EstimatedTotal.HasValue)
        {
            sb.AppendLine($"**Estimated Total:** ${list.EstimatedTotal.Value:F2}");
        }
        
        sb.AppendLine("\n## Items\n");
        
        // Pending items first
        var pendingItems = list.Items.Where(i => !i.IsChecked).OrderBy(i => i.Priority).ThenBy(i => i.Name);
        var checkedItems = list.Items.Where(i => i.IsChecked).OrderBy(i => i.Name);
        
        foreach (var item in pendingItems)
        {
            sb.AppendLine(FormatItemForMarkdown(item, false));
        }
        
        if (checkedItems.Any())
        {
            sb.AppendLine("\n### Completed\n");
            foreach (var item in checkedItems)
            {
                sb.AppendLine(FormatItemForMarkdown(item, true));
            }
        }
        
        return sb.ToString();
    }

    private string FormatItemForMarkdown(ShoppingListItem item, bool isChecked)
    {
        var check = isChecked ? "[x]" : "[ ]";
        var name = item.DisplayName;
        var qty = item.Quantity.HasValue ? $" ({item.Quantity.Value} {item.Unit})" : "";
        var price = item.EstimatedPrice.HasValue ? $" - ${item.EstimatedPrice.Value:F2}" : "";
        var priority = item.Priority > 0 ? $" {item.PriorityDisplay}" : "";
        
        return $"- {check} **{name}**{qty}{price}{priority}";
    }

    public ShoppingList? ImportShoppingListFromMarkdown(string markdownContent, string name)
    {
        var list = new ShoppingList
        {
            Name = name,
            Description = "Imported from markdown",
            Items = new List<ShoppingListItem>(),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            IsActive = true
        };

        var lines = markdownContent.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("- [ ]") || trimmedLine.StartsWith("- [x]"))
            {
                var item = ParseMarkdownItem(trimmedLine);
                if (item != null)
                {
                    list.Items.Add(item);
                }
            }
        }

        if (list.Items.Count > 0)
        {
            _shoppingListRepository.Add(list);
            _logger.LogInfo($"Imported shopping list '{name}' with {list.Items.Count} items from markdown");
            return list;
        }

        return null;
    }

    private ShoppingListItem? ParseMarkdownItem(string line)
    {
        try
        {
            var isChecked = line.StartsWith("- [x]");
            var content = line.Substring(5).Trim();
            
            // Remove bold markers
            content = content.Replace("**", "");
            
            // Parse priority
            int priority = 0;
            if (content.Contains("🔴 Urgent"))
            {
                priority = 2;
                content = content.Replace(" 🔴 Urgent", "");
            }
            else if (content.Contains("🟡 High"))
            {
                priority = 1;
                content = content.Replace(" 🟡 High", "");
            }
            else if (content.Contains("⚪ Normal"))
            {
                content = content.Replace(" ⚪ Normal", "");
            }
            
            // Parse price
            decimal? price = null;
            var priceMatch = System.Text.RegularExpressions.Regex.Match(content, @"- \$(\d+\.?\d*)");
            if (priceMatch.Success)
            {
                price = decimal.Parse(priceMatch.Groups[1].Value);
                content = content.Substring(0, priceMatch.Index).Trim();
            }
            
            // Parse quantity
            decimal? quantity = null;
            string? unit = null;
            var qtyMatch = System.Text.RegularExpressions.Regex.Match(content, @"\((\d+\.?\d*)\s*(\w+)\)");
            if (qtyMatch.Success)
            {
                quantity = decimal.Parse(qtyMatch.Groups[1].Value);
                unit = qtyMatch.Groups[2].Value;
                content = content.Substring(0, qtyMatch.Index).Trim();
            }
            
            return new ShoppingListItem
            {
                Name = content,
                Quantity = quantity ?? 1,
                Unit = unit ?? "ea",
                EstimatedPrice = price,
                Priority = priority,
                IsChecked = isChecked,
                AddedDate = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }
}
