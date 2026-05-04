using Community.PowerToys.Run.Plugin.PinyinHelper;
using Wox.Plugin;

namespace PinyinHelper.Tests;

public sealed class MainQueryTests
{
    [Fact]
    public void Query_ReturnsDefaultCharactersFirst_ForRawZhuyinKeyboardInput()
    {
        var plugin = new Main();

        var results = plugin.Query(new Query("su3 cl3"));

        Assert.True(results.Count >= 2);
        Assert.Equal("你好", results[0].Title);
        Assert.Contains("你:ni", results[0].SubTitle);
        Assert.Contains("好:hao", results[0].SubTitle);
        Assert.Equal("ni hao", results[1].Title);
        Assert.Contains("ㄋㄧˇ ㄏㄠˇ", results[1].SubTitle);
    }

    [Fact]
    public void Query_KeepsChineseInputPinyinFirst()
    {
        var plugin = new Main();

        var results = plugin.Query(new Query("今天"));

        Assert.True(results.Count >= 2);
        Assert.Equal("jin tian", results[0].Title);
        Assert.Equal("今:jin  天:tian", results[1].Title);
    }
}
