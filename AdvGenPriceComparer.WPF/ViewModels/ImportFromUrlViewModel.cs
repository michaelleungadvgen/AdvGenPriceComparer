using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for importing price data from a URL
/// </summary>
public class ImportFromUrlViewModel : ViewModelBase
{
    private readonly StaticDataImporter _staticDataImporter;
    private readonly ILoggerService _logger;
    private readonly IDialogService _dialogService;

    private string _url = string.Empty;
    private bool _isImporting;
    private int _progressPercentage;
    private string _statusMessage = string.Empty;
    private string _resultMessage = string.Empty;
    private bool _showResult;
    private StaticImportPreview? _preview;
    private bool _showPreview;
    private bool _validateChecksums = true;
    private DuplicateStrategy _duplicateStoreStrategy = DuplicateStrategy.Update;
    private DuplicateStrategy _duplicateProductStrategy = DuplicateStrategy.Update;
    private DuplicateStrategy _duplicatePriceStrategy = DuplicateStrategy.Skip;

    /// <summary>
    /// URL to import from
    /// </summary>
    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            OnPropertyChanged();
            ((RelayCommand)PreviewCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ImportCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Whether an import operation is in progress
    /// </summary>
    public bool IsImporting
    {
        get => _isImporting;
        set
        {
            _isImporting = value;
            OnPropertyChanged();
            ((RelayCommand)PreviewCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ImportCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CloseCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage
    {
        get => _progressPercentage;
        set
        {
            _progressPercentage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Current status message
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Result message after import completion
    /// </summary>
    public string ResultMessage
    {
        get => _resultMessage;
        set
        {
            _resultMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether to show the result panel
    /// </summary>
    public bool ShowResult
    {
        get => _showResult;
        set
        {
            _showResult = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Preview of the package before import
    /// </summary>
    public StaticImportPreview? Preview
    {
        get => _preview;
        set
        {
            _preview = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether to show the preview panel
    /// </summary>
    public bool ShowPreview
    {
        get => _showPreview;
        set
        {
            _showPreview = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether to validate checksums during import
    /// </summary>
    public bool ValidateChecksums
    {
        get => _validateChecksums;
        set
        {
            _validateChecksums = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Strategy for handling duplicate stores
    /// </summary>
    public DuplicateStrategy DuplicateStoreStrategy
    {
        get => _duplicateStoreStrategy;
        set
        {
            _duplicateStoreStrategy = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Strategy for handling duplicate products
    /// </summary>
    public DuplicateStrategy DuplicateProductStrategy
    {
        get => _duplicateProductStrategy;
        set
        {
            _duplicateProductStrategy = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Strategy for handling duplicate price records
    /// </summary>
    public DuplicateStrategy DuplicatePriceStrategy
    {
        get => _duplicatePriceStrategy;
        set
        {
            _duplicatePriceStrategy = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Available duplicate strategies
    /// </summary>
    public ObservableCollection<DuplicateStrategy> DuplicateStrategies { get; } = new()
    {
        DuplicateStrategy.Skip,
        DuplicateStrategy.Update,
        DuplicateStrategy.CreateNew
    };

    /// <summary>
    /// Command to preview the package
    /// </summary>
    public ICommand PreviewCommand { get; }

    /// <summary>
    /// Command to start import
    /// </summary>
    public ICommand ImportCommand { get; }

    /// <summary>
    /// Command to close the dialog
    /// </summary>
    public ICommand CloseCommand { get; }

    /// <summary>
    /// Event raised when the dialog should be closed
    /// </summary>
    public event EventHandler? RequestClose;

    public ImportFromUrlViewModel(
        StaticDataImporter staticDataImporter,
        ILoggerService logger,
        IDialogService dialogService)
    {
        _staticDataImporter = staticDataImporter;
        _logger = logger;
        _dialogService = dialogService;

        PreviewCommand = new RelayCommand(async () => await PreviewAsync(), () => CanPreview());
        ImportCommand = new RelayCommand(async () => await ImportAsync(), () => CanImport());
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty), () => !IsImporting);
    }

    private bool CanPreview()
    {
        return !IsImporting &&
               !string.IsNullOrWhiteSpace(Url) &&
               Uri.TryCreate(Url, UriKind.Absolute, out _);
    }

    private bool CanImport()
    {
        return !IsImporting &&
               !string.IsNullOrWhiteSpace(Url) &&
               Uri.TryCreate(Url, UriKind.Absolute, out _);
    }

    private async Task PreviewAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            _dialogService.ShowWarning("Please enter a URL to import from.", "Missing URL");
            return;
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
        {
            _dialogService.ShowWarning("Please enter a valid URL.", "Invalid URL");
            return;
        }

        IsImporting = true;
        ShowResult = false;
        ShowPreview = false;
        StatusMessage = "Downloading package preview...";
        ProgressPercentage = 0;

        try
        {
            _logger.LogInfo($"Previewing package from URL: {Url}");

            // Download to temp file for preview
            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"advgen_preview_{Guid.NewGuid():N}.zip");

            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                var response = await httpClient.GetAsync(Url);
                response.EnsureSuccessStatusCode();

                await using var fs = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                await response.Content.CopyToAsync(fs);

                ProgressPercentage = 50;
                StatusMessage = "Analyzing package contents...";

                // Get preview
                Preview = await _staticDataImporter.PreviewPackageAsync(tempFile);

                if (string.IsNullOrEmpty(Preview.ErrorMessage))
                {
                    ShowPreview = true;
                    _logger.LogInfo($"Package preview loaded: {Preview.StoreCount} stores, {Preview.ProductCount} products, {Preview.PriceRecordCount} prices");
                }
                else
                {
                    _dialogService.ShowError($"Failed to preview package: {Preview.ErrorMessage}", "Preview Error");
                }
            }
            finally
            {
                // Cleanup temp file
                try
                {
                    if (System.IO.File.Exists(tempFile))
                    {
                        System.IO.File.Delete(tempFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to cleanup preview temp file: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to preview package from URL: {ex.Message}", ex);
            _dialogService.ShowError($"Failed to preview package: {ex.Message}", "Preview Error");
        }
        finally
        {
            IsImporting = false;
            StatusMessage = string.Empty;
            ProgressPercentage = 0;
        }
    }

    private async Task ImportAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            _dialogService.ShowWarning("Please enter a URL to import from.", "Missing URL");
            return;
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
        {
            _dialogService.ShowWarning("Please enter a valid URL.", "Invalid URL");
            return;
        }

        // Confirm import if no preview was shown
        if (!ShowPreview)
        {
            var result = _dialogService.ShowConfirmation(
                "You haven't previewed the package yet. Are you sure you want to import?",
                "Confirm Import");
            if (!result) return;
        }

        IsImporting = true;
        ShowResult = false;
        StatusMessage = "Starting import...";
        ProgressPercentage = 0;

        try
        {
            _logger.LogInfo($"Starting import from URL: {Url}");

            var options = new StaticImportOptions
            {
                ValidateChecksums = ValidateChecksums,
                DuplicateStoreStrategy = DuplicateStoreStrategy,
                DuplicateProductStrategy = DuplicateProductStrategy,
                DuplicatePriceStrategy = DuplicatePriceStrategy,
                FailOnChecksumError = false,
                FailOnError = false
            };

            var progress = new Progress<StaticImportProgress>(p =>
            {
                ProgressPercentage = p.Percentage;
                StatusMessage = p.Status;
            });

            var result = await _staticDataImporter.ImportFromUrlAsync(Url, options, progress);

            ShowResult = true;

            if (result.Success)
            {
                ResultMessage = $"✅ Import successful!\n\n" +
                    $"📦 Package: {result.PackageId}\n" +
                    $"🏪 Stores: {result.StoresImported} imported, {result.StoresUpdated} updated, {result.StoresSkipped} skipped\n" +
                    $"📦 Products: {result.ProductsImported} imported, {result.ProductsUpdated} updated, {result.ProductsSkipped} skipped\n" +
                    $"💰 Prices: {result.PricesImported} imported, {result.PricesUpdated} updated, {result.PricesSkipped} skipped";

                if (result.Errors.Count > 0)
                {
                    ResultMessage += $"\n\n⚠️ {result.Errors.Count} errors occurred during import.";
                }

                _logger.LogInfo($"Import from URL completed successfully: {result.Message}");
                _dialogService.ShowSuccess("Data imported successfully!", "Import Complete");
            }
            else
            {
                ResultMessage = $"❌ Import failed!\n\nError: {result.ErrorMessage}";

                if (result.Errors.Count > 0)
                {
                    ResultMessage += $"\n\nErrors ({result.Errors.Count}):\n" + string.Join("\n", result.Errors);
                }

                _logger.LogError($"Import from URL failed: {result.ErrorMessage}");
                _dialogService.ShowError($"Import failed: {result.ErrorMessage}", "Import Error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to import from URL: {ex.Message}", ex);
            ResultMessage = $"❌ Import failed!\n\nError: {ex.Message}";
            ShowResult = true;
            _dialogService.ShowError($"Import failed: {ex.Message}", "Import Error");
        }
        finally
        {
            IsImporting = false;
            StatusMessage = string.Empty;
            ProgressPercentage = 0;
        }
    }
}
