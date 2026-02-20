using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.VM.Services;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

public partial class BackupEditorViewModel : ObservableObject
{
    private readonly BackupWorkService _backupService;
    private readonly ILocalizationService _localization;
    private readonly IShellNavigationService _navigation;
    private readonly IFolderPickerService _folders;
    private readonly IAppEvents _events;

    private int? _editIndex;
    private bool _isEditMode;

    public ObservableCollection<BackupTypeOption> AvailableBackupTypes { get; } = new();

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _sourcePath = string.Empty;
    [ObservableProperty] private string _destinationPath = string.Empty;
    [ObservableProperty] private BackupTypeOption? _selectedBackupType;

    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private string _saveButtonText = string.Empty;
    [ObservableProperty] private string _cancelButtonText = string.Empty;
    [ObservableProperty] private string _browseButtonText = string.Empty;
    [ObservableProperty] private string _nameLabel = string.Empty;
    [ObservableProperty] private string _sourceLabel = string.Empty;
    [ObservableProperty] private string _destinationLabel = string.Empty;
    [ObservableProperty] private string _typeLabel = string.Empty;

    public BackupEditorViewModel(
        BackupWorkService backupService,
        ILocalizationService localization,
        IShellNavigationService navigation,
        IFolderPickerService folders,
        IAppEvents events)
    {
        _backupService = backupService;
        _localization = localization;
        _navigation = navigation;
        _folders = folders;
        _events = events;

        RefreshLocalizedTexts();
        _events.LocalizationChanged += (_, __) => RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        AvailableBackupTypes.Clear();
        AvailableBackupTypes.Add(new BackupTypeOption("full", _localization.Get("backup_types.full")));
        AvailableBackupTypes.Add(new BackupTypeOption("diff", _localization.Get("backup_types.diff")));

        // Try to preserve current selection
        var current = SelectedBackupType?.Value;
        SelectedBackupType = AvailableBackupTypes.FirstOrDefault(t => t.Value == current) ?? AvailableBackupTypes.FirstOrDefault();

        SaveButtonText = _localization.Get("gui.buttons.save");
        CancelButtonText = _localization.Get("gui.buttons.cancel");
        BrowseButtonText = _localization.Get("gui.buttons.browse");
        NameLabel = _localization.Get("gui.pages.editor_name");
        SourceLabel = _localization.Get("gui.pages.editor_source");
        DestinationLabel = _localization.Get("gui.pages.editor_destination");
        TypeLabel = _localization.Get("gui.pages.editor_type");

        Title = _isEditMode ? _localization.Get("gui.editor.title_edit") : _localization.Get("gui.editor.title_create");
    }

    public void BeginCreate()
    {
        _editIndex = null;
        _isEditMode = false;
        Title = _localization.Get("gui.editor.title_create");
        Name = string.Empty;
        SourcePath = string.Empty;
        DestinationPath = string.Empty;
        SelectedBackupType = AvailableBackupTypes.FirstOrDefault(t => t.Value == "full");
        StatusMessage = _localization.Get("gui.status.ready");
    }

    public void BeginEdit(int index)
    {
        _editIndex = index;
        _isEditMode = true;
        var work = _backupService.GetWorkByIndex(index);

        Title = _localization.Get("gui.editor.title_edit");
        Name = work?.Name ?? string.Empty;
        SourcePath = work?.SourcePath ?? string.Empty;
        DestinationPath = work?.DestinationPath ?? string.Empty;

        var type = work?.GetBackupType() ?? BackupType.FULL_BACKUP;
        SelectedBackupType = AvailableBackupTypes.FirstOrDefault(t => t.Value == (type == BackupType.DIFFERENTIAL_BACKUP ? "diff" : "full"));

        StatusMessage = _localization.Get("gui.status.ready");
    }

    [RelayCommand]
    private void BrowseSource()
    {
        var picked = _folders.PickFolder(SourcePath);
        if (!string.IsNullOrWhiteSpace(picked))
            SourcePath = picked;
    }

    [RelayCommand]
    private void BrowseDestination()
    {
        var picked = _folders.PickFolder(DestinationPath);
        if (!string.IsNullOrWhiteSpace(picked))
            DestinationPath = picked;
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigation.RequestNavigate(NavigationTarget.Backups);
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = _localization.Get("gui.editor.error_name_required");
                return;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                StatusMessage = _localization.Get("gui.editor.error_source_required");
                return;
            }

            if (string.IsNullOrWhiteSpace(DestinationPath))
            {
                StatusMessage = _localization.Get("gui.editor.error_destination_required");
                return;
            }

            var type = SelectedBackupType?.Value ?? "full";

            if (_editIndex is int idx)
            {
                var ok = _backupService.ModifyWork(idx, newName: Name, newSourcePath: SourcePath, newDestinationPath: DestinationPath, newType: type);
                if (!ok)
                {
                    StatusMessage = _localization.Get("gui.editor.error_save_failed");
                    return;
                }

                StatusMessage = _localization.Get("gui.editor.saved");
            }
            else
            {
                _backupService.AddWork(Name, SourcePath, DestinationPath, type);
                StatusMessage = _localization.Get("gui.editor.saved");
            }

            _navigation.RequestNavigate(NavigationTarget.Backups);
        }
        catch (Exception ex)
        {
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        }
    }
}

