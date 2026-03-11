using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace AdvGenPriceComparer.Tests.Automation.Pages
{
    /// <summary>
    /// Page object for the Items management page.
    /// </summary>
    public class ItemsPage : BasePage
    {
        public ItemsPage(Window window, UIA3Automation automation) : base(window, automation)
        {
        }

        // Control automation IDs
        public const string ItemsDataGridId = "ItemsDataGrid";
        public const string SearchTextBoxId = "SearchTextBox";
        public const string CategoryFilterComboId = "CategoryFilterCombo";
        public const string AddItemButtonId = "AddItemButton";
        public const string RefreshButtonId = "RefreshButton";

        /// <summary>
        /// Waits for the items page to be fully loaded.
        /// </summary>
        public override bool WaitForPageLoad(TimeSpan? timeout = null)
        {
            var maxTimeout = timeout ?? TimeSpan.FromSeconds(5);
            var endTime = DateTime.Now + maxTimeout;

            while (DateTime.Now < endTime)
            {
                // Check if data grid or search box is present
                if (FindByAutomationId(ItemsDataGridId) != null ||
                    FindByAutomationId(SearchTextBoxId) != null)
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Gets the items data grid.
        /// </summary>
        public DataGridView? GetItemsGrid()
        {
            return FindDataGrid(ItemsDataGridId);
        }

        /// <summary>
        /// Gets the number of items displayed in the grid.
        /// </summary>
        public int GetItemCount()
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return 0;

            // For WPF DataGrid, rows are typically child elements
            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            return rows.Length;
        }

        /// <summary>
        /// Searches for items by text.
        /// </summary>
        public void SearchItems(string searchText)
        {
            var searchBox = FindTextBox(SearchTextBoxId);
            if (searchBox == null)
                throw new InvalidOperationException("Search text box not found.");

            searchBox.Enter(searchText);
            
            // Wait for search results
            Thread.Sleep(300);
        }

        /// <summary>
        /// Filters items by category.
        /// </summary>
        public void FilterByCategory(string category)
        {
            SelectComboBoxItem(CategoryFilterComboId, category);
            
            // Wait for filter to apply
            Thread.Sleep(300);
        }

        /// <summary>
        /// Clicks the Add Item button.
        /// </summary>
        public void ClickAddItem()
        {
            ClickButton(AddItemButtonId);
        }

        /// <summary>
        /// Clicks the Refresh button.
        /// </summary>
        public void ClickRefresh()
        {
            ClickButton(RefreshButtonId);
        }

        /// <summary>
        /// Selects an item in the grid by index.
        /// </summary>
        public void SelectItemByIndex(int index)
        {
            var grid = GetItemsGrid();
            if (grid == null)
                throw new InvalidOperationException("Items grid not found.");

            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            if (index >= 0 && index < rows.Length)
            {
                rows[index].Click();
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Selects an item in the grid by name.
        /// </summary>
        public bool SelectItemByName(string itemName)
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return false;

            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            
            foreach (var row in rows)
            {
                // Check if the row contains the item name
                var cells = row.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Custom));
                foreach (var cell in cells)
                {
                    if (cell.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        row.Click();
                        Thread.Sleep(200);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the text of the first item in the grid (for verification).
        /// </summary>
        public string? GetFirstItemText()
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return null;

            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            if (rows.Length == 0)
                return null;

            return rows[0].Name;
        }

        /// <summary>
        /// Checks if the grid contains an item with the specified text.
        /// </summary>
        public bool ContainsItem(string itemName)
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return false;

            // Search within the grid
            var element = grid.FindFirstDescendant(cf => cf.ByName(itemName));
            return element != null;
        }

        /// <summary>
        /// Gets the column headers of the items grid.
        /// </summary>
        public string[] GetColumnHeaders()
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return Array.Empty<string>();

            var headers = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Header));
            return headers.Select(h => h.Name).ToArray();
        }

        /// <summary>
        /// Double-clicks an item to open its details/edit view.
        /// </summary>
        public void DoubleClickItem(int index)
        {
            var grid = GetItemsGrid();
            if (grid == null)
                throw new InvalidOperationException("Items grid not found.");

            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            if (index >= 0 && index < rows.Length)
            {
                rows[index].DoubleClick();
                Thread.Sleep(300);
            }
        }

        /// <summary>
        /// Sorts the grid by clicking on a column header.
        /// </summary>
        public void SortByColumn(string columnName)
        {
            var grid = GetItemsGrid();
            if (grid == null)
                return;

            // Find the header with the specified name
            var headers = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Header));
            foreach (var header in headers)
            {
                if (header.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    header.Click();
                    Thread.Sleep(300);
                    return;
                }
            }
        }
    }
}
