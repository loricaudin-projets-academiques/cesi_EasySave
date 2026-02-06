using System;

namespace EasyLog.Models
{
    /// <summary>
    /// Représente une entrée du log journalier
    /// Enregistre les actions de transfert de fichiers durant les sauvegardes
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Nom du travail de sauvegarde
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Chemin complet du fichier source (format UNC)
        /// Ex: \\serveur\share\dossier\fichier.txt
        /// </summary>
        public string FileSource { get; set; } = string.Empty;

        /// <summary>
        /// Chemin complet du fichier destination (format UNC)
        /// Ex: \\serveur\share\backup\dossier\fichier.txt
        /// </summary>
        public string FileTarget { get; set; } = string.Empty;

        /// <summary>
        /// Taille du fichier en octets
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Temps de transfert du fichier en millisecondes
        /// Négatif si erreur
        /// </summary>
        public double FileTransferTime { get; set; }

        /// <summary>
        /// Horodatage de l'action (format: dd/MM/yyyy HH:mm:ss)
        /// </summary>
        public string Time { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        public override string ToString()
        {
            return $"{Name} | {FileSource} ? {FileTarget} | {FileSize} bytes | {FileTransferTime}ms";
        }
    }
}
