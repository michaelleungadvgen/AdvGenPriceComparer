using AdvGenPriceComparer.Tests.Automation;
using AdvGenPriceComparer.Tests.Automation.Pages;
using FlaUI.UIA3;
using Xunit;

namespace AdvGenPriceComparer.Tests.UI
{
    /// <summary>
    /// UI automation tests for the Main Window.
    /// </summary>
    public class MainWindowTests : IDisposable
    {
        private readonly ApplicationLauncher _launcher;
        private readonly UIA3Automation _automation;
        private bool _disposed;

        public MainWindowTests()
        {
            _launcher = new ApplicationLauncher();
            _automation = new UIA3Automation();
        }

        /// <summary>
        /// Tests that the application launches successfully and the main window is displayed.
        /// </summary>
        [Fact]
        public void Application_Launch_MainWindowDisplayed()
        {
            // Arrange & Act
            var application = _launcher.Launch();
            var mainWindow = _launcher.MainWindow;

            // Assert
            Assert.NotNull(mainWindow);
            Assert.True(mainWindow.IsEnabled);
            Assert.False(string.IsNullOrEmpty(mainWindow.Title));
        }

        /// <summary>
        /// Tests that the main window contains navigation elements.
        /// </summary>
        [Fact]
        public void MainWindow_ContainsNavigationElements()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);

            // Act & Assert
            Assert.True(mainPage.WaitForPageLoad(TimeSpan.FromSeconds(10)));
        }

        /// <summary>
        /// Tests navigation to different views.
        /// </summary>
        [Theory]
        [InlineData("Dashboard")]
        [InlineData("Items")]
        [InlineData("Stores")]
        [InlineData("Categories")]
        [InlineData("Alerts")]
        [InlineData("Reports")]
        public void MainWindow_Navigation_SwitchesViews(string viewName)
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            switch (viewName)
            {
                case "Dashboard":
                    mainPage.NavigateToDashboard();
                    break;
                case "Items":
                    mainPage.NavigateToItems();
                    break;
                case "Stores":
                    mainPage.NavigateToStores();
                    break;
                case "Categories":
                    mainPage.NavigateToCategories();
                    break;
                case "Alerts":
                    mainPage.NavigateToAlerts();
                    break;
                case "Reports":
                    mainPage.NavigateToReports();
                    break;
            }

            // Assert - Verify the navigation occurred (window is still responsive)
            Assert.True(mainWindow!.IsEnabled);
        }

        /// <summary>
        /// Tests that dashboard statistics are displayed.
        /// </summary>
        [Fact]
        public void MainWindow_Dashboard_DisplaysStatistics()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            mainPage.NavigateToDashboard();

            // Assert
            Assert.True(mainPage.AreStatisticsDisplayed());
        }

        /// <summary>
        /// Tests clicking the Add Item button opens the add item dialog.
        /// </summary>
        [Fact]
        public void MainWindow_ClickAddItem_OpensAddItemDialog()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            mainPage.ClickAddItem();

            // Assert
            var dialog = mainPage.GetOpenDialog();
            Assert.NotNull(dialog);

            // Cleanup - close the dialog
            mainPage.CloseDialog(false);
        }

        /// <summary>
        /// Tests clicking the Import Data button opens the import dialog.
        /// </summary>
        [Fact]
        public void MainWindow_ClickImportData_OpensImportDialog()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            mainPage.ClickImportData();

            // Assert
            var dialog = mainPage.GetOpenDialog();
            Assert.NotNull(dialog);

            // Cleanup - close the dialog
            mainPage.CloseDialog(false);
        }

        /// <summary>
        /// Tests clicking the Export Data button opens the export dialog.
        /// </summary>
        [Fact]
        public void MainWindow_ClickExportData_OpensExportDialog()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;
            var mainPage = new MainWindowPage(mainWindow!, _automation);
            mainPage.WaitForPageLoad();

            // Act
            mainPage.ClickExportData();

            // Assert
            var dialog = mainPage.GetOpenDialog();
            Assert.NotNull(dialog);

            // Cleanup - close the dialog
            mainPage.CloseDialog(false);
        }

        /// <summary>
        /// Tests that the window title contains expected text.
        /// </summary>
        [Fact]
        public void MainWindow_Title_ContainsExpectedText()
        {
            // Arrange
            _launcher.Launch();
            var mainWindow = _launcher.MainWindow;

            // Act
            var title = mainWindow!.Title;

            // Assert
            Assert.Contains("AdvGen", title, StringComparison.OrdinalIgnoreCase);
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
