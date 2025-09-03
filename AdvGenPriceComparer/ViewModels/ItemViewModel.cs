using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

public class ItemViewModel : BaseViewModel
{
    private string _name = string.Empty;
    private string _brand = string.Empty;
    private string _description = string.Empty;
    private string _category = string.Empty;
    private string _subCategory = string.Empty;
    private string _packageSize = string.Empty;
    private string _unit = string.Empty;
    private string _barcode = string.Empty;
    private string _imageUrl = string.Empty;
    private string _tags = string.Empty;
    private bool _isValid = false;
    private string _validationErrors = string.Empty;
    private string _packageSizeHint = string.Empty;
    private bool _hasPackageSizeError = false;
    private bool _hasBarcodeError = false;

    public ItemViewModel()
    {
        InitializeCategories();
        InitializeDietaryFlags();
        InitializeAllergens();
        
        PropertyChanged += (s, e) => ValidateItem();
    }

    #region Properties

    [Required(ErrorMessage = "Product name is required")]
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

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value, OnCategoryChanged);
    }

    public string SubCategory
    {
        get => _subCategory;
        set => SetProperty(ref _subCategory, value);
    }

    public string PackageSize
    {
        get => _packageSize;
        set => SetProperty(ref _packageSize, value, OnPackageSizeChanged);
    }

    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public string Barcode
    {
        get => _barcode;
        set => SetProperty(ref _barcode, value, OnBarcodeChanged);
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public string Tags
    {
        get => _tags;
        set => SetProperty(ref _tags, value);
    }

    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    public string ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public string PackageSizeHint
    {
        get => _packageSizeHint;
        private set => SetProperty(ref _packageSizeHint, value);
    }

    public bool HasPackageSizeError
    {
        get => _hasPackageSizeError;
        private set => SetProperty(ref _hasPackageSizeError, value);
    }

    public bool HasBarcodeError
    {
        get => _hasBarcodeError;
        private set => SetProperty(ref _hasBarcodeError, value);
    }

    #endregion

    #region Collections

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> SubCategories { get; } = new();
    public ObservableCollection<string> Units { get; } = new();
    public ObservableCollection<DietaryFlagViewModel> DietaryFlags { get; } = new();
    public ObservableCollection<AllergenViewModel> Allergens { get; } = new();

    #endregion

    #region Methods

    private void InitializeCategories()
    {
        var categories = new[] { "Bakery", "Dairy", "Meat", "Produce", "Pantry", "Frozen", "Beverages", "Snacks", "Health & Beauty", "Household", "Other" };
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        var units = new[] { "each", "g", "kg", "ml", "L", "pack", "dozen" };
        foreach (var unit in units)
        {
            Units.Add(unit);
        }
    }

    private void InitializeDietaryFlags()
    {
        var flags = new[] { "Organic", "Vegan", "Vegetarian", "Gluten-Free", "Sugar-Free", "Low-Fat" };
        foreach (var flag in flags)
        {
            DietaryFlags.Add(new DietaryFlagViewModel { Name = flag });
        }
    }

    private void InitializeAllergens()
    {
        var allergens = new[] { "Gluten", "Wheat", "Milk", "Eggs", "Nuts", "Soy", "Fish", "Shellfish" };
        foreach (var allergen in allergens)
        {
            Allergens.Add(new AllergenViewModel { Name = allergen });
        }
    }

    private readonly Dictionary<string, List<string>> _subCategoriesMap = new()
    {
        ["Bakery"] = new() { "Bread", "Pastries", "Cakes", "Rolls", "Bagels" },
        ["Dairy"] = new() { "Milk", "Cheese", "Yogurt", "Butter", "Cream" },
        ["Meat"] = new() { "Beef", "Chicken", "Pork", "Lamb", "Seafood", "Processed" },
        ["Produce"] = new() { "Fruit", "Vegetables", "Herbs", "Organic Produce" },
        ["Pantry"] = new() { "Canned Goods", "Pasta", "Rice", "Cereals", "Condiments", "Spices" },
        ["Frozen"] = new() { "Frozen Meals", "Ice Cream", "Frozen Vegetables", "Frozen Meat" },
        ["Beverages"] = new() { "Soft Drinks", "Juice", "Water", "Coffee", "Tea", "Alcohol" },
        ["Snacks"] = new() { "Chips", "Chocolate", "Cookies", "Nuts", "Crackers" },
        ["Health & Beauty"] = new() { "Personal Care", "Medicine", "Vitamins", "Cosmetics" },
        ["Household"] = new() { "Cleaning", "Laundry", "Paper Products", "Kitchen Items" }
    };

    private void OnCategoryChanged()
    {
        SubCategories.Clear();
        SubCategory = string.Empty;

        if (_subCategoriesMap.TryGetValue(Category, out var subCategories))
        {
            foreach (var subCategory in subCategories)
            {
                SubCategories.Add(subCategory);
            }
        }
    }

    private void OnPackageSizeChanged()
    {
        if (string.IsNullOrEmpty(PackageSize))
        {
            PackageSizeHint = string.Empty;
            HasPackageSizeError = false;
            return;
        }

        var tempItem = new Item { Name = "Test", PackageSize = PackageSize };
        var (value, unit) = tempItem.ParsePackageSize();

        if (value.HasValue && !string.IsNullOrEmpty(unit))
        {
            PackageSizeHint = $"Parsed as: {value} {unit}";
            HasPackageSizeError = false;
        }
        else
        {
            PackageSizeHint = "Could not parse package size. Try formats like: 500g, 2L, 12 pack";
            HasPackageSizeError = true;
        }
    }

    private void OnBarcodeChanged()
    {
        if (string.IsNullOrEmpty(Barcode))
        {
            HasBarcodeError = false;
            return;
        }

        var tempItem = new Item { Name = "Test", Barcode = Barcode };
        var validation = tempItem.ValidateItem();
        var barcodeErrors = validation.Errors.Where(e => e.Contains("barcode")).ToList();
        HasBarcodeError = barcodeErrors.Any();
    }

    private void ValidateItem()
    {
        var item = CreateItem();
        var validation = item.ValidateItem();
        IsValid = validation.IsValid;
        ValidationErrors = validation.IsValid ? string.Empty : validation.GetErrorsString();
    }

    public Item CreateItem()
    {
        var item = new Item
        {
            Name = Name.Trim()
        };

        if (!string.IsNullOrEmpty(Brand?.Trim()))
            item.Brand = Brand.Trim();

        if (!string.IsNullOrEmpty(Description?.Trim()))
            item.Description = Description.Trim();

        if (!string.IsNullOrEmpty(Category?.Trim()))
            item.Category = Category.Trim();

        if (!string.IsNullOrEmpty(SubCategory?.Trim()))
            item.SubCategory = SubCategory.Trim();

        if (!string.IsNullOrEmpty(PackageSize?.Trim()))
            item.PackageSize = PackageSize.Trim();

        if (!string.IsNullOrEmpty(Unit?.Trim()))
            item.Unit = Unit.Trim();

        if (!string.IsNullOrEmpty(Barcode?.Trim()))
            item.Barcode = Barcode.Trim();

        if (!string.IsNullOrEmpty(ImageUrl?.Trim()))
            item.ImageUrl = ImageUrl.Trim();

        // Add dietary flags
        foreach (var flag in DietaryFlags.Where(f => f.IsSelected))
        {
            item.AddDietaryFlag(flag.Name);
        }

        // Add allergens
        foreach (var allergen in Allergens.Where(a => a.IsSelected))
        {
            item.AddAllergen(allergen.Name);
        }

        // Add tags
        if (!string.IsNullOrEmpty(Tags?.Trim()))
        {
            var tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                item.AddTag(tag.Trim());
            }
        }

        return item;
    }

    public void ClearForm()
    {
        Name = string.Empty;
        Brand = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        SubCategory = string.Empty;
        PackageSize = string.Empty;
        Unit = string.Empty;
        Barcode = string.Empty;
        ImageUrl = string.Empty;
        Tags = string.Empty;

        foreach (var flag in DietaryFlags)
            flag.IsSelected = false;

        foreach (var allergen in Allergens)
            allergen.IsSelected = false;
    }

    public void LoadFromItem(Item item)
    {
        Name = item.Name ?? string.Empty;
        Brand = item.Brand ?? string.Empty;
        Description = item.Description ?? string.Empty;
        Category = item.Category ?? string.Empty;
        SubCategory = item.SubCategory ?? string.Empty;
        PackageSize = item.PackageSize ?? string.Empty;
        Unit = item.Unit ?? string.Empty;
        Barcode = item.Barcode ?? string.Empty;
        ImageUrl = item.ImageUrl ?? string.Empty;

        // Load dietary flags
        if (item.DietaryFlags != null)
        {
            foreach (var flag in DietaryFlags)
            {
                flag.IsSelected = item.DietaryFlags.Contains(flag.Name);
            }
        }

        // Load allergens
        if (item.Allergens != null)
        {
            foreach (var allergen in Allergens)
            {
                allergen.IsSelected = item.Allergens.Contains(allergen.Name);
            }
        }

        // Load tags
        if (item.Tags != null && item.Tags.Any())
        {
            Tags = string.Join(", ", item.Tags);
        }
        else
        {
            Tags = string.Empty;
        }
    }

    #endregion
}

public class DietaryFlagViewModel : BaseViewModel
{
    private bool _isSelected;

    public required string Name { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public class AllergenViewModel : BaseViewModel
{
    private bool _isSelected;

    public required string Name { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}