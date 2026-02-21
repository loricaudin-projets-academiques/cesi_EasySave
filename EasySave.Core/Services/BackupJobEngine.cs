using EasySave.Core.Models;

namespace EasySave.Core.Services;

/// <summary>
/// Orchestrator that launches backup jobs in parallel, exposes per-job
/// and global Pause/Resume/Stop controls, and integrates business software detection.
/// </summary>
public class BackupJobEngine
{
    private readonly BackupWorkService _service;
    private readonly BusinessSoftwareService? _businessService;
    private readonly List<BackupJobRunner> _runners = new();
    private readonly object _lock = new();

    /// <summary>Fired when any runner's state changes.</summary>
    public event Action<BackupJobRunner>? JobStateChanged;

    /// <summary>Fired when any runner's progress changes.</summary>
    public event Action<BackupJobRunner, double>? JobProgressChanged;

    /// <summary>Fired when all jobs in a batch are finished (Done/Stopped/Error).</summary>
    public event Action? AllJobsCompleted;

    public BackupJobEngine(BackupWorkService service, BusinessSoftwareService? businessService)
    {
        _service = service;
        _businessService = businessService;
    }

    /// <summary>
    /// Gets all active runners for the current batch.
    /// </summary>
    public IReadOnlyList<BackupJobRunner> Runners
    {
        get { lock (_lock) return _runners.ToList().AsReadOnly(); }
    }

    /// <summary>
    /// Launches the given work indices in parallel.
    /// Returns a Task that completes when all jobs finish.
    /// </summary>
    public async Task RunJobsAsync(IEnumerable<int> indices)
    {
        lock (_lock) _runners.Clear();

        var works = new List<(int index, BackupWork work)>();
        foreach (var i in indices)
        {
            var w = _service.GetWorkByIndex(i);
            if (w != null) works.Add((i, w));
        }

        if (works.Count == 0) return;

        Func<bool>? bizChecker = _businessService != null
            ? () => _businessService.IsRunning()
            : null;

        var tasks = new List<Task>();

        foreach (var (index, work) in works)
        {
            var runner = new BackupJobRunner(_service, index, work);
            runner.StateChanged += OnRunnerStateChanged;
            runner.ProgressChanged += OnRunnerProgressChanged;

            lock (_lock) _runners.Add(runner);

            tasks.Add(Task.Run(() => runner.Run(bizChecker)));
        }

        await Task.WhenAll(tasks);
        AllJobsCompleted?.Invoke();
    }

    /// <summary>Get a runner by its work index.</summary>
    public BackupJobRunner? GetRunner(int workIndex)
    {
        lock (_lock) return _runners.FirstOrDefault(r => r.Index == workIndex);
    }

    /// <summary>Pause all running jobs.</summary>
    public void PauseAll()
    {
        foreach (var r in Runners)
            r.Pause();
    }

    /// <summary>Resume all paused jobs.</summary>
    public void ResumeAll()
    {
        foreach (var r in Runners)
            r.Resume();
    }

    /// <summary>Stop all active jobs.</summary>
    public void StopAll()
    {
        foreach (var r in Runners)
            r.Stop();
    }

    /// <summary>Global progress: average of all runners.</summary>
    public double GlobalProgress
    {
        get
        {
            var runners = Runners;
            if (runners.Count == 0) return 0;
            return runners.Average(r => r.Progress);
        }
    }

    /// <summary>True if any runner is Running or Paused.</summary>
    public bool IsAnyActive => Runners.Any(r => r.State == JobState.Running || r.State == JobState.Paused);

    private void OnRunnerStateChanged(BackupJobRunner runner)
    {
        JobStateChanged?.Invoke(runner);
    }

    private void OnRunnerProgressChanged(BackupJobRunner runner, double progress)
    {
        JobProgressChanged?.Invoke(runner, progress);
    }
}
