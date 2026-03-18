namespace AdvGenPriceComparer.Core.Models
{
    /// <summary>
    /// Cloud provider types.
    /// </summary>
    public enum CloudProviderType
    {
        AdvGenCloud,
        CustomServer,
        AwsS3,
        AzureBlob,
        GoogleCloud
    }

    /// <summary>
    /// Sync schedule modes.
    /// </summary>
    public enum SyncScheduleMode
    {
        Manual,
        OnChange,
        Periodic
    }

    /// <summary>
    /// Conflict resolution strategies.
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        ServerWins,
        ClientWins,
        LastWriteWins,
        Manual,
        Merge
    }

    /// <summary>
    /// Configuration settings for cloud synchronization.
    /// </summary>
    public class CloudSyncSettings
    {
        /// <summary>
        /// Gets or sets whether cloud sync is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the cloud provider type.
        /// </summary>
        public CloudProviderType ProviderType { get; set; } = CloudProviderType.AdvGenCloud;

        /// <summary>
        /// Gets or sets the server URL for cloud synchronization.
        /// </summary>
        public string ServerUrl { get; set; } = "https://cloud.advgen.com";

        /// <summary>
        /// Gets or sets the API key for authentication.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID for the cloud account.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sync schedule mode.
        /// </summary>
        public SyncScheduleMode ScheduleMode { get; set; } = SyncScheduleMode.Manual;

        /// <summary>
        /// Gets or sets the automatic sync interval in minutes.
        /// </summary>
        public int AutoSyncIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets the default conflict resolution strategy.
        /// </summary>
        public ConflictResolutionStrategy DefaultConflictResolution { get; set; } = ConflictResolutionStrategy.LastWriteWins;

        /// <summary>
        /// Gets or sets whether to sync on startup.
        /// </summary>
        public bool SyncOnStartup { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to sync on shutdown.
        /// </summary>
        public bool SyncOnShutdown { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use Wi-Fi only for syncing.
        /// </summary>
        public bool WifiOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum sync retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the sync retry delay in seconds.
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to compress data during sync.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to encrypt data during sync.
        /// </summary>
        public bool EnableEncryption { get; set; } = true;

        /// <summary>
        /// Gets or sets the last successful sync timestamp.
        /// </summary>
        public DateTime? LastSyncTime { get; set; }

        /// <summary>
        /// Gets or sets the last sync checksum for incremental sync.
        /// </summary>
        public string LastSyncChecksum { get; set; } = string.Empty;

        /// <summary>
        /// Creates a deep copy of these settings.
        /// </summary>
        public CloudSyncSettings Clone()
        {
            return new CloudSyncSettings
            {
                IsEnabled = this.IsEnabled,
                ProviderType = this.ProviderType,
                ServerUrl = this.ServerUrl,
                ApiKey = this.ApiKey,
                UserId = this.UserId,
                DeviceId = this.DeviceId,
                ScheduleMode = this.ScheduleMode,
                AutoSyncIntervalMinutes = this.AutoSyncIntervalMinutes,
                DefaultConflictResolution = this.DefaultConflictResolution,
                SyncOnStartup = this.SyncOnStartup,
                SyncOnShutdown = this.SyncOnShutdown,
                WifiOnly = this.WifiOnly,
                MaxRetryAttempts = this.MaxRetryAttempts,
                RetryDelaySeconds = this.RetryDelaySeconds,
                EnableCompression = this.EnableCompression,
                EnableEncryption = this.EnableEncryption,
                LastSyncTime = this.LastSyncTime,
                LastSyncChecksum = this.LastSyncChecksum
            };
        }
    }

    /// <summary>
    /// Represents a synchronization conflict.
    /// </summary>
    public class SyncConflict
    {
        /// <summary>
        /// Gets or sets the unique identifier for the conflict.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the entity type (Item, Place, PriceRecord, etc.).
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity ID.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity name for display.
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the local version of the entity.
        /// </summary>
        public object? LocalVersion { get; set; }

        /// <summary>
        /// Gets or sets the server version of the entity.
        /// </summary>
        public object? ServerVersion { get; set; }

        /// <summary>
        /// Gets or sets when the conflict was detected.
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the local modification time.
        /// </summary>
        public DateTime LocalModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the server modification time.
        /// </summary>
        public DateTime ServerModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the conflict resolution status.
        /// </summary>
        public ConflictStatus Status { get; set; } = ConflictStatus.Pending;

        /// <summary>
        /// Gets or sets the resolution strategy used.
        /// </summary>
        public ConflictResolutionStrategy? ResolutionStrategy { get; set; }

        /// <summary>
        /// Gets or sets when the conflict was resolved.
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// Conflict resolution status.
    /// </summary>
    public enum ConflictStatus
    {
        Pending,
        Resolved,
        AutoResolved
    }

    /// <summary>
    /// Represents a synchronization result.
    /// </summary>
    public class SyncResult
    {
        /// <summary>
        /// Gets or sets whether the sync was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the sync operation type.
        /// </summary>
        public SyncOperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets when the sync started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Gets or sets when the sync completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets the sync duration.
        /// </summary>
        public TimeSpan? Duration => CompletedAt - StartedAt;

        /// <summary>
        /// Gets or sets the number of items uploaded.
        /// </summary>
        public int ItemsUploaded { get; set; }

        /// <summary>
        /// Gets or sets the number of items downloaded.
        /// </summary>
        public int ItemsDownloaded { get; set; }

        /// <summary>
        /// Gets or sets the number of items with conflicts.
        /// </summary>
        public int ConflictsDetected { get; set; }

        /// <summary>
        /// Gets or sets the number of conflicts resolved.
        /// </summary>
        public int ConflictsResolved { get; set; }

        /// <summary>
        /// Gets or sets the number of items skipped.
        /// </summary>
        public int ItemsSkipped { get; set; }

        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Gets or sets the list of errors that occurred.
        /// </summary>
        public List<SyncError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the sync message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sync checksum.
        /// </summary>
        public string Checksum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sync operation types.
    /// </summary>
    public enum SyncOperationType
    {
        FullSync,
        IncrementalSync,
        Export,
        Import
    }

    /// <summary>
    /// Represents a sync error.
    /// </summary>
    public class SyncError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the entity ID if related to a specific entity.
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// Gets or sets when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the exception details if available.
        /// </summary>
        public string? ExceptionDetails { get; set; }
    }

    /// <summary>
    /// Represents an item in the sync queue for offline support.
    /// </summary>
    public class SyncQueueItem
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the entity type.
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity ID.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operation type (Create, Update, Delete).
        /// </summary>
        public SyncQueueOperation Operation { get; set; }

        /// <summary>
        /// Gets or sets the entity data.
        /// </summary>
        public string EntityData { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the item was added to the queue.
        /// </summary>
        public DateTime QueuedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the number of retry attempts.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the last error message.
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// Gets or sets when to retry next.
        /// </summary>
        public DateTime? NextRetryAt { get; set; }
    }

    /// <summary>
    /// Sync queue operations.
    /// </summary>
    public enum SyncQueueOperation
    {
        Create,
        Update,
        Delete
    }
}
