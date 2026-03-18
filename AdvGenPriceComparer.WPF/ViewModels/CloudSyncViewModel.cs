using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for managing cloud synchronization.
    /// </summary>
    public class CloudSyncViewModel : ViewModelBase
    {
        private readonly ICloudSyncService _cloudSyncService;
        private readonly IDialogService _dialogService;
        private readonly ILoggerService _logger;

        private bool _isBusy;
        private string _statusMessage = string.Empty;
        private int _syncProgress;
        private SyncResult? _lastSyncResult;
        private CloudSyncSettings _settings;

        /// <summary>
        /// Gets or sets whether a sync operation is in progress.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                ((RelayCommand)SyncNowCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SyncIncrementalCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ImportCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the sync progress percentage.
        /// </summary>
        public int SyncProgress
        {
            get => _syncProgress;
            set { _syncProgress = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the current sync status.
        /// </summary>
        public SyncStatus CurrentStatus => _cloudSyncService.Status;

        /// <summary>
        /// Gets or sets the last sync result.
        /// </summary>
        public SyncResult? LastSyncResult
        {
            get => _lastSyncResult;
            set { _lastSyncResult = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the cloud sync settings.
        /// </summary>
        public CloudSyncSettings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets the collection of pending conflicts.
        /// </summary>
        public ObservableCollection<SyncConflict> PendingConflicts { get; } = new();

        /// <summary>
        /// Gets the collection of sync queue items.
        /// </summary>
        public ObservableCollection<SyncQueueItem> SyncQueueItems { get; } = new();

        /// <summary>
        /// Gets the last sync time display string.
        /// </summary>
        public string LastSyncDisplay
        {
            get
            {
                if (_cloudSyncService.Settings.LastSyncTime.HasValue)
                {
                    var time = _cloudSyncService.Settings.LastSyncTime.Value.ToLocalTime();
                    var diff = DateTime.Now - time;

                    if (diff.TotalMinutes < 1)
                        return "Just now";
                    if (diff.TotalHours < 1)
                        return $"{diff.TotalMinutes:0} minutes ago";
                    if (diff.TotalDays < 1)
                        return $"{diff.TotalHours:0} hours ago";
                    if (diff.TotalDays < 7)
                        return $"{diff.TotalDays:0} days ago";

                    return time.ToString("g");
                }
                return "Never";
            }
        }

        /// <summary>
        /// Gets whether cloud sync is enabled.
        /// </summary>
        public bool IsCloudSyncEnabled
        {
            get => _settings.IsEnabled;
            set
            {
                _settings.IsEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSync));
            }
        }

        /// <summary>
        /// Gets whether syncing is possible.
        /// </summary>
        public bool CanSync => _settings.IsEnabled && !IsBusy;

        /// <summary>
        /// Gets whether there are pending conflicts.
        /// </summary>
        public bool HasPendingConflicts => PendingConflicts.Count > 0;

        /// <summary>
        /// Gets the pending conflicts count.
        /// </summary>
        public int PendingConflictsCount => PendingConflicts.Count;

        /// <summary>
        /// Gets the sync queue count.
        /// </summary>
        public int SyncQueueCount => SyncQueueItems.Count;

        #region Commands

        public ICommand SyncNowCommand { get; }
        public ICommand SyncIncrementalCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResolveConflictCommand { get; }
        public ICommand ResolveAllConflictsCommand { get; }
        public ICommand ClearQueueCommand { get; }
        public ICommand EnableAutoSyncCommand { get; }
        public ICommand DisableAutoSyncCommand { get; }

        #endregion

        /// <summary>
        /// Creates a new instance of the CloudSyncViewModel.
        /// </summary>
        public CloudSyncViewModel(
            ICloudSyncService cloudSyncService,
            IDialogService dialogService,
            ILoggerService logger)
        {
            _cloudSyncService = cloudSyncService ?? throw new ArgumentNullException(nameof(cloudSyncService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _settings = _cloudSyncService.Settings.Clone();
            _lastSyncResult = _cloudSyncService.LastResult;

            // Initialize commands
            SyncNowCommand = new RelayCommand(async () => await SyncNowAsync(), () => CanSync);
            SyncIncrementalCommand = new RelayCommand(async () => await SyncIncrementalAsync(), () => CanSync);
            ExportCommand = new RelayCommand(async () => await ExportAsync(), () => CanSync);
            ImportCommand = new RelayCommand(async () => await ImportAsync(), () => CanSync);
            TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync(), () => !IsBusy);
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync(), () => !IsBusy);
            ResolveConflictCommand = new RelayCommand<SyncConflict>(async (conflict) => await ResolveConflictAsync(conflict));
            ResolveAllConflictsCommand = new RelayCommand(async () => await ResolveAllConflictsAsync(), () => HasPendingConflicts);
            ClearQueueCommand = new RelayCommand(async () => await ClearQueueAsync(), () => SyncQueueCount > 0 && !IsBusy);
            EnableAutoSyncCommand = new RelayCommand(() => EnableAutoSync(), () => !IsBusy);
            DisableAutoSyncCommand = new RelayCommand(() => DisableAutoSync(), () => !IsBusy);

            // Subscribe to events
            _cloudSyncService.StatusChanged += OnSyncStatusChanged;
            _cloudSyncService.ProgressChanged += OnSyncProgressChanged;
            _cloudSyncService.ConflictDetected += OnConflictDetected;

            // Load initial data
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                await LoadPendingConflictsAsync();
                await LoadSyncQueueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load cloud sync data", ex);
            }
        }

        private async Task SyncNowAsync()
        {
            IsBusy = true;
            StatusMessage = "Starting full synchronization...";
            SyncProgress = 0;

            try
            {
                var result = await _cloudSyncService.SyncAsync();
                LastSyncResult = result;

                if (result.Success)
                {
                    StatusMessage = result.Message;
                    _dialogService.ShowInfo($"Sync completed successfully!\n\n{result.Message}", "Sync Complete");
                }
                else
                {
                    StatusMessage = $"Sync failed: {result.Message}";
                    _dialogService.ShowError($"Sync failed:\n{result.Message}", "Sync Error");
                }

                OnPropertyChanged(nameof(LastSyncDisplay));
                await LoadPendingConflictsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Sync failed", ex);
                StatusMessage = $"Sync error: {ex.Message}";
                _dialogService.ShowError($"An error occurred during sync:\n{ex.Message}", "Sync Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SyncIncrementalAsync()
        {
            IsBusy = true;
            StatusMessage = "Starting incremental synchronization...";
            SyncProgress = 0;

            try
            {
                var result = await _cloudSyncService.SyncIncrementalAsync();
                LastSyncResult = result;

                if (result.Success)
                {
                    StatusMessage = result.Message;
                }
                else
                {
                    StatusMessage = $"Incremental sync failed: {result.Message}";
                }

                OnPropertyChanged(nameof(LastSyncDisplay));
                await LoadPendingConflictsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Incremental sync failed", ex);
                StatusMessage = $"Sync error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportAsync()
        {
            IsBusy = true;
            StatusMessage = "Exporting data to cloud...";

            try
            {
                var result = await _cloudSyncService.ExportToCloudAsync();
                LastSyncResult = result;

                if (result.Success)
                {
                    StatusMessage = result.Message;
                    _dialogService.ShowInfo($"Export completed!\n\n{result.Message}", "Export Complete");
                }
                else
                {
                    StatusMessage = $"Export failed: {result.Message}";
                    _dialogService.ShowError($"Export failed:\n{result.Message}", "Export Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Export failed", ex);
                StatusMessage = $"Export error: {ex.Message}";
                _dialogService.ShowError($"An error occurred during export:\n{ex.Message}", "Export Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ImportAsync()
        {
            var result = _dialogService.ShowQuestion(
                "Importing from cloud will merge remote data with your local data. " +
                "Conflicts may occur if the same items have been modified.\n\n" +
                "Do you want to continue?",
                "Confirm Import");

            if (result != true)
                return;

            IsBusy = true;
            StatusMessage = "Importing data from cloud...";

            try
            {
                var importResult = await _cloudSyncService.ImportFromCloudAsync();
                LastSyncResult = importResult;

                if (importResult.Success)
                {
                    StatusMessage = importResult.Message;
                    _dialogService.ShowInfo($"Import completed!\n\n{importResult.Message}", "Import Complete");
                }
                else
                {
                    StatusMessage = $"Import failed: {importResult.Message}";
                    _dialogService.ShowError($"Import failed:\n{importResult.Message}", "Import Error");
                }

                await LoadPendingConflictsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Import failed", ex);
                StatusMessage = $"Import error: {ex.Message}";
                _dialogService.ShowError($"An error occurred during import:\n{ex.Message}", "Import Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task TestConnectionAsync()
        {
            IsBusy = true;
            StatusMessage = "Testing cloud connection...";

            try
            {
                var isConnected = await _cloudSyncService.TestConnectionAsync();

                if (isConnected)
                {
                    StatusMessage = "Cloud connection successful!";
                    _dialogService.ShowInfo("Successfully connected to the cloud server.", "Connection Test");
                }
                else
                {
                    StatusMessage = "Cloud connection failed";
                    _dialogService.ShowError("Could not connect to the cloud server.\n\n" +
                        "Please check your internet connection and server settings.", "Connection Test");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection test error: {ex.Message}";
                _dialogService.ShowError($"Connection test failed:\n{ex.Message}", "Connection Test");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                await _cloudSyncService.InitializeAsync(Settings);
                StatusMessage = "Settings saved successfully";
                _dialogService.ShowInfo("Cloud sync settings have been saved.", "Settings Saved");
                OnPropertyChanged(nameof(IsCloudSyncEnabled));
                OnPropertyChanged(nameof(CanSync));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save settings", ex);
                _dialogService.ShowError($"Failed to save settings:\n{ex.Message}", "Settings Error");
            }
        }

        private async Task ResolveConflictAsync(SyncConflict conflict)
        {
            if (conflict == null)
                return;

            // Show conflict resolution dialog
            var strategies = new[]
            {
                ConflictResolutionStrategy.ServerWins,
                ConflictResolutionStrategy.ClientWins,
                ConflictResolutionStrategy.LastWriteWins,
                ConflictResolutionStrategy.Merge
            };

            // For simplicity, use server wins as default
            var strategy = ConflictResolutionStrategy.ServerWins;

            try
            {
                await _cloudSyncService.ResolveConflictAsync(conflict.EntityId, strategy);
                PendingConflicts.Remove(conflict);

                OnPropertyChanged(nameof(HasPendingConflicts));
                OnPropertyChanged(nameof(PendingConflictsCount));

                StatusMessage = $"Resolved conflict for {conflict.EntityName}";
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to resolve conflict", ex);
                _dialogService.ShowError($"Failed to resolve conflict:\n{ex.Message}", "Conflict Resolution Error");
            }
        }

        private async Task ResolveAllConflictsAsync()
        {
            var result = _dialogService.ShowQuestion(
                $"This will resolve all {PendingConflicts.Count} pending conflicts using the default strategy " +
                $"({Settings.DefaultConflictResolution}).\n\nDo you want to continue?",
                "Resolve All Conflicts");

            if (result != true)
                return;

            IsBusy = true;
            StatusMessage = "Resolving all conflicts...";

            try
            {
                var conflicts = PendingConflicts.ToList();
                int resolved = 0;

                foreach (var conflict in conflicts)
                {
                    await _cloudSyncService.ResolveConflictAsync(conflict.EntityId, Settings.DefaultConflictResolution);
                    PendingConflicts.Remove(conflict);
                    resolved++;
                }

                OnPropertyChanged(nameof(HasPendingConflicts));
                OnPropertyChanged(nameof(PendingConflictsCount));

                StatusMessage = $"Resolved {resolved} conflicts";
                _dialogService.ShowInfo($"Successfully resolved {resolved} conflicts.", "Conflicts Resolved");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to resolve conflicts", ex);
                _dialogService.ShowError($"Failed to resolve conflicts:\n{ex.Message}", "Conflict Resolution Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClearQueueAsync()
        {
            var result = _dialogService.ShowQuestion(
                $"This will clear {SyncQueueItems.Count} items from the sync queue.\n\n" +
                "These items will not be synchronized.\n\nDo you want to continue?",
                "Clear Sync Queue");

            if (result != true)
                return;

            try
            {
                await _cloudSyncService.ClearSyncQueueAsync();
                SyncQueueItems.Clear();
                OnPropertyChanged(nameof(SyncQueueCount));
                StatusMessage = "Sync queue cleared";
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to clear queue", ex);
                _dialogService.ShowError($"Failed to clear queue:\n{ex.Message}", "Error");
            }
        }

        private void EnableAutoSync()
        {
            _cloudSyncService.EnableAutoSync();
            Settings.ScheduleMode = SyncScheduleMode.Periodic;
            OnPropertyChanged(nameof(Settings));
            StatusMessage = "Auto sync enabled";
        }

        private void DisableAutoSync()
        {
            _cloudSyncService.DisableAutoSync();
            Settings.ScheduleMode = SyncScheduleMode.Manual;
            OnPropertyChanged(nameof(Settings));
            StatusMessage = "Auto sync disabled";
        }

        private async Task LoadPendingConflictsAsync()
        {
            try
            {
                var conflicts = await _cloudSyncService.GetPendingConflictsAsync();
                PendingConflicts.Clear();
                foreach (var conflict in conflicts)
                {
                    PendingConflicts.Add(conflict);
                }
                OnPropertyChanged(nameof(HasPendingConflicts));
                OnPropertyChanged(nameof(PendingConflictsCount));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load pending conflicts", ex);
            }
        }

        private async Task LoadSyncQueueAsync()
        {
            try
            {
                var items = await _cloudSyncService.GetSyncQueueAsync();
                SyncQueueItems.Clear();
                foreach (var item in items)
                {
                    SyncQueueItems.Add(item);
                }
                OnPropertyChanged(nameof(SyncQueueCount));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load sync queue", ex);
            }
        }

        private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentStatus));
            StatusMessage = e.Message ?? $"Status: {e.NewStatus}";
        }

        private void OnSyncProgressChanged(object? sender, SyncProgressEventArgs e)
        {
            SyncProgress = e.Percentage;
            if (!string.IsNullOrEmpty(e.CurrentOperation))
            {
                StatusMessage = $"{e.CurrentOperation} ({e.ProcessedItems}/{e.TotalItems})";
            }
        }

        private void OnConflictDetected(object? sender, SyncConflictEventArgs e)
        {
            // Add conflict to the collection on UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PendingConflicts.Add(e.Conflict);
                OnPropertyChanged(nameof(HasPendingConflicts));
                OnPropertyChanged(nameof(PendingConflictsCount));
            });
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Cleanup()
        {
            _cloudSyncService.StatusChanged -= OnSyncStatusChanged;
            _cloudSyncService.ProgressChanged -= OnSyncProgressChanged;
            _cloudSyncService.ConflictDetected -= OnConflictDetected;
        }
    }
}
