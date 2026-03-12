using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for managing and applying application themes
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Apply the specified theme to the application
    /// </summary>
    void ApplyTheme(ApplicationTheme theme);

    /// <summary>
    /// Get the current theme
    /// </summary>
    ApplicationTheme CurrentTheme { get; }

    /// <summary>
    /// Event fired when theme changes
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
}

/// <summary>
/// Event args for theme changes
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public ApplicationTheme OldTheme { get; init; }
    public ApplicationTheme NewTheme { get; init; }
}

/// <summary>
/// Implementation of theme service for WPF application
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILoggerService _logger;
    private ApplicationTheme _currentTheme = ApplicationTheme.Light;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ApplicationTheme CurrentTheme => _currentTheme;

    public ThemeService(ILoggerService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Apply the specified theme to the application
    /// </summary>
    public void ApplyTheme(ApplicationTheme theme)
    {
        if (_currentTheme == theme && theme != ApplicationTheme.System)
        {
            _logger.LogDebug($"ThemeService: Theme '{theme}' is already applied");
            return;
        }

        var oldTheme = _currentTheme;

        try
        {
            _logger.LogInfo($"ThemeService: Applying theme '{theme}'");

            // Determine the actual theme to apply
            var themeToApply = theme;
            if (theme == ApplicationTheme.System)
            {
                themeToApply = GetSystemTheme();
                _logger.LogInfo($"ThemeService: System theme detected as '{themeToApply}'");
            }

            // Apply theme
            ApplyThemeToApplication(themeToApply);

            // Update current theme
            _currentTheme = theme;

            // Notify subscribers
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs
            {
                OldTheme = oldTheme,
                NewTheme = theme
            });

            _logger.LogInfo($"ThemeService: Theme changed from '{oldTheme}' to '{theme}'");
        }
        catch (Exception ex)
        {
            _logger.LogError($"ThemeService: Failed to apply theme '{theme}'", ex);
        }
    }

    /// <summary>
    /// Apply theme by updating resource dictionaries
    /// </summary>
    private void ApplyThemeToApplication(ApplicationTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        // Remove existing theme dictionaries and add new one
        var mergedDicts = app.Resources.MergedDictionaries;
        
        // Find and remove the WPF UI theme dictionary
        ResourceDictionary? themeDictToRemove = null;
        foreach (var dict in mergedDicts)
        {
            // Look for theme dictionary by checking its source or type
            if (dict.Source != null && dict.Source.OriginalString.Contains("Theme", StringComparison.OrdinalIgnoreCase))
            {
                themeDictToRemove = dict;
                break;
            }
        }

        if (themeDictToRemove != null)
        {
            mergedDicts.Remove(themeDictToRemove);
        }

        // Create new theme dictionary
        var themeUri = theme switch
        {
            ApplicationTheme.Dark => new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/Dark.xaml"),
            ApplicationTheme.Light => new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/Light.xaml"),
            _ => new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/Light.xaml")
        };

        try
        {
            var newThemeDict = new ResourceDictionary { Source = themeUri };
            mergedDicts.Insert(0, newThemeDict);
            _logger.LogDebug($"ThemeService: Applied theme dictionary - {themeUri}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"ThemeService: Could not load theme dictionary from {themeUri}: {ex.Message}");
            // Fallback: Try using WPF UI's built-in theme switcher if available
            TryWpfUiThemeSwitch(theme);
        }

        // Update accent colors based on theme
        UpdateAccentColors(theme);
    }

    /// <summary>
    /// Try to use WPF UI's built-in theme switching
    /// </summary>
    private void TryWpfUiThemeSwitch(ApplicationTheme theme)
    {
        try
        {
            // Use reflection to access WPF UI's theme manager
            var app = System.Windows.Application.Current;
            var wpfUiType = Type.GetType("Wpf.Ui.Appearance.Theme, Wpf.Ui");
            if (wpfUiType != null)
            {
                var applyMethod = wpfUiType.GetMethod("Apply", new[] { typeof(string) });
                if (applyMethod != null)
                {
                    var themeName = theme == ApplicationTheme.Dark ? "Dark" : "Light";
                    applyMethod.Invoke(null, new object[] { themeName });
                    _logger.LogDebug($"ThemeService: Applied theme via WPF UI Theme.Apply - {themeName}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"ThemeService: WPF UI theme switch fallback failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Update accent colors for the theme
    /// </summary>
    private void UpdateAccentColors(ApplicationTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        // Update system colors based on theme
        if (theme == ApplicationTheme.Dark)
        {
            // Dark theme specific adjustments
            app.Resources["SystemColorWindowColor"] = Colors.Black;
            app.Resources["SystemColorWindowTextColor"] = Colors.White;
        }
        else
        {
            // Light theme specific adjustments
            app.Resources["SystemColorWindowColor"] = Colors.White;
            app.Resources["SystemColorWindowTextColor"] = Colors.Black;
        }
    }

    /// <summary>
    /// Get the system theme preference
    /// </summary>
    private ApplicationTheme GetSystemTheme()
    {
        try
        {
            // Check Windows theme using registry
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value is int lightTheme)
                {
                    return lightTheme == 0 ? ApplicationTheme.Dark : ApplicationTheme.Light;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"ThemeService: Failed to detect system theme, defaulting to Light: {ex.Message}");
        }

        return ApplicationTheme.Light;
    }
}
