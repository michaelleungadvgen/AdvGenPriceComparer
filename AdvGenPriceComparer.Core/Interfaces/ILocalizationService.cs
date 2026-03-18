namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Service for managing application localization and language switching
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture code (e.g., "en-US", "zh-CN")
    /// </summary>
    string CurrentCulture { get; }

    /// <summary>
    /// Gets the list of available cultures supported by the application
    /// </summary>
    IReadOnlyList<CultureInfo> AvailableCultures { get; }

    /// <summary>
    /// Event raised when the current culture changes
    /// </summary>
    event EventHandler<CultureChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Changes the current culture and reloads resources
    /// </summary>
    /// <param name="cultureCode">The culture code to switch to (e.g., "en-US", "zh-CN")</param>
    /// <returns>True if the culture was changed successfully, false otherwise</returns>
    bool ChangeCulture(string cultureCode);

    /// <summary>
    /// Gets a localized string for the specified key
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <returns>The localized string, or the key if not found</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string for the specified key and culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="cultureCode">The culture code</param>
    /// <returns>The localized string, or the key if not found</returns>
    string GetString(string key, string cultureCode);

    /// <summary>
    /// Gets a localized string with formatting
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>The formatted localized string</returns>
    string GetFormattedString(string key, params object[] args);
}

/// <summary>
/// Event arguments for culture change events
/// </summary>
public class CultureChangedEventArgs : EventArgs
{
    public string OldCulture { get; }
    public string NewCulture { get; }

    public CultureChangedEventArgs(string oldCulture, string newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }
}

/// <summary>
/// Culture information for available languages
/// </summary>
public class CultureInfo
{
    public string Code { get; }
    public string Name { get; }
    public string NativeName { get; }
    public string FlagEmoji { get; }

    public CultureInfo(string code, string name, string nativeName, string flagEmoji)
    {
        Code = code;
        Name = name;
        NativeName = nativeName;
        FlagEmoji = flagEmoji;
    }

    public override string ToString() => $"{FlagEmoji} {NativeName} ({Code})";
}
