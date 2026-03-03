namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a shopping list containing items to purchase
/// </summary>
public class ShoppingList
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public List<ShoppingListItem> Items { get; set; } = new();
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFavorite { get; set; } = false;
    
    public string? StorePreference { get; set; }
    
    public decimal? Budget { get; set; }

    // Computed Properties
    public int TotalItems => Items.Count;
    
    public int CheckedItems => Items.Count(i => i.IsChecked);
    
    public int PendingItems => Items.Count(i => !i.IsChecked);
    
    public bool IsComplete => Items.Count > 0 && Items.All(i => i.IsChecked);
    
    public decimal? EstimatedTotal => Items.Any(i => i.EstimatedPrice.HasValue) 
        ? Items.Sum(i => i.EstimatedPrice ?? 0) 
        : null;
    
    public decimal? ActualTotal => Items.Any(i => i.ActualPrice.HasValue) 
        ? Items.Sum(i => i.ActualPrice ?? 0) 
        : null;

    public void MarkAsUpdated()
    {
        LastModifiedDate = DateTime.UtcNow;
    }

    public void AddItem(ShoppingListItem item)
    {
        Items.Add(item);
        MarkAsUpdated();
    }

    public void RemoveItem(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            MarkAsUpdated();
        }
    }

    public void ClearCheckedItems()
    {
        Items.RemoveAll(i => i.IsChecked);
        MarkAsUpdated();
    }

    public void ClearAllItems()
    {
        Items.Clear();
        MarkAsUpdated();
    }
}

/// <summary>
/// Represents an item within a shopping list
/// </summary>
public class ShoppingListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Reference to the Item in the database (optional - can be custom item)
    /// </summary>
    public string? ItemId { get; set; }
    
    /// <summary>
    /// Display name for the item
    /// </summary>
    public required string Name { get; set; }
    
    public string? Brand { get; set; }
    
    public string? Category { get; set; }
    
    public string? Notes { get; set; }
    
    public decimal? Quantity { get; set; } = 1;
    
    public string? Unit { get; set; } = "ea";
    
    public decimal? EstimatedPrice { get; set; }
    
    public decimal? ActualPrice { get; set; }
    
    public bool IsChecked { get; set; } = false;
    
    public bool IsFavorite { get; set; } = false;
    
    public string? PreferredStore { get; set; }
    
    public int Priority { get; set; } = 0; // 0=Normal, 1=High, 2=Urgent
    
    public DateTime? DueDate { get; set; }
    
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;

    // Computed Properties
    public string DisplayName => !string.IsNullOrEmpty(Brand) ? $"{Brand} {Name}" : Name;
    
    public string? QuantityDisplay => Quantity.HasValue ? $"{Quantity.Value} {Unit}" : null;
    
    public string? PriceDisplay => EstimatedPrice.HasValue ? $"${EstimatedPrice.Value:F2}" : null;
    
    public string PriorityDisplay => Priority switch
    {
        2 => "🔴 Urgent",
        1 => "🟡 High",
        _ => "⚪ Normal"
    };

    public void MarkAsChecked()
    {
        IsChecked = true;
    }

    public void MarkAsUnchecked()
    {
        IsChecked = false;
    }

    public void ToggleChecked()
    {
        IsChecked = !IsChecked;
    }
}
