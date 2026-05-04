using Community.PowerToys.Run.Plugin.PinyinHelper;

namespace PinyinHelper.Tests;

public sealed class ZhuyinKeyboardDecoderTests
{
    private readonly ZhuyinKeyboardDecoder _decoder = new();

    [Fact]
    public void TryDecode_DecodesRawKeyboardInput_ForNiHao()
    {
        var result = _decoder.TryDecode("su3 cl3");

        Assert.NotNull(result);
        Assert.Equal("ni hao", result.Primary);
        Assert.Equal("ㄋㄧˇ ㄏㄠˇ", result.Zhuyin);
        Assert.Collection(
            result.Tokens,
            token =>
            {
                Assert.Equal("su3", token.Raw);
                Assert.Equal("ㄋㄧˇ", token.Zhuyin);
                Assert.Equal("ni", token.Pinyin);
            },
            token =>
            {
                Assert.Equal("cl3", token.Raw);
                Assert.Equal("ㄏㄠˇ", token.Zhuyin);
                Assert.Equal("hao", token.Pinyin);
            });
    }

    [Fact]
    public void TryDecode_DecodesZhuyinPracticeExample()
    {
        var result = _decoder.TryDecode("5j4 u4");

        Assert.NotNull(result);
        Assert.Equal("zhu yi", result.Primary);
        Assert.Equal("ㄓㄨˋ ㄧˋ", result.Zhuyin);
    }

    [Fact]
    public void TryDecode_DecodesUmlautInitials_WithInputFriendlyV()
    {
        var result = _decoder.TryDecode("sm3 xm3");

        Assert.NotNull(result);
        Assert.Equal("nv lv", result.Primary);
        Assert.Equal("ㄋㄩˇ ㄌㄩˇ", result.Zhuyin);
    }

    [Fact]
    public void TryDecode_DecodesJqxUmlautInitials_WithPlainU()
    {
        var result = _decoder.TryDecode("rmp fmp vmp");

        Assert.NotNull(result);
        Assert.Equal("jun qun xun", result.Primary);
        Assert.Equal("ㄐㄩㄣ ㄑㄩㄣ ㄒㄩㄣ", result.Zhuyin);
    }

    [Fact]
    public void TryDecode_DecodesNeutralTone_AsLeadingDot()
    {
        var result = _decoder.TryDecode("2k7");

        Assert.NotNull(result);
        Assert.Equal("de", result.Primary);
        Assert.Equal("˙ㄉㄜ", result.Zhuyin);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("su33")]
    [InlineData("s3u")]
    [InlineData("hello")]
    public void TryDecode_ReturnsNull_ForInvalidRawInput(string input)
    {
        Assert.Null(_decoder.TryDecode(input));
    }
}
