using EasySave.Core.Settings;

namespace EasySave.Core.Services;

/// <summary>
/// Shared gate that enforces priority file ordering across all parallel backup jobs.
/// Non-priority files are blocked until all priority files (across all jobs) are done.
/// Thread-safe singleton — same pattern as LargeFileTransferLock.
/// </summary>
public class PriorityFileGate
{
    private readonly Config _config;
    private readonly object _lock = new();
    private readonly ManualResetEventSlim _gate = new(true); // open by default

    /// <summary>
    /// Tracks how many priority files are still pending across all jobs.
    /// When this reaches 0, the gate opens and non-priority files can proceed.
    /// </summary>
    private int _pendingPriorityCount;

    public PriorityFileGate(Config config)
    {
        _config = config;
    }

    /// <summary>
    /// Whether priority file ordering is enabled (at least one priority extension configured).
    /// </summary>
    public bool IsEnabled => _config.GetPriorityExtensionsList().Length > 0;

    /// <summary>
    /// Checks if a file has a priority extension.
    /// </summary>
    public bool IsPriority(string filePath) => _config.IsPriorityFile(filePath);

    /// <summary>
    /// Registers a set of files for a job. Counts how many are priority
    /// and adds them to the global pending counter. Call once per job before execution.
    /// </summary>
    public void RegisterPendingFiles(string[] files)
    {
        if (!IsEnabled) return;

        int priorityCount = files.Count(f => IsPriority(f));
        if (priorityCount == 0) return;

        lock (_lock)
        {
            _pendingPriorityCount += priorityCount;
            _gate.Reset(); // close gate — there are priority files pending
        }
    }

    /// <summary>
    /// Called after a priority file has been copied. Decrements the counter.
    /// When counter reaches 0, opens the gate for non-priority files.
    /// </summary>
    public void MarkPriorityFileCompleted()
    {
        lock (_lock)
        {
            _pendingPriorityCount = Math.Max(0, _pendingPriorityCount - 1);
            if (_pendingPriorityCount == 0)
                _gate.Set(); // open gate — no more priority files pending
        }
    }

    /// <summary>
    /// Called before copying a non-priority file. Blocks until all priority files
    /// across all jobs are done. Priority files pass through without blocking.
    /// </summary>
    public void WaitIfNonPriority(string filePath, CancellationToken ct = default)
    {
        if (!IsEnabled) return;
        if (IsPriority(filePath)) return;

        // Non-priority file: wait until no priority files are pending
        _gate.Wait(ct);
    }

    /// <summary>
    /// Unregisters all remaining priority files for a cancelled/stopped job.
    /// Prevents a stopped job from permanently blocking non-priority files.
    /// </summary>
    public void UnregisterAll(string[] files)
    {
        if (!IsEnabled) return;

        int priorityCount = files.Count(f => IsPriority(f));
        if (priorityCount == 0) return;

        lock (_lock)
        {
            _pendingPriorityCount = Math.Max(0, _pendingPriorityCount - priorityCount);
            if (_pendingPriorityCount == 0)
                _gate.Set();
        }
    }
}
