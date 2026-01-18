using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.Data.LiteDB.Services;

namespace AdvGenPriceComparer.WPF.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly GroceryDataService _dataService;
        private readonly ILoggerService _logger;
        private ObservableCollection<string> _categories;
        private string _selectedCategory;
        private string _newCategoryName;
        private int _itemCount;

        public CategoryViewModel(GroceryDataService dataService, ILoggerService logger)
        {
            _dataService = dataService;
            _logger = logger;
            _categories = new ObservableCollection<string>();

            AddCategoryCommand = new RelayCommand(() => AddCategory(), () => CanAddCategory());
            DeleteCategoryCommand = new RelayCommand(() => DeleteCategory(), () => CanDeleteCategory());
            RefreshCommand = new RelayCommand(() => LoadCategories());

            LoadCategories();
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemCount));
                CommandManager.InvalidateRequerySuggested();
                UpdateItemCount();
            }
        }

        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                _newCategoryName = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadCategories()
        {
            try
            {
                _logger.LogInfo("Loading categories...");
                Categories.Clear();

                // Get all unique categories from items
                var allItems = _dataService.Items.GetAll();
                var uniqueCategories = allItems
                    .Select(i => i.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (var category in uniqueCategories)
                {
                    Categories.Add(category);
                }

                _logger.LogInfo($"Loaded {Categories.Count} categories");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading categories: {ex.Message}", ex);
            }
        }

        private void UpdateItemCount()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                ItemCount = 0;
                return;
            }

            try
            {
                var items = _dataService.Items.GetAll()
                    .Where(i => i.Category == SelectedCategory)
                    .ToList();

                ItemCount = items.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating item count: {ex.Message}", ex);
                ItemCount = 0;
            }
        }

        private bool CanAddCategory()
        {
            return !string.IsNullOrWhiteSpace(NewCategoryName) &&
                   !Categories.Contains(NewCategoryName.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        private void AddCategory()
        {
            try
            {
                var categoryName = NewCategoryName.Trim();
                _logger.LogInfo($"Adding new category: {categoryName}");

                // Add to the list (it will be saved when an item uses it)
                Categories.Add(categoryName);

                // Sort the list
                var sortedCategories = Categories.OrderBy(c => c).ToList();
                Categories.Clear();
                foreach (var cat in sortedCategories)
                {
                    Categories.Add(cat);
                }

                NewCategoryName = string.Empty;
                _logger.LogInfo($"Category '{categoryName}' added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding category: {ex.Message}", ex);
            }
        }

        private bool CanDeleteCategory()
        {
            return !string.IsNullOrWhiteSpace(SelectedCategory);
        }

        private void DeleteCategory()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedCategory))
                    return;

                _logger.LogInfo($"Deleting category: {SelectedCategory}");

                // Check if any items use this category
                var itemsWithCategory = _dataService.Items.GetAll()
                    .Where(i => i.Category == SelectedCategory)
                    .ToList();

                if (itemsWithCategory.Any())
                {
                    _logger.LogWarning($"Cannot delete category '{SelectedCategory}' - {itemsWithCategory.Count} items are using it");
                    System.Windows.MessageBox.Show(
                        $"Cannot delete category '{SelectedCategory}' because {itemsWithCategory.Count} item(s) are using it.\n\nPlease reassign or delete those items first.",
                        "Cannot Delete Category",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Remove from list
                Categories.Remove(SelectedCategory);
                SelectedCategory = null;

                _logger.LogInfo("Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting category: {ex.Message}", ex);
                System.Windows.MessageBox.Show(
                    $"Error deleting category: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
