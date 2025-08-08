using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvGenPriceComparer.Desktop.WinUI.Controls;

public sealed partial class AddItemControl : UserControl
{
    public event EventHandler<bool> ValidationChanged;
    
    private readonly Dictionary<string, List<string>> _subCategories = new()
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

    public AddItemControl()
    {
        this.InitializeComponent();
        CategoryComboBox.SelectionChanged += CategoryComboBox_SelectionChanged;
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SubCategoryComboBox.Items.Clear();
        
        if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var category = selectedItem.Content?.ToString();
            if (!string.IsNullOrEmpty(category) && _subCategories.ContainsKey(category))
            {
                foreach (var subCategory in _subCategories[category])
                {
                    SubCategoryComboBox.Items.Add(new ComboBoxItem { Content = subCategory });
                }
            }
        }
    }

    private void OnFieldChanged(object sender, object e)
    {
        ValidateForm();
    }

    private void OnPackageSizeChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var packageSize = textBox.Text;
            if (!string.IsNullOrEmpty(packageSize))
            {
                // Create a temporary item to test package size parsing
                var tempItem = new Item { Name = "Test", PackageSize = packageSize };
                var (value, unit) = tempItem.ParsePackageSize();
                
                if (value.HasValue && !string.IsNullOrEmpty(unit))
                {
                    PackageSizeHint.Text = $"Parsed as: {value} {unit}";
                    PackageSizeHint.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemAccentColor"];
                }
                else
                {
                    PackageSizeHint.Text = "Could not parse package size. Try formats like: 500g, 2L, 12 pack";
                    PackageSizeHint.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlErrorTextForegroundBrush"];
                }
            }
            else
            {
                PackageSizeHint.Text = "";
            }
        }
        
        ValidateForm();
    }

    private void OnBarcodeChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var barcode = textBox.Text;
            if (!string.IsNullOrEmpty(barcode))
            {
                // Create a temporary item to test barcode validation
                var tempItem = new Item { Name = "Test", Barcode = barcode };
                var validation = tempItem.ValidateItem();
                
                var barcodeErrors = validation.Errors.Where(e => e.Contains("barcode")).ToList();
                if (barcodeErrors.Any())
                {
                    BarcodeErrorText.Text = string.Join(", ", barcodeErrors);
                    BarcodeErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    BarcodeErrorText.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BarcodeErrorText.Visibility = Visibility.Collapsed;
            }
        }
        
        ValidateForm();
    }

    private void ValidateForm()
    {
        var item = CreateItemFromForm();
        var validation = item.ValidateItem();
        
        // Show/hide name error
        var nameErrors = validation.Errors.Where(e => e.Contains("Name")).ToList();
        if (nameErrors.Any())
        {
            NameErrorText.Text = string.Join(", ", nameErrors);
            NameErrorText.Visibility = Visibility.Visible;
        }
        else
        {
            NameErrorText.Visibility = Visibility.Collapsed;
        }

        // Show/hide validation summary
        if (!validation.IsValid)
        {
            ValidationErrors.Text = validation.GetErrorsString();
            ValidationSummary.Visibility = Visibility.Visible;
        }
        else
        {
            ValidationSummary.Visibility = Visibility.Collapsed;
        }

        // Notify parent about validation status
        ValidationChanged?.Invoke(this, validation.IsValid);
    }

    public Item CreateItemFromForm()
    {
        var item = new Item
        {
            Name = NameTextBox.Text.Trim()
        };

        // Basic information
        if (!string.IsNullOrEmpty(BrandTextBox.Text.Trim()))
            item.Brand = BrandTextBox.Text.Trim();
            
        if (!string.IsNullOrEmpty(DescriptionTextBox.Text.Trim()))
            item.Description = DescriptionTextBox.Text.Trim();

        // Category
        if (CategoryComboBox.SelectedItem is ComboBoxItem categoryItem)
            item.Category = categoryItem.Content?.ToString();
        else if (!string.IsNullOrEmpty(CategoryComboBox.Text.Trim()))
            item.Category = CategoryComboBox.Text.Trim();

        if (SubCategoryComboBox.SelectedItem is ComboBoxItem subCategoryItem)
            item.SubCategory = subCategoryItem.Content?.ToString();
        else if (!string.IsNullOrEmpty(SubCategoryComboBox.Text.Trim()))
            item.SubCategory = SubCategoryComboBox.Text.Trim();

        // Product details
        if (!string.IsNullOrEmpty(PackageSizeTextBox.Text.Trim()))
            item.PackageSize = PackageSizeTextBox.Text.Trim();

        if (UnitComboBox.SelectedItem is ComboBoxItem unitItem)
            item.Unit = unitItem.Content?.ToString();

        if (!string.IsNullOrEmpty(BarcodeTextBox.Text.Trim()))
            item.Barcode = BarcodeTextBox.Text.Trim();

        // Image URL
        if (!string.IsNullOrEmpty(ImageUrlTextBox.Text.Trim()))
            item.ImageUrl = ImageUrlTextBox.Text.Trim();

        // Dietary flags
        foreach (var child in DietaryFlagsPanel.Children)
        {
            if (child is CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag is string flag)
            {
                item.AddDietaryFlag(flag);
            }
        }

        // Allergens
        foreach (var child in AllergensPanel.Children)
        {
            if (child is CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag is string allergen)
            {
                item.AddAllergen(allergen);
            }
        }

        // Tags
        if (!string.IsNullOrEmpty(TagsTextBox.Text.Trim()))
        {
            var tags = TagsTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                item.AddTag(tag.Trim());
            }
        }

        return item;
    }

    public bool IsValid()
    {
        var item = CreateItemFromForm();
        return item.ValidateItem().IsValid;
    }

    public void ClearForm()
    {
        NameTextBox.Text = "";
        BrandTextBox.Text = "";
        DescriptionTextBox.Text = "";
        CategoryComboBox.SelectedIndex = -1;
        CategoryComboBox.Text = "";
        SubCategoryComboBox.SelectedIndex = -1;
        SubCategoryComboBox.Text = "";
        PackageSizeTextBox.Text = "";
        UnitComboBox.SelectedIndex = -1;
        BarcodeTextBox.Text = "";
        ImageUrlTextBox.Text = "";
        TagsTextBox.Text = "";

        // Clear dietary flags
        foreach (var child in DietaryFlagsPanel.Children)
        {
            if (child is CheckBox checkBox)
                checkBox.IsChecked = false;
        }

        // Clear allergens
        foreach (var child in AllergensPanel.Children)
        {
            if (child is CheckBox checkBox)
                checkBox.IsChecked = false;
        }

        // Clear validation messages
        NameErrorText.Visibility = Visibility.Collapsed;
        BarcodeErrorText.Visibility = Visibility.Collapsed;
        ValidationSummary.Visibility = Visibility.Collapsed;
        PackageSizeHint.Text = "";
    }
}