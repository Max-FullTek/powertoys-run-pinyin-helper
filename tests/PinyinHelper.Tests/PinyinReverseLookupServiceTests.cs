using Community.PowerToys.Run.Plugin.PinyinHelper;

namespace PinyinHelper.Tests;

public sealed class PinyinReverseLookupServiceTests
{
    private readonly ZhuyinKeyboardDecoder _decoder = new();
    private readonly PinyinReverseLookupService _reverseLookup = new();

    [Fact]
    public void Convert_UsesPreferredCharacters_ForNiHao()
    {
        var zhuyin = _decoder.TryDecode("su3 cl3");

        Assert.NotNull(zhuyin);
        var result = _reverseLookup.Convert(zhuyin);

        Assert.Equal("你好", result.Text);
        Assert.Collection(
            result.Tokens,
            token =>
            {
                Assert.Equal("你", token.Text);
                Assert.Equal("ni", token.Pinyin);
            },
            token =>
            {
                Assert.Equal("好", token.Text);
                Assert.Equal("hao", token.Pinyin);
            });
    }

    [Fact]
    public void Convert_UsesPreferredCharacters_ForZhuyi()
    {
        var zhuyin = _decoder.TryDecode("5j4 u4");

        Assert.NotNull(zhuyin);
        var result = _reverseLookup.Convert(zhuyin);

        Assert.Equal("注意", result.Text);
    }

    [Fact]
    public void Convert_FallsBackToIndexedCandidate_WhenNoPreferredCharacterExists()
    {
        var zhuyin = _decoder.TryDecode("y8");

        Assert.NotNull(zhuyin);
        var result = _reverseLookup.Convert(zhuyin);

        Assert.NotEqual("?", result.Text);
        Assert.Equal("za", Assert.Single(result.Tokens).Pinyin);
    }
}
