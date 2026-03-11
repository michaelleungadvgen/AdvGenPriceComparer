using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace AdvGenPriceComparer.Tests.Automation.Pages
{
    /// <summary>
    /// Page object for the Import Data dialog.
    /// </summary>
    public class ImportDialog : BasePage
    {
        public ImportDialog(Window window, UIA3Automation automation) : base(window, automation)
        {
        }

        // Control automation IDs
        public const string BrowseButtonId = "BrowseButton";
        public const string FilePathTextBoxId = "FilePathTextBox";
        public const string StoreComboBoxId = "StoreComboBox";
        public const string PreviewButtonId = "PreviewButton";
        public const string ImportButtonId = "ImportButton";
        public const string CancelButtonId = "CancelButton";
        public const string BackButtonId = "BackButton";
        public const string NextButtonId = "NextButton";
        public const string ProgressBarId = "ProgressBar";
        public const string StatusTextBlockId = "StatusTextBlock";
        public const string PreviewDataGridId = "PreviewDataGrid";

        /// <summary>
        /// Waits for the import dialog to be fully loaded.
        /// </summary>
        public override bool WaitForPageLoad(TimeSpan? timeout = null)
        {
            var maxTimeout = timeout ?? TimeSpan.FromSeconds(5);
            var endTime = DateTime.Now + maxTimeout;

            while (DateTime.Now < endTime)
            {
                // Check if browse button or file path text box is present
                if (FindByAutomationId(BrowseButtonId) != null ||
                    FindByAutomationId(FilePathTextBoxId) != null)
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Enters the file path for import.
        /// </summary>
        public void EnterFilePath(string filePath)
        {
            EnterText(FilePathTextBoxId, filePath);
        }

        /// <summary>
        /// Clicks the Browse button to open file dialog.
        /// </summary>
        public void ClickBrowse()
        {
            ClickButton(BrowseButtonId);
            Thread.Sleep(500); // Wait for file dialog to open
        }

        /// <summary>
        /// Selects a store from the dropdown.
        /// </summary>
        public void SelectStore(string storeName)
        {
            SelectComboBoxItem(StoreComboBoxId, storeName);
        }

        /// <summary>
        /// Clicks the Preview button.
        /// </summary>
        public void ClickPreview()
        {
            ClickButton(PreviewButtonId);
            Thread.Sleep(500); // Wait for preview to load
        }

        /// <summary>
        /// Clicks the Import button.
        /// </summary>
        public void ClickImport()
        {
            ClickButton(ImportButtonId);
            Thread.Sleep(500); // Wait for import to start
        }

        /// <summary>
        /// Clicks the Cancel button.
        /// </summary>
        public void ClickCancel()
        {
            ClickButton(CancelButtonId);
            Thread.Sleep(300);
        }

        /// <summary>
        /// Clicks the Back button.
        /// </summary>
        public void ClickBack()
        {
            ClickButton(BackButtonId);
            Thread.Sleep(300);
        }

        /// <summary>
        /// Clicks the Next button.
        /// </summary>
        public void ClickNext()
        {
            ClickButton(NextButtonId);
            Thread.Sleep(300);
        }

        /// <summary>
        /// Gets the preview data grid.
        /// </summary>
        public DataGridView? GetPreviewGrid()
        {
            return FindDataGrid(PreviewDataGridId);
        }

        /// <summary>
        /// Gets the number of items in the preview.
        /// </summary>
        public int GetPreviewItemCount()
        {
            var grid = GetPreviewGrid();
            if (grid == null)
                return 0;

            var rows = grid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            return rows.Length;
        }

        /// <summary>
        /// Gets the import progress percentage.
        /// </summary>
        public double GetImportProgress()
        {
            var progressBar = FindByAutomationId(ProgressBarId)?.AsProgressBar();
            return progressBar?.Value ?? 0;
        }

        /// <summary>
        /// Gets the status text.
        /// </summary>
        public string GetStatusText()
        {
            var statusText = FindByAutomationId(StatusTextBlockId);
            return statusText?.Name ?? string.Empty;
        }

        /// <summary>
        /// Waits for the import to complete.
        /// </summary>
        public bool WaitForImportComplete(TimeSpan? timeout = null)
        {
            var maxTimeout = timeout ?? TimeSpan.FromSeconds(30);
            var endTime = DateTime.Now + maxTimeout;

            while (DateTime.Now < endTime)
            {
                var status = GetStatusText();
                if (status.Contains("complete", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (status.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("failed", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                Thread.Sleep(500);
            }

            return false;
        }

        /// <summary>
        /// Performs the complete import workflow.
        /// </summary>
        public bool ImportFile(string filePath, string storeName, TimeSpan? timeout = null)
        {
            EnterFilePath(filePath);
            SelectStore(storeName);
            ClickNext(); // Move to preview step

            // Wait for preview
            Thread.Sleep(1000);

            ClickNext(); // Start import

            // Wait for completion
            return WaitForImportComplete(timeout);
        }

        /// <summary>
        /// Checks if an error message is displayed.
        /// </summary>
        public bool HasErrorMessage()
        {
            var status = GetStatusText();
            return status.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                   status.Contains("invalid", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the error message, if any.
        /// </summary>
        public string? GetErrorMessage()
        {
            var status = GetStatusText();
            if (HasErrorMessage())
                return status;

            return null;
        }
    }
}
