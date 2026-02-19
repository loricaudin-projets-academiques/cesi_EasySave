using EasyLog.Configuration;
using EasyLog.Models;
using System;
using System.Collections.Generic;

namespace EasyLog.Services
{
    public class EasyLogServerLogger
    {
        private readonly DailyLogService _dailyLogService;
        private readonly StateLogService _stateService;
        private readonly EasyLogServer _server;
        private readonly Dictionary<int, string> _workIndexToStateId = new();

        public EasyLogServerLogger(LogConfiguration? config = null)
        {
            var logConfig = config ?? new LogConfiguration();
            _dailyLogService = new DailyLogService(logConfig);
            _stateService = new StateLogService(logConfig);
            LoadExistingStateIds();

            // Connexion au serveur TCP
            _server = new EasyLogServer("127.0.0.1", 5000);
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

        public EasyLogServerLogger(object configObj)
        {
            var logConfig = new LogConfiguration();

            if (configObj != null)
            {
                var logTypeProp = configObj.GetType().GetProperty("LogType");
                if (logTypeProp != null)
                {
                    var logType = logTypeProp.GetValue(configObj)?.ToString() ?? "json";
                    logConfig.LogFormat = logType.ToLowerInvariant();
                }
            }

            _server = new EasyLogServer("127.0.0.1", 5000);
        }

        #region Daily Logs

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

            _server.SendJson(entry);
        }

        public void LogFileTransferError(string backupName, string sourceFile, string targetFile,
                                        long fileSize, string backupType = "")
        {
            LogFileTransfer(backupName, sourceFile, targetFile, fileSize, -1, 0, backupType);
        }

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

            _server.SendJson(entry);
        }

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

            _server.SendJson(entry);
        }

        #endregion

        #region Real-time State

        public void StartBackup(int workIndex, string backupName, long totalFiles, long totalSize)
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

            _server.SendJson(state);
        }

        public void UpdateProgress(int workIndex, string currentSource, string currentTarget,
                                  long filesLeft, long sizeLeft, double progression)
        {
            if (!_workIndexToStateId.ContainsKey(workIndex))
                return;

            var state = new StateEntry
            {
                WorkIndex = workIndex,
                SourceFilePath = currentSource,
                TargetFilePath = currentTarget,
                NbFilesLeftToDo = filesLeft,
                Progression = progression,
                State = BackupState.ACTIVE.ToString()
            };

            _server.SendJson(state);
        }

        public void CompleteBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.COMPLETED, true, 100);
        }

        public void ErrorBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.ERROR);
        }

        public void PauseBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.PAUSED);
        }

        public void ResumeBackup(int workIndex)
        {
            UpdateBackupState(workIndex, BackupState.ACTIVE);
        }

        public void RemoveBackupState(int workIndex)
        {
            var state = new StateEntry
            {
                WorkIndex = workIndex,
                State = "REMOVED"
            };

            _server.SendJson(state);
            _workIndexToStateId.Remove(workIndex);
        }

        private void UpdateBackupState(int workIndex, BackupState newState, bool clearPaths = false, double? progression = null)
        {
            if (!_workIndexToStateId.ContainsKey(workIndex))
                return;

            var state = new StateEntry
            {
                WorkIndex = workIndex,
                State = newState.ToString()
            };

            if (clearPaths)
            {
                state.SourceFilePath = "";
                state.TargetFilePath = "";
                state.NbFilesLeftToDo = 0;
            }

            if (progression.HasValue)
                state.Progression = progression.Value;

            _server.SendJson(state);
        }

        #endregion
    }
}
