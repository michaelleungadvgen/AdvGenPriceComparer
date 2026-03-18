using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces
{
    /// <summary>
    /// Service for synchronizing data with cloud storage.
    /// Provides conflict resolution, offline support, and automatic syncing.
    /// </summary>
    public interface ICloudSyncService
    {
        /// <summary>
        /// Gets the current synchronization status.
        /// </summary>
        SyncStatus Status { get; }

        /// <summary>
        /// Gets the last synchronization result.
        /// </summary>
        SyncResult? LastResult { get; }

        /// <summary>
        /// Gets the cloud sync configuration settings.
        /// </summary>
        CloudSyncSettings Settings { get; }

        /// <summary>
        /// Event raised when sync status changes.
        /// </summary>
        event EventHandler<SyncStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Event raised when a sync conflict is detected.
        /// </summary>
        event EventHandler<SyncConflictEventArgs>? ConflictDetected;

        /// <summary>
        /// Event raised when sync progress updates.
        /// </summary>
        event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Initializes the cloud sync service with the provided settings.
        /// </summary>
        Task InitializeAsync(CloudSyncSettings settings);

        /// <summary>
        /// Performs a manual synchronization.
        /// </summary>
        Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronizes only items that have changed since the last sync.
        /// </summary>
        Task<SyncResult> SyncIncrementalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a full synchronization of all data.
        /// </summary>
        Task<SyncResult> SyncFullAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports local data to cloud storage.
        /// </summary>
        Task<SyncResult> ExportToCloudAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Imports data from cloud storage to local database.
        /// </summary>
        Task<SyncResult> ImportFromCloudAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a sync conflict using the specified strategy.
        /// </summary>
        Task ResolveConflictAsync(string itemId, ConflictResolutionStrategy strategy);

        /// <summary>
        /// Gets a list of pending conflicts that need resolution.
        /// </summary>
        Task<List<SyncConflict>> GetPendingConflictsAsync();

        /// <summary>
        /// Enables automatic synchronization.
        /// </summary>
        void EnableAutoSync();

        /// <summary>
        /// Disables automatic synchronization.
        /// </summary>
        void DisableAutoSync();

        /// <summary>
        /// Tests the cloud connection.
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Gets the sync queue for offline items.
        /// </summary>
        Task<List<SyncQueueItem>> GetSyncQueueAsync();

        /// <summary>
        /// Clears the sync queue.
        /// </summary>
        Task ClearSyncQueueAsync();

        /// <summary>
        /// Disposes the service and releases resources.
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Synchronization status enumeration.
    /// </summary>
    public enum SyncStatus
    {
        Idle,
        Initializing,
        Syncing,
        Uploading,
        Downloading,
        ResolvingConflicts,
        Error,
        Offline
    }

    /// <summary>
    /// Event args for sync status changes.
    /// </summary>
    public class SyncStatusChangedEventArgs : EventArgs
    {
        public SyncStatus OldStatus { get; set; }
        public SyncStatus NewStatus { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Event args for sync conflicts.
    /// </summary>
    public class SyncConflictEventArgs : EventArgs
    {
        public SyncConflict Conflict { get; set; } = null!;
    }

    /// <summary>
    /// Event args for sync progress updates.
    /// </summary>
    public class SyncProgressEventArgs : EventArgs
    {
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int Percentage => TotalItems > 0 ? (ProcessedItems * 100) / TotalItems : 0;
        public string? CurrentOperation { get; set; }
    }
}
