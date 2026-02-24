using EasySave.Core.Models;
using EasySave.Core.ProgressBar;
using EasySave.Core.Services.Logging;

namespace EasySave.Core.Services;

/// <summary>
/// Possible states for a backup job.
/// </summary>
public enum JobState
{
    Idle,
    Running,
    Pausing,  // Pause requested, finishing current file
    Paused,
    Stopped,
    Done,
    Error
}

/// <summary>
/// Wraps a single BackupWork execution with pause/resume/stop controls
/// and progress tracking. One runner per launched job.
/// </summary>
public class BackupJobRunner
{
    private readonly BackupWorkService _service;
    private readonly int _index;
    private readonly BackupWork _work;

    private readonly ManualResetEventSlim _pauseGate = new(true); // signaled = not paused
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Current state of this job.</summary>
    public JobState State { get; private set; } = JobState.Idle;

    /// <summary>Progress 0-100.</summary>
    public double Progress { get; private set; }

    /// <summary>Name of the backup work.</summary>
    public string Name => _work.Name;

    /// <summary>Index in the work list.</summary>
    public int Index => _index;

    /// <summary>Fired when State changes.</summary>
    public event Action<BackupJobRunner>? StateChanged;

    /// <summary>Fired when Progress changes.</summary>
    public event Action<BackupJobRunner, double>? ProgressChanged;

    public BackupJobRunner(BackupWorkService service, int index, BackupWork work)
    {
        _service = service;
        _index = index;
        _work = work;
    }

    /// <summary>
    /// Pause this job (effective after the current file finishes transferring).
    /// Sets state to Pausing immediately so the UI can inform the user.
    /// </summary>
    public void Pause()
    {
        if (State != JobState.Running) return;
        _pauseGate.Reset();          // signal the gate — will be checked between files
        SetState(JobState.Pausing);   // UI shows "pausing after current file…"
    }

    /// <summary>
    /// Resume this job from pause.
    /// </summary>
    public void Resume()
    {
        if (State != JobState.Paused) return;
        SetState(JobState.Running);
        _pauseGate.Set();
    }

    /// <summary>
    /// Stop this job immediately (cancels after current chunk).
    /// </summary>
    public void Stop()
    {
        if (State != JobState.Running && State != JobState.Paused && State != JobState.Pausing) return;
        _cts.Cancel();
        _pauseGate.Set(); // unblock if paused so cancellation can propagate
        Progress = 0;
        SetState(JobState.Stopped);
        ProgressChanged?.Invoke(this, 0);
    }

    /// <summary>
    /// Runs the backup synchronously (call via Task.Run for parallel).
    /// Wires pause/cancel into the BackupWork, then delegates to BackupWorkService.
    /// </summary>
    public void Run(Func<bool>? businessSoftwareChecker, LargeFileTransferLock? largeFileLock = null, PriorityFileGate? priorityGate = null)
    {
        if (State != JobState.Idle) return;

        SetState(JobState.Running);

        // Business software pause checker (between chunks, mid-file)
        if (businessSoftwareChecker != null)
            _work.SetPauseChecker(businessSoftwareChecker);

        // Manual pause gate (between files, after current file finishes)
        _work.SetManualPauseGate(_pauseGate);

        _work.SetCancellationToken(_cts.Token);
        _work.SetLargeFileLock(largeFileLock);
        _work.SetPriorityGate(priorityGate);

        // Register all files with the priority gate so it knows how many priority files are pending
        string[]? allFiles = null;
        if (priorityGate != null && priorityGate.IsEnabled)
        {
            allFiles = Directory.GetFiles(_work.SourcePath, "*", SearchOption.AllDirectories);
            priorityGate.RegisterPendingFiles(allFiles);
        }

        // Subscribe to progress and pause events
        _work.FileProgress += OnFileProgress;
        _work.Paused += OnWorkPaused;
        _work.Resumed += OnWorkResumed;
        _work.ManualPaused += OnManualPaused;
        _work.ManualResumed += OnManualResumed;

        try
        {
            _service.ExecuteWork(_index);
            if (State == JobState.Running)
            {
                SetState(JobState.Done);
                Progress = 100;
                ProgressChanged?.Invoke(this, 100);
            }
        }
        catch (OperationCanceledException)
        {
            if (State != JobState.Stopped)
                SetState(JobState.Stopped);
            Progress = 0;
            ProgressChanged?.Invoke(this, 0);

            // Unregister remaining priority files to avoid blocking other jobs
            if (allFiles != null)
                priorityGate!.UnregisterAll(allFiles);
        }
        catch (Exception)
        {
            SetState(JobState.Error);
            Progress = 0;
            ProgressChanged?.Invoke(this, 0);

            // Unregister remaining priority files to avoid blocking other jobs
            if (allFiles != null)
                priorityGate!.UnregisterAll(allFiles);
        }
        finally
        {
            _work.FileProgress -= OnFileProgress;
            _work.Paused -= OnWorkPaused;
            _work.Resumed -= OnWorkResumed;
            _work.ManualPaused -= OnManualPaused;
            _work.ManualResumed -= OnManualResumed;
        }
    }

    private void OnFileProgress(object? sender, EventArgs args)
    {
        if (args is FileProgressEventArgs p)
        {
            Progress = p.CurrentProgress;
            ProgressChanged?.Invoke(this, p.CurrentProgress);
        }
    }

    // Business software pause (between chunks)
    private void OnWorkPaused()
    {
        if (State == JobState.Running)
            SetState(JobState.Paused);
    }

    private void OnWorkResumed()
    {
        if (State == JobState.Paused)
            SetState(JobState.Running);
    }

    // Manual pause (between files — after current file finishes)
    private void OnManualPaused()
    {
        if (State == JobState.Pausing)
            SetState(JobState.Paused);
    }

    private void OnManualResumed()
    {
        if (State == JobState.Paused)
            SetState(JobState.Running);
    }

    private void SetState(JobState newState)
    {
        if (State == newState) return;
        State = newState;
        StateChanged?.Invoke(this);
    }
}
