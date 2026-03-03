using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

public class ShoppingListViewModelTests
{
    private readonly TestShoppingListService _shoppingListService;
    private readonly TestDialogService _dialogService;
    private readonly ShoppingListViewModel _viewModel;

    public ShoppingListViewModelTests()
    {
        _shoppingListService = new TestShoppingListService();
        _dialogService = new TestDialogService();
        _viewModel = new ShoppingListViewModel(_shoppingListService, _dialogService);
    }

    [Fact]
    public void Constructor_Initializes_Commands_And_Collections()
    {
        Assert.NotNull(_viewModel.ShoppingLists);
        Assert.NotNull(_viewModel.CreateListCommand);
        Assert.NotNull(_viewModel.DeleteListCommand);
        Assert.NotNull(_viewModel.DuplicateListCommand);
        Assert.NotNull(_viewModel.ToggleFavoriteCommand);
        Assert.NotNull(_viewModel.SelectListCommand);
        Assert.NotNull(_viewModel.AddItemCommand);
        Assert.NotNull(_viewModel.DeleteItemCommand);
        Assert.NotNull(_viewModel.ToggleItemCheckedCommand);
        Assert.NotNull(_viewModel.ClearCheckedItemsCommand);
        Assert.NotNull(_viewModel.ClearAllItemsCommand);
        Assert.NotNull(_viewModel.ExportToMarkdownCommand);
    }

