using System.Text;

namespace Community.PowerToys.Run.Plugin.PinyinHelper;

internal sealed class ZhuyinKeyboardDecoder
{
    private static readonly Dictionary<char, string> KeyMap = new()
    {
        ['1'] = "ㄅ",
        ['q'] = "ㄆ",
        ['a'] = "ㄇ",
        ['z'] = "ㄈ",
        ['2'] = "ㄉ",
        ['w'] = "ㄊ",
        ['s'] = "ㄋ",
        ['x'] = "ㄌ",
        ['e'] = "ㄍ",
        ['d'] = "ㄎ",
        ['c'] = "ㄏ",
        ['r'] = "ㄐ",
        ['f'] = "ㄑ",
        ['v'] = "ㄒ",
        ['5'] = "ㄓ",
        ['t'] = "ㄔ",
        ['g'] = "ㄕ",
        ['b'] = "ㄖ",
        ['y'] = "ㄗ",
        ['h'] = "ㄘ",
        ['n'] = "ㄙ",
        ['u'] = "ㄧ",
        ['j'] = "ㄨ",
        ['m'] = "ㄩ",
        ['8'] = "ㄚ",
        ['i'] = "ㄛ",
        ['k'] = "ㄜ",
        [','] = "ㄝ",
        ['9'] = "ㄞ",
        ['o'] = "ㄟ",
        ['l'] = "ㄠ",
        ['.'] = "ㄡ",
        ['0'] = "ㄢ",
        ['p'] = "ㄣ",
        [';'] = "ㄤ",
        ['/'] = "ㄥ",
        ['-'] = "ㄦ",
    };

    private static readonly Dictionary<char, string> ToneMarks = new()
    {
        ['6'] = "ˊ",
        ['3'] = "ˇ",
        ['4'] = "ˋ",
        ['7'] = "˙",
    };

    private static readonly Dictionary<string, string> Initials = new()
    {
        ["ㄅ"] = "b",
        ["ㄆ"] = "p",
        ["ㄇ"] = "m",
        ["ㄈ"] = "f",
        ["ㄉ"] = "d",
        ["ㄊ"] = "t",
        ["ㄋ"] = "n",
        ["ㄌ"] = "l",
        ["ㄍ"] = "g",
        ["ㄎ"] = "k",
        ["ㄏ"] = "h",
        ["ㄐ"] = "j",
        ["ㄑ"] = "q",
        ["ㄒ"] = "x",
        ["ㄓ"] = "zh",
        ["ㄔ"] = "ch",
        ["ㄕ"] = "sh",
        ["ㄖ"] = "r",
        ["ㄗ"] = "z",
        ["ㄘ"] = "c",
        ["ㄙ"] = "s",
    };

    private static readonly Dictionary<string, string> StandaloneFinals = new()
    {
        ["ㄚ"] = "a",
        ["ㄛ"] = "o",
        ["ㄜ"] = "e",
        ["ㄝ"] = "e",
        ["ㄞ"] = "ai",
        ["ㄟ"] = "ei",
        ["ㄠ"] = "ao",
        ["ㄡ"] = "ou",
        ["ㄢ"] = "an",
        ["ㄣ"] = "en",
        ["ㄤ"] = "ang",
        ["ㄥ"] = "eng",
        ["ㄦ"] = "er",
    };

    private static readonly HashSet<string> ApicalInitials = ["zh", "ch", "sh", "r", "z", "c", "s"];

    public ZhuyinKeyboardResult? TryDecode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var tokens = new List<ZhuyinKeyboardToken>();
        foreach (var rawToken in input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var token = TryDecodeToken(rawToken);
            if (token is null)
            {
                return null;
            }

            tokens.Add(token);
        }

        if (tokens.Count == 0)
        {
            return null;
        }

