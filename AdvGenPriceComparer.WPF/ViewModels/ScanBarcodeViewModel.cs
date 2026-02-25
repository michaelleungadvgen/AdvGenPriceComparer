using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using Microsoft.Win32;
using ZXing;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for barcode scanning and generation
/// </summary>
public class ScanBarcodeViewModel : ViewModelBase
{
    private readonly IBarcodeService _barcodeService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;

    // Scan tab properties
    private string _scannedBarcodeText = string.Empty;
    private string _barcodeFormat = string.Empty;
    private BitmapImage? _scannedImage;
    private bool _isScanning;
    private string _scanStatusMessage = "Ready to scan";

    // Generate tab properties
    private string _generateText = string.Empty;
    private BarcodeFormat _selectedFormat;
    private int _barcodeWidth = 300;
    private int _barcodeHeight = 300;
    private BitmapImage? _generatedBarcode;
    private bool _isGenerating;

    // Validation
    private string _validationMessage = string.Empty;
    private bool _isValid;
    private List<BarcodeFormatInfo>? _supportedFormats;

    public ScanBarcodeViewModel(
        IBarcodeService barcodeService,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _barcodeService = barcodeService;
        _dialogService = dialogService;
        _logger = logger;
        _selectedFormat = ZXing.BarcodeFormat.QR_CODE;

        // Commands
        ScanFromFileCommand = new RelayCommand(async () => await ScanFromFileAsync(), () => !IsScanning);
        ClearScanCommand = new RelayCommand(ClearScan);
        CopyBarcodeCommand = new RelayCommand(CopyBarcode, () => !string.IsNullOrEmpty(ScannedBarcodeText));
        GenerateBarcodeCommand = new RelayCommand(async () => await GenerateBarcodeAsync(), () => !IsGenerating && !string.IsNullOrWhiteSpace(GenerateText));
        SaveBarcodeCommand = new RelayCommand(async () => await SaveBarcodeAsync(), () => GeneratedBarcode != null);
        ClearGenerateCommand = new RelayCommand(ClearGenerate);

        // Load supported formats lazily
        _supportedFormats = null;
    }

    #region Properties

    /// <summary>
    /// List of supported barcode formats
    /// </summary>
    public List<BarcodeFormatInfo> SupportedFormats => _supportedFormats ??= _barcodeService.GetSupportedFormats();

