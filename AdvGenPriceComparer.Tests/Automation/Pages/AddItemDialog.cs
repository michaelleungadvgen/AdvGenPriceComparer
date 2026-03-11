using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace AdvGenPriceComparer.Tests.Automation.Pages
{
    /// <summary>
    /// Page object for the Add Item dialog.
    /// </summary>
    public class AddItemDialog : BasePage
    {
        public AddItemDialog(Window window, UIA3Automation automation) : base(window, automation)
        {
        }

        // Control automation IDs
        public const string NameTextBoxId = "NameTextBox";
        public const string BrandTextBoxId = "BrandTextBox";
        public const string CategoryComboBoxId = "CategoryComboBox";
        public const string DescriptionTextBoxId = "DescriptionTextBox";
        public const string BarcodeTextBoxId = "BarcodeTextBox";
        public const string SaveButtonId = "SaveButton";
        public const string CancelButtonId = "CancelButton";
        public const string CategorySuggestionsPanelId = "CategorySuggestionsPanel";

        /// <summary>
        /// Waits for the add item dialog to be fully loaded.
        /// </summary>
        public override bool WaitForPageLoad(TimeSpan? timeout = null)
        {
            var maxTimeout = timeout ?? TimeSpan.FromSeconds(5);
            var endTime = DateTime.Now + maxTimeout;

            while (DateTime.Now < endTime)
            {
                // Check if name text box is present
                if (FindByAutomationId(NameTextBoxId) != null)
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Enters the item name.
        /// </summary>
        public void EnterName(string name)
        {
            EnterText(NameTextBoxId, name);
        }

        /// <summary>
        /// Enters the brand name.
        /// </summary>
        public void EnterBrand(string brand)
        {
            EnterText(BrandTextBoxId, brand);
        }

        /// <summary>
        /// Enters the description.
        /// </summary>
        public void EnterDescription(string description)
        {
            EnterText(DescriptionTextBoxId, description);
        }

        /// <summary>
        /// Enters the barcode.
        /// </summary>
        public void EnterBarcode(string barcode)
        {
            EnterText(BarcodeTextBoxId, barcode);
        }

        /// <summary>
        /// Selects a category from the dropdown.
        /// </summary>
        public void SelectCategory(string category)
        {
            SelectComboBoxItem(CategoryComboBoxId, category);
        }

        /// <summary>
        /// Gets the available category suggestions from ML prediction.
        /// </summary>
        public string[] GetCategorySuggestions()
        {
            var panel = FindByAutomationId(CategorySuggestionsPanelId);
            if (panel == null)
                return Array.Empty<string>();

            // Find all buttons in the suggestions panel
            var buttons = panel.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            return buttons.Select(b => b.Name).ToArray();
        }

        /// <summary>
        /// Clicks a category suggestion by index.
        /// </summary>
        public void ClickCategorySuggestion(int index)
        {
            var panel = FindByAutomationId(CategorySuggestionsPanelId);
            if (panel == null)
                return;

            var buttons = panel.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            if (index >= 0 && index < buttons.Length)
            {
                buttons[index].AsButton()?.Click();
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Clicks the Save button.
        /// </summary>
        public void ClickSave()
        {
            ClickButton(SaveButtonId);
        }

        /// <summary>
        /// Clicks the Cancel button.
        /// </summary>
        public void ClickCancel()
        {
            ClickButton(CancelButtonId);
        }

        /// <summary>
        /// Fills in all item details and saves.
        /// </summary>
        public void CreateItem(string name, string brand, string category, string? description = null, string? barcode = null)
        {
            EnterName(name);
            EnterBrand(brand);
            
            if (!string.IsNullOrEmpty(description))
            {
                EnterDescription(description);
            }

            if (!string.IsNullOrEmpty(barcode))
            {
                EnterBarcode(barcode);
            }

            SelectCategory(category);
            Thread.Sleep(200);

            ClickSave();
            Thread.Sleep(500); // Wait for dialog to close
        }

        /// <summary>
        /// Checks if the dialog is showing validation errors.
        /// </summary>
        public bool HasValidationErrors()
        {
            // Look for error text blocks or validation tooltips
            var errorElements = Window.FindAllDescendants(cf => cf.ByClassName("TextBlock"));
            foreach (var element in errorElements)
            {
                if (element.Name.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                    element.Name.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    element.Name.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the validation error message, if any.
        /// </summary>
        public string? GetValidationErrorMessage()
        {
            var errorElements = Window.FindAllDescendants(cf => cf.ByClassName("TextBlock"));
            foreach (var element in errorElements)
            {
                if (element.Name.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                    element.Name.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    element.Name.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    return element.Name;
                }
            }

            return null;
        }
    }
}
