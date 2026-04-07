with open('./AdvGenPriceComparer.WPF/Services/UpdateService.cs', 'r') as f:
    content = f.read()

content = content.replace('using System.Net.Http;', 'using System.Net.Http;\nusing System.Security.Cryptography;')

old_method = """    /// <inheritdoc />
    public async Task<bool> DownloadUpdateAsync(string downloadUrl)
    {
        try
        {
            _logger.LogInfo($"Starting download from: {downloadUrl}");

            // For MSI installers, we download to temp and execute
            var tempPath = Path.Combine(Path.GetTempPath(), "AdvGenPriceComparer_Update.msi");

            var response = await _httpClient.GetAsync(downloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to download update: HTTP {(int)response.StatusCode}");
                return false;
            }

            var data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(tempPath, data);

            _logger.LogInfo($"Download completed: {tempPath}");

            // Execute the installer
            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Verb = "open"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to download update", ex);
            return false;
        }
    }"""

new_method = """    /// <inheritdoc />
    public async Task<bool> DownloadUpdateAsync(UpdateCheckResult updateResult)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateResult.FileHash))
            {
                _logger.LogError("Failed to download update: Missing FileHash in update manifest.");
                return false;
            }

            _logger.LogInfo($"Starting download from: {updateResult.DownloadUrl}");

            // For MSI installers, we download to temp and execute
            var tempPath = Path.Combine(Path.GetTempPath(), "AdvGenPriceComparer_Update.msi");

            var response = await _httpClient.GetAsync(updateResult.DownloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to download update: HTTP {(int)response.StatusCode}");
                return false;
            }

            var data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(tempPath, data);

            _logger.LogInfo($"Download completed: {tempPath}. Verifying hash...");

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(data);
                var actualHash = Convert.ToHexString(hashBytes);

                if (!string.Equals(updateResult.FileHash, actualHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"Security Error: Hash mismatch for downloaded update. Expected: {updateResult.FileHash}, Actual: {actualHash}");
                    File.Delete(tempPath);
                    return false;
                }
            }

            _logger.LogInfo("Hash verification successful.");

            // Execute the installer
            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Verb = "open"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to download update", ex);
            return false;
        }
    }"""

content = content.replace(old_method, new_method)

with open('./AdvGenPriceComparer.WPF/Services/UpdateService.cs', 'w') as f:
    f.write(content)
