namespace EasySave.Core.Models
{
    /// <summary>
    /// Types of backup operations.
    /// </summary>
    public enum BackupType
    {
        /// <summary>Full backup - copies all files.</summary>
        FULL_BACKUP,
        
        /// <summary>Differential backup - copies only modified files.</summary>
        DIFFERENTIAL_BACKUP
    }
}

