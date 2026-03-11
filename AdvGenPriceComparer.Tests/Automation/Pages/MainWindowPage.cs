using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace AdvGenPriceComparer.Tests.Automation.Pages
{
    /// <summary>
    /// Page object for the Main Window of the application.
    /// </summary>
    public class MainWindowPage : BasePage
    {
        public MainWindowPage(Window window, UIA3Automation automation) : base(window, automation)
        {
        }

        // Navigation button automation IDs
        public const string DashboardNavButtonId = "DashboardNavButton";
        public const string ItemsNavButtonId = "ItemsNavButton";
        public const string StoresNavButtonId = "StoresNavButton";
        public const string CategoriesNavButtonId = "CategoriesNavButton";
        public const string AlertsNavButtonId = "AlertsNavButton";
        public const string ReportsNavButtonId = "ReportsNavButton";

        // Quick action button IDs
        public const string AddItemButtonId = "AddItemButton";
        public const string ImportDataButtonId = "ImportDataButton";
        public const string ExportDataButtonId = "ExportDataButton";
        public const string GlobalSearchButtonId = "GlobalSearchButton";

        /// <summary>
        /// Waits for the main window to be fully loaded.
        /// </summary>
        public override bool WaitForPageLoad(TimeSpan? timeout = null)
        {
            var maxTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var endTime = DateTime.Now + maxTimeout;

            while (DateTime.Now < endTime)
            {
                // Check if window is ready by looking for navigation buttons
                if (FindByAutomationId(DashboardNavButtonId) != null ||
                    FindByAutomationId(ItemsNavButtonId) != null)
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Navigates to the Dashboard view.
        /// </summary>
        public void NavigateToDashboard()
        {
            ClickButton(DashboardNavButtonId);
        }

        /// <summary>
        /// Navigates to the Items view.
        /// </summary>
        public void NavigateToItems()
        {
            ClickButton(ItemsNavButtonId);
        }

        /// <summary>
        /// Navigates to the Stores view.
        /// </summary>
        public void NavigateToStores()
        {
            ClickButton(StoresNavButtonId);
        }

        /// <summary>
        /// Navigates to the Categories view.
        /// </summary>
        public void NavigateToCategories()
        {
            ClickButton(CategoriesNavButtonId);
        }

        /// <summary>
        /// Navigates to the Alerts view.
        /// </summary>
        public void NavigateToAlerts()
        {
            ClickButton(AlertsNavButtonId);
        }

        /// <summary>
        /// Navigates to the Reports view.
        /// </summary>
        public void NavigateToReports()
        {
            ClickButton(ReportsNavButtonId);
        }

        /// <summary>
        /// Clicks the Add Item button.
        /// </summary>
        public void ClickAddItem()
        {
            ClickButton(AddItemButtonId);
        }

        /// <summary>
        /// Clicks the Import Data button.
        /// </summary>
        public void ClickImportData()
        {
            ClickButton(ImportDataButtonId);
        }

        /// <summary>
        /// Clicks the Export Data button.
        /// </summary>
        public void ClickExportData()
        {
            ClickButton(ExportDataButtonId);
        }

        /// <summary>
        /// Clicks the Global Search button.
        /// </summary>
        public void ClickGlobalSearch()
        {
            ClickButton(GlobalSearchButtonId);
        }

        /// <summary>
        /// Checks if the dashboard statistics are displayed.
        /// </summary>
        public bool AreStatisticsDisplayed()
        {
            // Look for statistics cards by common automation IDs or text patterns
            var totalItemsCard = FindByName("Total Items");
            var storesMonitoredCard = FindByName("Stores Monitored");
            var priceUpdatesCard = FindByName("Price Updates");

            return totalItemsCard != null || storesMonitoredCard != null || priceUpdatesCard != null;
        }

        /// <summary>
        /// Gets the dashboard statistics values.
        /// </summary>
        public Dictionary<string, string> GetDashboardStatistics()
        {
            var stats = new Dictionary<string, string>();

            // Try to find statistics by common patterns
            var statLabels = Window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));

            foreach (var label in statLabels)
            {
                var text = label.Name;
                if (text.Contains("Items") || text.Contains("Stores") || text.Contains("Updates") || text.Contains("Deals"))
                {
                    // Try to find the associated value (usually a sibling or nearby element)
                    var parent = label.Parent;
                    if (parent != null)
                    {
                        var siblings = parent.FindAllChildren();
                        foreach (var sibling in siblings)
                        {
                            if (sibling.ControlType == FlaUI.Core.Definitions.ControlType.Text &&
                                sibling.AutomationId != label.AutomationId)
                            {
                                var value = sibling.Name;
                                if (int.TryParse(value, out _))
                                {
                                    stats[text] = value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return stats;
        }

        /// <summary>
        /// Opens the Settings window from the menu.
        /// </summary>
        public void OpenSettings()
        {
            // Try to find and click Settings menu item
            var settingsMenu = FindMenuItem("Tools", "Settings");
            if (settingsMenu == null)
            {
                // Try alternative menu paths
                settingsMenu = FindMenuItem("File", "Settings");
            }

            settingsMenu?.Click();
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Opens the Help menu.
        /// </summary>
        public void OpenHelp()
        {
            var helpMenu = FindMenuItem("Help");
            helpMenu?.Click();
            System.Threading.Thread.Sleep(200);
        }

        /// <summary>
        /// Checks if a modal dialog is currently open.
        /// </summary>
        public bool IsDialogOpen()
        {
            // Look for modal windows
            var modalWindows = Window.ModalWindows;
            return modalWindows.Length > 0;
        }

        /// <summary>
        /// Gets the currently open modal dialog, if any.
        /// </summary>
        public Window? GetOpenDialog()
        {
            var modalWindows = Window.ModalWindows;
            return modalWindows.FirstOrDefault();
        }

        /// <summary>
        /// Closes the current modal dialog by clicking OK or the close button.
        /// </summary>
        public void CloseDialog(bool clickOk = true)
        {
            var dialog = GetOpenDialog();
            if (dialog == null)
                return;

            if (clickOk)
            {
                // Try to find OK button
                var okButton = dialog.FindFirstDescendant(cf => cf.ByName("OK"))?.AsButton() ??
                               dialog.FindFirstDescendant(cf => cf.ByAutomationId("OkButton"))?.AsButton();
                okButton?.Click();
            }
            else
            {
                // Try to find Cancel or Close button
                var cancelButton = dialog.FindFirstDescendant(cf => cf.ByName("Cancel"))?.AsButton() ??
                                   dialog.FindFirstDescendant(cf => cf.ByAutomationId("CancelButton"))?.AsButton() ??
                                   dialog.FindFirstDescendant(cf => cf.ByName("Close"))?.AsButton();
                cancelButton?.Click();
            }

            System.Threading.Thread.Sleep(300);
        }
    }
}
