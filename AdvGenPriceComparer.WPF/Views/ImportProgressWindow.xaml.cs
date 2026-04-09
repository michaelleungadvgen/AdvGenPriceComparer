using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// A modal progress window for import operations with detailed progress reporting
/// </summary>
public partial class ImportProgressWindow : Window, INotifyPropertyChanged
{
    private int _progressPercentage;
    private string _statusMessage = "Initializing...";
    private string _currentItem = "";
    private string _progressText = "0 / 0";
    private int _itemsCreated;
    private int _priceRecordsCreated;
    private int _errorCount;
    private int _totalItems;
    private bool _isIndeterminate = true;
    private bool _canCancel = true;
    private CancellationTokenSource? _cancellationTokenSource;

    public ImportProgressWindow()
    {
        InitializeComponent();
        DataContext = this;
        
        CancelCommand = new RelayCommand(Cancel, () => CanCancel);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Current progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage
    {
        get => _progressPercentage;
        set
        {
            if (_progressPercentage != value)
            {
                _progressPercentage = value;
                IsIndeterminate = false;
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }
    }

    /// <summary>
    /// Current status message displayed to the user
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    /// <summary>
    /// Current item being processed
    /// </summary>
    public string CurrentItem
    {
        get => _currentItem;
        set
        {
            if (_currentItem != value)
            {
                _currentItem = value;
                OnPropertyChanged(nameof(CurrentItem));
            }
        }
    }

    /// <summary>
    /// Progress text showing processed/total (e.g., "50 / 100")
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set
        {
            if (_progressText != value)
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }
    }

    /// <summary>
    /// Number of new items created
    /// </summary>
    public int ItemsCreated
    {
        get => _itemsCreated;
        set
        {
            if (_itemsCreated != value)
            {
                _itemsCreated = value;
                OnPropertyChanged(nameof(ItemsCreated));
            }
        }
    }

    /// <summary>
    /// Number of price records created
    /// </summary>
    public int PriceRecordsCreated
    {
        get => _priceRecordsCreated;
        set
        {
            if (_priceRecordsCreated != value)
            {
                _priceRecordsCreated = value;
                OnPropertyChanged(nameof(PriceRecordsCreated));
            }
        }
    }

    /// <summary>
    /// Number of errors encountered
    /// </summary>
    public int ErrorCount
    {
        get => _errorCount;
        set
        {
            if (_errorCount != value)
            {
                _errorCount = value;
                OnPropertyChanged(nameof(ErrorCount));
            }
        }
    }

    /// <summary>
    /// Total number of items to process
    /// </summary>
    public int TotalItems
    {
        get => _totalItems;
        set
        {
            if (_totalItems != value)
            {
                _totalItems = value;
                UpdateProgressText();
                OnPropertyChanged(nameof(TotalItems));
            }
        }
    }

    /// <summary>
    /// Current number of processed items
    /// </summary>
    public int ProcessedItems
    {
        get => _processedItems;
        set
        {
            if (_processedItems != value)
            {
                _processedItems = value;
                UpdateProgressText();
                CalculateProgress();
                OnPropertyChanged(nameof(ProcessedItems));
            }
        }
    }
    private int _processedItems;

    /// <summary>
    /// Whether the progress bar should show as indeterminate
    /// </summary>
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set
        {
            if (_isIndeterminate != value)
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }
    }

    /// <summary>
    /// Whether the operation can be cancelled
    /// </summary>
    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            if (_canCancel != value)
            {
                _canCancel = value;
                OnPropertyChanged(nameof(CanCancel));
            }
        }
    }

    /// <summary>
    /// Cancellation token source for the operation
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource => _cancellationTokenSource;

    /// <summary>
    /// Command to cancel the operation
    /// </summary>
    public ICommand CancelCommand { get; }

    private void UpdateProgressText()
    {
        ProgressText = $"{ProcessedItems} / {TotalItems}";
    }

    private void CalculateProgress()
    {
        if (TotalItems > 0)
        {
            ProgressPercentage = (int)((double)ProcessedItems / TotalItems * 100);
        }
    }

    /// <summary>
    /// Report progress update
    /// </summary>
    public void ReportProgress(int processed, int total, string currentItem)
    {
        Dispatcher.Invoke(() =>
        {
            ProcessedItems = processed;
            TotalItems = total;
            CurrentItem = currentItem;
            StatusMessage = $"Processing {processed} of {total} items...";
        });
    }

    /// <summary>
    /// Update statistics display
    /// </summary>
    public void UpdateStatistics(int itemsCreated, int priceRecords, int errors)
    {
        Dispatcher.Invoke(() =>
        {
            ItemsCreated = itemsCreated;
            PriceRecordsCreated = priceRecords;
            ErrorCount = errors;
        });
    }

    /// <summary>
    /// Mark the operation as completed and close the window
    /// </summary>
    public void Complete(string? finalMessage = null)
    {
        Dispatcher.Invoke(() =>
        {
            if (!string.IsNullOrEmpty(finalMessage))
            {
                StatusMessage = finalMessage;
            }
            else
            {
                StatusMessage = $"Complete! {ItemsCreated} items, {PriceRecordsCreated} price records";
            }
            CanCancel = false;
            DialogResult = true;
            Close();
        });
    }

    /// <summary>
    /// Mark the operation as failed and close the window
    /// </summary>
    public void Fail(string errorMessage)
    {
        Dispatcher.Invoke(() =>
        {
            StatusMessage = $"Error: {errorMessage}";
            CanCancel = false;
            DialogResult = false;
            Close();
        });
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Cancelling...";
        CanCancel = false;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // If the user tries to close the window while operation is running,
        // treat it as a cancel request
        if (CanCancel && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
            StatusMessage = "Cancelling...";
            CanCancel = false;
            e.Cancel = true; // Don't close immediately, let the operation handle cancellation
        }
        
        base.OnClosing(e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
