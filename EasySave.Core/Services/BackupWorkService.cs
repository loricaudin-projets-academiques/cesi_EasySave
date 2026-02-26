using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services.Logging;
using EasySave.Core.Settings;
using EasyLog.Services;
using EasySave.Core.ProgressBar;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Service for managing backup work operations.
    /// Handles CRUD operations and execution with event notifications.
    /// </summary>
    public class BackupWorkService
    {
        private readonly BackupWorkList _workList;
        private readonly ILocalizationService _localization;
        private readonly EasyLogger _logger;
        private readonly EasyLogServerLogger _serverLogger;
        private readonly CryptoSoftService? _cryptoService;
        private readonly BusinessSoftwareService? _businessService;
        private readonly Config _config;
        
        /// <summary>Event raised when a file transfer completes.</summary>
        public event EventHandler<FileTransferredEventArgs>? FileTransferred;
        
        /// <summary>Event raised when a file transfer fails.</summary>
        public event EventHandler<FileTransferErrorEventArgs>? FileTransferError;
        
        private readonly List<IBackupEventObserver> _observers = new();

        /// <summary>
        /// Creates a new BackupWorkService.
        /// </summary>
        /// <param name="localization">Localization service for messages.</param>
        /// <param name="workList">List of backup works.</param>
        /// <param name="logger">Logger for file transfer logging.</param>
        /// <param name="cryptoService">Optional crypto service for file encryption.</param>
        /// <param name="businessService">Optional business software detection service.</param>
        public BackupWorkService(
            ILocalizationService localization, 
            BackupWorkList workList,
            EasyLogger? logger = null,
            CryptoSoftService? cryptoService = null,
            BusinessSoftwareService? businessService = null,
            Config? config = null)
        {
            _localization = localization;
            _workList = workList;
            _logger = logger;
            _cryptoService = cryptoService;
            _businessService = businessService;
            _config = config ?? new Config();
        }

        #region Observer Pattern

        /// <summary>
        /// Adds an observer to receive backup events.
        /// </summary>
        /// <param name="observer">Observer to add.</param>
        public void AddObserver(IBackupEventObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        /// <summary>
        /// Removes an observer from receiving backup events.
        /// </summary>
        /// <param name="observer">Observer to remove.</param>
        public void RemoveObserver(IBackupEventObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(Action<IBackupEventObserver> action)
        {
            foreach (var observer in _observers)
            {
                try { action(observer); }
                catch { }
            }
        }

        #endregion

        #region Event Emitters

        protected virtual void OnFileTransferred(string backupName, string sourceFile, string targetFile, 
                                               long fileSize, double transferTimeMs)
        {
            FileTransferred?.Invoke(this, new FileTransferredEventArgs
            {
                BackupName = backupName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs
            });
        }

        protected virtual void OnFileTransferError(string backupName, string sourceFile, string targetFile, 
                                                  long fileSize, Exception ex)
        {
            FileTransferError?.Invoke(this, new FileTransferErrorEventArgs
            {
                BackupName = backupName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                Exception = ex
            });
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Adds a new backup work.
        /// </summary>
        /// <param name="name">Name of the backup job.</param>
        /// <param name="sourcePath">Source directory path.</param>
        /// <param name="destinationPath">Destination directory path.</param>
        /// <param name="typeString">Backup type (full/diff).</param>
        public void AddWork(string name, string sourcePath, string destinationPath, string typeString)
        {
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException(_localization.Get("errors.source_not_found", sourcePath));

            if (!Directory.Exists(destinationPath))
                throw new DirectoryNotFoundException(_localization.Get("errors.destination_not_found", destinationPath));

            var type = ParseBackupType(typeString);
            var backupWork = new BackupWork(name, sourcePath, destinationPath, type);
            _workList.AddBackupWork(backupWork);
        }

        /// <summary>
        /// Gets all backup works.
        /// </summary>
        /// <returns>List of all backup works.</returns>
        public List<BackupWork> GetAllWorks() => _workList.GetAllWorks();

        /// <summary>
        /// Gets a backup work by index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <returns>Backup work or null if not found.</returns>
        public BackupWork? GetWorkByIndex(int index)
        {
            var works = _workList.GetAllWorks();
            return (index >= 0 && index < works.Count) ? works[index] : null;
        }

        /// <summary>
        /// Removes a backup work by index. Also cleans up state.json.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <returns>True if successful.</returns>
        public bool RemoveWorkByIndex(int index)
        {
            try
            {
                var result = _workList.RemoveBackupWorkById(index);
                if (result)
                    _logger.RemoveBackupState(index);
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Modifies an existing backup work.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="newName">New name (optional).</param>
        /// <param name="newSourcePath">New source path (optional).</param>
        /// <param name="newDestinationPath">New destination path (optional).</param>
        /// <param name="newType">New backup type (optional).</param>
        /// <returns>True if successful.</returns>
        public bool ModifyWork(int index, string? newName = null, string? newSourcePath = null, 
            string? newDestinationPath = null, string? newType = null)
        {
            var works = _workList.GetAllWorks();
            if (index < 0 || index >= works.Count)
                return false;

            var original = works[index];
            var modified = new BackupWork(
                newName ?? original.Name,
                newSourcePath ?? original.SourcePath,
                newDestinationPath ?? original.DestinationPath,
                newType != null ? ParseBackupType(newType) : original.GetBackupType()
            );

            return _workList.EditBackupWork(original, modified) != null;
        }

        #endregion

        #region Execution


        /// <summary>
        /// Executes a backup work by index.
        /// Events are captured and emitted automatically.
        /// If business software is running before start, waits until it closes.
        /// If business software starts during backup, pauses and resumes automatically.
        /// </summary>
        /// <param name="index">Zero-based index of the work to execute.</param>
        public void ExecuteWork(int index)
        {
            try
            {
                var work = GetWorkByIndex(index);
                if (work == null)
                    return;

                // If business software is running BEFORE starting, wait for it to close
                // Skip if a BackupJobRunner already configured pause/cancel (HasExternalControls)
                if (_businessService != null && _businessService.IsRunning() && !work.HasExternalControls)
                {
                    var softwareName = _businessService.GetBusinessSoftwareName() ?? "Unknown";
                    _logger.LogBackupBlocked(index, work.Name, softwareName);
                    NotifyObservers(o => o.OnBackupPaused(work.Name));

                    while (_businessService.IsRunning())
                        Thread.Sleep(500);

                    NotifyObservers(o => o.OnBackupResumed(work.Name));
                }

                var files = Directory.GetFiles(work.SourcePath, "*", SearchOption.AllDirectories);
                var totalSize = files.Sum(f => new FileInfo(f).Length);
                
                NotifyObservers(o => o.OnBackupStarted(work.Name, files.Length, totalSize));
                _logger.StartBackup(index, work.Name, files.Length, totalSize);

                // Connect file transfer events - encryption happens HERE at service level
                work.FileTransferred += (sender, args) => 
                {
                    if (args is FileCopiedEventArgs fileArgs)
                    {
                        // Encrypt file if needed (service responsibility, not model)
                        double encryptionTimeMs = 0;
                        if (_cryptoService != null && _config.ShouldEncrypt(fileArgs.DestFile))
                        {
                            work.RaiseEncryptionWaiting();
                            void onStarted(string f) => work.RaiseEncryptionStarted(f);
                            _cryptoService.EncryptionStarted += onStarted;
                            var encryptResult = _cryptoService.Encrypt(fileArgs.DestFile);
                            _cryptoService.EncryptionStarted -= onStarted;
                            encryptionTimeMs = encryptResult.EncryptionTimeMs;
                            work.RaiseEncryptionCompleted();
                        }

                        _logger.LogFileTransfer(work.Name, fileArgs.SourceFile, fileArgs.DestFile, fileArgs.FileSize, fileArgs.TransferTimeMs, encryptionTimeMs);
                        OnFileTransferred(work.Name, fileArgs.SourceFile, fileArgs.DestFile, fileArgs.FileSize, fileArgs.TransferTimeMs);
                    }
                };
                
                work.FileTransferError += (sender, args) => 
                {
                    if (args is FileCopyErrorEventArgs errorArgs)
                    {
                        _logger.LogFileTransferError(work.Name, errorArgs.SourceFile, errorArgs.DestFile, errorArgs.FileSize);
                        OnFileTransferError(work.Name, errorArgs.SourceFile, errorArgs.DestFile, errorArgs.FileSize, errorArgs.Exception);
                    }
                };
                
                // Connect progress events
                work.FileProgress += (sender, args) =>
                {
                    if (args is FileProgressEventArgs progressArgs)
                    {
                        long filesLeft = files.Length - (int)(progressArgs.CurrentProgress / 100 * files.Length);
                        long sizeLeft = (long)(totalSize * (100 - progressArgs.CurrentProgress) / 100);
                        
                        _logger.UpdateProgress(index, progressArgs.SourceFile, progressArgs.DestFile, filesLeft, sizeLeft, progressArgs.CurrentProgress);
                        NotifyObservers(o => o.OnProgressUpdated(work.Name, progressArgs.SourceFile, progressArgs.DestFile, filesLeft, sizeLeft, progressArgs.CurrentProgress));
                    }
                };

                _workList.ExecuteBackupWork(index);

                _logger.CompleteBackup(index);
                NotifyObservers(o => o.OnBackupCompleted(work.Name));
            }
            catch (OperationCanceledException)
            {
                // Stopped by user — do NOT log as completed or error
                var work2 = GetWorkByIndex(index);
                if (work2 != null)
                {
                    _logger.StopBackup(index);
                }
                throw; // re-throw so BackupJobRunner catches it as Stopped
            }
            catch (Exception ex)
            {
                var work2 = GetWorkByIndex(index);
                if (work2 != null)
                {
                    _logger.ErrorBackup(index);
                    NotifyObservers(o => o.OnBackupError(work2.Name, ex));
                }
            }
        }

        /// <summary>
        /// Executes all backup works sequentially.
        /// </summary>
        public void ExecuteAllWorks()
        {
            _workList.ExecuteAllBackupWorks();
        }

        /// <summary>
        /// Gets the total count of backup works.
        /// </summary>
        /// <returns>Number of backup works.</returns>
        public int GetWorkCount() => _workList.GetCount();

        /// <summary>
        /// Updates the real-time state log to PAUSED for the given work index.
        /// Called by BackupJobEngine when a runner pauses.
        /// </summary>
        public void LogStatePaused(int workIndex) => _logger.PauseBackup(workIndex);

        /// <summary>
        /// Updates the real-time state log to ACTIVE for the given work index.
        /// Called by BackupJobEngine when a runner resumes.
        /// </summary>
        public void LogStateResumed(int workIndex) => _logger.ResumeBackup(workIndex);

        /// <summary>
        /// Updates the real-time state log to STOPPED for the given work index.
        /// Called by BackupJobEngine when a runner is stopped.
        /// </summary>
        public void LogStateStopped(int workIndex) => _logger.StopBackup(workIndex);

        #endregion

        #region Helpers

        private BackupType ParseBackupType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "full" or "complete" => BackupType.FULL_BACKUP,
                "diff" or "differential" => BackupType.DIFFERENTIAL_BACKUP,
                _ => throw new ArgumentException(_localization.Get("errors.invalid_backup_type", type))
            };
        }

        /// <summary>
        /// Gets the localized name of a backup type.
        /// </summary>
        /// <param name="type">Backup type.</param>
        /// <returns>Localized type name.</returns>
        public string GetLocalizedBackupTypeName(BackupType type)
        {
            return type == BackupType.FULL_BACKUP 
                ? _localization.Get("backup_types.full")
                : _localization.Get("backup_types.diff");
        }

        #endregion
    }
}
