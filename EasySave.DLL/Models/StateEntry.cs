using System;

namespace EasyLog.Models
{
    /// <summary>
    /// État d'un travail de sauvegarde en temps réel
    /// </summary>
    public enum BackupState
    {
        INACTIVE,
        ACTIVE,
        PAUSED,
        COMPLETED,
        STOPPED,
        ERROR
    }

    /// <summary>
    /// Représente l'état d'un travail de sauvegarde à un instant T
    /// Mis à jour en temps réel durant l'exécution
    /// </summary>
    public class StateEntry
    {
        /// <summary>
        /// ID unique du travail (GUID) - Évite les collisions de noms
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 
        /// </summary>
        public string MachineName { get; set; } = Environment.MachineName;

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; } = Environment.UserName;

        /// <summary>
        /// Index du travail de sauvegarde dans BackupWorkList
        /// ? Utilisé pour reconnaître le même travail entre les exécutions
        /// </summary>
        public int WorkIndex { get; set; } = -1;

        /// <summary>
        /// Nom du travail de sauvegarde
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Chemin complet du fichier en cours de sauvegarde (format UNC)
        /// Vide si pas actif
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Chemin complet de la destination en cours de sauvegarde (format UNC)
        /// Vide si pas actif
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// État du travail (INACTIVE, ACTIVE, PAUSED, COMPLETED, ERROR)
        /// </summary>
        public string State { get; set; } = BackupState.INACTIVE.ToString();

        /// <summary>
        /// Nombre total de fichiers à copier
        /// </summary>
        public long TotalFilesToCopy { get; set; }

        /// <summary>
        /// Taille totale des fichiers à copier (en octets)
        /// </summary>
        public long TotalFilesSize { get; set; }

        /// <summary>
        /// Nombre de fichiers restant à faire
        /// </summary>
        public long NbFilesLeftToDo { get; set; }

        /// <summary>
        /// Pourcentage de progression (0-100)
        /// </summary>
        public double Progression { get; set; }

        /// <summary>
        /// Horodatage de la dernière action
        /// </summary>
        public string LastActionTime { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        public override string ToString()
        {
            return $"{Name} | {State} | {Progression:F1}% | Fichiers: {NbFilesLeftToDo}/{TotalFilesToCopy}";
        }
    }
}


