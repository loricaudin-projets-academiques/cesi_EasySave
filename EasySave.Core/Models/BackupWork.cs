using EasySave.Core.ProgressBar;
using System.Text.Json.Serialization;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents a backup work/job with source, destination and backup type.
    /// Pure model - no service dependencies.
    /// </summary>
    public class BackupWork
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string DestinationPath { get; private set; }

        /// <summary>Backup type serialized as string (FULL_BACKUP / DIFFERENTIAL_BACKUP).</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BackupType Type { get; private set; }

        /// <summary>Runtime state, excluded from JSON serialization.</summary>
        [JsonIgnore]
        public BackupState State { get; private set; }

        /// <summary>Event raised when file copy progress updates.</summary>
        public event EventHandler? FileProgress;

        /// <summary>Event raised when a file transfer completes successfully.</summary>
        public event EventHandler? FileTransferred;

        /// <summary>Event raised when a file transfer fails.</summary>
        public event EventHandler? FileTransferError;

        /// <summary>
        /// Single instance of the copy handler to keep global progress consistent.
        /// </summary>
        private readonly CopyFileWithProgressBar _cp;

        /// <summary>
        /// Creates a new backup work.
        /// </summary>
        /// <param name="name">Name of the backup job.</param>
        /// <param name="sourcePath">Source directory path.</param>
        /// <param name="destinationPath">Destination directory path.</param>
        /// <param name="type">Type of backup (Full or Differential).</param>
        public BackupWork(string name, string sourcePath, string destinationPath, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;

            this.State = new BackupState(this.Name, DateTime.UtcNow, 0, 0.0, 0, this.SourcePath, this.DestinationPath);

            _cp = new CopyFileWithProgressBar(this.State);

            _cp.FileProgress += (s, e) => FileProgress?.Invoke(s, e);
            _cp.FileTransferred += (s, e) => FileTransferred?.Invoke(s, e);
            _cp.FileTransferError += (s, e) => FileTransferError?.Invoke(s, e);
        }

        public string GetName() => this.Name;
        public string GetDestinationPath() => this.DestinationPath;
        public string GetSourcePath() => this.SourcePath;
        public BackupType GetBackupType() => this.Type;

        public void SetName(string name) => Name = name;
        public void SetDestinationPath(string destinationPath) => DestinationPath = destinationPath;
        public void SetSourcePath(string sourcePath) => SourcePath = sourcePath;
        public void SetType(BackupType type) => Type = type;

        /// <summary>
        /// Sets a pause checker function that is called between file chunks.
        /// If it returns true, the copy pauses until it returns false.
        /// </summary>
        public void SetPauseChecker(Func<bool>? checker)
        {
            _cp.PauseChecker = checker;
        }

        /// <summary>
        /// Returns true if pause/cancel controls have already been configured (e.g. by BackupJobRunner).
        /// </summary>
        public bool HasExternalControls => _cp.PauseChecker != null || _cp.ManualPauseGate != null;

        /// <summary>
        /// Sets a cancellation token checked between chunks to support stop.
        /// </summary>
        public void SetCancellationToken(CancellationToken ct)
        {
            _cp.CancellationToken = ct;
        }

        /// <summary>
        /// Sets the shared large file transfer lock (prevents parallel large file transfers).
        /// </summary>
        public void SetLargeFileLock(Services.LargeFileTransferLock? lockObj)
        {
            _cp.LargeFileLock = lockObj;
        }

        /// <summary>
        /// Sets the manual pause gate (checked between files, not mid-file).
        /// </summary>
        public void SetManualPauseGate(ManualResetEventSlim? gate)
        {
            _cp.ManualPauseGate = gate;
        }

        /// <summary>Event raised when the backup pauses due to business software.</summary>
        public event Action? Paused
        {
            add => _cp.Paused += value;
            remove => _cp.Paused -= value;
        }

        /// <summary>Event raised when the backup resumes after pause.</summary>
        public event Action? Resumed
        {
            add => _cp.Resumed += value;
            remove => _cp.Resumed -= value;
        }

        /// <summary>Event raised when manual pause takes effect (after current file).</summary>
        public event Action? ManualPaused
        {
            add => _cp.ManualPaused += value;
            remove => _cp.ManualPaused -= value;
        }

        /// <summary>Event raised when manual pause is released.</summary>
        public event Action? ManualResumed
        {
            add => _cp.ManualResumed += value;
            remove => _cp.ManualResumed -= value;
        }

        /// <summary>
        /// Executes the backup based on its type.
        /// </summary>
        /// <exception cref="Exception">Thrown when paths are invalid or backup type is unknown.</exception>
        public void Execute()
        {
            if (!Directory.Exists(this.SourcePath))
                throw new Exception($"Source path is invalid or not accessible: {this.SourcePath}");

            if (!Directory.Exists(this.DestinationPath))
                throw new Exception($"Destination path is invalid or not accessible: {this.DestinationPath}");

            long totalBytes = this.Type switch
            {
                BackupType.FULL_BACKUP => ComputeTotalBytesFull(this.SourcePath),
                BackupType.DIFFERENTIAL_BACKUP => ComputeTotalBytesDifferential(this.SourcePath, this.DestinationPath),
                _ => throw new Exception("Unknown backup type.")
            };

            _cp.SetGlobalTotalBytes(totalBytes);

            switch (this.Type)
            {
                case BackupType.DIFFERENTIAL_BACKUP:
                    ExecuteDifferentialBackup();
                    break;

                case BackupType.FULL_BACKUP:
                    ExecuteFullBackup();
                    break;
                
                default:
                    throw new Exception("Unknown backup type.");
            }
        }

        /// <summary>
        /// Computes the total size of all files in the source directory (recursive).
        /// Used for FULL backup.
        /// </summary>
        private long ComputeTotalBytesFull(string sourceDir)
        {
            long total = 0;

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
                total += new FileInfo(file).Length;

            return total;
        }

        /// <summary>
        /// Computes the total size of files that WILL be copied in a differential backup.
        /// </summary>
        private long ComputeTotalBytesDifferential(string sourceDir, string destDir)
        {
            long total = 0;

            // Fichiers du dossier courant
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);

                bool shouldCopy = true;

                if (File.Exists(destFile))
                {
                    var srcInfo = new FileInfo(file);
                    var dstInfo = new FileInfo(destFile);

                    shouldCopy =
                        srcInfo.LastWriteTime > dstInfo.LastWriteTime ||
                        srcInfo.Length != dstInfo.Length;
                }

                if (shouldCopy)
                    total += new FileInfo(file).Length;
            }

            // Sous-dossiers
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destSubDir = Path.Combine(destDir, dirName);

                total += ComputeTotalBytesDifferential(directory, destSubDir);
            }

            return total;
        }

        private void ExecuteFullBackup()
        {
            _cp.InitProgressBar($"Full Backup in progress for: {this.Name}");
            CopyDirectoryRecursively(this.SourcePath, this.DestinationPath, differential: false);
        }

        private void ExecuteDifferentialBackup()
        {
            _cp.InitProgressBar($"Differential Backup in progress for: {this.Name}");
            CopyDirectoryRecursively(this.SourcePath, this.DestinationPath, differential: true);
        }

        private void CopyDirectoryRecursively(string sourceDir, string destDir, bool differential)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);

                bool shouldCopy = true;

                if (differential && File.Exists(destFile))
                {
                    var srcInfo = new FileInfo(file);
                    var dstInfo = new FileInfo(destFile);

                    shouldCopy =
                        srcInfo.LastWriteTime > dstInfo.LastWriteTime ||
                        srcInfo.Length != dstInfo.Length;
                }

                if (shouldCopy)
                    _cp.CopyFiles(sourceDir, destDir, new[] { file });
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destSubDir = Path.Combine(destDir, dirName);

                CopyDirectoryRecursively(directory, destSubDir, differential);
            }
        }
    }
}
