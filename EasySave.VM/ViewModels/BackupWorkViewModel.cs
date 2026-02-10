using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Models;

namespace EasySave.VM.ViewModels;

/// <summary>
/// ViewModel pour une sauvegarde individuelle.
/// </summary>
public partial class BackupWorkViewModel : ObservableObject
{
    private readonly BackupWork _model;

    /// <summary>Nom de la sauvegarde.</summary>
    public string Name => _model.Name;

    /// <summary>Chemin source.</summary>
    public string SourcePath => _model.SourcePath;

    /// <summary>Chemin de destination.</summary>
    public string DestinationPath => _model.DestinationPath;

    /// <summary>Type de sauvegarde (Complète/Différentielle).</summary>
    public BackupType Type => _model.Type;

    /// <summary>
    /// Progression de la sauvegarde (0-100).
    /// </summary>
    [ObservableProperty]
    private double _progress;

    /// <summary>
    /// Indique si la sauvegarde est en cours.
    /// </summary>
    [ObservableProperty]
    private bool _isRunning;

    /// <summary>
    /// Statut actuel de la sauvegarde.
    /// </summary>
    [ObservableProperty]
    private string _status = "En attente";

    public BackupWorkViewModel(BackupWork model)
    {
        _model = model;
    }

    /// <summary>
    /// Exécute cette sauvegarde.
    /// </summary>
    [RelayCommand]
    public async Task RunAsync()
    {
        if (IsRunning) return;
        
        IsRunning = true;
        Status = "En cours...";
        Progress = 0;

        try
        {
            await Task.Run(() =>
            {
                _model.Execute();
            });

            Progress = 100;
            Status = "Terminé";
        }
        catch (Exception ex)
        {
            Status = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }
}
