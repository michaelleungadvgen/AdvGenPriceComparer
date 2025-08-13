using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class CategoryListView : Page
    {
        public CategoryListView()
        {
            this.InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            // TODO: Load categories from database
            // For now, this displays the static category data defined in the XAML
            // Future enhancement: Connect to database service to get dynamic category data
        }

        private async void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddEditCategoryDialogAsync();
        }

        private async void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoryName = button?.Tag as string;
            
            if (!string.IsNullOrEmpty(categoryName))
            {
                await ShowAddEditCategoryDialogAsync(categoryName);
            }
        }

        private async Task ShowAddEditCategoryDialogAsync(string categoryName = null)
        {
            var addEditControl = new Controls.AddEditCategoryControl();
            
            // Set up edit mode if category name is provided
            if (!string.IsNullOrEmpty(categoryName))
            {
                var categoryData = GetCategoryData(categoryName);
                addEditControl.SetEditMode(categoryData.Name, categoryData.Description, 
                                         categoryData.Icon, categoryData.Subcategories);
            }
            
            var dialog = new ContentDialog
            {
                Title = string.IsNullOrEmpty(categoryName) ? "Add New Category" : "Edit Category",
                Content = addEditControl,
                PrimaryButtonText = string.IsNullOrEmpty(categoryName) ? "Add Category" : "Save Changes",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            // Update primary button enabled state based on validation
            addEditControl.ValidationChanged += (s, isValid) =>
            {
                dialog.IsPrimaryButtonEnabled = isValid;
            };

            // Initial validation check
            dialog.IsPrimaryButtonEnabled = addEditControl.IsValid();

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var categoryData = addEditControl.GetCategoryData();
                    
                    if (categoryData.IsEditMode)
                    {
                        await UpdateCategoryAsync(categoryData);
                        await ShowSuccessMessageAsync($"Category '{categoryData.Name}' updated successfully!");
                    }
                    else
                    {
                        await AddCategoryAsync(categoryData);
                        await ShowSuccessMessageAsync($"Category '{categoryData.Name}' added successfully!");
                    }
                    
                    // TODO: Refresh the category list display
                }
                catch (Exception ex)
                {
                    await ShowErrorMessageAsync($"Error saving category: {ex.Message}");
                }
            }
        }

        private (string Name, string Description, string Icon, List<string> Subcategories) GetCategoryData(string categoryName)
        {
            // Sample data - in real implementation, this would come from database
            var sampleData = new Dictionary<string, (string, string, string, List<string>)>
            {
                ["Bakery"] = ("Bakery", "Bread, pastries, cakes and baked goods", "🍞", 
                             new List<string> { "Bread", "Pastries", "Cakes", "Rolls", "Bagels" }),
                ["Dairy"] = ("Dairy & Chilled", "Milk, cheese, yogurt and refrigerated products", "🥛", 
                            new List<string> { "Milk", "Cheese", "Yogurt", "Butter", "Cream" }),
                ["Meat"] = ("Meat & Seafood", "Fresh and processed meat, fish and seafood", "🥩", 
                           new List<string> { "Beef", "Chicken", "Pork", "Lamb", "Seafood", "Processed" }),
                ["Produce"] = ("Fresh Produce", "Fresh fruits, vegetables and herbs", "🥬", 
                              new List<string> { "Fruit", "Vegetables", "Herbs", "Organic Produce" }),
                ["Pantry"] = ("Pantry & Cooking", "Canned goods, pasta, rice, spices and condiments", "🥫", 
                             new List<string> { "Canned Goods", "Pasta", "Rice", "Cereals", "Condiments", "Spices" }),
                ["Frozen"] = ("Frozen", "Frozen meals, ice cream and frozen foods", "🧊", 
                             new List<string> { "Frozen Meals", "Ice Cream", "Frozen Vegetables", "Frozen Meat" })
            };

            return sampleData.ContainsKey(categoryName) ? sampleData[categoryName] : 
                   (categoryName, "", "", new List<string>());
        }

        private async Task AddCategoryAsync(Controls.CategoryData categoryData)
        {
            // TODO: Add category to database
            // For now, this is a placeholder
            await Task.Delay(100); // Simulate async operation
        }

        private async Task UpdateCategoryAsync(Controls.CategoryData categoryData)
        {
            // TODO: Update category in database
            // For now, this is a placeholder
            await Task.Delay(100); // Simulate async operation
        }

        private async Task ShowSuccessMessageAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}