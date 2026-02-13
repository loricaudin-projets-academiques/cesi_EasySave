using System.Diagnostics;
using EasySave.Core.Settings;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Service to detect if business software is running and block backups accordingly.
    /// </summary>
    public class BusinessSoftwareService
    {
        private readonly Config _config;

        public BusinessSoftwareService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Checks if the configured business software is currently running.
        /// </summary>
        /// <returns>True if business software is running, false otherwise.</returns>
        public bool IsRunning()
        {
            if (string.IsNullOrWhiteSpace(_config.BusinessSoftware))
                return false;

            try
            {
                var processName = _config.BusinessSoftware.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the name of the configured business software.
        /// </summary>
        public string? GetBusinessSoftwareName()
        {
            return string.IsNullOrWhiteSpace(_config.BusinessSoftware) ? null : _config.BusinessSoftware;
        }

        /// <summary>
        /// Checks if business software blocking is enabled.
        /// </summary>
        public bool IsBlockingEnabled()
        {
            return !string.IsNullOrWhiteSpace(_config.BusinessSoftware);
        }

        /// <summary>
        /// Gets a list of currently running processes with a visible window.
        /// Useful for GUI to let user select a process.
        /// </summary>
        public static string[] GetRunningProcesses()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.ProcessName)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }

    /// <summary>
    /// Exception thrown when backup is blocked due to business software running.
    /// </summary>
    public class BusinessSoftwareRunningException : Exception
    {
        public string SoftwareName { get; }

        public BusinessSoftwareRunningException(string softwareName)
            : base($"Backup blocked: business software '{softwareName}' is running")
        {
            SoftwareName = softwareName;
        }
    }
}
