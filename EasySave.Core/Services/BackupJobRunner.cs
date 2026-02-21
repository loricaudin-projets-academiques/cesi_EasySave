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
    /// Pause this job (effective after current chunk finishes).
    /// </summary>
    public void Pause()
    {
        if (State != JobState.Running) return;
        _pauseGate.Reset();
        SetState(JobState.Paused);
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
        if (State != JobState.Running && State != JobState.Paused) return;
        _cts.Cancel();
        _pauseGate.Set(); // unblock if paused so cancellation can propagate
        SetState(JobState.Stopped);
    }

    /// <summary>
    /// Runs the backup synchronously (call via Task.Run for parallel).
    /// Wires pause/cancel into the BackupWork, then delegates to BackupWorkService.
    /// </summary>
    public void Run(Func<bool>? businessSoftwareChecker)
    {
        if (State != JobState.Idle) return;

        SetState(JobState.Running);

        // Combined pause checker: manual pause OR business software running
        _work.SetPauseChecker(() =>
        {
            // If manually paused, block here
            if (!_pauseGate.IsSet)
            {
                _pauseGate.Wait(_cts.Token);
                return false; // just resumed, don't report as "still paused"
            }
            // Business software check
            return businessSoftwareChecker != null && businessSoftwareChecker();
        });

        _work.SetCancellationToken(_cts.Token);

        // Subscribe to progress
        _work.FileProgress += OnFileProgress;
        _work.Paused += OnWorkPaused;
        _work.Resumed += OnWorkResumed;

        try
        {
            _service.ExecuteWork(_index);
            if (State == JobState.Running)
                SetState(JobState.Done);
            Progress = 100;
            ProgressChanged?.Invoke(this, 100);
        }
        catch (OperationCanceledException)
        {
            SetState(JobState.Stopped);
        }
        catch (Exception)
        {
            SetState(JobState.Error);
        }
        finally
        {
            _work.FileProgress -= OnFileProgress;
            _work.Paused -= OnWorkPaused;
            _work.Resumed -= OnWorkResumed;
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

    private void SetState(JobState newState)
    {
        if (State == newState) return;
        State = newState;
        StateChanged?.Invoke(this);
    }
}