    [Fact]
    public void CreateListCommand_Creates_New_List_And_Selects_It()
    {
        // Arrange
        _viewModel.NewListName = "My New Groceries";

        // Act
        _viewModel.CreateListCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.ShoppingLists);
        Assert.Equal("My New Groceries", _viewModel.ShoppingLists[0].Name);
        Assert.NotNull(_viewModel.SelectedList);
        Assert.Equal("My New Groceries", _viewModel.SelectedList.Name);
        Assert.Empty(_viewModel.NewListName);
        Assert.True(_dialogService.InfoShown);
    }

    [Fact]
    public void DeleteListCommand_WithConfirmation_Deletes_List()
    {
        // Arrange
        _viewModel.NewListName = "List to Delete";
        _viewModel.CreateListCommand.Execute(null);
        var list = _viewModel.SelectedList;
        _dialogService.SetupConfirmationResult(true);

        // Act
        _viewModel.DeleteListCommand.Execute(list);

        // Assert
        Assert.Empty(_viewModel.ShoppingLists);
        Assert.Null(_viewModel.SelectedList);
    }

    [Fact]
    public void AddItemCommand_Adds_Item_To_Selected_List()
    {
        // Arrange
        _viewModel.NewListName = "List with Items";
        _viewModel.CreateListCommand.Execute(null);
        _viewModel.NewItemName = "Milk";

        // Act
        _viewModel.AddItemCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.SelectedListItems);
        Assert.Equal("Milk", _viewModel.SelectedListItems[0].Name);
        Assert.Empty(_viewModel.NewItemName);
        Assert.Equal("0 of 1 items", _viewModel.ListProgressText);
    }

    [Fact]
    public void ToggleItemCheckedCommand_Toggles_Item_State()
    {
        // Arrange
        _viewModel.NewListName = "List with Items";
        _viewModel.CreateListCommand.Execute(null);
        _viewModel.NewItemName = "Milk";
        _viewModel.AddItemCommand.Execute(null);
        var item = _viewModel.SelectedListItems[0];

        // Act
        _viewModel.ToggleItemCheckedCommand.Execute(item);

        // Assert
        var updatedItem = _viewModel.SelectedListItems.FirstOrDefault(i => i.Id == item.Id);
        Assert.NotNull(updatedItem);
        Assert.True(updatedItem.IsChecked);
        Assert.Equal("1 of 1 items", _viewModel.ListProgressText);
    }

    [Fact]
    public void ClearCheckedItemsCommand_Removes_Only_Checked_Items()
    {
        // Arrange
        _viewModel.NewListName = "My List";
        _viewModel.CreateListCommand.Execute(null);

        _viewModel.NewItemName = "Milk";
        _viewModel.AddItemCommand.Execute(null);
        var item1 = _viewModel.SelectedListItems.First(i => i.Name == "Milk");

        _viewModel.NewItemName = "Bread";
        _viewModel.AddItemCommand.Execute(null);

        // Check "Milk"
        _viewModel.ToggleItemCheckedCommand.Execute(item1);

        // Act
        _viewModel.ClearCheckedItemsCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.SelectedListItems);
        Assert.Equal("Bread", _viewModel.SelectedListItems[0].Name);
    }

    [Fact]
    public void DuplicateListCommand_Duplicates_List()
    {
        // Arrange
        _viewModel.NewListName = "Original List";
        _viewModel.CreateListCommand.Execute(null);
        var list = _viewModel.SelectedList;

        // Act
        _viewModel.DuplicateListCommand.Execute(list);

        // Assert
        Assert.Equal(2, _viewModel.ShoppingLists.Count);
        Assert.Contains(_viewModel.ShoppingLists, l => l.Name == "Original List (Copy)");
        Assert.True(_dialogService.InfoShown);
    }

    [Fact]
    public void ToggleFavoriteCommand_Toggles_Favorite_Status()
    {
        // Arrange
        _viewModel.NewListName = "Favorite List";
        _viewModel.CreateListCommand.Execute(null);
        var list = _viewModel.SelectedList;

        // Act
        _viewModel.ToggleFavoriteCommand.Execute(list);

        // Assert
        Assert.True(_viewModel.SelectedList.IsFavorite);

        // Act 2
        _viewModel.ToggleFavoriteCommand.Execute(list);

        // Assert 2
        Assert.False(_viewModel.SelectedList.IsFavorite);
    }

    [Fact]
    public void ShowFavoritesOnly_Filters_List()
    {
        // Arrange
        _viewModel.NewListName = "Normal List";
        _viewModel.CreateListCommand.Execute(null);
        var normalList = _viewModel.SelectedList;

        _viewModel.NewListName = "Favorite List";
        _viewModel.CreateListCommand.Execute(null);
        var favoriteList = _viewModel.SelectedList;
        _viewModel.ToggleFavoriteCommand.Execute(favoriteList);

        Assert.Equal(2, _viewModel.ShoppingLists.Count);

        // Act
        _viewModel.ShowFavoritesOnly = true;

        // Assert
        Assert.Single(_viewModel.ShoppingLists);
        Assert.Equal("Favorite List", _viewModel.ShoppingLists[0].Name);
    }

    [Fact]
    public void SearchText_Filters_ShoppingLists()
    {
        // Arrange
        _viewModel.NewListName = "Weekly Groceries";
        _viewModel.CreateListCommand.Execute(null);

        _viewModel.NewListName = "Party Supplies";
        _viewModel.CreateListCommand.Execute(null);

        // Act
        _viewModel.SearchText = "Party";

        // Assert
        Assert.Single(_viewModel.ShoppingLists);
        Assert.Equal("Party Supplies", _viewModel.ShoppingLists[0].Name);
    }


    #region Test Mocks

    private class TestShoppingListService : IShoppingListService
    {
        private readonly List<ShoppingList> _lists = new();

        public ShoppingList CreateShoppingList(string name, string? description = null)
        {
            var list = new ShoppingList
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description ?? "",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
            _lists.Add(list);
            return list;
        }

        public bool UpdateShoppingList(ShoppingList shoppingList)
        {
            var existing = _lists.FirstOrDefault(l => l.Id == shoppingList.Id);
            if (existing == null) return false;

            existing.Name = shoppingList.Name;
            existing.Description = shoppingList.Description;
            existing.IsFavorite = shoppingList.IsFavorite;
            return true;
        }

        public bool DeleteShoppingList(string id)
        {
            var list = _lists.FirstOrDefault(l => l.Id == id);
            if (list != null)
            {
                _lists.Remove(list);
                return true;
            }
            return false;
        }

        public ShoppingList? GetShoppingList(string id) => _lists.FirstOrDefault(l => l.Id == id);

        public IEnumerable<ShoppingList> GetAllShoppingLists() => _lists;

        public IEnumerable<ShoppingList> GetFavoriteShoppingLists() => _lists.Where(l => l.IsFavorite);

        public void AddItemToList(string listId, ShoppingListItem item)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            if (list != null)
            {
                item.Id = Guid.NewGuid().ToString();
                list.Items.Add(item);
            }
        }

        public void RemoveItemFromList(string listId, string itemId)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            var item = list?.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                list.Items.Remove(item);
            }
        }

        public void UpdateItemInList(string listId, ShoppingListItem item)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            var existing = list?.Items.FirstOrDefault(i => i.Id == item.Id);
            if (existing != null)
            {
                existing.Name = item.Name;
                existing.IsChecked = item.IsChecked;
            }
        }

        public void ToggleItemChecked(string listId, string itemId)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            var existing = list?.Items.FirstOrDefault(i => i.Id == itemId);
            if (existing != null)
            {
                existing.IsChecked = !existing.IsChecked;
            }
        }

        public void ClearCheckedItems(string listId)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            if (list != null)
            {
                list.Items = list.Items.Where(i => !i.IsChecked).ToList();
            }
        }

        public void DuplicateShoppingList(string sourceId, string newName)
        {
            var source = _lists.FirstOrDefault(l => l.Id == sourceId);
            if (source != null)
            {
                CreateShoppingList(newName, source.Description);
            }
        }

        public void ClearShoppingList(string listId)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            if (list != null)
            {
                list.Items.Clear();
            }
        }

        public void MarkListAsFavorite(string listId, bool isFavorite)
        {
            var list = _lists.FirstOrDefault(l => l.Id == listId);
            if (list != null)
            {
                list.IsFavorite = isFavorite;
            }
        }

        public IEnumerable<ShoppingList> SearchShoppingLists(string searchTerm)
        {
            return _lists.Where(l => l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ShoppingListItem> GetPendingItems(string listId) => throw new NotImplementedException();
        public IEnumerable<ShoppingListItem> GetCheckedItems(string listId) => throw new NotImplementedException();
        public string ExportShoppingListToMarkdown(string listId) => throw new NotImplementedException();
        public ShoppingList? ImportShoppingListFromMarkdown(string markdownContent, string name) => throw new NotImplementedException();
    }

    private class TestDialogService : IDialogService
    {
        private bool _confirmationResult = true;
        public bool InfoShown { get; private set; }

        public void SetupConfirmationResult(bool result)
        {
            _confirmationResult = result;
        }

        public bool ShowConfirmation(string message, string title) => _confirmationResult;
        public bool ShowQuestion(string message, string title = "Question") => _confirmationResult;
        public void ShowError(string message, string title = "Error") { }
        public void ShowInfo(string message, string title = "Information") { InfoShown = true; }
        public void ShowSuccess(string message, string title = "Success") { }
        public void ShowWarning(string message, string title = "Warning") { }
        public void ShowComparePricesDialog(string? category = null) { }
        public SearchResult? ShowGlobalSearchDialog() => null;
        public void ShowBarcodeScannerDialog() { }
        public void ShowPriceDropNotificationsDialog() { }
        public void ShowFavoritesDialog() { }
        public void ShowDealExpirationRemindersDialog() { }
        public void ShowWeeklySpecialsDigestDialog() { }
        public void ShowShoppingListsDialog() { }
    }

    #endregion
}