using EasyLog.Configuration;
using EasyLog.Models;

namespace EasyLog.Services
{
    /// <summary>
    /// Service principal EasyLog
    /// Point d'entrée unique pour la gestion des logs
    /// Façade qui expose les deux services (daily logs + state)
    /// ? Utilise workIndex comme clé pour les IDs (pas le nom!)
    /// ? Accepte Config ou LogConfiguration
    /// </summary>
    public class EasyLogger
    {
        private readonly DailyLogService _dailyLogService;
        private readonly StateLogService _stateService;
        
        // ? Mapper workIndex ? StateEntry.Id (pas backupName!)
        private readonly Dictionary<int, string> _workIndexToStateId = new();

        /// <summary>
        /// Constructeur avec LogConfiguration
        /// </summary>
        public EasyLogger(LogConfiguration? config = null)
        {
            var logConfig = config ?? new LogConfiguration();
            _dailyLogService = new DailyLogService(logConfig);
            _stateService = new StateLogService(logConfig);
            
            // ? Charger les IDs existants depuis state.json
            LoadExistingStateIds();
        }

        /// <summary>
        /// Constructeur avec Config (provient de appsettings.json)
        /// ? PRÉFÉRÉ : Utiliser Config plutôt que LogConfiguration
        /// </summary>
        public EasyLogger(object configObj)
        {
            System.Console.WriteLine($"?? EasyLogger reçoit config de type: {configObj?.GetType().Name}");
            
            // Si c'est un objet Config avec LogType et LogPath
            if (configObj != null && configObj.GetType().GetProperty("LogType") != null)
            {
                var logTypeProp = configObj.GetType().GetProperty("LogType");
                var logPathProp = configObj.GetType().GetProperty("LogPath");
                
                var logType = logTypeProp?.GetValue(configObj)?.ToString() ?? "json";
                var logPath = logPathProp?.GetValue(configObj)?.ToString() ?? "./logs/";
                
                System.Console.WriteLine($"? LogPath avant: {logPath}");
                
                // Normaliser le chemin
                logPath = Path.GetFullPath(logPath);
                
                System.Console.WriteLine($"? LogPath après: {logPath}");
                System.Console.WriteLine($"? LogType: {logType}");
                System.Console.WriteLine($"? Répertoire existe? {Directory.Exists(logPath)}");
                
                var logConfig = new LogConfiguration 
                { 
                    LogFormat = logType,
                    LogDirectory = logPath
                };
                
                System.Console.WriteLine($"? LogConfiguration créée avec LogDirectory: {logConfig.LogDirectory}");
                System.Console.WriteLine($"? DailyLogsPath: {logConfig.GetDailyLogsPath()}");
                
                _dailyLogService = new DailyLogService(logConfig);
                _stateService = new StateLogService(logConfig);
            }
            else
            {
                System.Console.WriteLine("??  Config invalide, utilisation des valeurs par défaut");
                var logConfig = new LogConfiguration();
                _dailyLogService = new DailyLogService(logConfig);
                _stateService = new StateLogService(logConfig);
            }
            
            LoadExistingStateIds();
        }



        /// <summary>
        /// Charge les mappages workIndex ? StateId depuis le fichier state.json
        /// ? Permet de reconnaître les travaux entre les exécutions
        /// </summary>
        private void LoadExistingStateIds()
        {
            var allStates = _stateService.GetAllStates();
            foreach (var state in allStates)
            {
                if (state.WorkIndex >= 0)
                {
                    _workIndexToStateId[state.WorkIndex] = state.Id;
                }
            }
        }

        // ============ LOGS JOURNALIERS ============

