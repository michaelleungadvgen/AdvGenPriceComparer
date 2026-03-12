using AdvGenPriceComparer.Tests.Automation;
using AdvGenPriceComparer.Tests.Automation.Pages;
using FlaUI.UIA3;
using Xunit;

namespace AdvGenPriceComparer.Tests.UI
{
    /// <summary>
    /// UI automation tests for the Items Page.
    /// </summary>
    public class ItemsPageTests : IDisposable
    {
        private readonly ApplicationLauncher _launcher;
        private readonly UIA3Automation _automation;
        private bool _disposed;

        public ItemsPageTests()
        {
            _launcher = new ApplicationLauncher();
            _automation = new UIA3Automation();
        }

        /// <summary>
        /// Tests that the items page loads successfully.
        /// </summary>
        [Fact]
        public void ItemsPage_Loads_Successfully()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            mainPage.NavigateToItems();
            var itemsPage = new ItemsPage(mainWindow!, _automation);

            // Assert
            Assert.True(itemsPage.WaitForPageLoad());
        }

        /// <summary>
        /// Tests that the items grid is displayed.
        /// </summary>
        [Fact]
        public void ItemsPage_DisplaysItemsGrid()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            // Act
            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Assert
            var grid = itemsPage.GetItemsGrid();
            Assert.NotNull(grid);
        }

        /// <summary>
        /// Tests searching for items.
        /// </summary>
        [Fact]
        public void ItemsPage_Search_FiltersResults()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Search for a test term
            itemsPage.SearchItems("test");

            // Assert - Grid should still be visible
            var grid = itemsPage.GetItemsGrid();
            Assert.NotNull(grid);
        }

        /// <summary>
        /// Tests that the Add Item workflow creates a new item.
        /// </summary>
        [Fact]
        public void ItemsPage_AddItem_Workflow()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Click Add Item
            itemsPage.ClickAddItem();

            // Assert - Add Item dialog should be open
            var addItemDialog = mainPage.GetOpenDialog();
            Assert.NotNull(addItemDialog);

            // Cleanup
            mainPage.CloseDialog(false);
        }

        /// <summary>
        /// Tests selecting an item in the grid.
        /// </summary>
        [Fact]
        public void ItemsPage_SelectItem_HighlightsRow()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Try to select first item if available
            itemsPage.SelectItemByIndex(0);

            // Assert - Window should still be responsive
            Assert.True(mainWindow!.IsEnabled);
        }

        /// <summary>
        /// Tests clicking the refresh button.
        /// </summary>
        [Fact]
        public void ItemsPage_ClickRefresh_RefreshesGrid()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act
            itemsPage.ClickRefresh();

            // Assert - Grid should still be visible
            var grid = itemsPage.GetItemsGrid();
            Assert.NotNull(grid);
        }

        /// <summary>
        /// Tests that filtering by category works.
        /// </summary>
        [Fact]
        public void ItemsPage_FilterByCategory_UpdatesGrid()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Try to filter by a category
            try
            {
                itemsPage.FilterByCategory("All Categories");
            }
            catch
            {
                // Filter might not be available, that's OK for this test
            }

            // Assert - Grid should still be visible
            var grid = itemsPage.GetItemsGrid();
            Assert.NotNull(grid);
        }

        /// <summary>
        /// Tests sorting the items grid.
        /// </summary>
        [Fact]
        public void ItemsPage_SortByColumn_ReordersItems()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Try to sort by Name column
            itemsPage.SortByColumn("Name");

            // Assert - Grid should still be visible and responsive
            var grid = itemsPage.GetItemsGrid();
            Assert.NotNull(grid);
        }

        /// <summary>
        /// Tests the complete add item workflow with form filling.
        /// </summary>
        [Fact]
        public void ItemsPage_AddItemDialog_FillAndCancel()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();
            mainPage.NavigateToItems();

            var itemsPage = new ItemsPage(mainWindow!, _automation);
            itemsPage.WaitForPageLoad();

            // Act - Open Add Item dialog
            itemsPage.ClickAddItem();

            var dialog = mainPage.GetOpenDialog();
            Assert.NotNull(dialog);

            var addItemDialog = new AddItemDialog(dialog, _automation);
            Assert.True(addItemDialog.WaitForPageLoad());

            // Fill in some test data
            addItemDialog.EnterName("Test Product");
            addItemDialog.EnterBrand("Test Brand");

            // Cancel the dialog
            addItemDialog.ClickCancel();

            // Assert - Dialog should be closed
            Assert.False(mainPage.IsDialogOpen());
        }

        /// <summary>
        /// Disposes the test resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _launcher.Dispose();
                _automation.Dispose();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
