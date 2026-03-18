using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Use FlaUI Application explicitly to avoid namespace conflict with AdvGenPriceComparer.Application
using FlaApplication = FlaUI.Core.Application;

namespace AdvGenPriceComparer.Tests.Automation
{
    /// <summary>
    /// Utility class for launching and managing the WPF application for UI automation tests.
    /// </summary>
    public class ApplicationLauncher : IDisposable
    {
        private FlaApplication? _application;
        private UIA3Automation? _automation;
        private bool _disposed;

        /// <summary>
        /// Gets the FlaUI Application instance.
        /// </summary>
        public FlaApplication? Application => _application;

        /// <summary>
        /// Gets the UIA3 Automation instance.
        /// </summary>
        public UIA3Automation? Automation => _automation;

        /// <summary>
        /// Gets the main window of the application.
        /// </summary>
        public Window? MainWindow => _application?.GetMainWindow(_automation!);

        /// <summary>
        /// Launches the WPF application from the build output.
        /// </summary>
        /// <param name="arguments">Optional command line arguments.</param>
        /// <returns>The launched application instance.</returns>
        public FlaApplication Launch(string arguments = "")
        {
            // Find the application executable
            var appPath = GetApplicationPath();
            if (!File.Exists(appPath))
            {
                throw new FileNotFoundException($"Application executable not found at: {appPath}");
            }

            // Create automation instance
            _automation = new UIA3Automation();

            // Launch the application
            var startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            _application = FlaApplication.Launch(startInfo);

            // Wait for the application to be ready
            _application.WaitWhileBusy(TimeSpan.FromSeconds(10));

            return _application;
        }

        /// <summary>
        /// Attaches to an already running application instance.
        /// </summary>
        /// <param name="processId">The process ID of the running application.</param>
        /// <returns>The attached application instance.</returns>
        public FlaApplication Attach(int processId)
        {
            _automation = new UIA3Automation();
            _application = FlaApplication.Attach(processId);
            return _application;
        }

        /// <summary>
        /// Attaches to an already running application instance by process name.
        /// </summary>
        /// <param name="processName">The process name of the running application.</param>
        /// <returns>The attached application instance.</returns>
        public FlaApplication Attach(string processName)
        {
            _automation = new UIA3Automation();
            _application = FlaApplication.Attach(processName);
            return _application;
        }

        /// <summary>
        /// Waits for the application to become idle.
        /// </summary>
        /// <param name="timeout">Maximum time to wait.</param>
        public void WaitForIdle(TimeSpan? timeout = null)
        {
            _application?.WaitWhileBusy(timeout ?? TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Closes the application gracefully.
        /// </summary>
        public void Close()
        {
            if (_application != null)
            {
                try
                {
                    _application.Close();
                    _application.WaitWhileBusy(TimeSpan.FromSeconds(5));
                }
                catch (Exception)
                {
                    // Force kill if graceful close fails
                    try
                    {
                        _application.Kill();
                    }
                    catch
                    {
                        // Best effort
                    }
                }
                _application = null;
            }
        }

        /// <summary>
        /// Gets the path to the application executable.
        /// </summary>
        private static string GetApplicationPath()
        {
            // Try to find the application in the solution
            var currentDirectory = AppContext.BaseDirectory;
            var solutionDirectory = FindSolutionDirectory(currentDirectory);

            if (!string.IsNullOrEmpty(solutionDirectory))
            {
                // Look for the WPF application executable
                var possiblePaths = new[]
                {
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "Debug", "net8.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "Release", "net8.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "x64", "Debug", "net8.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "x64", "Release", "net8.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "Debug", "net9.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "Release", "net9.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "x64", "Debug", "net9.0-windows", "AdvGenPriceComparer.WPF.exe"),
                    Path.Combine(solutionDirectory, "AdvGenPriceComparer.WPF", "bin", "x64", "Release", "net9.0-windows", "AdvGenPriceComparer.WPF.exe"),
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            // Fallback: look in current directory or use environment variable
            var envPath = Environment.GetEnvironmentVariable("ADVGENCOMPARER_TEST_APP_PATH");
            if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
            {
                return envPath;
            }

            // Last resort: return a path that will be checked later
            return Path.Combine(solutionDirectory ?? currentDirectory, "AdvGenPriceComparer.WPF.exe");
        }

        /// <summary>
        /// Finds the solution directory by looking for the .sln file.
        /// </summary>
        private static string? FindSolutionDirectory(string startDirectory)
        {
            var currentDir = new DirectoryInfo(startDirectory);

            while (currentDir != null)
            {
                if (currentDir.GetFiles("*.sln").Any())
                {
                    return currentDir.FullName;
                }

                currentDir = currentDir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Disposes the launcher and closes the application.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _automation?.Dispose();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
