using System.Diagnostics;

namespace EasySave.Core.Services
{
    public class CryptoSoftLauncher
    {
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
