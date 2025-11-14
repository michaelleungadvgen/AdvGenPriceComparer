using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddItemViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;

    private string? _itemId;
    private string _name = string.Empty;
    private string _brand = string.Empty;
    private string _category = string.Empty;
    private string _subCategory = string.Empty;
    private string _description = string.Empty;
    private string _packageSize = string.Empty;
    private string _unit = string.Empty;
    private string _barcode = string.Empty;
    private string _imageUrl = string.Empty;
    private string _tagsText = string.Empty;
    private string _allergensText = string.Empty;
    private string _dietaryFlagsText = string.Empty;
    private bool _isEditMode;
    private string _newPrice = string.Empty;
    private string _salePrice = string.Empty;
    private bool _isOnSale;
    private Place? _selectedStore;

    public AddItemViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;

        AvailableStores = new ObservableCollection<Place>();
        PriceRecords = new ObservableCollection<PriceRecordDisplay>();

        AddPriceRecordCommand = new RelayCommand(AddPriceRecord, CanAddPriceRecord);

        LoadStores();
    }

    public ObservableCollection<Place> AvailableStores { get; }
    public ObservableCollection<PriceRecordDisplay> PriceRecords { get; }
    public ICommand AddPriceRecordCommand { get; }

    public string? ItemId
    {
        get => _itemId;
        set
        {
            if (SetProperty(ref _itemId, value))
            {
                IsEditMode = !string.IsNullOrEmpty(value);
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string WindowTitle => IsEditMode ? "Edit Item" : "Add Item";
    public string SaveButtonText => IsEditMode ? "Save" : "Add Item";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Brand
    {
        get => _brand;
        set => SetProperty(ref _brand, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string SubCategory
    {
        get => _subCategory;
        set => SetProperty(ref _subCategory, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string PackageSize
    {
        get => _packageSize;
        set => SetProperty(ref _packageSize, value);
    }

    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public string Barcode
    {
        get => _barcode;
        set => SetProperty(ref _barcode, value);
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public string TagsText
    {
        get => _tagsText;
        set => SetProperty(ref _tagsText, value);
    }

    public string AllergensText
    {
        get => _allergensText;
        set => SetProperty(ref _allergensText, value);
    }

    public string DietaryFlagsText
    {
        get => _dietaryFlagsText;
        set => SetProperty(ref _dietaryFlagsText, value);
    }

    public string NewPrice
    {
        get => _newPrice;
        set
        {
            if (SetProperty(ref _newPrice, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string SalePrice
    {
        get => _salePrice;
        set
        {
            if (SetProperty(ref _salePrice, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsOnSale
    {
        get => _isOnSale;
        set
        {
            if (SetProperty(ref _isOnSale, value))
            {
                // Clear sale price when unchecked
                if (!value)
                {
                    SalePrice = string.Empty;
                }
                OnPropertyChanged(nameof(SalePrice));
            }
        }
    }

    public Place? SelectedStore
    {
        get => _selectedStore;
        set
        {
            if (SetProperty(ref _selectedStore, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private void LoadStores()
    {
        AvailableStores.Clear();
        var stores = _dataService.GetAllPlaces();
        foreach (var store in stores)
        {
            AvailableStores.Add(store);
        }

        if (AvailableStores.Any())
        {
            SelectedStore = AvailableStores.First();
        }
    }

    private void LoadPriceRecords()
    {
        if (string.IsNullOrEmpty(ItemId)) return;

        PriceRecords.Clear();
        var records = _dataService.PriceRecords.GetByItem(ItemId);

        foreach (var record in records.OrderByDescending(r => r.DateRecorded))
        {
            var place = _dataService.GetPlaceById(record.PlaceId);
            var display = new PriceRecordDisplay
            {
                Price = record.Price,
                OriginalPrice = record.OriginalPrice,
                PlaceName = place?.Name ?? "Unknown Store",
                DateRecorded = record.DateRecorded,
                IsOnSale = record.IsOnSale
            };

            // Calculate savings text
            if (record.IsOnSale && record.OriginalPrice.HasValue)
            {
                var savings = record.OriginalPrice.Value - record.Price;
                var savingsPercent = (savings / record.OriginalPrice.Value) * 100;
                display.SavingsText = $"Save ${savings:F2} ({savingsPercent:F0}% off)";
            }

            PriceRecords.Add(display);
        }
    }

    private bool CanAddPriceRecord()
    {
        return !string.IsNullOrEmpty(ItemId) &&
               !string.IsNullOrWhiteSpace(NewPrice) &&
               decimal.TryParse(NewPrice, out _) &&
               SelectedStore != null;
    }

    private void AddPriceRecord()
    {
        if (!CanAddPriceRecord() || string.IsNullOrEmpty(ItemId) || SelectedStore == null)
            return;

        if (!decimal.TryParse(NewPrice, out var regularPrice))
        {
            _dialogService.ShowWarning("Please enter a valid regular price.");
            return;
        }

        decimal? originalPrice = null;
        decimal finalPrice = regularPrice;

        // Handle sale price
        if (IsOnSale && !string.IsNullOrWhiteSpace(SalePrice))
        {
            if (!decimal.TryParse(SalePrice, out var salePrice))
            {
                _dialogService.ShowWarning("Please enter a valid sale price.");
                return;
            }

            if (salePrice >= regularPrice)
            {
                _dialogService.ShowWarning("Sale price must be less than regular price.");
                return;
            }

            originalPrice = regularPrice;
            finalPrice = salePrice;
        }

        try
        {
            _dataService.RecordPrice(
                ItemId,
                SelectedStore.Id,
                finalPrice,
                IsOnSale,
                originalPrice);

            var message = IsOnSale && originalPrice.HasValue
                ? $"Sale price ${finalPrice:F2} (was ${originalPrice.Value:F2}) recorded for {SelectedStore.Name}"
                : $"Price ${finalPrice:F2} recorded for {SelectedStore.Name}";

            _dialogService.ShowSuccess(message);

            LoadPriceRecords();
            NewPrice = string.Empty;
            SalePrice = string.Empty;
            IsOnSale = false;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to add price record: {ex.Message}");
        }
    }

    public bool SaveItem()
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(Name))
        {
            _dialogService.ShowWarning("Item name is required.");
            return false;
        }

        try
        {
            if (string.IsNullOrEmpty(ItemId))
            {
                // Add new item
                var itemId = _dataService.AddGroceryItem(Name);
                var item = _dataService.GetItemById(itemId);

                if (item != null)
                {
                    UpdateItemFields(item);
                    _dataService.Items.Update(item);
                    ItemId = itemId; // Set ItemId so user can add prices
                }

                _dialogService.ShowSuccess($"Item '{Name}' added successfully!");
            }
            else
            {
                // Update existing item
                var item = _dataService.GetItemById(ItemId);
                if (item != null)
                {
                    item.Name = Name;
                    UpdateItemFields(item);
                    _dataService.Items.Update(item);
                    _dialogService.ShowSuccess($"Item '{Name}' updated successfully!");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to save item: {ex.Message}");
            return false;
        }
    }

    private void UpdateItemFields(Item item)
    {
        item.Brand = string.IsNullOrWhiteSpace(Brand) ? null : Brand;
        item.Category = string.IsNullOrWhiteSpace(Category) ? null : Category;
        item.SubCategory = string.IsNullOrWhiteSpace(SubCategory) ? null : SubCategory;
        item.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
        item.PackageSize = string.IsNullOrWhiteSpace(PackageSize) ? null : PackageSize;
        item.Unit = string.IsNullOrWhiteSpace(Unit) ? null : Unit;
        item.Barcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode;
        item.ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl;

        // Parse comma-separated lists
        if (!string.IsNullOrWhiteSpace(TagsText))
        {
            item.Tags = TagsText.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(AllergensText))
        {
            item.Allergens = AllergensText.Split(',')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(DietaryFlagsText))
        {
            item.DietaryFlags = DietaryFlagsText.Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();
        }

        item.MarkAsUpdated();
    }

    public void LoadItem(Item item)
    {
        ItemId = item.Id;
        Name = item.Name;
        Brand = item.Brand ?? string.Empty;
        Category = item.Category ?? string.Empty;
        SubCategory = item.SubCategory ?? string.Empty;
        Description = item.Description ?? string.Empty;
        PackageSize = item.PackageSize ?? string.Empty;
        Unit = item.Unit ?? string.Empty;
        Barcode = item.Barcode ?? string.Empty;
        ImageUrl = item.ImageUrl ?? string.Empty;
        TagsText = item.Tags.Any() ? string.Join(", ", item.Tags) : string.Empty;
        AllergensText = item.Allergens.Any() ? string.Join(", ", item.Allergens) : string.Empty;
        DietaryFlagsText = item.DietaryFlags.Any() ? string.Join(", ", item.DietaryFlags) : string.Empty;

        LoadPriceRecords();
    }
}

public class PriceRecordDisplay
{
    public required decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public required string PlaceName { get; set; }
    public required DateTime DateRecorded { get; set; }
    public required bool IsOnSale { get; set; }
    public string? SavingsText { get; set; }
}
