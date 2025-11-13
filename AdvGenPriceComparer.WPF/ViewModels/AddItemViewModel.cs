using System;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
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

    public AddItemViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
    }

    public string? ItemId
    {
        get => _itemId;
        set => SetProperty(ref _itemId, value);
    }

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

    private void UpdateItemFields(Core.Models.Item item)
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

    public void LoadItem(Core.Models.Item item)
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
    }
}
