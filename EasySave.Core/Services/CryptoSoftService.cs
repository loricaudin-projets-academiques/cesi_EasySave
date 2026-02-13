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

                var psi = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

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
        /// Finds the CryptoSoft.exe path.
        /// </summary>
        private string? FindCryptoSoftPath()
        {
            // 1. Check configured path
            if (!string.IsNullOrEmpty(_config.CryptoSoftPath) && File.Exists(_config.CryptoSoftPath))
                return _config.CryptoSoftPath;

            // 2. Check in application directory
            var appDirPath = Path.Combine(AppContext.BaseDirectory, "CryptoSoft.exe");
            if (File.Exists(appDirPath))
                return appDirPath;

            // 3. Check in current directory
            if (File.Exists("CryptoSoft.exe"))
                return Path.GetFullPath("CryptoSoft.exe");

            // 4. Check in PATH (just return the name, let OS find it)
            return _config.CryptoSoftPath;
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