        return new ZhuyinKeyboardResult(
            string.Join(" ", tokens.Select(static token => token.Pinyin)),
            string.Join(" ", tokens.Select(static token => token.Zhuyin)),
            tokens);
    }

    private static ZhuyinKeyboardToken? TryDecodeToken(string raw)
    {
        var symbols = new List<string>();
        string? toneMark = null;

        for (var index = 0; index < raw.Length; index++)
        {
            var key = char.ToLowerInvariant(raw[index]);
            if (ToneMarks.TryGetValue(key, out var mark))
            {
                if (toneMark is not null || index != raw.Length - 1)
                {
                    return null;
                }

                toneMark = mark;
                continue;
            }

            if (!KeyMap.TryGetValue(key, out var symbol))
            {
                return null;
            }

            symbols.Add(symbol);
        }

        if (symbols.Count == 0)
        {
            return null;
        }

        var pinyin = ConvertSymbolsToPinyin(symbols);
        if (pinyin is null)
        {
            return null;
        }

        var zhuyin = FormatZhuyin(symbols, toneMark);
        return new ZhuyinKeyboardToken(raw, zhuyin, pinyin);
    }

    private static string? ConvertSymbolsToPinyin(IReadOnlyList<string> symbols)
    {
        var index = 0;
        string? initial = null;

        if (Initials.TryGetValue(symbols[index], out var initialValue))
        {
            initial = initialValue;
            index++;
        }

        string? medial = null;
        if (index < symbols.Count && IsMedial(symbols[index]))
        {
            medial = symbols[index];
            index++;
        }

        string? final = null;
        if (index < symbols.Count)
        {
            final = symbols[index];
            index++;
        }

        if (index != symbols.Count)
        {
            return null;
        }

        if (initial is null)
        {
            return ConvertInitialless(medial, final);
        }

        if (medial is null && final is null)
        {
            return ApicalInitials.Contains(initial) ? $"{initial}i" : null;
        }

        var suffix = ConvertSuffixAfterInitial(initial, medial, final);
        return suffix is null ? null : initial + suffix;
    }

    private static string? ConvertInitialless(string? medial, string? final)
    {
        if (medial is null)
        {
            return final is not null && StandaloneFinals.TryGetValue(final, out var value) ? value : null;
        }

        return medial switch
        {
            "ㄧ" => final switch
            {
                null => "yi",
                "ㄚ" => "ya",
                "ㄛ" => "yo",
                "ㄝ" => "ye",
                "ㄞ" => "yai",
                "ㄠ" => "yao",
                "ㄡ" => "you",
                "ㄢ" => "yan",
                "ㄣ" => "yin",
                "ㄤ" => "yang",
                "ㄥ" => "ying",
                _ => null,
            },
            "ㄨ" => final switch
            {
                null => "wu",
                "ㄚ" => "wa",
                "ㄛ" => "wo",
                "ㄞ" => "wai",
                "ㄟ" => "wei",
                "ㄢ" => "wan",
                "ㄣ" => "wen",
                "ㄤ" => "wang",
                "ㄥ" => "weng",
                _ => null,
            },
            "ㄩ" => final switch
            {
                null => "yu",
                "ㄝ" => "yue",
                "ㄢ" => "yuan",
                "ㄣ" => "yun",
                "ㄥ" => "yong",
                _ => null,
            },
            _ => null,
        };
    }

    private static string? ConvertSuffixAfterInitial(string initial, string? medial, string? final)
    {
        if (medial is null)
        {
            return final is not null && StandaloneFinals.TryGetValue(final, out var value) ? value : null;
        }

        return medial switch
        {
            "ㄧ" => final switch
            {
                null => "i",
                "ㄚ" => "ia",
                "ㄛ" => "io",
                "ㄝ" => "ie",
                "ㄞ" => "iai",
                "ㄠ" => "iao",
                "ㄡ" => "iu",
                "ㄢ" => "ian",
                "ㄣ" => "in",
                "ㄤ" => "iang",
                "ㄥ" => "ing",
                _ => null,
            },
            "ㄨ" => final switch
            {
                null => "u",
                "ㄚ" => "ua",
                "ㄛ" => "uo",
                "ㄞ" => "uai",
                "ㄟ" => "ui",
                "ㄢ" => "uan",
                "ㄣ" => "un",
                "ㄤ" => "uang",
                "ㄥ" => "ong",
                _ => null,
            },
            "ㄩ" => final switch
            {
                null => UsesPlainUForUmlaut(initial) ? "u" : "v",
                "ㄝ" => UsesPlainUForUmlaut(initial) ? "ue" : "ve",
                "ㄢ" => UsesPlainUForUmlaut(initial) ? "uan" : "van",
                "ㄣ" => UsesPlainUForUmlaut(initial) ? "un" : "vn",
                "ㄥ" => UsesPlainUForUmlaut(initial) ? "iong" : null,
                _ => null,
            },
            _ => null,
        };
    }

    private static bool UsesPlainUForUmlaut(string initial)
    {
        return initial is "j" or "q" or "x";
    }

    private static bool IsMedial(string symbol)
    {
        return symbol is "ㄧ" or "ㄨ" or "ㄩ";
    }

    private static string FormatZhuyin(IReadOnlyList<string> symbols, string? toneMark)
    {
        var text = new StringBuilder();
        if (toneMark == "˙")
        {
            text.Append(toneMark);
        }

        foreach (var symbol in symbols)
        {
            text.Append(symbol);
        }

        if (toneMark is not null && toneMark != "˙")
        {
            text.Append(toneMark);
        }

        return text.ToString();
    }
}

internal sealed record ZhuyinKeyboardToken(string Raw, string Zhuyin, string Pinyin);

internal sealed record ZhuyinKeyboardResult(
    string Primary,
    string Zhuyin,
    IReadOnlyList<ZhuyinKeyboardToken> Tokens);
