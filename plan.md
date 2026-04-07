1. **Understand the Vulnerability:**
   - The application downloads an MSI/EXE installer via HTTP/HTTPS in `UpdateService.DownloadUpdateAsync(string downloadUrl)` and then executes it using `Process.Start()`.
   - The execution uses the downloaded file without verifying its integrity or authenticity, meaning if an attacker alters the payload or redirects the download, arbitrary code will be executed.
   - The memory context clearly states: "The WPF application's auto-update mechanism (`UpdateService`) must cryptographically verify downloaded installers using SHA-256 against the `FileHash` property in the `UpdateCheckResult` manifest before calling `Process.Start()`. This validation must fail securely: if the `FileHash` is missing or empty, the update must be rejected to prevent attackers from bypassing validation via an empty manifest. The `DownloadUpdateAsync` method expects the full `UpdateCheckResult` payload to perform this check."
   - The memory also states: "To ensure broader .NET version compatibility when validating cryptographic hashes (e.g., SHA-256), use `Convert.ToHexString()` combined with an `OrdinalIgnoreCase` string comparison instead of the .NET 9-specific `Convert.ToHexStringLower()`."
   - `DownloadUpdateAsync(string downloadUrl)` interface must be updated to `DownloadUpdateAsync(UpdateCheckResult updateResult)`.

2. **Fix Steps:**
   - Update `IUpdateService.cs` so `DownloadUpdateAsync` takes `UpdateCheckResult updateResult` instead of `string downloadUrl`.
   - Update `UpdateService.cs` implementation of `DownloadUpdateAsync`:
     - Validate that `updateResult.FileHash` is not null or empty. If it is empty, reject the update.
     - Download the file as a byte array or stream.
     - Compute the SHA256 hash of the downloaded bytes.
     - Compare the computed hash with `updateResult.FileHash` using `Convert.ToHexString()` and `OrdinalIgnoreCase`.
     - Only if the hashes match, write the file to disk and call `Process.Start()`. Otherwise, reject the update and log an error.
   - Update `UpdateNotificationWindow.xaml.cs` to pass `_updateResult` to `DownloadUpdateAsync` instead of `_updateResult.DownloadUrl`.
   - Create or update `.jules/sentinel.md` with the new learning.

3. **Verification:**
   - Use `dotnet build` to ensure the changes compile.
   - I might also run tests if there are any applicable.
