using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// A modal progress window for export operations with progress reporting
/// </summary>
public partial class ExportProgressWindow : Window, INotifyPropertyChanged
{
    private int _progressPercentage;
    private string _statusMessage = "Initializing...";
    private bool _isIndeterminate = true;
    private bool _canCancel = true;
    private CancellationTokenSource? _cancellationTokenSource;

    public ExportProgressWindow()
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

    /// <summary>
    /// Report progress update
    /// </summary>
    public void ReportProgress(int percentage, string status)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressPercentage = percentage;
            StatusMessage = status;
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
