using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Interface for database providers (LiteDB, AdvGenNoSQLServer, etc.)
/// </summary>
public interface IDatabaseProvider : IDisposable
{
    /// <summary>
    /// Name of the provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether the provider is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect to the database using the provided settings
    /// </summary>
    Task<bool> ConnectAsync(DatabaseConnectionSettings settings);

    /// <summary>
    /// Disconnect from the database
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Test the connection to the database without establishing a full connection
    /// </summary>
    /// <returns>True if connection is successful, false otherwise</returns>
    Task<bool> TestConnectionAsync();

    // Repository accessors
    IItemRepository Items { get; }
    IPlaceRepository Places { get; }
    IPriceRecordRepository PriceRecords { get; }
    IAlertRepository Alerts { get; }
    IExportHistoryRepository ExportHistories { get; }
}
