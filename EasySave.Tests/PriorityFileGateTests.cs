using Xunit;
using EasySave.Core.Services;
using EasySave.Core.Settings;

namespace EasySave.Tests;

/// <summary>
/// Tests for PriorityFileGate — the global gate that blocks non-priority files
/// until all priority files across all jobs are done.
/// </summary>
public class PriorityFileGateTests
{
    private Config MakeConfig(string priorityExtensions = ".docx,.pdf")
    {
        return new Config { PriorityExtensions = priorityExtensions };
    }

    [Fact]
    public void IsEnabled_WithExtensions_ReturnsTrue()
    {
        var gate = new PriorityFileGate(MakeConfig());
        Assert.True(gate.IsEnabled);
    }

    [Fact]
    public void IsEnabled_Empty_ReturnsFalse()
    {
        var gate = new PriorityFileGate(MakeConfig(""));
        Assert.False(gate.IsEnabled);
    }

    [Fact]
    public void IsPriority_MatchingExtension_ReturnsTrue()
    {
        var gate = new PriorityFileGate(MakeConfig());
        Assert.True(gate.IsPriority(@"C:\data\report.docx"));
    }

    [Fact]
    public void IsPriority_NonMatchingExtension_ReturnsFalse()
    {
        var gate = new PriorityFileGate(MakeConfig());
        Assert.False(gate.IsPriority(@"C:\data\image.png"));
    }

    [Fact]
    public void RegisterPendingFiles_SetsPendingCount()
    {
        var gate = new PriorityFileGate(MakeConfig());
        var files = new[] { @"C:\a.docx", @"C:\b.pdf", @"C:\c.txt" };

        gate.RegisterPendingFiles(files);

        Assert.True(gate.HasPendingPriority); // 2 priority files pending
    }

    [Fact]
    public void MarkPriorityFileCompleted_DecrementsAndOpensGate()
    {
        var gate = new PriorityFileGate(MakeConfig());
        gate.RegisterPendingFiles(new[] { @"C:\a.docx", @"C:\b.txt" }); // 1 priority

        Assert.True(gate.HasPendingPriority);

        gate.MarkPriorityFileCompleted();

        Assert.False(gate.HasPendingPriority);
    }

    [Fact]
    public void WaitIfNonPriority_PriorityFile_PassesThrough()
    {
        var gate = new PriorityFileGate(MakeConfig());
        gate.RegisterPendingFiles(new[] { @"C:\a.docx" });

        // Priority file should not block even if gate is closed
        gate.WaitIfNonPriority(@"C:\other.docx"); // should return immediately
    }

    [Fact]
    public void WaitIfNonPriority_NonPriority_BlocksUntilGateOpens()
    {
        var gate = new PriorityFileGate(MakeConfig());
        gate.RegisterPendingFiles(new[] { @"C:\a.docx" });

        // Non-priority should block — verify with cancellation timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        Assert.Throws<OperationCanceledException>(() =>
            gate.WaitIfNonPriority(@"C:\data.txt", cts.Token));
    }

    [Fact]
    public void WaitIfNonPriority_AfterAllPriorityDone_PassesThrough()
    {
        var gate = new PriorityFileGate(MakeConfig());
        gate.RegisterPendingFiles(new[] { @"C:\a.docx" });

        gate.MarkPriorityFileCompleted(); // all done

        // Non-priority should pass now
        gate.WaitIfNonPriority(@"C:\data.txt"); // should not block
    }

    [Fact]
    public void UnregisterAll_OpensGateWhenCountReachesZero()
    {
        var gate = new PriorityFileGate(MakeConfig());
        var files = new[] { @"C:\a.docx", @"C:\b.pdf" };
        gate.RegisterPendingFiles(files);

        Assert.True(gate.HasPendingPriority);

        gate.UnregisterAll(files);

        Assert.False(gate.HasPendingPriority);
    }

    [Fact]
    public void RegisterPendingFiles_NoPriorityFiles_DoesNothing()
    {
        var gate = new PriorityFileGate(MakeConfig());
        gate.RegisterPendingFiles(new[] { @"C:\a.txt", @"C:\b.png" });

        Assert.False(gate.HasPendingPriority);
    }
}
