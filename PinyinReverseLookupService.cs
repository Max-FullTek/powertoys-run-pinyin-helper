using Microsoft.International.Converters.PinYinConverter;

namespace Community.PowerToys.Run.Plugin.PinyinHelper;

internal sealed class PinyinReverseLookupService
{
    private static readonly Dictionary<string, char> PreferredCandidates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["de"] = '的',
        ["hao"] = '好',
        ["jun"] = '軍',
        ["lv"] = '旅',
        ["ni"] = '你',
        ["nv"] = '女',
        ["qun"] = '群',
        ["xun"] = '尋',
        ["yi"] = '意',
        ["zhu"] = '注',
    };

    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<char>>> CandidatesByPinyin = new(BuildIndex);

    public PinyinReverseResult Convert(ZhuyinKeyboardResult input)
    {
        var tokens = new List<PinyinReverseToken>();

        foreach (var token in input.Tokens)
        {
            var candidate = FindCandidate(token.Pinyin);
            tokens.Add(new PinyinReverseToken(candidate?.ToString() ?? "?", token.Pinyin));
        }

        return new PinyinReverseResult(
            string.Concat(tokens.Select(static token => token.Text)),
            tokens);
    }

    private static char? FindCandidate(string pinyin)
    {
        if (PreferredCandidates.TryGetValue(pinyin, out var preferred))
        {
            return preferred;
        }

        return CandidatesByPinyin.Value.TryGetValue(pinyin, out var candidates) && candidates.Count > 0
            ? candidates[0]
            : null;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<char>> BuildIndex()
    {
        var index = new Dictionary<string, List<char>>(StringComparer.OrdinalIgnoreCase);

        for (var codePoint = PinyinLookupService.BasicCjkStart; codePoint <= PinyinLookupService.BasicCjkEnd; codePoint++)
        {
            var character = (char)codePoint;
            if (!ChineseChar.IsValidChar(character))
            {
                continue;
            }

            var chinese = new ChineseChar(character);
            var pinyins = chinese.Pinyins
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(PinyinLookupService.NormalizePinyin)
                .Where(static value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var pinyin in pinyins)
            {
                if (!index.TryGetValue(pinyin, out var candidates))
                {
                    candidates = [];
                    index[pinyin] = candidates;
                }

                candidates.Add(character);
            }
        }

        return index.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<char>)pair.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}

internal sealed record PinyinReverseToken(string Text, string Pinyin);

internal sealed record PinyinReverseResult(string Text, IReadOnlyList<PinyinReverseToken> Tokens);
