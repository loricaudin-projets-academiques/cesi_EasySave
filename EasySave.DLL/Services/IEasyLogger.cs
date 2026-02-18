using EasyLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog.Services
{
    public interface IEasyLogger
    {

        #region Daily Logs

        /// <summary>
        /// Logs a file transfer action.
        /// </summary>
        public void LogFileTransfer(string backupName, string sourceFile, string targetFile,
                                   long fileSize, double transferTimeMs, double encryptionTimeMs = 0, string backupType = "");

        /// <summary>
        /// Logs a file transfer error.
        /// </summary>
        public void LogFileTransferError(string backupName, string sourceFile, string targetFile,
                                        long fileSize, string backupType = "");

        /// <summary>
        /// Logs when backup is blocked due to business software.
        /// </summary>
        public void LogBackupBlocked(int workIndex, string backupName, string softwareName);

        /// <summary>
        /// Logs when backup is stopped mid-execution due to business software.
        /// </summary>
        public void LogBackupStopped(int workIndex, string backupName, string softwareName);

        /// <summary>Gets today's logs.</summary>
        public List<LogEntry> GetTodayLogs();

        /// <summary>Gets logs for a specific date.</summary>
        public List<LogEntry> GetDateLogs(DateTime date);

        /// <summary>Gets logs for a date range.</summary>
        public List<LogEntry> GetLogs(DateTime start, DateTime end);

        #endregion

        #region Real-time State

        /// <summary>
        /// Marks a backup as active (by index).
        /// </summary>
        public void StartBackup(int workIndex, string backupName, long totalFiles, long totalSize);

        protected StateEntry CreateNewState(int workIndex, string backupName, long totalFiles, long totalSize);

        /// <summary>
        /// Updates backup progress (by index).
        /// </summary>
        public void UpdateProgress(int workIndex, string currentSource, string currentTarget,
                                  long filesLeft, long sizeLeft, double progression);

        /// <summary>
        /// Marks a backup as completed (by index).
        /// </summary>
        public void CompleteBackup(int workIndex);

        /// <summary>
        /// Marks a backup as error (by index).
        /// </summary>
        public void ErrorBackup(int workIndex);

        /// <summary>
        /// Marks a backup as paused (by index).
        /// </summary>
        public void PauseBackup(int workIndex);

        /// <summary>
        /// Resumes a backup (by index).
        /// </summary>
        public void ResumeBackup(int workIndex);

        /// <summary>
        /// Removes backup state (by index).
        /// </summary>
        public void RemoveBackupState(int workIndex);

        /// <summary>
        /// Gets all current states.
        /// </summary>
        public List<StateEntry> GetAllStates();

        public void UpdateBackupState(int workIndex, BackupState newState, bool clearPaths = false, double? progression = null);

        #endregion
    }
}
