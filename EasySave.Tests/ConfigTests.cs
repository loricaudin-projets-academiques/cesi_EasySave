using Xunit;
using EasySave.Core.Settings;

namespace EasySave.Tests;

/// <summary>
/// Tests for Config extension management (encrypt, priority, business software).
/// These are pure in-memory tests — no file I/O.
/// </summary>
public class ConfigTests
{
    // ===== Encrypt Extensions =====

    [Fact]
    public void AddEncryptExtension_WithDot_AddsNormalized()
    {
        var config = new Config();
        var result = config.AddEncryptExtension(".PDF");

        Assert.True(result);
        Assert.Contains(".pdf", config.GetEncryptExtensionsList());
    }

    [Fact]
    public void AddEncryptExtension_WithoutDot_AddsDotPrefix()
    {
        var config = new Config();
        var result = config.AddEncryptExtension("docx");

        Assert.True(result);
        Assert.Contains(".docx", config.GetEncryptExtensionsList());
    }

    [Fact]
    public void AddEncryptExtension_Duplicate_ReturnsFalse()
    {
        var config = new Config();
        config.AddEncryptExtension(".pdf");
        var result = config.AddEncryptExtension("PDF");

        Assert.False(result);
        Assert.Single(config.GetEncryptExtensionsList());
    }

    [Fact]
    public void RemoveEncryptExtension_Existing_ReturnsTrue()
    {
        var config = new Config();
        config.AddEncryptExtension(".txt");
        var result = config.RemoveEncryptExtension("txt");

        Assert.True(result);
        Assert.Empty(config.GetEncryptExtensionsList());
    }

    [Fact]
    public void RemoveEncryptExtension_NotFound_ReturnsFalse()
    {
        var config = new Config();
        Assert.False(config.RemoveEncryptExtension(".xyz"));
    }

    [Fact]
    public void ShouldEncrypt_MatchingExtension_ReturnsTrue()
    {
        var config = new Config();
        config.AddEncryptExtension(".txt");

        Assert.True(config.ShouldEncrypt(@"C:\docs\readme.txt"));
        Assert.True(config.ShouldEncrypt(@"C:\docs\README.TXT"));
    }

    [Fact]
    public void ShouldEncrypt_NoMatch_ReturnsFalse()
    {
        var config = new Config();
        config.AddEncryptExtension(".txt");

        Assert.False(config.ShouldEncrypt(@"C:\docs\image.png"));
    }

    [Fact]
    public void ShouldEncrypt_EmptyConfig_ReturnsFalse()
    {
        var config = new Config();
        Assert.False(config.ShouldEncrypt(@"C:\docs\readme.txt"));
    }

    // ===== Priority Extensions =====

    [Fact]
    public void AddPriorityExtension_NormalizesCaseAndDot()
    {
        var config = new Config();
        config.AddPriorityExtension("DOCX");

        Assert.Contains(".docx", config.GetPriorityExtensionsList());
    }

    [Fact]
    public void IsPriorityFile_MatchingExtension_ReturnsTrue()
    {
        var config = new Config();
        config.AddPriorityExtension(".docx");

        Assert.True(config.IsPriorityFile(@"C:\docs\report.docx"));
    }

    [Fact]
    public void IsPriorityFile_EmptyConfig_ReturnsFalse()
    {
        var config = new Config();
        Assert.False(config.IsPriorityFile(@"C:\docs\report.docx"));
    }

    // ===== Business Software =====

    [Fact]
    public void SetBusinessSoftware_StripsExeAndTrims()
    {
        var config = new Config();
        config.SetBusinessSoftware("  Calculator.exe  ");

        Assert.Equal("Calculator", config.BusinessSoftware);
    }

    [Fact]
    public void HasBusinessSoftware_WhenSet_ReturnsTrue()
    {
        var config = new Config();
        config.SetBusinessSoftware("notepad");

        Assert.True(config.HasBusinessSoftware());
    }

    [Fact]
    public void HasBusinessSoftware_WhenEmpty_ReturnsFalse()
    {
        var config = new Config();
        Assert.False(config.HasBusinessSoftware());
    }

    [Fact]
    public void ClearBusinessSoftware_ResetsToEmpty()
    {
        var config = new Config();
        config.SetBusinessSoftware("notepad");
        config.ClearBusinessSoftware();

        Assert.False(config.HasBusinessSoftware());
        Assert.Equal(string.Empty, config.BusinessSoftware);
    }
}
