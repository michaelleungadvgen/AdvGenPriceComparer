using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvGenPriceComparer.Desktop.WinUI.Controls
{
    public sealed partial class AddEditCategoryControl : UserControl
    {
        private List<string> _subcategories = new();
        private bool _isEditMode = false;
        private string _originalCategoryName = "";

        public event EventHandler<bool> ValidationChanged;

        public AddEditCategoryControl()
        {
            this.InitializeComponent();
            CategoryNameTextBox.TextChanged += OnTextChanged;
            CategoryDescriptionTextBox.TextChanged += OnTextChanged;
            CategoryIconComboBox.SelectionChanged += OnSelectionChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidationChanged?.Invoke(this, IsValid());
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidationChanged?.Invoke(this, IsValid());
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CategoryNameTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(CategoryDescriptionTextBox.Text) &&
                   CategoryIconComboBox.SelectedItem != null;
        }

        public void SetEditMode(string categoryName, string description, string icon, List<string> subcategories)
        {
            _isEditMode = true;
            _originalCategoryName = categoryName;
            HeaderText.Text = $"Edit Category: {categoryName}";

            CategoryNameTextBox.Text = categoryName;
            CategoryDescriptionTextBox.Text = description;
            
            // Set icon selection
            foreach (ComboBoxItem item in CategoryIconComboBox.Items)
            {
                if (item.Content.ToString().Contains(icon))
                {
                    CategoryIconComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load subcategories
            _subcategories = new List<string>(subcategories ?? new List<string>());
            RefreshSubcategoriesDisplay();
        }

        public CategoryData GetCategoryData()
        {
            var selectedIcon = CategoryIconComboBox.SelectedItem as ComboBoxItem;
            var iconText = selectedIcon?.Content.ToString() ?? "";
            var icon = iconText.Split(' ')[0]; // Extract just the emoji

            return new CategoryData
            {
                Name = CategoryNameTextBox.Text.Trim(),
                Description = CategoryDescriptionTextBox.Text.Trim(),
                Icon = icon,
                Subcategories = new List<string>(_subcategories),
                IsEditMode = _isEditMode,
                OriginalName = _originalCategoryName
            };
        }

        private void AddSubcategory_Click(object sender, RoutedEventArgs e)
        {
            SubcategoryInputGrid.Visibility = Visibility.Visible;
            SubcategoryTextBox.Focus(FocusState.Keyboard);
        }

        private void ConfirmAddSubcategory_Click(object sender, RoutedEventArgs e)
        {
            var subcategoryName = SubcategoryTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(subcategoryName) && !_subcategories.Contains(subcategoryName))
            {
                _subcategories.Add(subcategoryName);
                RefreshSubcategoriesDisplay();
            }

            SubcategoryTextBox.Text = "";
            SubcategoryInputGrid.Visibility = Visibility.Collapsed;
        }

        private void CancelAddSubcategory_Click(object sender, RoutedEventArgs e)
        {
            SubcategoryTextBox.Text = "";
            SubcategoryInputGrid.Visibility = Visibility.Collapsed;
        }

        private void RefreshSubcategoriesDisplay()
        {
            SubcategoriesPanel.Children.Clear();
            SampleSubcategoriesPanel.Visibility = _subcategories.Any() ? Visibility.Collapsed : Visibility.Visible;

            foreach (var subcategory in _subcategories)
            {
                var border = new Border
                {
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    Padding = new Thickness(8),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 0, 0, 4)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var textBlock = new TextBlock
                {
                    Text = subcategory,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var removeButton = new Button
                {
                    Content = "×",
                    FontSize = 12,
                    Padding = new Thickness(4),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    BorderThickness = new Thickness(0),
                    Tag = subcategory
                };

                removeButton.Click += (s, e) =>
                {
                    var buttonSender = s as Button;
                    var subcategoryToRemove = buttonSender?.Tag as string;
                    if (!string.IsNullOrEmpty(subcategoryToRemove))
                    {
                        _subcategories.Remove(subcategoryToRemove);
                        RefreshSubcategoriesDisplay();
                    }
                };

                Grid.SetColumn(textBlock, 0);
                Grid.SetColumn(removeButton, 1);

                grid.Children.Add(textBlock);
                grid.Children.Add(removeButton);
                border.Child = grid;

                SubcategoriesPanel.Children.Add(border);
            }
        }

        public void LoadSampleSubcategories()
        {
            var categoryName = CategoryNameTextBox.Text.ToLower();
            var samples = GetSampleSubcategories(categoryName);
            
            if (samples.Any())
            {
                _subcategories.AddRange(samples.Where(s => !_subcategories.Contains(s)));
                RefreshSubcategoriesDisplay();
            }
        }

        private List<string> GetSampleSubcategories(string categoryName)
        {
            var sampleData = new Dictionary<string, List<string>>
            {
                ["bakery"] = new() { "Bread", "Pastries", "Cakes", "Rolls", "Bagels" },
                ["dairy"] = new() { "Milk", "Cheese", "Yogurt", "Butter", "Cream" },
                ["meat"] = new() { "Beef", "Chicken", "Pork", "Lamb", "Seafood", "Processed" },
                ["produce"] = new() { "Fruit", "Vegetables", "Herbs", "Organic Produce" },
                ["pantry"] = new() { "Canned Goods", "Pasta", "Rice", "Cereals", "Condiments", "Spices" },
                ["frozen"] = new() { "Frozen Meals", "Ice Cream", "Frozen Vegetables", "Frozen Meat" },
                ["beverages"] = new() { "Soft Drinks", "Juices", "Water", "Tea & Coffee", "Energy Drinks" },
                ["snacks"] = new() { "Chips", "Chocolate", "Cookies", "Nuts", "Crackers" }
            };

            return sampleData.ContainsKey(categoryName) ? sampleData[categoryName] : new List<string>();
        }
    }

    public class CategoryData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public List<string> Subcategories { get; set; } = new();
        public bool IsEditMode { get; set; } = false;
        public string OriginalName { get; set; } = "";
    }
}