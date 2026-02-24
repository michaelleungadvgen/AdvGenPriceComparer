using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using Microsoft.Win32;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for exporting data with advanced filtering and progress tracking
/// </summary>
public class ExportDataViewModel : ViewModelBase
{
    private readonly ExportService _exportService;
    private readonly IDialogService _dialogService;

    // Export Options
    private bool _exportShops = true;
    private bool _exportGoods = true;
    private bool _exportPrices = true;
    private bool _exportManifest = true;
    private bool _enableCompression = false;

    // Filters
    private string? _selectedCategory;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private decimal? _minPrice;
    private decimal? _maxPrice;
    private bool _onlyOnSale;

    // Progress
    private int _progressPercentage;
    private string _progressStatus = string.Empty;
    private bool _isExporting;

    // Output
    private string _outputPath = string.Empty;
    private string _statusMessage = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private ObservableCollection<string> _exportedFiles = new();

    public event EventHandler<bool>? ExportCompleted;

    public ExportDataViewModel(ExportService exportService, IDialogService dialogService)
    {
        _exportService = exportService;
        _dialogService = dialogService;

        // Initialize commands
        BrowseCommand = new RelayCommand(BrowseDirectory);
        ExportCommand = new RelayCommand(async () => await PerformExportAsync(), CanExport);

        // Set default output directory
        _outputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AdvGenPriceComparer",
            "Exports",
            $"export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    #region Export Options

    public bool ExportShops
    {
        get => _exportShops;
        set => SetProperty(ref _exportShops, value);
    }

    public bool ExportGoods
    {
        get => _exportGoods;
        set => SetProperty(ref _exportGoods, value);
    }

    public bool ExportPrices
    {
        get => _exportPrices;
        set => SetProperty(ref _exportPrices, value);
    }

    public bool ExportManifest
    {
        get => _exportManifest;
        set => SetProperty(ref _exportManifest, value);
    }

    public bool EnableCompression
    {
        get => _enableCompression;
        set => SetProperty(ref _enableCompression, value);
    }

    #endregion

    #region Filters

    public string? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public decimal? MinPrice
    {
        get => _minPrice;
        set => SetProperty(ref _minPrice, value);
    }

    public decimal? MaxPrice
    {
        get => _maxPrice;
        set => SetProperty(ref _maxPrice, value);
    }

    public bool OnlyOnSale
    {
        get => _onlyOnSale;
        set => SetProperty(ref _onlyOnSale, value);
    }

    #endregion

    #region Progress

    public int ProgressPercentage
    {
        get => _progressPercentage;
        set => SetProperty(ref _progressPercentage, value);
    }

    public string ProgressStatus
    {
        get => _progressStatus;
        set => SetProperty(ref _progressStatus, value);
    }

    public bool IsExporting
    {
        get => _isExporting;
        set
        {
            SetProperty(ref _isExporting, value);
            ExportCommand.RaiseCanExecuteChanged();
        }
    }

    #endregion

    #region Output

    public string OutputPath
    {
        get => _outputPath;
        set => SetProperty(ref _outputPath, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public Visibility StatusVisibility
    {
        get => _statusVisibility;
        set => SetProperty(ref _statusVisibility, value);
    }

    public ObservableCollection<string> ExportedFiles
    {
        get => _exportedFiles;
        set => SetProperty(ref _exportedFiles, value);
    }

    #endregion

    public RelayCommand BrowseCommand { get; }
    public RelayCommand ExportCommand { get; }

    private void BrowseDirectory()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export Data",
            FileName = Path.GetFileName(OutputPath),
            Filter = EnableCompression
                ? "Compressed JSON (*.json.gz)|*.json.gz|JSON files (*.json)|*.json"
                : "JSON files (*.json)|*.json|Compressed JSON (*.json.gz)|*.json.gz",
            DefaultExt = EnableCompression ? ".json.gz" : ".json"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    private bool CanExport()
    {
        return !IsExporting &&
               !string.IsNullOrWhiteSpace(OutputPath) &&
               (ExportShops || ExportGoods || ExportPrices || ExportManifest);
    }

    private async Task PerformExportAsync()
    {
        try
        {
            IsExporting = true;
            ExportedFiles.Clear();
            StatusMessage = "Starting export...";
            StatusVisibility = Visibility.Visible;
            ProgressPercentage = 0;

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Build export options
            var options = new ExportOptions
            {
                Category = SelectedCategory,
                ValidFrom = FromDate,
                ValidTo = ToDate,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                OnlyOnSale = OnlyOnSale,
                ActiveOnly = true,
                LocationSuburb = "Brisbane",
                LocationState = "QLD",
                LocationCountry = "Australia"
            };

            // Create progress reporter
            var progress = new Progress<ExportProgress>(p =>
            {
                ProgressPercentage = p.Percentage;
                ProgressStatus = p.Status ?? string.Empty;
                StatusMessage = $"{p.Percentage}% - {p.Status}";
            });

            ExportResult result;

            // Perform export based on compression setting
            if (EnableCompression)
            {
                // Ensure correct extension
                if (!OutputPath.EndsWith(".json.gz"))
                {
                    OutputPath = OutputPath.EndsWith(".json")
                        ? OutputPath + ".gz"
                        : OutputPath + ".json.gz";
                }

                result = await _exportService.ExportToJsonGzAsync(options, OutputPath, progress);
            }
            else
            {
                // Ensure correct extension
                if (!OutputPath.EndsWith(".json"))
                {
                    OutputPath = OutputPath.EndsWith(".json.gz")
                        ? OutputPath.Replace(".json.gz", ".json")
                        : OutputPath + ".json";
                }

                result = await _exportService.ExportToJsonAsync(options, OutputPath, progress);
            }

            if (result.Success)
            {
                var fileSize = result.FileSizeBytes / 1024.0;
                var compressionInfo = EnableCompression && result.CompressionRatio < 1.0
                    ? $" (compressed to {result.CompressionRatio:P0})"
                    : string.Empty;

                ExportedFiles.Add($"✓ {Path.GetFileName(result.FilePath)} ({fileSize:F1} KB{compressionInfo})");
                StatusMessage = $"Export completed successfully!\n\n{result.ItemsExported} items exported to:\n{result.FilePath}";
                _dialogService.ShowSuccess($"Successfully exported {result.ItemsExported} items to:\n{result.FilePath}");
                ExportCompleted?.Invoke(this, true);
            }
            else
            {
                StatusMessage = $"Export failed: {result.ErrorMessage}";
                _dialogService.ShowError($"Export failed: {result.ErrorMessage}");
                ExportCompleted?.Invoke(this, false);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            _dialogService.ShowError($"Export failed: {ex.Message}");
            ExportCompleted?.Invoke(this, false);
        }
        finally
        {
            IsExporting = false;
        }
    }
}
