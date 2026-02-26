namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Settings for connecting to a database provider
/// </summary>
public class DatabaseConnectionSettings
{
    public DatabaseProviderType ProviderType { get; set; }
    
    // LiteDB specific
    public string LiteDbPath { get; set; } = "GroceryPrices.db";
    
    // AdvGenNoSQLServer specific
    public string ServerHost { get; set; } = "localhost";
    public int ServerPort { get; set; } = 5000;
    public string ApiKey { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "GroceryPrices";
    public bool UseSsl { get; set; } = true;
    
    // Connection pool settings
    public int ConnectionTimeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}

/// <summary>
/// Types of database providers
/// </summary>
public enum DatabaseProviderType
{
    LiteDB,
    AdvGenNoSQLServer
}
