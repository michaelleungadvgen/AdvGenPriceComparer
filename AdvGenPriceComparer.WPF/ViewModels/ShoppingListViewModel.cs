using System.IO;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class ShoppingListViewModel : ViewModelBase
{
    private readonly IShoppingListService _shoppingListService;
    private readonly IDialogService _dialogService;
    
    private ObservableCollection<ShoppingList> _shoppingLists = new();
    private ShoppingList? _selectedList;
    private ObservableCollection<ShoppingListItem> _selectedListItems = new();
    private ShoppingListItem? _selectedItem;
    private string _searchText = "";
    private string _newListName = "";
    private string _newItemName = "";
    private bool _showFavoritesOnly = false;
    private bool _isEditingList = false;

    public ShoppingListViewModel(IShoppingListService shoppingListService, IDialogService dialogService)
    {
        _shoppingListService = shoppingListService;
        _dialogService = dialogService;

        // Commands
        CreateListCommand = new RelayCommand(CreateList, () => !string.IsNullOrWhiteSpace(NewListName));
        DeleteListCommand = new RelayCommand<ShoppingList>(DeleteList);
        DuplicateListCommand = new RelayCommand<ShoppingList>(DuplicateList);
        ToggleFavoriteCommand = new RelayCommand<ShoppingList>(ToggleFavorite);
        SelectListCommand = new RelayCommand<ShoppingList>(SelectList);
        
        AddItemCommand = new RelayCommand(AddItem, () => SelectedList != null && !string.IsNullOrWhiteSpace(NewItemName));
        DeleteItemCommand = new RelayCommand<ShoppingListItem>(DeleteItem);
        ToggleItemCheckedCommand = new RelayCommand<ShoppingListItem>(ToggleItemChecked);
        ClearCheckedItemsCommand = new RelayCommand(ClearCheckedItems, () => SelectedList?.CheckedItems > 0);
        ClearAllItemsCommand = new RelayCommand(ClearAllItems, () => SelectedList?.TotalItems > 0);
        
        ExportToMarkdownCommand = new RelayCommand(ExportToMarkdown, () => SelectedList != null);
        
        LoadShoppingLists();
    }

    #region Properties

    public ObservableCollection<ShoppingList> ShoppingLists
    {
        get => _shoppingLists;
        set => SetProperty(ref _shoppingLists, value);
    }

    public ShoppingList? SelectedList
    {
        get => _selectedList;
        set
        {
            if (SetProperty(ref _selectedList, value))
            {
                LoadSelectedListItems();
                OnPropertyChanged(nameof(HasSelectedList));
                OnPropertyChanged(nameof(ListProgressText));
                OnPropertyChanged(nameof(ListProgressPercent));
                
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ObservableCollection<ShoppingListItem> SelectedListItems
    {
        get => _selectedListItems;
        set => SetProperty(ref _selectedListItems, value);
    }

    public ShoppingListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                SearchLists();
            }
        }
    }

    public string NewListName
    {
        get => _newListName;
        set
        {
            if (SetProperty(ref _newListName, value))
            {
                ((RelayCommand)CreateListCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string NewItemName
    {
        get => _newItemName;
        set
        {
            if (SetProperty(ref _newItemName, value))
            {
                ((RelayCommand)AddItemCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool ShowFavoritesOnly
    {
        get => _showFavoritesOnly;
        set
        {
            if (SetProperty(ref _showFavoritesOnly, value))
            {
                LoadShoppingLists();
            }
        }
    }

    public bool IsEditingList
    {
        get => _isEditingList;
        set => SetProperty(ref _isEditingList, value);
    }

    public bool HasSelectedList => SelectedList != null;

    public string ListProgressText => SelectedList != null 
        ? $"{SelectedList.CheckedItems} of {SelectedList.TotalItems} items" 
        : "";

    public double ListProgressPercent => SelectedList != null && SelectedList.TotalItems > 0
        ? (double)SelectedList.CheckedItems / SelectedList.TotalItems * 100
        : 0;

    #endregion

    #region Commands

    public ICommand CreateListCommand { get; }
    public ICommand DeleteListCommand { get; }
    public ICommand DuplicateListCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SelectListCommand { get; }
    
    public ICommand AddItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand ToggleItemCheckedCommand { get; }
    public ICommand ClearCheckedItemsCommand { get; }
    public ICommand ClearAllItemsCommand { get; }
    
    public ICommand ExportToMarkdownCommand { get; }

    #endregion

    #region Methods

    private void LoadShoppingLists()
    {
        var lists = ShowFavoritesOnly 
            ? _shoppingListService.GetFavoriteShoppingLists()
            : _shoppingListService.GetAllShoppingLists();
            
        ShoppingLists = new ObservableCollection<ShoppingList>(lists);
    }

    private void SearchLists()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            LoadShoppingLists();
            return;
        }

        var results = _shoppingListService.SearchShoppingLists(SearchText);
        ShoppingLists = new ObservableCollection<ShoppingList>(results);
    }

    private void LoadSelectedListItems()
    {
        if (SelectedList == null)
        {
            SelectedListItems = new ObservableCollection<ShoppingListItem>();
            return;
        }

        // Refresh from service to get latest data
        var refreshedList = _shoppingListService.GetShoppingList(SelectedList.Id);
        if (refreshedList != null)
        {
            SelectedList = refreshedList;
            var items = refreshedList.Items.OrderBy(i => i.IsChecked).ThenByDescending(i => i.Priority).ThenBy(i => i.Name);
            SelectedListItems = new ObservableCollection<ShoppingListItem>(items);
        }
    }

    private void CreateList()
    {
        if (string.IsNullOrWhiteSpace(NewListName)) return;

        var list = _shoppingListService.CreateShoppingList(NewListName.Trim());
        NewListName = "";
        LoadShoppingLists();
        SelectedList = list;
        
        _dialogService.ShowInfo($"Created shopping list '{list.Name}'");
    }

    private void DeleteList(ShoppingList? list)
    {
        if (list == null) return;

        var result = _dialogService.ShowQuestion(
            $"Are you sure you want to delete '{list.Name}'?",
            "Confirm Delete");

        if (result)
        {
            _shoppingListService.DeleteShoppingList(list.Id);
            LoadShoppingLists();
            if (SelectedList?.Id == list.Id)
            {
                SelectedList = null;
            }
        }
    }

    private void DuplicateList(ShoppingList? list)
    {
        if (list == null) return;

        var newName = $"{list.Name} (Copy)";
        _shoppingListService.DuplicateShoppingList(list.Id, newName);
        LoadShoppingLists();
        
        _dialogService.ShowInfo($"Duplicated shopping list to '{newName}'");
    }

    private void ToggleFavorite(ShoppingList? list)
    {
        if (list == null) return;

        _shoppingListService.MarkListAsFavorite(list.Id, !list.IsFavorite);
        LoadShoppingLists();
        
        // Refresh selected list if it was the one toggled
        if (SelectedList?.Id == list.Id)
        {
            SelectedList = _shoppingListService.GetShoppingList(list.Id);
        }
    }

    private void SelectList(ShoppingList? list)
    {
        SelectedList = list;
    }

    private void AddItem()
    {
        if (SelectedList == null || string.IsNullOrWhiteSpace(NewItemName)) return;

        var item = new ShoppingListItem
        {
            Name = NewItemName.Trim(),
            Quantity = 1,
            Unit = "ea",
            IsChecked = false,
            AddedDate = DateTime.UtcNow
        };

        _shoppingListService.AddItemToList(SelectedList.Id, item);
        NewItemName = "";
        LoadSelectedListItems();
        
        // Refresh list stats
        OnPropertyChanged(nameof(ListProgressText));
        OnPropertyChanged(nameof(ListProgressPercent));
        ((RelayCommand)ClearCheckedItemsCommand).RaiseCanExecuteChanged();
        ((RelayCommand)ClearAllItemsCommand).RaiseCanExecuteChanged();
    }

    private void DeleteItem(ShoppingListItem? item)
    {
        if (item == null || SelectedList == null) return;

        _shoppingListService.RemoveItemFromList(SelectedList.Id, item.Id);
        LoadSelectedListItems();
        
        OnPropertyChanged(nameof(ListProgressText));
        OnPropertyChanged(nameof(ListProgressPercent));
    }

    private void ToggleItemChecked(ShoppingListItem? item)
    {
        if (item == null || SelectedList == null) return;

        _shoppingListService.ToggleItemChecked(SelectedList.Id, item.Id);
        LoadSelectedListItems();
        
        OnPropertyChanged(nameof(ListProgressText));
        OnPropertyChanged(nameof(ListProgressPercent));
        ((RelayCommand)ClearCheckedItemsCommand).RaiseCanExecuteChanged();
    }

    private void ClearCheckedItems()
    {
        if (SelectedList == null) return;

        _shoppingListService.ClearCheckedItems(SelectedList.Id);
        LoadSelectedListItems();
        
        OnPropertyChanged(nameof(ListProgressText));
        OnPropertyChanged(nameof(ListProgressPercent));
        ((RelayCommand)ClearCheckedItemsCommand).RaiseCanExecuteChanged();
    }

    private void ClearAllItems()
    {
        if (SelectedList == null) return;

        var result = _dialogService.ShowQuestion(
            "Are you sure you want to remove all items from this list?",
            "Confirm Clear");

        if (result)
        {
            _shoppingListService.ClearShoppingList(SelectedList.Id);
            LoadSelectedListItems();
            
            OnPropertyChanged(nameof(ListProgressText));
            OnPropertyChanged(nameof(ListProgressPercent));
            ((RelayCommand)ClearCheckedItemsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ClearAllItemsCommand).RaiseCanExecuteChanged();
        }
    }

    private void ExportToMarkdown()
    {
        if (SelectedList == null) return;

        try
        {
            var markdown = _shoppingListService.ExportShoppingListToMarkdown(SelectedList.Id);
            
            // Save to file
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                DefaultExt = "md",
                FileName = $"{SelectedList.Name.Replace(" ", "_")}.md"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, markdown);
                _dialogService.ShowInfo($"Exported shopping list to {saveFileDialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to export: {ex.Message}");
        }
    }

    #endregion
}
