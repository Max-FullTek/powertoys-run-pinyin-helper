using Community.PowerToys.Run.Plugin.PinyinHelper;

namespace PinyinHelper.Tests;

public sealed class PinyinLookupServiceTests
{
    private readonly PinyinLookupService _service = new();

    [Fact]
    public void Convert_ReturnsEmptyResult_ForWhitespace()
    {
        var result = _service.Convert("  ");

        Assert.Equal(string.Empty, result.Primary);
        Assert.Equal(string.Empty, result.Details);
        Assert.Empty(result.Tokens);
    }

    [Fact]
    public void Convert_ReturnsPrimaryPinyinAndBreakdownTokens_ForTraditionalChinese()
    {
        var result = _service.Convert("今天");

        Assert.Equal("jin tian", result.Primary);
        Assert.Collection(
            result.Tokens,
            token =>
            {
                Assert.Equal("今", token.Text);
                Assert.Equal("jin", token.Primary);
                Assert.Contains("jin", token.Alternatives);
            },
            token =>
            {
                Assert.Equal("天", token.Text);
                Assert.Equal("tian", token.Primary);
                Assert.Contains("tian", token.Alternatives);
            });
    }

    [Fact]
    public void Convert_PreservesNonChineseCharacters_AsSearchableLiterals()
    {
        var result = _service.Convert("A今!");

        Assert.Equal("A jin !", result.Primary);
        Assert.Collection(
            result.Tokens,
            token =>
            {
                Assert.Equal("A", token.Text);
                Assert.Equal("A", token.Primary);
            },
            token =>
            {
                Assert.Equal("今", token.Text);
                Assert.Equal("jin", token.Primary);
            },
            token =>
            {
                Assert.Equal("!", token.Text);
                Assert.Equal("!", token.Primary);
            });
    }

    [Fact]
    public void Convert_IncludesAlternativeReadings_ForPolyphonicCharacters()
    {
        var result = _service.Convert("重");

        var token = Assert.Single(result.Tokens);
        Assert.Equal("重", token.Text);
        Assert.Contains("zhong", token.Alternatives);
        Assert.Contains("chong", token.Alternatives);
        Assert.Contains("重:", result.Details);
        Assert.Contains("zhong", result.Details);
        Assert.Contains("chong", result.Details);
    }

    [Fact]
    public void Convert_PrefersVSpelling_WhenUmlautAndPlainUReadingsCollide()
    {
        var result = _service.Convert("绿");

        var token = Assert.Single(result.Tokens);
        Assert.Equal("lv", token.Primary);
        Assert.Contains("lu", token.Alternatives);
    }

    [Fact]
    public void ContainsChinese_ReturnsTrue_WhenInputIncludesChinese()
    {
        Assert.True(PinyinLookupService.ContainsChinese("A今!"));
    }

    [Fact]
    public void ContainsChinese_ReturnsFalse_ForRawZhuyinKeyboardInput()
    {
        Assert.False(PinyinLookupService.ContainsChinese("su3 cl3"));
    }
}
