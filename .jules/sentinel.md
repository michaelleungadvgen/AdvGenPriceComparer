## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Silent Data Export and Overwrites
**Vulnerability:** Files were being automatically exported and written directly to the `MyDocuments` folder with a generic format (e.g., `WeeklySpecials_yyyyMMdd.md`) without any user consent prompt or file overwrite confirmation, leading to potential silent data loss.
**Learning:** Even internal file operations (like exporting user-generated or fetched data) can pose a threat of data loss if the system assumes safe filenames, especially when using standard system folders without verifying existing files.
**Prevention:** Ensure any logic writing files to user-accessible directories invokes an interactive consent mechanism (like `SaveFileDialog` in WPF or its equivalent) to securely prompt for permission and handle file collisions gracefully.
