using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for Server Data Transfer window - handles upload/download of price data to/from server
/// </summary>
public class ServerDataTransferViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly IGroceryDataService _groceryDataService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;
    private HttpClient? _httpClient;

    // Connection settings
    private string _serverHost = "localhost";
    private int _serverPort = 5000;
    private string _apiKey = "";
    private bool _useSsl = true;
    private string _databaseName = "GroceryPrices";

    // Status
    private bool _isConnected;
    private bool _isBusy;
    private string _statusMessage = "Ready";
    private int _progressPercentage;
    private string _connectionStatus = "Not connected";

    // Transfer options
    private bool _uploadItems = true;
    private bool _uploadPlaces = true;
    private bool _uploadPriceRecords = true;
    private bool _downloadItems = true;
    private bool _downloadPlaces = true;
    private bool _downloadPriceRecords = true;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;

    // Results
    private string _transferResult = "";
    private bool _showResult;

    public ServerDataTransferViewModel(
        ISettingsService settingsService,
        IGroceryDataService groceryDataService,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _settingsService = settingsService;
        _groceryDataService = groceryDataService;
        _dialogService = dialogService;
        _logger = logger;

        // Load settings
        LoadSettings();

        // Initialize commands
        ConnectCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsBusy);
        DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected && !IsBusy);
        UploadCommand = new RelayCommand(async () => await UploadAsync(), () => IsConnected && !IsBusy);
        DownloadCommand = new RelayCommand(async () => await DownloadAsync(), () => IsConnected && !IsBusy);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));

        _logger.LogInfo("ServerDataTransferViewModel initialized");
    }

    #region Properties

    public string ServerHost
    {
        get => _serverHost;
        set { _serverHost = value; OnPropertyChanged(); }
    }

    public int ServerPort
    {
        get => _serverPort;
        set { _serverPort = value; OnPropertyChanged(); }
    }

    public string ApiKey
    {
        get => _apiKey;
        set { _apiKey = value; OnPropertyChanged(); }
    }

    public bool UseSsl
    {
        get => _useSsl;
        set { _useSsl = value; OnPropertyChanged(); }
    }

    public string DatabaseName
    {
        get => _databaseName;
        set { _databaseName = value; OnPropertyChanged(); }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set { _isConnected = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set 
        { 
            _isBusy = value; 
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public int ProgressPercentage
    {
        get => _progressPercentage;
        set { _progressPercentage = value; OnPropertyChanged(); }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set { _connectionStatus = value; OnPropertyChanged(); }
    }

    public bool UploadItems
    {
        get => _uploadItems;
        set { _uploadItems = value; OnPropertyChanged(); }
    }

    public bool UploadPlaces
    {
        get => _uploadPlaces;
        set { _uploadPlaces = value; OnPropertyChanged(); }
    }

    public bool UploadPriceRecords
    {
        get => _uploadPriceRecords;
        set { _uploadPriceRecords = value; OnPropertyChanged(); }
    }

    public bool DownloadItems
    {
        get => _downloadItems;
        set { _downloadItems = value; OnPropertyChanged(); }
    }

    public bool DownloadPlaces
    {
        get => _downloadPlaces;
        set { _downloadPlaces = value; OnPropertyChanged(); }
    }

    public bool DownloadPriceRecords
    {
        get => _downloadPriceRecords;
        set { _downloadPriceRecords = value; OnPropertyChanged(); }
    }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set { _dateFrom = value; OnPropertyChanged(); }
    }

    public DateTime? DateTo
    {
        get => _dateTo;
        set { _dateTo = value; OnPropertyChanged(); }
    }

    public string TransferResult
    {
        get => _transferResult;
        set { _transferResult = value; OnPropertyChanged(); }
    }

    public bool ShowResult
    {
        get => _showResult;
        set { _showResult = value; OnPropertyChanged(); }
    }

    public string ConnectionString => $"{(UseSsl ? "https" : "http")}://{ServerHost}:{ServerPort}";

    #endregion

    #region Commands

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand UploadCommand { get; }
    public ICommand DownloadCommand { get; }
    public ICommand CloseCommand { get; }

    #endregion

    #region Methods

    private void LoadSettings()
    {
        ServerHost = _settingsService.ServerHost ?? "localhost";
        ServerPort = _settingsService.ServerPort > 0 ? _settingsService.ServerPort : 5000;
        ApiKey = _settingsService.ApiKey ?? "";
        UseSsl = _settingsService.UseSsl;
        DatabaseName = _settingsService.DatabaseName ?? "GroceryPrices";
    }

    private async Task ConnectAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Connecting to server...";
            ProgressPercentage = 0;

            _logger.LogInfo($"Connecting to server at {ConnectionString}");

            // Create HTTP client
            _httpClient?.Dispose();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ConnectionString),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _httpClient.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Database-Name", DatabaseName);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Test connection with health check
            var response = await _httpClient.GetAsync("/api/prices/stats");
            
            ProgressPercentage = 50;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<ServerStats>(json);
                
                IsConnected = true;
                ConnectionStatus = $"Connected - Server has {stats?.TotalItems ?? 0} items, {stats?.TotalPlaces ?? 0} stores";
                StatusMessage = "Connected successfully";
                
                _logger.LogInfo("Successfully connected to server");
            }
            else
            {
                StatusMessage = $"Connection failed: {response.StatusCode}";
                ConnectionStatus = "Connection failed";
                _logger.LogWarning($"Server connection failed: {response.StatusCode}");
            }

            ProgressPercentage = 100;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection error: {ex.Message}";
            ConnectionStatus = "Connection error";
            _logger.LogError("Failed to connect to server", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Disconnect()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        IsConnected = false;
        ConnectionStatus = "Not connected";
        StatusMessage = "Disconnected";
        _logger.LogInfo("Disconnected from server");
    }

    private async Task UploadAsync()
    {
        if (_httpClient == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Preparing data for upload...";
            ProgressPercentage = 0;
            ShowResult = false;

            var uploadData = new DataUploadRequest();

            // Collect items
            if (UploadItems)
            {
                StatusMessage = "Collecting items...";
                var items = _groceryDataService.GetAllItems();
                foreach (var item in items)
                {
                    uploadData.Items.Add(new SharedItem
                    {
                        ProductId = item.Id,
                        Name = item.Name,
                        Brand = item.Brand,
                        Category = item.Category,
                        Description = item.Description,
                        Barcode = item.Barcode,
                        Unit = item.Unit,
                        Size = item.PackageSize
                    });
                }
            }

            ProgressPercentage = 30;

            // Collect places
            if (UploadPlaces)
            {
                StatusMessage = "Collecting places...";
                var places = _groceryDataService.GetAllPlaces();
                foreach (var place in places)
                {
                    uploadData.Places.Add(new SharedPlace
                    {
                        StoreId = place.Id,
                        Name = place.Name,
                        Chain = place.Chain,
                        Address = place.Address,
                        Suburb = place.Suburb,
                        State = place.State,
                        Postcode = place.Postcode,
                        Country = "Australia"
                    });
                }
            }

            ProgressPercentage = 60;

            // Collect price records
            if (UploadPriceRecords)
            {
                StatusMessage = "Collecting price records...";
                var priceHistory = _groceryDataService.GetPriceHistory(null, null, DateFrom, DateTo);
                foreach (var record in priceHistory)
                {
                    uploadData.PriceRecords.Add(new SharedPriceRecord
                    {
                        ItemId = record.ItemId,
                        PlaceId = record.PlaceId,
                        Price = record.Price,
                        OriginalPrice = record.OriginalPrice,
                        ValidFrom = record.ValidFrom,
                        ValidUntil = record.ValidTo,
                        DateRecorded = record.DateRecorded,
                        Source = record.Source
                    });
                }
            }

            ProgressPercentage = 80;

            // Upload to server
            StatusMessage = "Uploading to server...";
            var json = JsonSerializer.Serialize(uploadData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/prices/upload", content);
            
            if (response.IsSuccessStatusCode)
            {
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UploadResult>(resultJson);
                
                TransferResult = $"Upload successful!\n" +
                    $"Items uploaded: {result?.ItemsUploaded ?? 0}\n" +
                    $"Places uploaded: {result?.PlacesUploaded ?? 0}\n" +
                    $"Prices uploaded: {result?.PricesUploaded ?? 0}";
                
                StatusMessage = "Upload completed";
                _logger.LogInfo($"Upload completed: {result?.ItemsUploaded} items, {result?.PlacesUploaded} places, {result?.PricesUploaded} prices");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TransferResult = $"Upload failed: {response.StatusCode}\n{error}";
                StatusMessage = "Upload failed";
                _logger.LogWarning($"Upload failed: {response.StatusCode}");
            }

            ShowResult = true;
            ProgressPercentage = 100;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload error: {ex.Message}";
            TransferResult = $"Upload error: {ex.Message}";
            ShowResult = true;
            _logger.LogError("Upload failed", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DownloadAsync()
    {
        if (_httpClient == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Downloading data from server...";
            ProgressPercentage = 0;
            ShowResult = false;

            int itemsDownloaded = 0;
            int placesDownloaded = 0;
            int pricesDownloaded = 0;

            // Download items
            if (DownloadItems)
            {
                StatusMessage = "Downloading items...";
                var response = await _httpClient.GetAsync("/api/items?pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiListResponse<SharedItem>>(json);
                    
                    if (result?.Data != null)
                    {
                        foreach (var sharedItem in result.Data)
                        {
                            var item = new Item
                            {
                                Id = sharedItem.ProductId ?? Guid.NewGuid().ToString(),
                                Name = sharedItem.Name ?? "Unknown",
                                Brand = sharedItem.Brand,
                                Category = sharedItem.Category,
                                Description = sharedItem.Description,
                                Barcode = sharedItem.Barcode,
                                Unit = sharedItem.Unit,
                                PackageSize = sharedItem.Size
                            };
                            
                            try
                            {
                                _groceryDataService.AddGroceryItem(
                                    item.Name, item.Brand, item.Category, 
                                    item.Barcode, item.PackageSize, item.Unit);
                                itemsDownloaded++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Failed to add item {item.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            ProgressPercentage = 40;

            // Download places
            if (DownloadPlaces)
            {
                StatusMessage = "Downloading places...";
                var response = await _httpClient.GetAsync("/api/places?pageSize=1000");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiListResponse<SharedPlace>>(json);
                    
                    if (result?.Data != null)
                    {
                        foreach (var sharedPlace in result.Data)
                        {
                            try
                            {
                                _groceryDataService.AddSupermarket(
                                    sharedPlace.Name ?? "Unknown",
                                    sharedPlace.Chain ?? "Unknown",
                                    sharedPlace.Address,
                                    sharedPlace.Suburb,
                                    sharedPlace.State,
                                    sharedPlace.Postcode);
                                placesDownloaded++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Failed to add place {sharedPlace.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            ProgressPercentage = 80;

            // Download price records
            if (DownloadPriceRecords)
            {
                StatusMessage = "Downloading price records...";
                var fromParam = DateFrom?.ToString("yyyy-MM-dd");
                var toParam = DateTo?.ToString("yyyy-MM-dd");
                var url = $"/api/prices/download?pageSize=1000";
                if (!string.IsNullOrEmpty(fromParam)) url += $"&from={fromParam}";
                if (!string.IsNullOrEmpty(toParam)) url += $"&to={toParam}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiListResponse<SharedPriceRecord>>(json);
                    
                    if (result?.Data != null)
                    {
                        foreach (var sharedPrice in result.Data)
                        {
                            try
                            {
                                _groceryDataService.RecordPrice(
                                    sharedPrice.ItemId ?? "",
                                    sharedPrice.PlaceId ?? "",
                                    sharedPrice.Price,
                                    sharedPrice.OriginalPrice.HasValue && sharedPrice.OriginalPrice > sharedPrice.Price,
                                    sharedPrice.OriginalPrice,
                                    null,
                                    sharedPrice.ValidFrom,
                                    sharedPrice.ValidUntil,
                                    sharedPrice.Source ?? "server");
                                pricesDownloaded++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Failed to add price record: {ex.Message}");
                            }
                        }
                    }
                }
            }

            TransferResult = $"Download completed!\n" +
                $"Items downloaded: {itemsDownloaded}\n" +
                $"Places downloaded: {placesDownloaded}\n" +
                $"Prices downloaded: {pricesDownloaded}";
            
            StatusMessage = "Download completed";
            ShowResult = true;
            ProgressPercentage = 100;
            
            _logger.LogInfo($"Download completed: {itemsDownloaded} items, {placesDownloaded} places, {pricesDownloaded} prices");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download error: {ex.Message}";
            TransferResult = $"Download error: {ex.Message}";
            ShowResult = true;
            _logger.LogError("Download failed", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? RequestClose;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Data Models

    private class DataUploadRequest
    {
        public List<SharedItem> Items { get; set; } = new();
        public List<SharedPlace> Places { get; set; } = new();
        public List<SharedPriceRecord> PriceRecords { get; set; } = new();
    }

    private class SharedItem
    {
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Unit { get; set; }
        public string? Size { get; set; }
    }

    private class SharedPlace
    {
        public string? StoreId { get; set; }
        public string? Name { get; set; }
        public string? Chain { get; set; }
        public string? Address { get; set; }
        public string? Suburb { get; set; }
        public string? State { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
    }

    private class SharedPriceRecord
    {
        public string? ItemId { get; set; }
        public string? PlaceId { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime DateRecorded { get; set; }
        public string? Source { get; set; }
    }

    private class ServerStats
    {
        public int TotalItems { get; set; }
        public int TotalPlaces { get; set; }
        public int TotalPriceRecords { get; set; }
        public DateTime LatestUpdate { get; set; }
    }

    private class UploadResult
    {
        public bool Success { get; set; }
        public int ItemsUploaded { get; set; }
        public int PlacesUploaded { get; set; }
        public int PricesUploaded { get; set; }
        public string? ErrorMessage { get; set; }
    }

    private class ApiListResponse<T>
    {
        public bool Success { get; set; }
        public List<T>? Data { get; set; }
        public int TotalCount { get; set; }
    }

    #endregion
}
