using Xunit;
using EasySave.Core.Localization;

namespace EasySave.Tests;

/// <summary>
/// Tests for the LocalizationService — key resolution, language switching,
/// parameterized strings, and missing key handling.
/// </summary>
public class LocalizationServiceTests
{
    [Fact]
    public void Get_ExistingKey_ReturnsTranslation()
    {
        var svc = new LocalizationService(Language.English);
        var result = svc.Get("gui.status.ready");

        Assert.Equal("Ready", result);
    }

    [Fact]
    public void Get_MissingKey_ReturnsMissingMarker()
    {
        var svc = new LocalizationService(Language.English);
        var result = svc.Get("this.key.does.not.exist");

        Assert.StartsWith("[MISSING:", result);
    }

    [Fact]
    public void Get_WithArgs_FormatsString()
    {
        var svc = new LocalizationService(Language.English);
        var result = svc.Get("gui.status.loaded", 5);

        Assert.Contains("5", result);
    }

    [Fact]
    public void SetLanguage_SwitchesToFrench()
    {
        var svc = new LocalizationService(Language.English);
        svc.SetLanguage(Language.French);

        Assert.Equal(Language.French, svc.CurrentLanguage);

        var result = svc.Get("gui.status.ready");
        Assert.Equal("Prêt", result);
    }

    [Fact]
    public void SetLanguage_BackToEnglish_Works()
    {
        var svc = new LocalizationService(Language.French);
        svc.SetLanguage(Language.English);

        var result = svc.Get("gui.status.ready");
        Assert.Equal("Ready", result);
    }

    [Fact]
    public void DefaultLanguage_IsFrench()
    {
        var svc = new LocalizationService();
        Assert.Equal(Language.French, svc.CurrentLanguage);
    }

    [Fact]
    public void AvailableLanguages_ContainsBothLanguages()
    {
        var svc = new LocalizationService();

        Assert.Contains(Language.French, svc.AvailableLanguages);
        Assert.Contains(Language.English, svc.AvailableLanguages);
    }
}
