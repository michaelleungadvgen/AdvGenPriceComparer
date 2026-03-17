using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using AdvGenPriceComparer.Core.Interfaces;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Implementation of ILocalizationService for WPF application
/// Manages culture switching and resource loading for multi-language support
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ILoggerService _logger;
    private readonly ISettingsService _settingsService;
    private string _currentCulture = "en-US";

    // Resource manager for accessing .resx files
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Available cultures for the application
    /// </summary>
    public static readonly List<Core.Interfaces.CultureInfo> SupportedCultures = new()
    {
        new Core.Interfaces.CultureInfo("en-US", "English (US)", "English", "US"),
        new Core.Interfaces.CultureInfo("en-GB", "English (UK)", "English", "GB"),
        new Core.Interfaces.CultureInfo("en-AU", "English (Australia)", "English", "AU"),
        new Core.Interfaces.CultureInfo("zh-CN", "Chinese (Simplified)", "Chinese (Simplified)", "CN"),
        new Core.Interfaces.CultureInfo("zh-TW", "Chinese (Traditional)", "Chinese (Traditional)", "TW"),
        new Core.Interfaces.CultureInfo("ja-JP", "Japanese", "Japanese", "JP"),
        new Core.Interfaces.CultureInfo("ko-KR", "Korean", "Korean", "KR"),
        new Core.Interfaces.CultureInfo("es-ES", "Spanish", "Spanish", "ES"),
        new Core.Interfaces.CultureInfo("fr-FR", "French", "French", "FR"),
        new Core.Interfaces.CultureInfo("de-DE", "German", "German", "DE")
    };

    /// <inheritdoc />
    public string CurrentCulture => _currentCulture;

    /// <inheritdoc />
    public IReadOnlyList<Core.Interfaces.CultureInfo> AvailableCultures => SupportedCultures;

    /// <inheritdoc />
    public event EventHandler<CultureChangedEventArgs>? CultureChanged;

    public LocalizationService(ILoggerService logger, ISettingsService settingsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Initialize with default resource manager (will be replaced by specific resource files)
        _resourceManager = new ResourceManager("AdvGenPriceComparer.WPF.Resources.Strings", typeof(LocalizationService).Assembly);
        
        // Load culture from settings
        var settingsCulture = _settingsService.Culture;
        if (!string.IsNullOrEmpty(settingsCulture) && IsCultureSupported(settingsCulture))
        {
            _currentCulture = settingsCulture;
            ApplyCulture(_currentCulture, false);
            _logger.LogInfo($"LocalizationService initialized with culture: {_currentCulture}");
        }
        else
        {
            _logger.LogInfo($"LocalizationService initialized with default culture: {_currentCulture}");
        }
    }

    /// <inheritdoc />
    public bool ChangeCulture(string cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            _logger.LogWarning("Cannot change culture: culture code is null or empty");
            return false;
        }

        if (!IsCultureSupported(cultureCode))
        {
            _logger.LogWarning($"Culture '{cultureCode}' is not supported");
            return false;
        }

        if (_currentCulture.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInfo($"Culture is already set to '{cultureCode}'");
            return true;
        }

        try
        {
            var oldCulture = _currentCulture;
            _currentCulture = cultureCode;
            
            // Apply the culture change
            ApplyCulture(cultureCode, true);
            
            // Save to settings
            _settingsService.Culture = cultureCode;
            _ = _settingsService.SaveSettingsAsync();
            
            // Raise event
            CultureChanged?.Invoke(this, new CultureChangedEventArgs(oldCulture, cultureCode));
            
            _logger.LogInfo($"Culture changed from '{oldCulture}' to '{cultureCode}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to change culture to '{cultureCode}'", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public string GetString(string key)
    {
        return GetString(key, _currentCulture);
    }

    /// <inheritdoc />
    public string GetString(string key, string cultureCode)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        try
        {
            var culture = new System.Globalization.CultureInfo(cultureCode);
            var value = _resourceManager.GetString(key, culture);
            
            if (string.IsNullOrEmpty(value))
            {
                // Fallback to current culture
                value = _resourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);
                
                // If still not found, return the key
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning($"Localization key '{key}' not found for culture '{cultureCode}'");
                    return $"[{key}]";
                }
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving localization string for key '{key}'", ex);
            return $"[{key}]";
        }
    }

    /// <inheritdoc />
    public string GetFormattedString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch (FormatException ex)
        {
            _logger.LogError($"Error formatting localization string for key '{key}'", ex);
            return format;
        }
    }

    /// <summary>
    /// Applies the culture to the current thread and UI
    /// </summary>
    private void ApplyCulture(string cultureCode, bool updateThreadCulture)
    {
        try
        {
            var culture = new System.Globalization.CultureInfo(cultureCode);
            
            if (updateThreadCulture)
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            
            // Set the default culture for the resource manager
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            
            _logger.LogInfo($"Applied culture: {cultureCode} ({culture.DisplayName})");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to apply culture '{cultureCode}'", ex);
            throw;
        }
    }

    /// <summary>
    /// Checks if a culture is supported
    /// </summary>
    private bool IsCultureSupported(string cultureCode)
    {
        return SupportedCultures.Any(c => 
            c.Code.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));
    }
}