    /// <summary>
    /// The scanned barcode text
    /// </summary>
    public string ScannedBarcodeText
    {
        get => _scannedBarcodeText;
        set
        {
            if (SetProperty(ref _scannedBarcodeText, value))
            {
                ((RelayCommand)CopyBarcodeCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// The format of the scanned barcode
    /// </summary>
    public string BarcodeFormat
    {
        get => _barcodeFormat;
        set => SetProperty(ref _barcodeFormat, value);
    }

    /// <summary>
    /// The scanned image
    /// </summary>
    public BitmapImage? ScannedImage
    {
        get => _scannedImage;
        set => SetProperty(ref _scannedImage, value);
    }

    /// <summary>
    /// Whether a scan operation is in progress
    /// </summary>
    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (SetProperty(ref _isScanning, value))
            {
                ((RelayCommand)ScanFromFileCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Status message for scanning operations
    /// </summary>
    public string ScanStatusMessage
    {
        get => _scanStatusMessage;
        set => SetProperty(ref _scanStatusMessage, value);
    }

    /// <summary>
    /// Text to generate barcode from
    /// </summary>
    public string GenerateText
    {
        get => _generateText;
        set
        {
            if (SetProperty(ref _generateText, value))
            {
                ValidateInput();
                ((RelayCommand)GenerateBarcodeCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Selected barcode format for generation
    /// </summary>
    public BarcodeFormat SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            if (SetProperty(ref _selectedFormat, value))
            {
                ValidateInput();
            }
        }
    }

    /// <summary>
    /// Width of generated barcode
    /// </summary>
    public int BarcodeWidth
    {
        get => _barcodeWidth;
        set => SetProperty(ref _barcodeWidth, value);
    }

    /// <summary>
    /// Height of generated barcode
    /// </summary>
    public int BarcodeHeight
    {
        get => _barcodeHeight;
        set => SetProperty(ref _barcodeHeight, value);
    }

    /// <summary>
    /// The generated barcode image
    /// </summary>
    public BitmapImage? GeneratedBarcode
    {
        get => _generatedBarcode;
        set
        {
            if (SetProperty(ref _generatedBarcode, value))
            {
                ((RelayCommand)SaveBarcodeCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Whether a generation operation is in progress
    /// </summary>
    public bool IsGenerating
    {
        get => _isGenerating;
        set
        {
            if (SetProperty(ref _isGenerating, value))
            {
                ((RelayCommand)GenerateBarcodeCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Validation message for input
    /// </summary>
    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    /// <summary>
    /// Whether the current input is valid
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    #endregion

    #region Commands

    public ICommand ScanFromFileCommand { get; }
    public ICommand ClearScanCommand { get; }
    public ICommand CopyBarcodeCommand { get; }
    public ICommand GenerateBarcodeCommand { get; }
    public ICommand SaveBarcodeCommand { get; }
    public ICommand ClearGenerateCommand { get; }

    #endregion

    #region Methods

    private async Task ScanFromFileAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Image with Barcode",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
            CheckFileExists = true
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        IsScanning = true;
        ScanStatusMessage = "Scanning...";

        try
        {
            // Load and display the image
            var imageBytes = await File.ReadAllBytesAsync(openFileDialog.FileName);
            ScannedImage = LoadBitmapImage(imageBytes);

            // Decode barcode
            var result = await _barcodeService.DecodeFromImageAsync(openFileDialog.FileName);

            if (result != null)
            {
                ScannedBarcodeText = result.Text;
                BarcodeFormat = result.Format.ToString().Replace("_", "-");
                ScanStatusMessage = $"Barcode found: {result.Format}";
                _logger.LogInfo($"Barcode scanned: {result.Text} ({result.Format})");
            }
            else
            {
                ScannedBarcodeText = string.Empty;
                BarcodeFormat = string.Empty;
                ScanStatusMessage = "No barcode found in image";
                _logger.LogInfo("No barcode found in selected image");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to scan barcode", ex);
            _dialogService.ShowError($"Failed to scan barcode: {ex.Message}");
            ScanStatusMessage = "Scan failed";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void ClearScan()
    {
        ScannedBarcodeText = string.Empty;
        BarcodeFormat = string.Empty;
        ScannedImage = null;
        ScanStatusMessage = "Ready to scan";
    }

    private void CopyBarcode()
    {
        if (!string.IsNullOrEmpty(ScannedBarcodeText))
        {
            Clipboard.SetText(ScannedBarcodeText);
            _dialogService.ShowSuccess($"Barcode '{ScannedBarcodeText}' copied to clipboard!");
        }
    }

    private async Task GenerateBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(GenerateText))
            return;

        IsGenerating = true;

        try
        {
            var barcodeBytes = await _barcodeService.GenerateBarcodeAsync(
                GenerateText,
                SelectedFormat,
                BarcodeWidth,
                BarcodeHeight);

            GeneratedBarcode = LoadBitmapImage(barcodeBytes);
            _logger.LogInfo($"Generated {SelectedFormat} barcode: {GenerateText}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to generate barcode", ex);
            _dialogService.ShowError($"Failed to generate barcode: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private async Task SaveBarcodeAsync()
    {
        if (GeneratedBarcode == null)
            return;

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save Barcode Image",
            Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
            FileName = $"barcode_{GenerateText}.png"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;

        try
        {
            // Encode the bitmap to the selected format
            BitmapEncoder encoder = saveFileDialog.FilterIndex switch
            {
                2 => new JpegBitmapEncoder(),
                3 => new BmpBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };

            encoder.Frames.Add(BitmapFrame.Create(GeneratedBarcode));

            using var stream = File.OpenWrite(saveFileDialog.FileName);
            encoder.Save(stream);

            _dialogService.ShowSuccess($"Barcode saved to: {saveFileDialog.FileName}");
            _logger.LogInfo($"Barcode saved to: {saveFileDialog.FileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save barcode", ex);
            _dialogService.ShowError($"Failed to save barcode: {ex.Message}");
        }
    }

    private void ClearGenerate()
    {
        GenerateText = string.Empty;
        GeneratedBarcode = null;
        ValidationMessage = string.Empty;
        IsValid = false;
    }

    private void ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(GenerateText))
        {
            ValidationMessage = string.Empty;
            IsValid = false;
            return;
        }

        IsValid = _barcodeService.ValidateBarcode(GenerateText, SelectedFormat);
        
        ValidationMessage = IsValid 
            ? $"Valid {SelectedFormat} format" 
            : $"Invalid format for {SelectedFormat}. Check format requirements.";
    }

    private static BitmapImage LoadBitmapImage(byte[] imageData)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = new MemoryStream(imageData);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    #endregion
}
