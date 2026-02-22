using EasySave.Core.Settings;

namespace EasySave.Core.Services;

/// <summary>
/// Ensures that only one large file (> threshold) is transferred at a time
/// across all parallel backup jobs. Small files are not affected.
/// Thread-safe singleton gate using SemaphoreSlim(1,1).
/// </summary>
public class LargeFileTransferLock
{
    private readonly Config _config;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LargeFileTransferLock(Config config)
    {
        _config = config;
    }

    /// <summary>
    /// Returns the threshold in bytes. 0 means disabled.
    /// </summary>
    public long ThresholdBytes => _config.LargeFileThresholdKB * 1024;

    /// <summary>
    /// Returns true if the given file size exceeds the threshold (and threshold is enabled).
    /// </summary>
    public bool IsLargeFile(long fileSizeBytes)
    {
        var threshold = ThresholdBytes;
        return threshold > 0 && fileSizeBytes > threshold;
    }

    /// <summary>
    /// Acquires the lock if the file is large. Returns true if lock was acquired.
    /// Must call Release() after transfer if this returns true.
    /// Supports cancellation.
    /// </summary>
    public bool Acquire(long fileSizeBytes, CancellationToken ct = default)
    {
        if (!IsLargeFile(fileSizeBytes))
            return false;

        _semaphore.Wait(ct);
        return true;
    }

    /// <summary>
    /// Releases the lock after a large file transfer.
    /// </summary>
    public void Release()
    {
        _semaphore.Release();
    }
}
