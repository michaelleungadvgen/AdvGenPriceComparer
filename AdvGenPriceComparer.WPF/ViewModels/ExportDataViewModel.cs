using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using Microsoft.Win32;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class ExportDataViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private readonly JsonExportService _exportService;

    private bool _exportShops = true;
    private bool _exportGoods = true;
    private bool _exportPrices = true;
    private bool _exportManifest = true;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string _outputDirectory = string.Empty;
    private string _statusMessage = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private ObservableCollection<string> _exportedFiles = new();

    public event EventHandler<bool>? ExportCompleted;

    public ExportDataViewModel(IGroceryDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _exportService = new JsonExportService(dataService);

        // Initialize commands
        BrowseCommand = new RelayCommand(BrowseDirectory);
        ExportCommand = new RelayCommand(PerformExport, CanExport);

        // Set default output directory
        _outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AdvGenPriceComparer",
            "Exports",
            DateTime.Now.ToString("yyyyMMdd_HHmmss"));
    }

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

    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
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

    public RelayCommand BrowseCommand { get; }
    public RelayCommand ExportCommand { get; }

    private void BrowseDirectory()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select Export Directory",
            FileName = "Select Folder",
            Filter = "Folder|*.folder",
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = Path.GetDirectoryName(dialog.FileName) ?? OutputDirectory;
        }
    }

    private bool CanExport()
    {
        return !string.IsNullOrWhiteSpace(OutputDirectory) &&
               (ExportShops || ExportGoods || ExportPrices || ExportManifest);
    }

    private void PerformExport()
    {
        try
        {
            // Create output directory
            Directory.CreateDirectory(OutputDirectory);

            ExportedFiles.Clear();
            StatusMessage = "Exporting data...";
            StatusVisibility = Visibility.Visible;

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            // Export Shops
            if (ExportShops)
            {
                var shopsPath = Path.Combine(OutputDirectory, "Shop.json");
                _exportService.ExportShops(shopsPath);
                ExportedFiles.Add($"✓ Shop.json ({new FileInfo(shopsPath).Length / 1024} KB)");
            }

            // Export Goods
            if (ExportGoods)
            {
                var goodsPath = Path.Combine(OutputDirectory, "Goods.json");
                _exportService.ExportGoods(goodsPath);
                ExportedFiles.Add($"✓ Goods.json ({new FileInfo(goodsPath).Length / 1024} KB)");
            }

            // Export Prices
            if (ExportPrices)
            {
                var pricesPath = Path.Combine(OutputDirectory, $"price-{timestamp}.json");
                _exportService.ExportPrices(pricesPath, FromDate, ToDate);
                ExportedFiles.Add($"✓ price-{timestamp}.json ({new FileInfo(pricesPath).Length / 1024} KB)");
            }

            // Export Manifest
            if (ExportManifest)
            {
                var recordsPath = Path.Combine(OutputDirectory, "records.json");
                _exportService.ExportRecordsManifest(recordsPath, OutputDirectory);
                ExportedFiles.Add($"✓ records.json ({new FileInfo(recordsPath).Length / 1024} KB)");
            }

            StatusMessage = $"Export completed successfully!\n\nFiles exported to:\n{OutputDirectory}";
            _dialogService.ShowSuccess($"Successfully exported {ExportedFiles.Count} file(s) to:\n{OutputDirectory}");

            ExportCompleted?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            _dialogService.ShowError($"Export failed: {ex.Message}");
            ExportCompleted?.Invoke(this, false);
        }
    }
}
