using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class ItemListView : Page
    {
        private readonly IGroceryDataService _groceryDataService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private ObservableCollection<ItemWithPricesViewModel> _items;
        private ObservableCollection<ItemWithPricesViewModel> _filteredItems;

        public ItemListView()
        {
            this.InitializeComponent();
            
            // Initialize services
            _groceryDataService = App.Services.GetRequiredService<IGroceryDataService>();
            _dialogService = App.Services.GetRequiredService<IDialogService>();
            _notificationService = App.Services.GetRequiredService<INotificationService>();
            
            _items = new ObservableCollection<ItemWithPricesViewModel>();
            _filteredItems = new ObservableCollection<ItemWithPricesViewModel>();
            
            ItemsList.ItemsSource = _filteredItems;
            
            LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                ItemsList.Visibility = Visibility.Collapsed;
                EmptyPanel.Visibility = Visibility.Collapsed;
                
                // Load items from database
                var items = await Task.Run(() => _groceryDataService.GetAllItems());
                
                _items.Clear();
                
                foreach (var item in items)
                {
                    // Load price history for each item
                    var priceHistory = _groceryDataService.GetPriceHistory(item.Id);
                    var itemViewModel = new ItemWithPricesViewModel(item, priceHistory);
                    _items.Add(itemViewModel);
                }
                
                // If no items found, generate some demo data
                if (_items.Count == 0)
                {
                    await GenerateDemoItems();
                }
                
                ApplyFilter();
                
                LoadingPanel.Visibility = Visibility.Collapsed;
                
                if (_filteredItems.Count == 0)
                {
                    EmptyPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    ItemsList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                EmptyPanel.Visibility = Visibility.Visible;
                
                await _notificationService.ShowErrorAsync($"Error loading items: {ex.Message}");
            }
        }

        private async Task GenerateDemoItems()
        {
            try
            {
                // Create some demo items if none exist
                var demoItems = new[]
                {
                    new Item { Name = "Bread White Sliced 680g", Brand = "Tip Top", Category = "Bakery" },
                    new Item { Name = "Milk Full Cream 2L", Brand = "Dairy Farmers", Category = "Dairy" },
                    new Item { Name = "Bananas per kg", Brand = "Fresh", Category = "Produce", Unit = "kg" },
                    new Item { Name = "Chicken Breast 1kg", Brand = "Fresh", Category = "Meat", Unit = "kg" },
                    new Item { Name = "Rice Jasmine 1kg", Brand = "SunRice", Category = "Pantry" }
                };

                var places = _groceryDataService.GetAllPlaces().ToList();
                if (places.Count == 0)
                {
                    // Create demo places first
                    places = new List<Place>
                    {
                        new Place { Name = "Coles Westfield", Chain = "Coles", Suburb = "Sydney", State = "NSW" },
                        new Place { Name = "Woolworths Metro", Chain = "Woolworths", Suburb = "Sydney", State = "NSW" },
                        new Place { Name = "IGA Express", Chain = "IGA", Suburb = "Sydney", State = "NSW" }
                    };
                    
                    foreach (var place in places)
                    {
                        _groceryDataService.AddSupermarket(place.Name, place.Chain, suburb: place.Suburb, state: place.State);
                    }
                }

                foreach (var item in demoItems)
                {
                    var itemId = _groceryDataService.AddGroceryItem(item.Name, item.Brand, item.Category, unit: item.Unit);
                    
                    // Add some price history
                    var random = new Random();
                    var basePrice = new decimal[] { 3.50m, 4.20m, 3.80m, 12.50m, 4.00m }[Array.IndexOf(demoItems, item)];
                    
                    for (int i = 0; i < 3; i++)
                    {
                        var place = places[random.Next(places.Count)];
                        var priceVariation = (decimal)(random.NextDouble() * 0.4 - 0.2); // ±20%
                        var price = Math.Max(0.50m, basePrice + (basePrice * priceVariation));
                        var date = DateTime.Now.AddDays(-random.Next(1, 30));
                        
                        _groceryDataService.RecordPrice(itemId, place.Id, price, source: "demo");
                    }
                    
                    // Create ViewModel with price history  
                    var createdItem = _groceryDataService.GetItemById(itemId);
                    if (createdItem != null)
                    {
                        var priceHistory = _groceryDataService.GetPriceHistory(itemId);
                        var itemViewModel = new ItemWithPricesViewModel(createdItem, priceHistory);
                        _items.Add(itemViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowErrorAsync($"Error generating demo data: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            _filteredItems.Clear();
            
            var searchText = SearchBox?.Text?.Trim().ToLowerInvariant() ?? "";
            
            var filtered = string.IsNullOrEmpty(searchText) 
                ? _items 
                : _items.Where(item => 
                    item.Name.ToLowerInvariant().Contains(searchText) ||
                    item.Brand.ToLowerInvariant().Contains(searchText) ||
                    item.Category.ToLowerInvariant().Contains(searchText));
            
            foreach (var item in filtered.OrderBy(i => i.Name))
            {
                _filteredItems.Add(item);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
            
            if (_filteredItems.Count == 0 && !string.IsNullOrEmpty(SearchBox.Text))
            {
                ItemsList.Visibility = Visibility.Collapsed;
                EmptyPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ItemsList.Visibility = Visibility.Visible;
                EmptyPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var itemViewModel = new ItemViewModel();
                // TODO: Implement proper item dialog
                var itemId = _groceryDataService.AddGroceryItem("New Item", "Brand", "Category");
                var item = _groceryDataService.GetItemById(itemId);
                
                if (item != null)
                {
                    
                    // Add to our collection
                    var newItemViewModel = new ItemWithPricesViewModel(item);
                    _items.Add(newItemViewModel);
                    ApplyFilter();
                    
                    await _notificationService.ShowSuccessAsync("Item added successfully!");
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowErrorAsync($"Error adding item: {ex.Message}");
            }
        }

        private async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ItemWithPricesViewModel itemViewModel)
                {
                    var editViewModel = new ItemViewModel();
                    editViewModel.LoadFromItem(itemViewModel.GetItem());
                    
                    // Create a simple edit dialog using ContentDialog
                    var dialog = new ContentDialog()
                    {
                        Title = "Edit Item",
                        Content = CreateEditContent(editViewModel),
                        PrimaryButtonText = "Save",
                        CloseButtonText = "Cancel",
                        XamlRoot = this.XamlRoot
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary && editViewModel.IsValid)
                    {
                        var updatedItem = editViewModel.CreateItem();
                        updatedItem.Id = itemViewModel.Id; // Preserve the original ID
                        
                        // Update the item in the database
                        _groceryDataService.Items.Update(updatedItem);
                        
                        // Update the view model
                        var updatedItemFromDb = _groceryDataService.GetItemById(itemViewModel.Id);
                        if (updatedItemFromDb != null)
                        {
                            // Remove old item from collections
                            var oldItem = _items.FirstOrDefault(i => i.Id == itemViewModel.Id);
                            if (oldItem != null)
                            {
                                _items.Remove(oldItem);
                                _filteredItems.Remove(oldItem);
                            }
                            
                            // Create new view model with updated data
                            var priceHistory = _groceryDataService.GetPriceHistory(itemViewModel.Id);
                            var newItemViewModel = new ItemWithPricesViewModel(updatedItemFromDb, priceHistory);
                            
                            // Add updated item to collections
                            _items.Add(newItemViewModel);
                            ApplyFilter();
                            
                            await _notificationService.ShowSuccessAsync($"Item '{updatedItem.Name}' updated successfully!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowErrorAsync($"Error editing item: {ex.Message}");
            }
        }

        private FrameworkElement CreateEditContent(ItemViewModel editViewModel)
        {
            var panel = new StackPanel { Spacing = 12 };

            // Name
            panel.Children.Add(new TextBlock { Text = "Name *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var nameBox = new TextBox { Text = editViewModel.Name };
            nameBox.TextChanged += (s, e) => editViewModel.Name = nameBox.Text;
            panel.Children.Add(nameBox);

            // Brand
            panel.Children.Add(new TextBlock { Text = "Brand", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var brandBox = new TextBox { Text = editViewModel.Brand };
            brandBox.TextChanged += (s, e) => editViewModel.Brand = brandBox.Text;
            panel.Children.Add(brandBox);

            // Category
            panel.Children.Add(new TextBlock { Text = "Category", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var categoryBox = new ComboBox { ItemsSource = editViewModel.Categories, SelectedItem = editViewModel.Category };
            categoryBox.SelectionChanged += (s, e) => editViewModel.Category = categoryBox.SelectedItem?.ToString() ?? "";
            panel.Children.Add(categoryBox);

            // Description
            panel.Children.Add(new TextBlock { Text = "Description", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var descBox = new TextBox { Text = editViewModel.Description, AcceptsReturn = true, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap, Height = 60 };
            descBox.TextChanged += (s, e) => editViewModel.Description = descBox.Text;
            panel.Children.Add(descBox);

            // Package Size
            panel.Children.Add(new TextBlock { Text = "Package Size", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var sizeBox = new TextBox { Text = editViewModel.PackageSize, PlaceholderText = "e.g. 500g, 2L, 12 pack" };
            sizeBox.TextChanged += (s, e) => editViewModel.PackageSize = sizeBox.Text;
            panel.Children.Add(sizeBox);

            // Unit
            panel.Children.Add(new TextBlock { Text = "Unit", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var unitBox = new ComboBox { ItemsSource = editViewModel.Units, SelectedItem = editViewModel.Unit };
            unitBox.SelectionChanged += (s, e) => editViewModel.Unit = unitBox.SelectedItem?.ToString() ?? "";
            panel.Children.Add(unitBox);

            return new ScrollViewer { Content = panel, MaxHeight = 400 };
        }

        private async void AddPrice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ItemWithPricesViewModel itemViewModel)
                {
                    // For now, show a simple dialog - in the future this could be a proper price entry dialog
                    var places = _groceryDataService.GetAllPlaces().ToList();
                    
                    if (places.Count == 0)
                    {
                        await _notificationService.ShowWarningAsync("Please add at least one store before recording prices.");
                        return;
                    }
                    
                    // Simple price entry - in a real app this would be a proper dialog
                    var place = places.First();
                    var random = new Random();
                    var basePrice = itemViewModel.PriceHistory.Count > 0 
                        ? itemViewModel.PriceHistory.First().Price 
                        : 5.00m;
                    var newPrice = Math.Max(0.50m, basePrice + ((decimal)(random.NextDouble() * 2 - 1))); // ±$1
                    
                    _groceryDataService.RecordPrice(itemViewModel.Id, place.Id, newPrice);
                    
                    // Update the view model by reloading price history
                    var updatedPriceHistory = _groceryDataService.GetPriceHistory(itemViewModel.Id);
                    itemViewModel.LoadPriceHistory(updatedPriceHistory);
                    
                    await _notificationService.ShowSuccessAsync($"Price ${newPrice:F2} added for {itemViewModel.Name} at {place.Name}!");
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowErrorAsync($"Error adding price: {ex.Message}");
            }
        }
    }
}