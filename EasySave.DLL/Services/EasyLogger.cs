using EasyLog.Configuration;
using EasyLog.Models;

namespace EasyLog.Services
{
    /// <summary>
    /// Main EasyLog service.
    /// Facade exposing daily logs and real-time state services.
    /// Uses workIndex as key for IDs (not backup name).
    /// </summary>
    public class EasyLogger : IEasyLogger
    {
        private readonly DailyLogService _dailyLogService;
        private readonly StateLogService _stateService;
        private readonly Dictionary<int, string> _workIndexToStateId = new();

        private List<IEasyLogger> _loggers = new();


        public EasyLogger(LogConfiguration logConfig) 
        { 
            _dailyLogService = new DailyLogService(logConfig);
            _stateService = new StateLogService(logConfig);
            
            if (logConfig.LogOnServer) {
                _loggers.Add(new EasyLogServerLogger(logConfig));
            } if (logConfig.LogInLocal) {
                _loggers.Add(new EasyLogLocalLogger(logConfig, _dailyLogService, _stateService));
            }
            LoadExistingStateIds();
        }

        private void LoadExistingStateIds()
        {
            var allStates = _stateService.GetAllStates();
            foreach (var state in allStates)
            {
                if (state.WorkIndex >= 0)
                    _workIndexToStateId[state.WorkIndex] = state.Id;
            }
        }

        #region Daily Logs

        /// <summary>
        /// Logs a file transfer action.
        /// </summary>
        public void LogFileTransfer(string backupName, string sourceFile, string targetFile,
                                   long fileSize, double transferTimeMs, double encryptionTimeMs = 0, string backupType = "")
        {
            var entry = new LogEntry
            {
                Name = backupName,
                FileSource = sourceFile,
                FileTarget = targetFile,
                FileSize = fileSize,
                FileTransferTime = transferTimeMs,
                EncryptionTime = encryptionTimeMs,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupType = backupType
            };

            UpdateLog(entry);
        }

        /// <summary>
        /// Logs a file transfer error.
        /// </summary>
        public void LogFileTransferError(string backupName, string sourceFile, string targetFile,
                                        long fileSize, string backupType = "")
        {
            LogFileTransfer(backupName, sourceFile, targetFile, fileSize, -1, 0, backupType);
        }

        /// <summary>
        /// Logs when backup is blocked due to business software.
        /// </summary>
        public void LogBackupBlocked(int workIndex, string backupName, string softwareName)
        {
            var entry = new LogEntry
            {
                Name = backupName,
                FileSource = $"BLOCKED: {softwareName} is running",
                FileTarget = "",
                FileSize = 0,
                FileTransferTime = -1,
                EncryptionTime = 0,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupType = "BLOCKED"
            };

            UpdateLog(entry);
        }

        /// <summary>
        /// Logs when backup is stopped mid-execution due to business software.
        /// </summary>
        public void LogBackupStopped(int workIndex, string backupName, string softwareName)
        {
            var entry = new LogEntry
            {
                Name = backupName,
                FileSource = $"STOPPED: {softwareName} launched during backup",
                FileTarget = "",
                FileSize = 0,
                FileTransferTime = -2,
                EncryptionTime = 0,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupType = "STOPPED"
            };

            UpdateLog(entry);
        }

        /// <summary>Gets today's logs.</summary>
        public List<LogEntry> GetTodayLogs() => _dailyLogService.GetDayLogs(DateTime.Today);

        /// <summary>Gets logs for a specific date.</summary>
        public List<LogEntry> GetDateLogs(DateTime date) => _dailyLogService.GetDayLogs(date);

        /// <summary>Gets logs for a date range.</summary>
        public List<LogEntry> GetLogs(DateTime start, DateTime end) => _dailyLogService.GetLogs(start, end);

        #endregion

        #region Real-time State

        /// <summary>
        /// Marks a backup as active (by index).
        /// </summary>
        public void StartBackup(int workIndex, string backupName, long totalFiles, long totalSize)
        {
            StateEntry state;

            if (_workIndexToStateId.TryGetValue(workIndex, out var existingStateId))
            {
                var existingState = _stateService.GetStateById(existingStateId);
                if (existingState != null)
                {
                    state = existingState;
                    state.State = BackupState.ACTIVE.ToString();
                    state.TotalFilesToCopy = totalFiles;
                    state.TotalFilesSize = totalSize;
                    state.NbFilesLeftToDo = totalFiles;
                    state.Progression = 0;
                    state.SourceFilePath = string.Empty;
                    state.TargetFilePath = string.Empty;
                }
                else
                {
                    state = CreateNewState(workIndex, backupName, totalFiles, totalSize);
                }
            }
            else
            {
                state = CreateNewState(workIndex, backupName, totalFiles, totalSize);
            }

            UpdateState(state);
        }

        private StateEntry CreateNewState(int workIndex, string backupName, long totalFiles, long totalSize)
        {
            var state = new StateEntry
            {
                WorkIndex = workIndex,
                Name = backupName,
                State = BackupState.ACTIVE.ToString(),
                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalSize,
                NbFilesLeftToDo = totalFiles,
                Progression = 0
            };
            _workIndexToStateId[workIndex] = state.Id;
            return state;
        }

        /// <summary>
        /// Updates backup progress (by index).
        /// </summary>
        public void UpdateProgress(int workIndex, string currentSource, string currentTarget,
                                  long filesLeft, long sizeLeft, double progression)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.SourceFilePath = currentSource;
            state.TargetFilePath = currentTarget;
            state.NbFilesLeftToDo = filesLeft;
            state.Progression = progression;

            UpdateState(state);
        }

        /// <summary>
        /// Marks a backup as completed (by index).
        /// </summary>
        public void CompleteBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.COMPLETED, clearPaths: true, progression: 100);
        }

        /// <summary>
        /// Marks a backup as stopped by user (by index).
        /// </summary>
        public void StopBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.STOPPED, clearPaths: true);
        }

        /// <summary>
        /// Marks a backup as error (by index).
        /// </summary>
        public void ErrorBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.ERROR);
        }

        /// <summary>
        /// Marks a backup as paused (by index).
        /// </summary>
        public void PauseBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.PAUSED);
        }

        /// <summary>
        /// Resumes a backup (by index).
        /// </summary>
        public void ResumeBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.ACTIVE);
        }

        /// <summary>
        /// Removes backup state (by index).
        /// </summary>
        public void RemoveBackupState(int workIndex)
        {
            _stateService.RemoveStateByWorkIndex(workIndex);
            _workIndexToStateId.Remove(workIndex);
        }

        /// <summary>
        /// Gets all current states.
        /// </summary>
        public List<StateEntry> GetAllStates() => _stateService.GetAllStates();

        private void UpdateBackupState(int workIndex, BackupState newState, bool clearPaths = false, double? progression = null)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.State = newState.ToString();

            if (clearPaths)
            {
                state.SourceFilePath = string.Empty;
                state.TargetFilePath = string.Empty;
                state.NbFilesLeftToDo = 0;
            }

            if (progression.HasValue)
                state.Progression = progression.Value;

            UpdateState(state);
        }

        public void UpdateLog(LogEntry entry)
        {
            foreach (IEasyLogger logger in _loggers)
            {
                logger.UpdateLog(entry);
            }
        }

        public void UpdateState(StateEntry state)
        {
            foreach (IEasyLogger logger in _loggers)
            {
                logger.UpdateState(state);
            }
        }

        #endregion
    }
}

