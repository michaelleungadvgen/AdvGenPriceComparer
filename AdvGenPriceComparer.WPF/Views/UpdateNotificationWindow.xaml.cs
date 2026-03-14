using System;
using System.Diagnostics;
using System.Windows;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for UpdateNotificationWindow.xaml
/// </summary>
public partial class UpdateNotificationWindow : Window
{
    private readonly UpdateCheckResult _updateResult;
    private readonly IUpdateService _updateService;

    /// <summary>
    /// Creates a new instance of UpdateNotificationWindow
    /// </summary>
    public UpdateNotificationWindow(UpdateCheckResult updateResult, IUpdateService updateService)
    {
        InitializeComponent();
        _updateResult = updateResult;
        _updateService = updateService;

        LoadUpdateInfo();
    }

    /// <summary>
    /// Loads the update information into the UI
    /// </summary>
    private void LoadUpdateInfo()
    {
        try
        {
            // Set version information
            VersionText.Text = $"Version {_updateResult.LatestVersion} is now available";
            CurrentVersionText.Text = _updateResult.CurrentVersion?.ToString() ?? "Unknown";
            LatestVersionText.Text = _updateResult.LatestVersion?.ToString() ?? "Unknown";

            // Set release date
            if (_updateResult.ReleaseDate.HasValue)
            {
                ReleaseDateText.Text = _updateResult.ReleaseDate.Value.ToString("MMMM dd, yyyy");
            }
            else
            {
                ReleaseDateText.Text = "Not specified";
            }

            // Set release notes
            if (!string.IsNullOrWhiteSpace(_updateResult.ReleaseNotes))
            {
                ReleaseNotesText.Text = _updateResult.ReleaseNotes;
            }
            else
            {
                ReleaseNotesText.Text = "No release notes available for this update.";
            }

            // Show mandatory banner if applicable
            if (_updateResult.IsMandatory)
            {
                MandatoryBanner.Visibility = Visibility.Visible;
                LaterButton.Content = "Skip";
            }

            // Update button text based on file size
            if (_updateResult.FileSize > 0)
            {
                var sizeText = FormatFileSize(_updateResult.FileSize);
                DownloadButton.Content = $"Download Update ({sizeText})";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading update info: {ex.Message}");
            ReleaseNotesText.Text = "Error loading update information.";
        }
    }

    /// <summary>
    /// Formats file size in human-readable format
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        if (bytes >= GB)
            return $"{bytes / (double)GB:F2} GB";
        if (bytes >= MB)
            return $"{bytes / (double)MB:F2} MB";
        if (bytes >= KB)
            return $"{bytes / (double)KB:F2} KB";
        return $"{bytes} bytes";
    }

    /// <summary>
    /// Handles the Remind Me Later button click
    /// </summary>
    private void LaterButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Handles the Download Update button click
    /// </summary>
    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_updateResult.DownloadUrl))
            {
                // Check if URL is a direct download or a webpage
                if (_updateResult.DownloadUrl.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) ||
                    _updateResult.DownloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to download directly and pass the expected file hash for cryptographic verification
                    _ = _updateService.DownloadUpdateAsync(_updateResult.DownloadUrl, _updateResult.FileHash);
                }
                else
                {
                    // Open download page in browser
                    _updateService.OpenDownloadPage(_updateResult.DownloadUrl);
                }
            }

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start download: {ex.Message}", "Download Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
