using Xunit;
using EasySave.Core.Services;
using EasySave.Core.Settings;

namespace EasySave.Tests;

/// <summary>
/// Tests for LargeFileTransferLock threshold logic and mutual exclusion.
/// </summary>
public class LargeFileTransferLockTests
{
    [Fact]
    public void IsLargeFile_AboveThreshold_ReturnsTrue()
    {
        var config = new Config { LargeFileThresholdKB = 100 }; // 100 KB
        var lockObj = new LargeFileTransferLock(config);

        Assert.True(lockObj.IsLargeFile(200 * 1024)); // 200 KB
    }

    [Fact]
    public void IsLargeFile_BelowThreshold_ReturnsFalse()
    {
        var config = new Config { LargeFileThresholdKB = 100 };
        var lockObj = new LargeFileTransferLock(config);

        Assert.False(lockObj.IsLargeFile(50 * 1024)); // 50 KB
    }

    [Fact]
    public void IsLargeFile_ThresholdDisabled_ReturnsFalse()
    {
        var config = new Config { LargeFileThresholdKB = 0 };
        var lockObj = new LargeFileTransferLock(config);

        Assert.False(lockObj.IsLargeFile(10_000_000)); // 10 MB
    }

    [Fact]
    public void ThresholdBytes_ConvertsCorrectly()
    {
        var config = new Config { LargeFileThresholdKB = 512 };
        var lockObj = new LargeFileTransferLock(config);

        Assert.Equal(512 * 1024, lockObj.ThresholdBytes);
    }

    [Fact]
    public void Acquire_SmallFile_ReturnsFalse_NoLock()
    {
        var config = new Config { LargeFileThresholdKB = 100 };
        var lockObj = new LargeFileTransferLock(config);

        var acquired = lockObj.Acquire(10 * 1024); // 10 KB — small

        Assert.False(acquired);
    }

    [Fact]
    public void Acquire_LargeFile_ReturnsTrueAndBlocks()
    {
        var config = new Config { LargeFileThresholdKB = 100 };
        var lockObj = new LargeFileTransferLock(config);

        var acquired = lockObj.Acquire(200 * 1024); // 200 KB — large
        Assert.True(acquired);

        // Second acquire should block — verify with a timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        Assert.Throws<OperationCanceledException>(() => lockObj.Acquire(200 * 1024, cts.Token));

        lockObj.Release();
    }

    [Fact]
    public void Acquire_AfterRelease_Succeeds()
    {
        var config = new Config { LargeFileThresholdKB = 100 };
        var lockObj = new LargeFileTransferLock(config);

        lockObj.Acquire(200 * 1024);
        lockObj.Release();

        var acquired = lockObj.Acquire(200 * 1024);
        Assert.True(acquired);
        lockObj.Release();
    }
}
