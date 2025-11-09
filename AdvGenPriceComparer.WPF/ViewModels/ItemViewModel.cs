using System;
using System.Collections.ObjectModel;
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
    private Item? _selectedItem;

    public ItemViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _items = new ObservableCollection<Item>();

        AddItemCommand = new RelayCommand(AddItem);
        EditItemCommand = new RelayCommand(EditItem, CanEditOrDelete);
        DeleteItemCommand = new RelayCommand(DeleteItem, CanEditOrDelete);
        RefreshCommand = new RelayCommand(LoadItems);

        LoadItems();
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
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

    public ICommand AddItemCommand { get; }
    public ICommand EditItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadItems()
    {
        try
        {
            Items.Clear();
            var items = _dataService.GetAllItems();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load items: {ex.Message}");
        }
    }

    private void AddItem()
    {
        _dialogService.ShowInfo("Add Item dialog will be implemented.");
    }

    private void EditItem()
    {
        if (SelectedItem == null) return;
        _dialogService.ShowInfo($"Edit Item dialog will be implemented for: {SelectedItem.Name}");
    }

    private void DeleteItem()
    {
        if (SelectedItem == null) return;

        var result = _dialogService.ShowConfirmation(
            $"Are you sure you want to delete '{SelectedItem.Name}'?",
            "Confirm Delete");

        if (result)
        {
            try
            {
                _dataService.Items.Delete(SelectedItem.Id);
                Items.Remove(SelectedItem);
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
