using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AddItemViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly CategoryPredictionService? _categoryPredictionService;

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

    // Category suggestions
    private ObservableCollection<CategorySuggestion> _categorySuggestions = new();
    private bool _showCategorySuggestions;

    public AddItemViewModel(
        IMediator mediator,
        IDialogService dialogService,
        CategoryPredictionService? categoryPredictionService = null)
    {
        _mediator = mediator;
        _dialogService = dialogService;
        _categoryPredictionService = categoryPredictionService;

        AvailableStores = new ObservableCollection<Place>();
        PriceRecords = new ObservableCollection<PriceRecordDisplay>();
        CategorySuggestions = new ObservableCollection<CategorySuggestion>();

        AddPriceRecordCommand = new RelayCommand(AddPriceRecord, CanAddPriceRecord);
        ApplyCategorySuggestionCommand = new RelayCommand<CategorySuggestion>(ApplyCategorySuggestion);
        ClearCategorySuggestionsCommand = new RelayCommand(() => ShowCategorySuggestions = false);

        LoadStores();
    }

    public ObservableCollection<Place> AvailableStores { get; }
    public ObservableCollection<PriceRecordDisplay> PriceRecords { get; }
    public ObservableCollection<CategorySuggestion> CategorySuggestions
    {
        get => _categorySuggestions;
        set => SetProperty(ref _categorySuggestions, value);
    }

    public ICommand AddPriceRecordCommand { get; }
    public ICommand ApplyCategorySuggestionCommand { get; }
    public ICommand ClearCategorySuggestionsCommand { get; }

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
        set
        {
            if (SetProperty(ref _name, value))
            {
                UpdateCategorySuggestions();
            }
        }
    }

    public string Brand
    {
        get => _brand;
        set
        {
            if (SetProperty(ref _brand, value))
            {
                UpdateCategorySuggestions();
            }
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            if (SetProperty(ref _category, value))
            {
                // Hide suggestions when user manually selects a category
                if (!string.IsNullOrEmpty(value))
                {
                    ShowCategorySuggestions = false;
                }
            }
        }
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

    public bool ShowCategorySuggestions
    {
        get => _showCategorySuggestions;
        set => SetProperty(ref _showCategorySuggestions, value);
    }

    public bool HasCategoryPredictionService => _categoryPredictionService?.IsModelLoaded ?? false;

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

    private void UpdateCategorySuggestions()
    {
        // Only show suggestions if we have a prediction service with a loaded model
        if (_categoryPredictionService?.IsModelLoaded != true)
            return;

        // Don't show suggestions if the user has already manually set a category
        if (!string.IsNullOrEmpty(Category) && !CategorySuggestions.Any(cs => cs.Category == Category))
            return;

        // Need at least the product name to make predictions
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowCategorySuggestions = false;
            return;
        }

        // Create product data for prediction
        var productData = new ProductData
        {
            ProductName = Name,
            Brand = Brand,
            Description = Description,
            Store = ""
        };

        // Get top 3 category suggestions
        var suggestions = _categoryPredictionService.GetTopSuggestions(productData, topN: 3);

        CategorySuggestions.Clear();
        foreach (var (category, confidence) in suggestions)
        {
            // Only show suggestions with at least 10% confidence
            if (confidence >= 0.1f)
            {
                CategorySuggestions.Add(new CategorySuggestion
                {
                    Category = category,
                    Confidence = confidence,
                    ConfidenceText = $"{confidence:P0}"
                });
            }
        }

        ShowCategorySuggestions = CategorySuggestions.Any();
        OnPropertyChanged(nameof(HasCategoryPredictionService));
    }

    private void ApplyCategorySuggestion(CategorySuggestion? suggestion)
    {
        if (suggestion != null)
        {
            Category = suggestion.Category;
            ShowCategorySuggestions = false;
        }
    }

    private void LoadStores()
    {
        AvailableStores.Clear();
        var stores = _mediator.Send(new GetAllPlacesQuery()).GetAwaiter().GetResult();
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
        var records = _mediator.Send(new GetPriceHistoryQuery(ItemId: ItemId, PlaceId: null, From: null, To: null))
            .GetAwaiter().GetResult();

        foreach (var record in records.OrderByDescending(r => r.DateRecorded))
        {
            var place = _mediator.Send(new GetPlaceByIdQuery(record.PlaceId)).GetAwaiter().GetResult();
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
            var result = _mediator.Send(new RecordPriceCommand(
                ItemId,
                SelectedStore.Id,
                finalPrice,
                IsOnSale,
                originalPrice)).GetAwaiter().GetResult();

            if (!result.Success)
            {
                _dialogService.ShowError($"Failed to add price record: {result.ErrorMessage}");
                return;
            }

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
                // Add new item via CreateItemCommand (covers 7 core fields)
                var createResult = _mediator.Send(new CreateItemCommand(
                    Name,
                    string.IsNullOrWhiteSpace(Brand) ? null : Brand,
                    string.IsNullOrWhiteSpace(Category) ? null : Category,
                    string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    string.IsNullOrWhiteSpace(PackageSize) ? null : PackageSize,
                    string.IsNullOrWhiteSpace(Unit) ? null : Unit,
                    string.IsNullOrWhiteSpace(Description) ? null : Description
                )).GetAwaiter().GetResult();

                if (!createResult.Success || createResult.Item == null)
                {
                    _dialogService.ShowError($"Failed to create item: {createResult.ErrorMessage}");
                    return false;
                }

                // Persist extended fields (SubCategory, ImageUrl, Tags, Allergens, DietaryFlags)
                // via UpdateItemCommand now that the command supports them.
                var parsedTags = string.IsNullOrWhiteSpace(TagsText)
                    ? null
                    : (IEnumerable<string>)TagsText.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                var parsedAllergens = string.IsNullOrWhiteSpace(AllergensText)
                    ? null
                    : (IEnumerable<string>)AllergensText.Split(',')
                        .Select(a => a.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .ToList();
                var parsedDietaryFlags = string.IsNullOrWhiteSpace(DietaryFlagsText)
                    ? null
                    : (IEnumerable<string>)DietaryFlagsText.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .ToList();

                _mediator.Send(new UpdateItemCommand(
                    createResult.Item.Id,
                    SubCategory: string.IsNullOrWhiteSpace(SubCategory) ? null : SubCategory,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                    Tags: parsedTags,
                    Allergens: parsedAllergens,
                    DietaryFlags: parsedDietaryFlags
                )).GetAwaiter().GetResult();

                ItemId = createResult.ItemId; // Set ItemId so user can add prices
                _dialogService.ShowSuccess($"Item '{Name}' added successfully!");
            }
            else
            {
                // Update existing item including all extended fields
                var editTags = string.IsNullOrWhiteSpace(TagsText)
                    ? null
                    : (IEnumerable<string>)TagsText.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                var editAllergens = string.IsNullOrWhiteSpace(AllergensText)
                    ? null
                    : (IEnumerable<string>)AllergensText.Split(',')
                        .Select(a => a.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .ToList();
                var editDietaryFlags = string.IsNullOrWhiteSpace(DietaryFlagsText)
                    ? null
                    : (IEnumerable<string>)DietaryFlagsText.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .ToList();

                var updateResult = _mediator.Send(new UpdateItemCommand(
                    ItemId,
                    string.IsNullOrWhiteSpace(Name) ? null : Name,
                    string.IsNullOrWhiteSpace(Brand) ? null : Brand,
                    string.IsNullOrWhiteSpace(Category) ? null : Category,
                    string.IsNullOrWhiteSpace(Barcode) ? null : Barcode,
                    string.IsNullOrWhiteSpace(PackageSize) ? null : PackageSize,
                    string.IsNullOrWhiteSpace(Unit) ? null : Unit,
                    string.IsNullOrWhiteSpace(Description) ? null : Description,
                    SubCategory: string.IsNullOrWhiteSpace(SubCategory) ? null : SubCategory,
                    ImageUrl: string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl,
                    Tags: editTags,
                    Allergens: editAllergens,
                    DietaryFlags: editDietaryFlags
                )).GetAwaiter().GetResult();

                if (!updateResult.Success)
                {
                    _dialogService.ShowError($"Failed to update item: {updateResult.ErrorMessage}");
                    return false;
                }

                _dialogService.ShowSuccess($"Item '{Name}' updated successfully!");
            }

            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to save item: {ex.Message}");
            return false;
        }
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

        // Don't show suggestions when loading an existing item
        ShowCategorySuggestions = false;
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

/// <summary>
/// Represents a category suggestion with confidence level
/// </summary>
public class CategorySuggestion
{
    public string Category { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string ConfidenceText { get; set; } = string.Empty;
}
