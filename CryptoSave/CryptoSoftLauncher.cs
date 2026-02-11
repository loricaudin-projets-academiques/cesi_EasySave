using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cesi_EasySave.CryptoSave
{
    class CryptoSoftLauncher
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

            using Process process = Process.Start(psi);
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}