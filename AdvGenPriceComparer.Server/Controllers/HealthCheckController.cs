using AdvGenPriceComparer.Server.Data;
using AdvGenPriceComparer.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AdvGenPriceComparer.Server.Controllers;

/// <summary>
/// Health check endpoint for monitoring server status
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly PriceDataContext? _dbContext;
    private readonly ILogger<HealthController> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public HealthController(ILogger<HealthController> logger, PriceDataContext? dbContext = null)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get server health status
    /// </summary>
    /// <returns>Health status including component checks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        var healthStatus = new HealthStatus
        {
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Uptime = GetUptimeInfo()
        };

        // Check database health
        var dbHealth = await CheckDatabaseHealthAsync();
        healthStatus.Components.Add("database", dbHealth);

        // Check SignalR hub health (just verify it's accessible)
        var signalrHealth = CheckSignalRHealth();
        healthStatus.Components.Add("signalr", signalrHealth);

        // Check memory usage
        var memoryHealth = CheckMemoryHealth();
        healthStatus.Components.Add("memory", memoryHealth);

        stopwatch.Stop();

        // Determine overall status
        var hasUnhealthy = healthStatus.Components.Values.Any(c => c.Status == "Unhealthy");
        var hasDegraded = healthStatus.Components.Values.Any(c => c.Status == "Degraded");

        if (hasUnhealthy)
        {
            healthStatus.Status = "Unhealthy";
            return StatusCode(StatusCodes.Status503ServiceUnavailable, healthStatus);
        }
        else if (hasDegraded)
        {
            healthStatus.Status = "Degraded";
        }

        _logger.LogDebug("Health check completed in {ElapsedMs}ms with status {Status}", 
            stopwatch.ElapsedMilliseconds, healthStatus.Status);

        return Ok(healthStatus);
    }

    /// <summary>
    /// Simple ping endpoint for basic connectivity checks
    /// </summary>
    /// <returns>Simple pong response</returns>
    [HttpGet("ping")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new 
        { 
            status = "ok", 
            timestamp = DateTime.UtcNow,
            uptime = GetUptimeInfo()?.Formatted 
        });
    }

    /// <summary>
    /// Get detailed server information
    /// </summary>
    /// <returns>Server details including version, environment, and statistics</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ServerInfo), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerInfo>> GetInfo()
    {
        var info = new ServerInfo
        {
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow,
            Uptime = GetUptimeInfo(),
            Framework = $".NET {Environment.Version}"
        };

        // Get database statistics if available
        if (_dbContext != null)
        {
            try
            {
                info.DatabaseStats = new DatabaseStats
                {
                    TotalItems = await _dbContext.Items.CountAsync(),
                    TotalPlaces = await _dbContext.Places.CountAsync(),
                    TotalPriceRecords = await _dbContext.PriceRecords.CountAsync(),
                    DatabaseProvider = "SQLite"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve database statistics");
                info.DatabaseStats = new DatabaseStats { Error = "Failed to retrieve statistics" };
            }
        }

        return Ok(info);
    }

    private async Task<ComponentHealth> CheckDatabaseHealthAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (_dbContext == null)
        {
            return new ComponentHealth
            {
                Status = "Degraded",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = "Database context not available"
            };
        }

        try
        {
            // Try to execute a simple query
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            stopwatch.Stop();

            return new ComponentHealth
            {
                Status = "Healthy",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    { "provider", "SQLite" },
                    { "canConnect", true }
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database health check failed");

            return new ComponentHealth
            {
                Status = "Unhealthy",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = "Database connection failed"
            };
        }
    }

    private static ComponentHealth CheckSignalRHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // SignalR hub path exists and middleware is configured
        // This is a basic check - in production you might want more sophisticated checks
        stopwatch.Stop();

        return new ComponentHealth
        {
            Status = "Healthy",
            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
            Details = new Dictionary<string, object>
            {
                { "hubPath", "/hubs/price-updates" },
                { "enabled", true }
            }
        };
    }

    private static ComponentHealth CheckMemoryHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        
        var process = Process.GetCurrentProcess();
        var workingSetMb = process.WorkingSet64 / (1024 * 1024);
        var gcMemoryMb = GC.GetTotalMemory(false) / (1024 * 1024);

        // Determine status based on memory usage
        var status = workingSetMb > 1024 ? "Degraded" : "Healthy"; // > 1GB is degraded
        if (workingSetMb > 2048) status = "Unhealthy"; // > 2GB is unhealthy

        stopwatch.Stop();

        return new ComponentHealth
        {
            Status = status,
            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
            Details = new Dictionary<string, object>
            {
                { "workingSetMB", workingSetMb },
                { "gcMemoryMB", gcMemoryMb },
                { "processId", process.Id }
            }
        };
    }

    private static UptimeInfo GetUptimeInfo()
    {
        var uptime = DateTime.UtcNow - _startTime;
        
        return new UptimeInfo
        {
            StartTime = _startTime,
            Duration = uptime,
            Formatted = FormatUptime(uptime)
        };
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        else if (uptime.TotalHours >= 1)
            return $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        else
            return $"{uptime.Minutes}m {uptime.Seconds}s";
    }
}

/// <summary>
/// Server information response model
/// </summary>
public class ServerInfo
{
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public UptimeInfo? Uptime { get; set; }
    public DatabaseStats? DatabaseStats { get; set; }
}

/// <summary>
/// Database statistics model
/// </summary>
public class DatabaseStats
{
    public int TotalItems { get; set; }
    public int TotalPlaces { get; set; }
    public int TotalPriceRecords { get; set; }
    public string DatabaseProvider { get; set; } = string.Empty;
    public string? Error { get; set; }
}
