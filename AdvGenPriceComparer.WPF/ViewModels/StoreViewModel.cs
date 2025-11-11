using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class StoreViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<Place> _stores = new();
    private ObservableCollection<Place> _allStores = new();
    private Place? _selectedStore;
    private string _searchText = string.Empty;
    private string _selectedChain = "All Chains";
    private ObservableCollection<string> _chains = new();

    public StoreViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;

        // Initialize commands
        AddStoreCommand = new RelayCommand(AddStore);
        EditStoreCommand = new RelayCommand<Place>(EditStore);
        DeleteStoreCommand = new RelayCommand<Place>(DeleteStore);

        // Load data
        LoadStores();
        LoadChains();
    }

    public ObservableCollection<Place> Stores
    {
        get => _stores;
        set => SetProperty(ref _stores, value);
    }

    public Place? SelectedStore
    {
        get => _selectedStore;
        set => SetProperty(ref _selectedStore, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterStores();
            }
        }
    }

    public string SelectedChain
    {
        get => _selectedChain;
        set
        {
            if (SetProperty(ref _selectedChain, value))
            {
                FilterStores();
            }
        }
    }

    public ObservableCollection<string> Chains
    {
        get => _chains;
        set => SetProperty(ref _chains, value);
    }

    public string StoreCountText => $"{Stores.Count} {(Stores.Count == 1 ? "store" : "stores")}";

    public RelayCommand AddStoreCommand { get; }
    public RelayCommand<Place> EditStoreCommand { get; }
    public RelayCommand<Place> DeleteStoreCommand { get; }

    private void LoadStores()
    {
        try
        {
            var stores = _dataService.GetAllPlaces().ToList();
            _allStores = new ObservableCollection<Place>(stores);
            FilterStores();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load stores: {ex.Message}");
        }
    }

    private void LoadChains()
    {
        try
        {
            var chains = _dataService.GetAllPlaces()
                .Select(s => s.Chain)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            Chains.Clear();
            Chains.Add("All Chains");
            foreach (var chain in chains)
            {
                Chains.Add(chain!);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load chains: {ex.Message}");
        }
    }

    private void FilterStores()
    {
        Stores.Clear();
        var filtered = _allStores.AsEnumerable();

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(s =>
                s.Name.ToLowerInvariant().Contains(searchLower) ||
                (s.Chain?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (s.Suburb?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (s.State?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (s.Address?.ToLowerInvariant().Contains(searchLower) ?? false));
        }

        // Filter by chain
        if (SelectedChain != "All Chains")
        {
            filtered = filtered.Where(s => s.Chain == SelectedChain);
        }

        foreach (var store in filtered)
        {
            Stores.Add(store);
        }

        OnPropertyChanged(nameof(StoreCountText));
    }

    private void AddStore()
    {
        try
        {
            var dialog = new Views.AddStoreWindow(
                new AddStoreViewModel(_dataService, _dialogService))
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                LoadStores();
                LoadChains();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to open add store dialog: {ex.Message}");
        }
    }

    private void EditStore(Place? store)
    {
        if (store == null) return;

        try
        {
            var viewModel = new AddStoreViewModel(_dataService, _dialogService)
            {
                StoreId = store.Id,
                StoreName = store.Name,
                Chain = store.Chain ?? string.Empty,
                Address = store.Address ?? string.Empty,
                Suburb = store.Suburb ?? string.Empty,
                State = store.State ?? string.Empty,
                Postcode = store.Postcode ?? string.Empty,
                Phone = store.Phone ?? string.Empty
            };

            var dialog = new Views.AddStoreWindow(viewModel)
            {
                Owner = Application.Current.MainWindow,
                Title = "Edit Store"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadStores();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to edit store: {ex.Message}");
        }
    }

    private void DeleteStore(Place? store)
    {
        if (store == null) return;

        try
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{store.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _dataService.Places.Delete(store.Id);
                LoadStores();
                LoadChains();
                _dialogService.ShowSuccess($"Store '{store.Name}' deleted successfully.");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to delete store: {ex.Message}");
        }
    }
}
