using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class ItemViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<Item> _items;
    private ObservableCollection<string> _categories;
    private Item? _selectedItem;
    private string _searchText = string.Empty;
    private string _selectedCategory = "All Categories";
    private List<Item> _allItems = new();

    public ItemViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _items = new ObservableCollection<Item>();
        _categories = new ObservableCollection<string> { "All Categories" };

        AddItemCommand = new RelayCommand(AddItem);
        EditItemCommand = new RelayCommand<Item>(EditItem);
        DeleteItemCommand = new RelayCommand<Item>(DeleteItem);
        RefreshCommand = new RelayCommand(LoadItems);

        LoadItems();
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ObservableCollection<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterItems();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                FilterItems();
            }
        }
    }

    public string ItemCountText => $"{Items.Count} items";

    public ICommand AddItemCommand { get; }
    public ICommand EditItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadItems()
    {
        try
        {
            _allItems = _dataService.GetAllItems().ToList();

            // Load categories
            Categories.Clear();
            Categories.Add("All Categories");
            var categories = _allItems
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .Select(i => i.Category!)
                .Distinct()
                .OrderBy(c => c);

            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            FilterItems();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load items: {ex.Message}");
        }
    }

    private void FilterItems()
    {
        Items.Clear();

        var filtered = _allItems.AsEnumerable();

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(i =>
                i.Name.ToLowerInvariant().Contains(searchLower) ||
                (i.Brand?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (i.Category?.ToLowerInvariant().Contains(searchLower) ?? false));
        }

        // Filter by category
        if (SelectedCategory != "All Categories")
        {
            filtered = filtered.Where(i => i.Category == SelectedCategory);
        }

        foreach (var item in filtered)
        {
            Items.Add(item);
        }

        OnPropertyChanged(nameof(ItemCountText));
    }

    private void AddItem()
    {
        var viewModel = new AddItemViewModel(_dataService, _dialogService);
        var window = new Views.AddItemWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            LoadItems();
        }
    }

    private void EditItem(Item? item)
    {
        if (item == null) return;

        var viewModel = new AddItemViewModel(_dataService, _dialogService);
        viewModel.LoadItem(item);
        var window = new Views.AddItemWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            LoadItems();
        }
    }

    private void DeleteItem(Item? item)
    {
        if (item == null) return;

        var result = _dialogService.ShowConfirmation(
            $"Are you sure you want to delete '{item.Name}'?",
            "Confirm Delete");

        if (result)
        {
            try
            {
                _dataService.Items.Delete(item.Id);
                _allItems.Remove(item);
                FilterItems();
                _dialogService.ShowSuccess("Item deleted successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to delete item: {ex.Message}");
            }
        }
    }

    private bool CanEditOrDelete()
    {
        return SelectedItem != null;
    }
}
