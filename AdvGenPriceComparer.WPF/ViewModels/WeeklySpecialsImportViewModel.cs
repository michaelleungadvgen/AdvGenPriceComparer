using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Services;
using Microsoft.Win32;


namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for Weekly Specials Import dialog
/// </summary>
public class WeeklySpecialsImportViewModel : ViewModelBase
{
    private readonly IWeeklySpecialsImportService _importService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;

    // Properties
    private SupermarketChain _selectedChain = SupermarketChain.Coles;
    public SupermarketChain SelectedChain
    {
        get => _selectedChain;
        set
        {
            _selectedChain = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileFilter));
            OnPropertyChanged(nameof(SelectedChainDescription));
        }
    }

    private string _selectedFilePath = string.Empty;
    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set
        {
            _selectedFilePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanImport));
            OnPropertyChanged(nameof(CanPreview));
            _ = DetectChainAndDatesAsync();
        }
    }

    private DateTime? _validFrom;
    public DateTime? ValidFrom
    {
        get => _validFrom;
        set { _validFrom = value; OnPropertyChanged(); }
    }

    private DateTime? _validTo;
    public DateTime? ValidTo
    {
        get => _validTo;
        set { _validTo = value; OnPropertyChanged(); }
    }

    private bool _skipDuplicates = true;
    public bool SkipDuplicates
    {
        get => _skipDuplicates;
        set { _skipDuplicates = value; OnPropertyChanged(); }
    }

    private bool _autoCategorize = true;
    public bool AutoCategorize
    {
        get => _autoCategorize;
        set { _autoCategorize = value; OnPropertyChanged(); }
    }

    private bool _isImporting;
    public bool IsImporting
    {
        get => _isImporting;
        set
        {
            _isImporting = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanImport));
            OnPropertyChanged(nameof(CanPreview));
            OnPropertyChanged(nameof(CanSelectFile));
        }
    }

    private int _importProgress;
    public int ImportProgress
    {
        get => _importProgress;
        set { _importProgress = value; OnPropertyChanged(); }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    private string _currentProductName = string.Empty;
    public string CurrentProductName
    {
        get => _currentProductName;
        set { _currentProductName = value; OnPropertyChanged(); }
    }

    private ObservableCollection<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem> _previewItems = new();
    public ObservableCollection<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem> PreviewItems
    {
        get => _previewItems;
        set { _previewItems = value; OnPropertyChanged(); }
    }

    private bool _showPreview;
    public bool ShowPreview
    {
        get => _showPreview;
        set { _showPreview = value; OnPropertyChanged(); }
    }

    private WeeklySpecialsImportResult? _lastImportResult;
    public WeeklySpecialsImportResult? LastImportResult
    {
        get => _lastImportResult;
        set { _lastImportResult = value; OnPropertyChanged(); }
    }

    // Computed properties
    public string FileFilter => SelectedChain switch
    {
        SupermarketChain.Coles or SupermarketChain.Woolworths => "JSON files (*.json)|*.json",
        SupermarketChain.Aldi or SupermarketChain.Drakes => "Markdown files (*.md;*.markdown;*.txt)|*.md;*.markdown;*.txt",
        _ => "All files (*.*)|*.*"
    };

    public string SelectedChainDescription => SelectedChain switch
    {
        SupermarketChain.Coles => "Coles - Import from JSON catalogue files",
        SupermarketChain.Woolworths => "Woolworths - Import from JSON catalogue files",
        SupermarketChain.Aldi => "ALDI - Import from Markdown catalogue files",
        SupermarketChain.Drakes => "Drakes - Import from Markdown catalogue files",
        _ => "Select a supermarket chain"
    };

    public bool CanImport => !string.IsNullOrEmpty(SelectedFilePath) && !IsImporting;
    public bool CanPreview => !string.IsNullOrEmpty(SelectedFilePath) && !IsImporting;
    public bool CanSelectFile => !IsImporting;

    public Array SupermarketChains => Enum.GetValues(typeof(SupermarketChain));

    // Commands
    public ICommand SelectFileCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler? RequestClose;

    public WeeklySpecialsImportViewModel(
        IWeeklySpecialsImportService importService,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SelectFileCommand = new RelayCommand(SelectFile);
        PreviewCommand = new RelayCommand(async () => await PreviewAsync(), () => CanPreview);
        ImportCommand = new RelayCommand(async () => await ImportAsync(), () => CanImport);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = FileFilter,
            Title = $"Select {SelectedChain} Catalogue File"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            _logger.LogInfo($"Selected file: {SelectedFilePath}");
        }
    }

    private async Task DetectChainAndDatesAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        try
        {
            // Try to auto-detect chain
            var detectedChain = await _importService.DetectChainAsync(SelectedFilePath);
            if (detectedChain.HasValue && detectedChain.Value != SelectedChain)
            {
                SelectedChain = detectedChain.Value;
                StatusMessage = $"Auto-detected: {SelectedChain}";
            }

            // Try to extract dates
            var (from, to) = await _importService.ExtractDateRangeAsync(SelectedFilePath, SelectedChain);
            if (from.HasValue) ValidFrom = from.Value;
            if (to.HasValue) ValidTo = to.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not auto-detect chain or dates: {ex.Message}");
        }
    }

    private async Task PreviewAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        IsImporting = true;
        StatusMessage = "Loading preview...";

        try
        {
            var options = CreateImportOptions();
            var items = await _importService.PreviewImportAsync(options);

            PreviewItems = new ObservableCollection<WeeklySpecialItem>(items);
            ShowPreview = true;
            StatusMessage = $"Preview loaded: {items.Count} items found";
            _logger.LogInfo($"Preview loaded with {items.Count} items");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Preview failed: {ex.Message}";
            _logger.LogError("Preview failed", ex);
            _dialogService.ShowError($"Failed to load preview: {ex.Message}", "Preview Error");
        }
        finally
        {
            IsImporting = false;
        }
    }

    private async Task ImportAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        // Confirm import
        if (!_dialogService.ShowQuestion($"Import weekly specials from {SelectedChain}?", "Confirm Import"))
        {
            return;
        }

        IsImporting = true;
        ImportProgress = 0;
        StatusMessage = "Starting import...";
        ShowPreview = false;

        try
        {
            var progress = new Progress<WeeklySpecialsImportProgress>(p =>
            {
                ImportProgress = p.PercentageComplete;
                CurrentProductName = p.CurrentProductName;
                StatusMessage = $"Importing... {p.CurrentItem} of {p.TotalItems} ({p.PercentageComplete}%)";
            });

            var options = CreateImportOptions();
            options.Progress = progress;

            var result = await _importService.ImportFromFileAsync(options);
            LastImportResult = result;

            if (result.Success)
            {
                StatusMessage = result.Message;
                _logger.LogInfo($"Import successful: {result.Message}");
                _dialogService.ShowInfo($"Import completed successfully!\n\n{result.Message}", "Import Complete");
                
                // Close dialog on successful import
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = $"Import failed: {result.Message}";
                _logger.LogWarning($"Import failed: {result.Message}");
                _dialogService.ShowError($"Import failed:\n{result.Message}", "Import Error");
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Import cancelled";
            _logger.LogInfo("Import cancelled by user");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import error: {ex.Message}";
            _logger.LogError("Import error", ex);
            _dialogService.ShowError($"Import error:\n{ex.Message}", "Import Error");
        }
        finally
        {
            IsImporting = false;
        }
    }

    private WeeklySpecialsImportOptions CreateImportOptions()
    {
        return new WeeklySpecialsImportOptions
        {
            Chain = SelectedChain,
            FilePath = SelectedFilePath,
            ValidFrom = ValidFrom,
            ValidTo = ValidTo,
            SkipDuplicates = SkipDuplicates,
            AutoCategorize = AutoCategorize
        };
    }
}
