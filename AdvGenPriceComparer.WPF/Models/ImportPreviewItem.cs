using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Models;

/// <summary>
/// Represents an item in the import preview with user-selectable import action
/// </summary>
public class ImportPreviewItem : ViewModelBase
{
    private bool _addPriceRecordOnly;
    private string _productName = string.Empty;
    private string _category = string.Empty;
    private string _brand = string.Empty;
    private decimal _price;
    private decimal? _originalPrice;
    private bool _isOnSale;
    private string? _existingItemId;
    private string? _existingItemInfo;

    /// <summary>
    /// If true, add price record to existing item. If false, create new item with price record.
    /// </summary>
    public bool AddPriceRecordOnly
    {
        get => _addPriceRecordOnly;
        set
        {
            if (SetProperty(ref _addPriceRecordOnly, value))
            {
                OnPropertyChanged(nameof(ActionDescription));
            }
        }
    }

    public string ProductName
    {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string Brand
    {
        get => _brand;
        set => SetProperty(ref _brand, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public decimal? OriginalPrice
    {
        get => _originalPrice;
        set => SetProperty(ref _originalPrice, value);
    }

    public bool IsOnSale
    {
        get => _isOnSale;
        set => SetProperty(ref _isOnSale, value);
    }

    /// <summary>
    /// ID of existing item if a match was found
    /// </summary>
    public string? ExistingItemId
    {
        get => _existingItemId;
        set
        {
            if (SetProperty(ref _existingItemId, value))
            {
                OnPropertyChanged(nameof(HasExistingItem));
            }
        }
    }

    /// <summary>
    /// Display information about the existing item (e.g., "Match: Coca Cola 1.25L - Beverages")
    /// </summary>
    public string? ExistingItemInfo
    {
        get => _existingItemInfo;
        set => SetProperty(ref _existingItemInfo, value);
    }

    /// <summary>
    /// Whether an existing item match was found
    /// </summary>
    public bool HasExistingItem => !string.IsNullOrEmpty(ExistingItemId);

    /// <summary>
    /// Description of the action that will be taken
    /// </summary>
    public string ActionDescription =>
        AddPriceRecordOnly ? "Add price record" : "Create new item";

    /// <summary>
    /// Display text for price
    /// </summary>
    public string PriceDisplay
    {
        get
        {
            if (IsOnSale && OriginalPrice.HasValue)
            {
                return $"${Price:F2} (was ${OriginalPrice.Value:F2})";
            }
            return $"${Price:F2}";
        }
    }

    /// <summary>
    /// Raw product data from JSON (stored for actual import)
    /// </summary>
    public object? RawProductData { get; set; }
}