        /// <summary>
        /// Enregistre une action de sauvegarde (transfert de fichier)
        /// </summary>
        public void LogFileTransfer(string backupName, string sourceFile, string targetFile, 
                                   long fileSize, double transferTimeMs)
        {
            var entry = new LogEntry
            {
                Name = backupName,
                FileSource = sourceFile,
                FileTarget = targetFile,
                FileSize = fileSize,
                FileTransferTime = transferTimeMs,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            _dailyLogService.AddLogEntry(entry);
        }

        /// <summary>
        /// Enregistre une erreur de transfert
        /// </summary>
        public void LogFileTransferError(string backupName, string sourceFile, string targetFile, 
                                        long fileSize)
        {
            LogFileTransfer(backupName, sourceFile, targetFile, fileSize, -1);
        }

        /// <summary>
        /// Récupère les logs du jour
        /// </summary>
        public List<LogEntry> GetTodayLogs() => _dailyLogService.GetDayLogs(DateTime.Today);

        /// <summary>
        /// Récupère les logs d'une date
        /// </summary>
        public List<LogEntry> GetDateLogs(DateTime date) => _dailyLogService.GetDayLogs(date);

        /// <summary>
        /// Récupère les logs d'une plage
        /// </summary>
        public List<LogEntry> GetLogs(DateTime start, DateTime end) => 
            _dailyLogService.GetLogs(start, end);

        // ============ ÉTAT EN TEMPS RÉEL (avec index) ============

        /// <summary>
        /// Marque un travail comme actif (par index)
        /// ? L'index est stocké dans StateEntry pour la persistance
        /// ? Re-run même index = même ID (pas de dédoublement)
        /// </summary>
        public void StartBackup(int workIndex, string backupName, long totalFiles, long totalSize)
        {
            // ? Chercher si un state existe déjà pour ce workIndex
            StateEntry state;
            
            if (_workIndexToStateId.TryGetValue(workIndex, out var existingStateId))
            {
                // ? État existe déjà ? le réutiliser
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
                    // État supprimé? En créer un nouveau
                    state = new StateEntry
                    {
                        WorkIndex = workIndex,  // ? Stocker l'index!
                        Name = backupName,
                        State = BackupState.ACTIVE.ToString(),
                        TotalFilesToCopy = totalFiles,
                        TotalFilesSize = totalSize,
                        NbFilesLeftToDo = totalFiles,
                        Progression = 0
                    };
                    _workIndexToStateId[workIndex] = state.Id;
                }
            }
            else
            {
                // ? État n'existe pas ? le créer
                state = new StateEntry
                {
                    WorkIndex = workIndex,  // ? Stocker l'index!
                    Name = backupName,
                    State = BackupState.ACTIVE.ToString(),
                    TotalFilesToCopy = totalFiles,
                    TotalFilesSize = totalSize,
                    NbFilesLeftToDo = totalFiles,
                    Progression = 0
                };

                _workIndexToStateId[workIndex] = state.Id;
            }

            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Met à jour la progression d'un travail (par index)
        /// </summary>
        public void UpdateProgress(int workIndex, string currentSource, string currentTarget,
                                  long filesLeft, long sizeLeft, double progression)
        {
            // ? Chercher par index
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.SourceFilePath = currentSource;
            state.TargetFilePath = currentTarget;
            state.NbFilesLeftToDo = filesLeft;
            state.Progression = progression;

            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Marque un travail comme terminé (par index)
        /// </summary>
        public void CompleteBackup(int workIndex)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.State = BackupState.COMPLETED.ToString();
            state.SourceFilePath = string.Empty;
            state.TargetFilePath = string.Empty;
            state.NbFilesLeftToDo = 0;
            state.Progression = 100;

            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Marque un travail en erreur (par index)
        /// </summary>
        public void ErrorBackup(int workIndex)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.State = BackupState.ERROR.ToString();
            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Marque un travail en pause (par index)
        /// </summary>
        public void PauseBackup(int workIndex)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.State = BackupState.PAUSED.ToString();
            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Reprend un travail (par index)
        /// </summary>
        public void ResumeBackup(int workIndex)
        {
            if (!_workIndexToStateId.TryGetValue(workIndex, out var stateId))
                return;

            var state = _stateService.GetStateById(stateId);
            if (state == null)
                return;

            state.State = BackupState.ACTIVE.ToString();
            _stateService.UpdateState(state);
        }

        /// <summary>
        /// Récupère l'état d'un travail par nom
        /// </summary>
        public StateEntry? GetState(string backupName) => _stateService.GetState(backupName);

        /// <summary>
        /// Récupère tous les états
        /// </summary>
        public List<StateEntry> GetAllStates() => _stateService.GetAllStates();

        /// <summary>
        /// Supprime l'état d'un travail (par index)
        /// ? Appelé lors de la suppression d'un travail
        /// </summary>
        public void RemoveBackupState(int workIndex)
        {
            // Nettoyer le cache
            _workIndexToStateId.Remove(workIndex);
            
            // Supprimer du state.json
            _stateService.RemoveStateByWorkIndex(workIndex);
        }
    }
}


