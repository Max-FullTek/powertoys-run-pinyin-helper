using Microsoft.International.Converters.PinYinConverter;

namespace Community.PowerToys.Run.Plugin.PinyinHelper;

internal sealed class PinyinLookupService
{
    internal const int BasicCjkStart = 0x4E00;
    internal const int BasicCjkEnd = 0x9FFF;

    public static bool ContainsChinese(string input)
    {
        foreach (var rune in input.EnumerateRunes())
        {
            if (rune.Value <= char.MaxValue && ChineseChar.IsValidChar((char)rune.Value))
            {
                return true;
            }
        }

        return false;
    }

    public PinyinResult Convert(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new PinyinResult(string.Empty, string.Empty, []);
        }

        var tokens = new List<string>();
        var displayTokens = new List<PinyinToken>();
        var detailLines = new List<string>();

        foreach (var rune in input.EnumerateRunes())
        {
            var text = rune.ToString();

            if (rune.Value > char.MaxValue)
            {
                AppendLiteral(tokens, text);
                AddLiteralToken(displayTokens, text);
                continue;
            }

            var character = (char)rune.Value;
            if (!ChineseChar.IsValidChar(character))
            {
                AppendLiteral(tokens, text);
                AddLiteralToken(displayTokens, text);
                continue;
            }

            var pinyins = GetPinyins(character);
            if (pinyins.Count == 0)
            {
                AppendLiteral(tokens, text);
                AddLiteralToken(displayTokens, text);
                continue;
            }

            tokens.Add(pinyins[0]);
            displayTokens.Add(new PinyinToken(text, pinyins[0], pinyins));

            if (pinyins.Count > 1)
            {
                detailLines.Add($"{text}: {string.Join(" / ", pinyins)}");
            }
        }

        return new PinyinResult(BuildPrimaryText(tokens), string.Join(Environment.NewLine, detailLines), displayTokens);
    }

    private static List<string> GetPinyins(char character)
    {
        var chinese = new ChineseChar(character);
        var normalized = chinese.Pinyins
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizePinyin)
            .Where(static value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        PreferUmlautInputSpelling(normalized);
        return normalized;
    }

    internal static string NormalizePinyin(string pinyin)
    {
        var value = pinyin.Trim().ToLowerInvariant();
        value = value.Replace("u:", "v", StringComparison.Ordinal);
        value = value.Replace("ü", "v", StringComparison.Ordinal);

        while (value.Length > 0 && char.IsDigit(value[^1]))
        {
            value = value[..^1];
        }

        return value;
    }

    private static void PreferUmlautInputSpelling(List<string> pinyins)
    {
        for (var index = 0; index < pinyins.Count; index++)
        {
            var pinyin = pinyins[index];
            if (!pinyin.Contains('v', StringComparison.Ordinal))
            {
                continue;
            }

            var plainU = pinyin.Replace('v', 'u');
            var plainIndex = pinyins.FindIndex(value => string.Equals(value, plainU, StringComparison.OrdinalIgnoreCase));
            if (plainIndex < 0)
            {
                continue;
            }

            var targetIndex = plainIndex;
            pinyins.RemoveAt(index);
            if (plainIndex > index)
            {
                targetIndex--;
            }

            pinyins.Insert(targetIndex, pinyin);
            return;
        }
    }

    private static void AppendLiteral(List<string> tokens, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        tokens.Add(text);
    }

    private static void AddLiteralToken(List<PinyinToken> tokens, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        tokens.Add(new PinyinToken(text, text, [text]));
    }

    private static string BuildPrimaryText(IEnumerable<string> tokens)
    {
        var cleanTokens = tokens.Where(static token => !string.IsNullOrWhiteSpace(token));
        return string.Join(" ", cleanTokens);
    }
}

internal sealed record PinyinToken(string Text, string Primary, IReadOnlyList<string> Alternatives);

internal sealed record PinyinResult(string Primary, string Details, IReadOnlyList<PinyinToken> Tokens);
