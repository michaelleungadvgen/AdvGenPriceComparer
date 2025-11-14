namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for logging application events and errors
/// </summary>
public interface ILoggerService
{
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    void LogCritical(string message, Exception? exception = null);
    string GetLogFilePath();
}
