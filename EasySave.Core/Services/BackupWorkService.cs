using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services.Logging;
using EasyLog.Services;
using System.Diagnostics;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Service de gestion des travaux de sauvegarde.
    /// 
    /// ARCHITECTURE PROPRE:
    /// - BackupWork, BackupWorkList: PURES (pas de logger)
    /// - BackupWorkService: Émet des événements lors de transferts
    /// - Observers: Écoutent les événements et logguent
    /// </summary>
    public class BackupWorkService
    {
        private readonly BackupWorkList _workList;
        private readonly ILocalizationService _localization;
        private readonly EasyLogger _logger;
        
        // ? Événements pour notifier les observateurs
        public event EventHandler<FileTransferredEventArgs>? FileTransferred;
        public event EventHandler<FileTransferErrorEventArgs>? FileTransferError;
        
        // ? Liste des observers (TEMPS RÉEL SEULEMENT)
        private readonly List<IBackupEventObserver> _observers = new();

        public BackupWorkService(ILocalizationService localization, BackupWorkList workList, EasyLogger? logger = null)
        {
            _localization = localization;
            _workList = workList;
            _logger = logger ?? new EasyLogger();
        }

        // ============ OBSERVER PATTERN (TEMPS RÉEL SEULEMENT) ============

        public void AddObserver(IBackupEventObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void RemoveObserver(IBackupEventObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(Action<IBackupEventObserver> action)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    action(observer);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"??  Erreur observer: {ex.Message}");
                }
            }
        }

        // ============ ÉVÉNEMENTS POUR LOGGING ============

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

        /// <summary>
        /// Ajoute un nouveau travail de sauvegarde.
        /// </summary>
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
        /// Récupère tous les travaux.
        /// </summary>
        public List<BackupWork> GetAllWorks() => _workList.GetAllWorks();

        /// <summary>
        /// Récupère un travail par index (basé sur 0 ou 1 selon utilisation).
        /// </summary>
        public BackupWork? GetWorkByIndex(int index)
        {
            var works = _workList.GetAllWorks();
            if (index >= 0 && index < works.Count)
                return works[index];
            return null;
        }

        /// <summary>
        /// Supprime un travail par index.
        /// ? Nettoie aussi l'état dans state.json
        /// </summary>
        public bool RemoveWorkByIndex(int index)
        {
            try
            {
                var result = _workList.RemoveBackupWorkById(index);
                
                // ? Nettoyer l'état du travail supprimé
                if (result)
                {
                    _logger.RemoveBackupState(index);
                }
                
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Modifie un travail existant.
        /// </summary>
        public bool ModifyWork(int index, string? newName = null, string? newSourcePath = null, 
            string? newDestinationPath = null, string? newType = null)
        {
            var works = _workList.GetAllWorks();
            if (index < 0 || index >= works.Count)
                return false;

            var original = works[index];
            
            // Créer une copie avec les modifications
            var modified = new BackupWork(
                newName ?? original.Name,
                newSourcePath ?? original.SourcePath,
                newDestinationPath ?? original.DestinationPath,
                newType != null ? ParseBackupType(newType) : original.GetBackupType()
            );

            var result = _workList.EditBackupWork(original, modified);
            return result != null;
        }

        /// <summary>
        /// Exécute un travail par index.
        /// Les événements sont capturés et émis automatiquement.
        /// ? Passe l'index au logger (pas le nom) pour éviter les doublons avec même nom
        /// </summary>
        public void ExecuteWork(int index)
        {
            try
            {
                var work = GetWorkByIndex(index);
                if (work == null)
                    return;

                // ?? Notifier: Début (Temps réel)
                var files = Directory.GetFiles(work.SourcePath, "*", SearchOption.AllDirectories);
                var totalSize = files.Sum(f => new FileInfo(f).Length);
                NotifyObservers(o => o.OnBackupStarted(work.Name, files.Length, totalSize));

                // ? Passer l'INDEX au logger (pas le nom!)
                _logger.StartBackup(index, work.Name, files.Length, totalSize);

                // ? CONNECTER LES ÉVÉNEMENTS AVANT L'EXÉCUTION
                work.FileTransferred += (sender, args) => 
                {
                    if (args is FileCopiedEventArgs fileArgs)
                    {
                        OnFileTransferred(work.Name, fileArgs.SourceFile, fileArgs.DestFile, fileArgs.FileSize, fileArgs.TransferTimeMs);
                    }
                };
                
                work.FileTransferError += (sender, args) => 
                {
                    if (args is FileCopyErrorEventArgs errorArgs)
                    {
                        OnFileTransferError(work.Name, errorArgs.SourceFile, errorArgs.DestFile, errorArgs.FileSize, errorArgs.Exception);
                    }
                };
                
                // ? Capturer la progression (pour remplir SourceFilePath et TargetFilePath)
                work.FileProgress += (sender, args) =>
                {
                    if (args is FileProgressEventArgs progressArgs)
                    {
                        long filesLeft = files.Length - (int)(progressArgs.CurrentProgress / 100 * files.Length);
                        long sizeLeft = (long)(totalSize * (100 - progressArgs.CurrentProgress) / 100);
                        
                        // ? Passer l'INDEX à UpdateProgress aussi!
                        _logger.UpdateProgress(index, progressArgs.SourceFile, progressArgs.DestFile, filesLeft, sizeLeft, progressArgs.CurrentProgress);
                        
                        NotifyObservers(o => o.OnProgressUpdated(work.Name, progressArgs.SourceFile, progressArgs.DestFile, filesLeft, sizeLeft, progressArgs.CurrentProgress));
                    }
                };

                // ? MAINTENANT exécuter (les événements seront émis)
                _workList.ExecuteBackupWork(index);

                // ?? Notifier: Fin (Temps réel)
                _logger.CompleteBackup(index);
                NotifyObservers(o => o.OnBackupCompleted(work.Name));
            }
            catch (Exception ex)
            {
                var work = GetWorkByIndex(index);
                if (work != null)
                {
                    _logger.ErrorBackup(index);
                    NotifyObservers(o => o.OnBackupError(work.Name, ex));
                }
            }
        }


        /// <summary>
        /// Exécute tous les travaux séquentiellement.
        /// </summary>
        public void ExecuteAllWorks()
        {
            _workList.ExecuteAllBackupWorks();
        }

        /// <summary>
        /// Retourne le nombre total de travaux.
        /// </summary>
        public int GetWorkCount() => _workList.GetCount();

        /// <summary>
        /// Convertit une string en BackupType.
        /// </summary>
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
        /// Retourne le nom localisé d'un type de sauvegarde.
        /// </summary>
        public string GetLocalizedBackupTypeName(BackupType type)
        {
            return type == BackupType.FULL_BACKUP 
                ? _localization.Get("backup_types.full")
                : _localization.Get("backup_types.diff");
        }
    }
}
