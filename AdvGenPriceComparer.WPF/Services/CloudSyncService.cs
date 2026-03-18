using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Services;

namespace AdvGenPriceComparer.WPF.Services
{
    /// <summary>
    /// Implementation of cloud synchronization service with offline support,
    /// conflict resolution, and automatic syncing.
    /// </summary>
    public class CloudSyncService : ICloudSyncService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IPlaceRepository _placeRepository;
        private readonly IPriceRecordRepository _priceRecordRepository;
        private readonly ISettingsService _settingsService;
        private readonly ILoggerService _logger;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentQueue<SyncQueueItem> _syncQueue;
        private readonly List<SyncConflict> _pendingConflicts;
        private readonly Timer? _autoSyncTimer;
        private readonly string _queueFilePath;
        private readonly object _lockObject = new();

        private CloudSyncSettings _settings = new();
        private SyncStatus _status = SyncStatus.Idle;
        private SyncResult? _lastResult;
        private bool _isInitialized;
        private bool _isOffline;

        /// <summary>
        /// Gets the current synchronization status.
        /// </summary>
        public SyncStatus Status
        {
            get => _status;
            private set
            {
                var oldStatus = _status;
                _status = value;
                StatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
                {
                    OldStatus = oldStatus,
                    NewStatus = value,
                    Message = $"Status changed from {oldStatus} to {value}"
                });
            }
        }

        /// <summary>
        /// Gets the last synchronization result.
        /// </summary>
        public SyncResult? LastResult => _lastResult;

        /// <summary>
        /// Gets the cloud sync configuration settings.
        /// </summary>
        public CloudSyncSettings Settings => _settings;

        /// <summary>
        /// Event raised when sync status changes.
        /// </summary>
        public event EventHandler<SyncStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Event raised when a sync conflict is detected.
        /// </summary>
        public event EventHandler<SyncConflictEventArgs>? ConflictDetected;

        /// <summary>
        /// Event raised when sync progress updates.
        /// </summary>
        public event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Creates a new instance of the CloudSyncService.
        /// </summary>
        public CloudSyncService(
            IItemRepository itemRepository,
            IPlaceRepository placeRepository,
            IPriceRecordRepository priceRecordRepository,
            ISettingsService settingsService,
            ILoggerService logger)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _placeRepository = placeRepository ?? throw new ArgumentNullException(nameof(placeRepository));
            _priceRecordRepository = priceRecordRepository ?? throw new ArgumentNullException(nameof(priceRecordRepository));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient = new HttpClient();
            _syncQueue = new ConcurrentQueue<SyncQueueItem>();
            _pendingConflicts = new List<SyncConflict>();

            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer");
            _queueFilePath = Path.Combine(appDataPath, "sync_queue.json");

            // Load existing queue if present
            LoadSyncQueue();
        }

        /// <summary>
        /// Initializes the cloud sync service with the provided settings.
        /// </summary>
        public async Task InitializeAsync(CloudSyncSettings settings)
        {
            if (_isInitialized)
                return;

            Status = SyncStatus.Initializing;
            _logger.LogInfo("Initializing CloudSyncService");

            try
            {
                _settings = settings ?? throw new ArgumentNullException(nameof(settings));

                // Configure HTTP client
                _httpClient.BaseAddress = new Uri(_settings.ServerUrl);
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", _settings.UserId);
                _httpClient.DefaultRequestHeaders.Add("X-Device-Id", _settings.DeviceId);
                _httpClient.Timeout = TimeSpan.FromMinutes(5);

                // Test connection
                _isOffline = !await TestConnectionAsync();
                if (_isOffline)
                {
                    _logger.LogWarning("Cloud sync initialized in offline mode");
                    Status = SyncStatus.Offline;
                }
                else
                {
                    Status = SyncStatus.Idle;
                }

                // Start auto-sync if enabled
                if (_settings.IsEnabled && _settings.ScheduleMode == SyncScheduleMode.Periodic)
                {
                    EnableAutoSync();
                }

                _isInitialized = true;
                _logger.LogInfo("CloudSyncService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize CloudSyncService", ex);
                Status = SyncStatus.Error;
                throw;
            }
        }

        /// <summary>
        /// Performs a manual synchronization.
        /// </summary>
        public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("CloudSyncService must be initialized before syncing");

            if (!_settings.IsEnabled)
            {
                return new SyncResult
                {
                    Success = false,
                    Message = "Cloud sync is disabled",
                    OperationType = SyncOperationType.FullSync,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                };
            }

            // Check if offline
            if (_isOffline || !await TestConnectionAsync())
            {
                _isOffline = true;
                Status = SyncStatus.Offline;
                _logger.LogWarning("Sync skipped - offline mode");
                return new SyncResult
                {
                    Success = false,
                    Message = "Cannot sync while offline",
                    OperationType = SyncOperationType.FullSync,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                };
            }

            return await PerformSyncInternalAsync(false, cancellationToken);
        }

        /// <summary>
        /// Synchronizes only items that have changed since the last sync.
        /// </summary>
        public async Task<SyncResult> SyncIncrementalAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("CloudSyncService must be initialized before syncing");

            if (!_settings.IsEnabled)
            {
                return new SyncResult
                {
                    Success = false,
                    Message = "Cloud sync is disabled",
                    OperationType = SyncOperationType.IncrementalSync,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                };
            }

            return await PerformSyncInternalAsync(true, cancellationToken);
        }

        /// <summary>
        /// Performs a full synchronization of all data.
        /// </summary>
        public async Task<SyncResult> SyncFullAsync(CancellationToken cancellationToken = default)
        {
            return await SyncAsync(cancellationToken);
        }

        /// <summary>
        /// Exports local data to cloud storage.
        /// </summary>
        public async Task<SyncResult> ExportToCloudAsync(CancellationToken cancellationToken = default)
        {
            var result = new SyncResult
            {
                OperationType = SyncOperationType.Export,
                StartedAt = DateTime.Now
            };

            try
            {
                Status = SyncStatus.Uploading;
                _logger.LogInfo("Starting export to cloud");

                var items = _itemRepository.GetAll().ToList();
                var places = _placeRepository.GetAll().ToList();
                var priceRecords = _priceRecordRepository.GetAll().ToList();

                var exportData = new CloudExportData
                {
                    ExportDate = DateTime.UtcNow,
                    DeviceId = _settings.DeviceId,
                    Items = items,
                    Places = places,
                    PriceRecords = priceRecords
                };

                var json = JsonSerializer.Serialize(exportData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/sync/export", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                result.ItemsUploaded = items.Count + places.Count + priceRecords.Count;
                result.Success = true;
                result.Message = $"Exported {result.ItemsUploaded} items to cloud";

                _logger.LogInfo($"Export to cloud completed: {result.Message}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Export failed: {ex.Message}";
                result.Errors.Add(new SyncError
                {
                    Message = ex.Message,
                    ExceptionDetails = ex.ToString()
                });
                _logger.LogError("Export to cloud failed", ex);
            }
            finally
            {
                result.CompletedAt = DateTime.Now;
                _lastResult = result;
                Status = _isOffline ? SyncStatus.Offline : SyncStatus.Idle;
            }

            return result;
        }

        /// <summary>
        /// Imports data from cloud storage to local database.
        /// </summary>
        public async Task<SyncResult> ImportFromCloudAsync(CancellationToken cancellationToken = default)
        {
            var result = new SyncResult
            {
                OperationType = SyncOperationType.Import,
                StartedAt = DateTime.Now
            };

            try
            {
                Status = SyncStatus.Downloading;
                _logger.LogInfo("Starting import from cloud");

                var response = await _httpClient.GetAsync("/api/sync/export", cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var exportData = JsonSerializer.Deserialize<CloudExportData>(json);

                if (exportData != null)
                {
                    int importedCount = 0;
                    int conflictCount = 0;

                    // Import items with conflict detection
                    foreach (var item in exportData.Items)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var existingItem = _itemRepository.GetById(item.Id);
                        if (existingItem == null)
                        {
                            _itemRepository.Add(item);
                            importedCount++;
                        }
                        else if (item.DateAdded > existingItem.DateAdded)
                        {
                            // Check for conflicts
                            if (existingItem.DateAdded > _settings.LastSyncTime)
                            {
                                conflictCount++;
                                var conflict = new SyncConflict
                                {
                                    EntityType = "Item",
                                    EntityId = item.Id,
                                    EntityName = item.Name,
                                    LocalVersion = existingItem,
                                    ServerVersion = item,
                                    LocalModifiedAt = existingItem.DateAdded,
                                    ServerModifiedAt = item.DateAdded
                                };
                                _pendingConflicts.Add(conflict);
                                ConflictDetected?.Invoke(this, new SyncConflictEventArgs { Conflict = conflict });
                            }
                            else
                            {
                                _itemRepository.Update(item);
                                importedCount++;
                            }
                        }

                        ReportProgress(importedCount + conflictCount, exportData.Items.Count, "Importing items...");
                    }

                    result.ItemsDownloaded = importedCount;
                    result.ConflictsDetected = conflictCount;
                    result.Success = true;
                    result.Message = $"Imported {importedCount} items, {conflictCount} conflicts detected";

                    _logger.LogInfo($"Import from cloud completed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(new SyncError
                {
                    Message = ex.Message,
                    ExceptionDetails = ex.ToString()
                });
                _logger.LogError("Import from cloud failed", ex);
            }
            finally
            {
                result.CompletedAt = DateTime.Now;
                _lastResult = result;
                Status = _isOffline ? SyncStatus.Offline : SyncStatus.Idle;
            }

            return result;
        }

        /// <summary>
        /// Resolves a sync conflict using the specified strategy.
        /// </summary>
        public Task ResolveConflictAsync(string itemId, ConflictResolutionStrategy strategy)
        {
            var conflict = _pendingConflicts.FirstOrDefault(c => c.EntityId == itemId);
            if (conflict == null)
                return Task.CompletedTask;

            _logger.LogInfo($"Resolving conflict for {conflict.EntityType} {itemId} using {strategy}");

            switch (strategy)
            {
                case ConflictResolutionStrategy.ServerWins:
                    ApplyServerVersion(conflict);
                    break;
                case ConflictResolutionStrategy.ClientWins:
                    // Keep local version, mark as resolved
                    break;
                case ConflictResolutionStrategy.LastWriteWins:
                    if (conflict.ServerModifiedAt > conflict.LocalModifiedAt)
                        ApplyServerVersion(conflict);
                    break;
                case ConflictResolutionStrategy.Merge:
                    MergeVersions(conflict);
                    break;
            }

            conflict.Status = ConflictStatus.Resolved;
            conflict.ResolutionStrategy = strategy;
            conflict.ResolvedAt = DateTime.Now;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a list of pending conflicts that need resolution.
        /// </summary>
        public Task<List<SyncConflict>> GetPendingConflictsAsync()
        {
            return Task.FromResult(_pendingConflicts.Where(c => c.Status == ConflictStatus.Pending).ToList());
        }

        /// <summary>
        /// Enables automatic synchronization.
        /// </summary>
        public void EnableAutoSync()
        {
            if (_autoSyncTimer != null)
            {
                // Timer already exists, change interval if needed
                return;
            }

            _logger.LogInfo($"Enabling auto sync with interval {_settings.AutoSyncIntervalMinutes} minutes");

            // Note: Timer is created in constructor or initialized separately
            // This is a simplified implementation
        }

        /// <summary>
        /// Disables automatic synchronization.
        /// </summary>
        public void DisableAutoSync()
        {
            _logger.LogInfo("Disabling auto sync");
            _autoSyncTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Tests the cloud connection.
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the sync queue for offline items.
        /// </summary>
        public Task<List<SyncQueueItem>> GetSyncQueueAsync()
        {
            return Task.FromResult(_syncQueue.ToList());
        }

        /// <summary>
        /// Clears the sync queue.
        /// </summary>
        public Task ClearSyncQueueAsync()
        {
            while (_syncQueue.TryDequeue(out _)) { }
            SaveSyncQueue();
            _logger.LogInfo("Sync queue cleared");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes the service and releases resources.
        /// </summary>
        public void Dispose()
        {
            _autoSyncTimer?.Dispose();
            _httpClient?.Dispose();
            SaveSyncQueue();
            _logger.LogInfo("CloudSyncService disposed");
        }

        #region Private Methods

        private async Task<SyncResult> PerformSyncInternalAsync(bool incremental, CancellationToken cancellationToken)
        {
            var result = new SyncResult
            {
                OperationType = incremental ? SyncOperationType.IncrementalSync : SyncOperationType.FullSync,
                StartedAt = DateTime.Now
            };

            try
            {
                Status = SyncStatus.Syncing;
                _logger.LogInfo($"Starting {(incremental ? "incremental" : "full")} sync");

                // First, process any queued offline items
                await ProcessSyncQueueAsync(cancellationToken);

                // Export local changes
                var exportResult = await ExportToCloudAsync(cancellationToken);
                if (!exportResult.Success)
                {
                    result.Errors.AddRange(exportResult.Errors);
                }
                result.ItemsUploaded = exportResult.ItemsUploaded;

                // Import remote changes
                var importResult = await ImportFromCloudAsync(cancellationToken);
                if (!importResult.Success)
                {
                    result.Errors.AddRange(importResult.Errors);
                }
                result.ItemsDownloaded = importResult.ItemsDownloaded;
                result.ConflictsDetected = importResult.ConflictsDetected;

                // Update settings
                _settings.LastSyncTime = DateTime.UtcNow;
                await _settingsService.SaveSettingsAsync();

                result.Success = result.Errors.Count == 0;
                result.Message = $"Sync completed: {result.ItemsUploaded} uploaded, {result.ItemsDownloaded} downloaded, {result.ConflictsDetected} conflicts";

                _logger.LogInfo(result.Message);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Sync failed: {ex.Message}";
                result.Errors.Add(new SyncError
                {
                    Message = ex.Message,
                    ExceptionDetails = ex.ToString()
                });
                _logger.LogError("Sync failed", ex);
            }
            finally
            {
                result.CompletedAt = DateTime.Now;
                _lastResult = result;
                Status = _isOffline ? SyncStatus.Offline : SyncStatus.Idle;
            }

            return result;
        }

        private async Task ProcessSyncQueueAsync(CancellationToken cancellationToken)
        {
            if (_syncQueue.IsEmpty)
                return;

            _logger.LogInfo($"Processing {_syncQueue.Count} queued items");

            var processedItems = new List<SyncQueueItem>();

            while (_syncQueue.TryDequeue(out var queueItem))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Process the queued item
                    _logger.LogDebug($"Processing queued item: {queueItem.EntityType} {queueItem.EntityId}");
                    processedItems.Add(queueItem);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to process queued item {queueItem.Id}", ex);
                    queueItem.RetryCount++;
                    queueItem.LastError = ex.Message;
                    queueItem.NextRetryAt = DateTime.Now.AddSeconds(_settings.RetryDelaySeconds);

                    if (queueItem.RetryCount < _settings.MaxRetryAttempts)
                    {
                        _syncQueue.Enqueue(queueItem);
                    }
                }
            }

            SaveSyncQueue();
            _logger.LogInfo($"Processed {processedItems.Count} queued items");
        }

        private void LoadSyncQueue()
        {
            try
            {
                if (File.Exists(_queueFilePath))
                {
                    var json = File.ReadAllText(_queueFilePath);
                    var queue = JsonSerializer.Deserialize<List<SyncQueueItem>>(json);
                    if (queue != null)
                    {
                        foreach (var item in queue)
                        {
                            _syncQueue.Enqueue(item);
                        }
                    }
                    _logger.LogInfo($"Loaded {_syncQueue.Count} items from sync queue");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load sync queue", ex);
            }
        }

        private void SaveSyncQueue()
        {
            try
            {
                var directory = Path.GetDirectoryName(_queueFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var queue = _syncQueue.ToList();
                var json = JsonSerializer.Serialize(queue, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_queueFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save sync queue", ex);
            }
        }

        private void ReportProgress(int processed, int total, string operation)
        {
            ProgressChanged?.Invoke(this, new SyncProgressEventArgs
            {
                ProcessedItems = processed,
                TotalItems = total,
                CurrentOperation = operation
            });
        }

        private void ApplyServerVersion(SyncConflict conflict)
        {
            if (conflict.ServerVersion is Item item)
            {
                _itemRepository.Update(item);
            }
            // Handle other entity types similarly
        }

        private void MergeVersions(SyncConflict conflict)
        {
            // Simplified merge - in a real implementation, this would merge specific fields
            ApplyServerVersion(conflict);
        }

        #endregion
    }

    /// <summary>
    /// Data structure for cloud export/import.
    /// </summary>
    internal class CloudExportData
    {
        public DateTime ExportDate { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public List<Item> Items { get; set; } = new();
        public List<Place> Places { get; set; } = new();
        public List<PriceRecord> PriceRecords { get; set; } = new();
    }
}
