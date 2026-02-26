using System.Diagnostics;
using EasySave.Core.Settings;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Service for encrypting files using CryptoSoft.exe.
    /// </summary>
    public class CryptoSoftService
    {
        private readonly Config _config;
        private static Mutex _mutex = new Mutex(false);

        /// <summary>Fired when the encryption mutex is acquired (encryption actually starts).</summary>
        public event Action<string>? EncryptionStarted;

        public CryptoSoftService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Encrypts a file if its extension matches the configured extensions.
        /// </summary>
        /// <param name="filePath">Path to the file to encrypt.</param>
        /// <returns>
        /// Encryption result containing:
        /// - WasEncrypted: true if encryption was attempted
        /// - EncryptionTimeMs: time in ms (0 = no encryption, greater than 0 = success, less than 0 = error code)
        /// </returns>
        public EncryptionResult EncryptIfNeeded(string filePath)
        {
            // Check if file should be encrypted
            if (!_config.ShouldEncrypt(filePath))
            {
                return new EncryptionResult { WasEncrypted = false, EncryptionTimeMs = 0 };
            }

            return Encrypt(filePath);
        }

        /// <summary>
        /// Encrypts a file using CryptoSoft.exe.
        /// </summary>
        /// <param name="filePath">Path to the file to encrypt.</param>
        /// <returns>Encryption result with timing information.</returns>
        public EncryptionResult Encrypt(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var cryptoSoftPath = FindCryptoSoftPath();
                
                if (string.IsNullOrEmpty(cryptoSoftPath))
                {
                    return new EncryptionResult 
                    { 
                        WasEncrypted = false, 
                        EncryptionTimeMs = -1,  // Error: CryptoSoft not found
                        ErrorMessage = "CryptoSoft.exe not found"
                    };
                }

                var arguments = string.IsNullOrWhiteSpace(_config.EncryptionPassword)
                    ? $"\"{filePath}\""
                    : $"\"{filePath}\" \"{_config.EncryptionPassword}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _mutex.WaitOne();
                try
                {
                    EncryptionStarted?.Invoke(filePath);
                    using var process = Process.Start(psi);

                    if (process == null)
                    {
                        return new EncryptionResult 
                        { 
                            WasEncrypted = false, 
                            EncryptionTimeMs = -2,  // Error: Process failed to start
                            ErrorMessage = "Failed to start CryptoSoft process"
                        };
                    }

                    process.WaitForExit();
                    stopwatch.Stop();

                    if (process.ExitCode == 0)
                    {
                        return new EncryptionResult 
                        { 
                            WasEncrypted = true, 
                            EncryptionTimeMs = stopwatch.Elapsed.TotalMilliseconds 
                        };
                    }
                    else
                    {
                        return new EncryptionResult 
                        { 
                            WasEncrypted = false, 
                            EncryptionTimeMs = -process.ExitCode,  // Negative exit code as error
                            ErrorMessage = $"CryptoSoft returned error code: {process.ExitCode}"
                        };
                    }
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new EncryptionResult 
                { 
                    WasEncrypted = false, 
                    EncryptionTimeMs = -99,  // General error
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Finds the CryptoSoft.exe path by probing several well-known locations.
        /// </summary>
        private string? FindCryptoSoftPath()
        {
            // 1. Check in application directory (deployed side-by-side)
            var appDirPath = Path.Combine(AppContext.BaseDirectory, "CryptoSoft.exe");
            if (File.Exists(appDirPath))
                return appDirPath;

            // 2. Check sibling project output (dev mode: ../CryptoSave/bin/<config>/net8.0/)
            var baseDir = AppContext.BaseDirectory;
            foreach (var config in new[] { "Debug", "Release" })
            {
                var siblingPath = Path.GetFullPath(
                    Path.Combine(baseDir, "..", "..", "..", "..", "CryptoSave", "bin", config, "net8.0", "CryptoSoft.exe"));
                if (File.Exists(siblingPath))
                    return siblingPath;
            }

            // 3. Check in current directory
            if (File.Exists("CryptoSoft.exe"))
                return Path.GetFullPath("CryptoSoft.exe");

            // 4. Fallback: configured path from appsettings.json (advanced users)
            if (!string.IsNullOrEmpty(_config.CryptoSoftPath) && File.Exists(_config.CryptoSoftPath))
                return _config.CryptoSoftPath;

            return null;
        }
    }

    /// <summary>
    /// Result of an encryption operation.
    /// </summary>
    public class EncryptionResult
    {
        /// <summary>True if encryption was attempted.</summary>
        public bool WasEncrypted { get; set; }

        /// <summary>
        /// Encryption time in milliseconds.
        /// 0 = no encryption needed
        /// greater than 0 = encryption successful (time in ms)
        /// less than 0 = error code
        /// </summary>
        public double EncryptionTimeMs { get; set; }

        /// <summary>Error message if encryption failed.</summary>
        public string? ErrorMessage { get; set; }
    }
}
