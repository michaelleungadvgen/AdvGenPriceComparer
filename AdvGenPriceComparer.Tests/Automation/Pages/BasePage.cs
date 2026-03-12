using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using System.IO;

namespace AdvGenPriceComparer.Tests.Automation.Pages
{
    /// <summary>
    /// Base class for page objects in UI automation tests.
    /// Implements the Page Object pattern for maintainable UI tests.
    /// </summary>
    public abstract class BasePage
    {
        protected readonly Window Window;
        protected readonly UIA3Automation Automation;

        /// <summary>
        /// Initializes a new instance of the BasePage class.
        /// </summary>
        /// <param name="window">The application window.</param>
        /// <param name="automation">The automation instance.</param>
        protected BasePage(Window window, UIA3Automation automation)
        {
            Window = window ?? throw new ArgumentNullException(nameof(window));
            Automation = automation ?? throw new ArgumentNullException(nameof(automation));
        }

        /// <summary>
        /// Gets the title of the window.
        /// </summary>
        public string Title => Window.Title;

        /// <summary>
        /// Waits for the page to be fully loaded.
        /// </summary>
        /// <param name="timeout">Maximum time to wait.</param>
        /// <returns>True if the page is loaded; otherwise, false.</returns>
        public abstract bool WaitForPageLoad(TimeSpan? timeout = null);

        /// <summary>
        /// Finds an element by automation ID.
        /// </summary>
        /// <param name="automationId">The automation ID of the element.</param>
        /// <param name="timeout">Maximum time to wait for the element.</param>
        /// <returns>The found element, or null if not found.</returns>
        protected AutomationElement? FindByAutomationId(string automationId, TimeSpan? timeout = null)
        {
            try
            {
                var retryInterval = TimeSpan.FromMilliseconds(100);
                var maxTimeout = timeout ?? TimeSpan.FromSeconds(5);
                var endTime = DateTime.Now + maxTimeout;

                while (DateTime.Now < endTime)
                {
                    var element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                    if (element != null)
                    {
                        return element;
                    }

                    Thread.Sleep(retryInterval);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds an element by name.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="timeout">Maximum time to wait for the element.</param>
        /// <returns>The found element, or null if not found.</returns>
        protected AutomationElement? FindByName(string name, TimeSpan? timeout = null)
        {
            try
            {
                var retryInterval = TimeSpan.FromMilliseconds(100);
                var maxTimeout = timeout ?? TimeSpan.FromSeconds(5);
                var endTime = DateTime.Now + maxTimeout;

                while (DateTime.Now < endTime)
                {
                    var element = Window.FindFirstDescendant(cf => cf.ByName(name));
                    if (element != null)
                    {
                        return element;
                    }

                    Thread.Sleep(retryInterval);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds a button by automation ID.
        /// </summary>
        protected Button? FindButton(string automationId)
        {
            var element = FindByAutomationId(automationId);
            return element?.AsButton();
        }

        /// <summary>
        /// Finds a text box by automation ID.
        /// </summary>
        protected TextBox? FindTextBox(string automationId)
        {
            var element = FindByAutomationId(automationId);
            return element?.AsTextBox();
        }

        /// <summary>
        /// Finds a combo box by automation ID.
        /// </summary>
        protected ComboBox? FindComboBox(string automationId)
        {
            var element = FindByAutomationId(automationId);
            return element?.AsComboBox();
        }

        /// <summary>
        /// Finds a data grid by automation ID.
        /// </summary>
        protected DataGridView? FindDataGrid(string automationId)
        {
            var element = FindByAutomationId(automationId);
            return element?.AsDataGridView();
        }

        /// <summary>
        /// Finds a menu item by name.
        /// </summary>
        protected MenuItem? FindMenuItem(params string[] menuPath)
        {
            if (menuPath.Length == 0)
                return null;

            // Find the menu bar
            var menuBar = Window.FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuBar));
            if (menuBar == null)
                return null;

            MenuItem? currentItem = null;

            foreach (var menuName in menuPath)
            {
                if (currentItem == null)
                {
                    // First level - search in menu bar
                    currentItem = menuBar.FindFirstDescendant(cf => cf.ByName(menuName))?.AsMenuItem();
                }
                else
                {
                    // Expand current menu and search for next item
                    currentItem.Click();
                    System.Threading.Thread.Sleep(100);
                    currentItem = currentItem.FindFirstDescendant(cf => cf.ByName(menuName))?.AsMenuItem();
                }

                if (currentItem == null)
                    return null;
            }

            return currentItem;
        }

        /// <summary>
        /// Clicks a button with the specified automation ID.
        /// </summary>
        protected void ClickButton(string automationId)
        {
            var button = FindButton(automationId);
            if (button == null)
                throw new InvalidOperationException($"Button with automation ID '{automationId}' not found.");

            button.Click();
            System.Threading.Thread.Sleep(200);
        }

        /// <summary>
        /// Enters text into a text box.
        /// </summary>
        protected void EnterText(string automationId, string text)
        {
            var textBox = FindTextBox(automationId);
            if (textBox == null)
                throw new InvalidOperationException($"TextBox with automation ID '{automationId}' not found.");

            textBox.Text = text;
        }

        /// <summary>
        /// Selects an item in a combo box by index.
        /// </summary>
        protected void SelectComboBoxItem(string automationId, int index)
        {
            var comboBox = FindComboBox(automationId);
            if (comboBox == null)
                throw new InvalidOperationException($"ComboBox with automation ID '{automationId}' not found.");

            comboBox.Select(index);
        }

        /// <summary>
        /// Selects an item in a combo box by text.
        /// </summary>
        protected void SelectComboBoxItem(string automationId, string text)
        {
            var comboBox = FindComboBox(automationId);
            if (comboBox == null)
                throw new InvalidOperationException($"ComboBox with automation ID '{automationId}' not found.");

            comboBox.Select(text);
        }

        /// <summary>
        /// Takes a screenshot of the current window.
        /// </summary>
        public void TakeScreenshot(string filePath)
        {
            var screenshot = Window.Capture();
            screenshot.Save(filePath);
        }

        /// <summary>
        /// Waits for an element to be visible and enabled.
        /// </summary>
        protected bool WaitForElement(string automationId, TimeSpan? timeout = null)
        {
            var element = FindByAutomationId(automationId, timeout);
            if (element == null)
                return false;

            // Check if element is enabled
            return element.IsEnabled;
        }

        /// <summary>
        /// Gets all child elements of a specific control type.
        /// </summary>
        protected AutomationElement[] FindAllByControlType(ControlType controlType)
        {
            return Window.FindAllDescendants(cf => cf.ByControlType(controlType));
        }
    }
}
