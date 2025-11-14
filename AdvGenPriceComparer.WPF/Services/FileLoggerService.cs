using System;
using System.IO;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// File-based logging service that writes to AppData
/// </summary>
public class FileLoggerService : ILoggerService
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public FileLoggerService()
    {
        // Create logs directory in AppData
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "Logs");

        Directory.CreateDirectory(appDataPath);

        // Create log file with date
        var logFileName = $"app_{DateTime.Now:yyyyMMdd}.log";
        _logFilePath = Path.Combine(appDataPath, logFileName);

        // Log startup
        LogInfo("=== Application Started ===");
    }

    public void LogDebug(string message)
    {
        WriteLog("DEBUG", message);
    }

    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("WARN", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}"
            : message;

        WriteLog("ERROR", fullMessage);
    }

    public void LogCritical(string message, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}"
            : message;

        WriteLog("CRITICAL", fullMessage);
    }

    public string GetLogFilePath()
    {
        return _logFilePath;
    }

    private void WriteLog(string level, string message)
    {
        try
        {
            lock (_lockObject)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}";

                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }
        catch
        {
            // Swallow logging errors to prevent cascading failures
        }
    }
}
