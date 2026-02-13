using System.Diagnostics;

namespace CryptoSave
{
    /// <summary>
    /// Launches CryptoSoft executable to encrypt files.
    /// Used by EasySave.Core to delegate encryption.
    /// </summary>
    class CryptoSoftLauncher
    {
        /// <summary>
        /// Encrypts a file by launching CryptoSoft.exe.
        /// </summary>
        /// <param name="filePath">Path to the file to encrypt.</param>
        /// <returns>CryptoSoft exit code (0 = success).</returns>
        public int EncryptFile(string filePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "CryptoSoft.exe",
                Arguments = $"\"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi)!;
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}