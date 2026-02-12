using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using EasySave.Core.Services; // 

namespace EasySave.Core.Models
{
    public class BackupWork
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string DestinationPath { get; private set; }

        public bool StopIfBusinessDetected { get; set; } = true;

        private DateTime _backupStartTime;
        private DateTime _backupEndTime;
        private long _encryptionTimeMs;
        private int _cryptoSoftReturnCode;

        private readonly string[] _businessExtensions =
        {
            ".exe", ".bat", ".cmd", ".sh"
        };

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BackupType Type { get; private set; }

        [JsonIgnore]
        public BackupState State { get; private set; }

        public BackupWork(string name, string sourcePath, string destinationPath, BackupType type)
        {
            Name = name;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            Type = type;

            State = new BackupState(
                Name,
                DateTime.UtcNow,
                0,
                0.0,
                0,
                SourcePath,
                DestinationPath
            );
        }

        public void Execute()
        {
            _backupStartTime = DateTime.UtcNow;

            try
            {
                string? detectedFile = DetectBusinessFile(SourcePath);
                if (detectedFile != null && StopIfBusinessDetected)
                {
                    throw new OperationCanceledException(
                        $"Backup stopped: business file detected ({Path.GetFileName(detectedFile)})"
                    );
                }

                switch (Type)
                {
                    case BackupType.FULL_BACKUP:
                        ExecuteFullBackup();
                        break;
                    case BackupType.DIFFERENTIAL_BACKUP:
                        ExecuteDifferentialBackup();
                        break;
                }

                _backupEndTime = DateTime.UtcNow;
                WriteDailyLog(true);
            }
            catch (Exception ex)
            {
                _backupEndTime = DateTime.UtcNow;
                WriteDailyLog(false, ex.Message);
                throw;
            }
        }

        private void ExecuteFullBackup()
        {
            string[] files = Directory.GetFiles(SourcePath);

            var cp = new CopyFileWithProgressBar(State);
            cp.CopyFiles(SourcePath, DestinationPath, files);

            foreach (string file in files)
            {
                string destFile = Path.Combine(DestinationPath, Path.GetFileName(file));
                EncryptFile(destFile);
            }
        }

        private void ExecuteDifferentialBackup()
        {
            string[] files = Directory.GetFiles(this.SourcePath);
            var filesToUpdate = new List<string>();

            var cp = new CopyFileWithProgressBar(this.State);
            cp.InitProgressBar($"Differential Backup in progress for: {this.Name}");

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string sourceFile = Path.Combine(this.SourcePath, fileName);
                string destFile = Path.Combine(this.DestinationPath, fileName);

                bool needsUpdate = !File.Exists(destFile)
                    || File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile)
                    || new FileInfo(sourceFile).Length != new FileInfo(destFile).Length;

                if (needsUpdate)
                    filesToUpdate.Add(file);
            }

            cp.CopyFiles(this.SourcePath, this.DestinationPath, filesToUpdate.ToArray());
        }



        private void EncryptFile(string filePath)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                CryptoSoftLauncher launcher = new CryptoSoftLauncher();
                _cryptoSoftReturnCode = launcher.EncryptFile(filePath);

                sw.Stop();
                _encryptionTimeMs = _cryptoSoftReturnCode == 0 ? sw.ElapsedMilliseconds : -1;
            }
            catch
            {
                sw.Stop();
                _encryptionTimeMs = -1;
                _cryptoSoftReturnCode = -1;
                throw;
            }
        }

        private string? DetectBusinessFile(string sourcePath)
        {
            foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (_businessExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    return file;
            }
            return null;
        }

        private void WriteDailyLog(bool success, string? errorMessage = null)
        {
            string log =
                $"Date: {DateTime.UtcNow}\n" +
                $"Backup: {Name}\n" +
                $"Type: {Type}\n" +
                $"Duration(ms): {(_backupEndTime - _backupStartTime).TotalMilliseconds}\n" +
                $"Encryption(ms): {_encryptionTimeMs}\n" +
                $"CryptoSoft code: {_cryptoSoftReturnCode}\n" +
                $"Status: {(success ? "SUCCESS" : "ERROR")}\n";

            if (!success && errorMessage != null)
                log += $"Error: {errorMessage}\n";

            log += "----------------------------\n";

            File.AppendAllText("DailyLog.txt", log);
        }
    }
}
