using System.Windows;
using ManagedCommon;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.PinyinHelper;

public sealed class Main : IPlugin, IPluginI18n, IContextMenu
{
    internal const string PluginId = "130D8B8D-BD50-4F07-9EA8-6C2A0EB56B62";
    public static string PluginID => PluginId;

    private readonly PinyinLookupService _lookupService = new();
    private readonly ZhuyinKeyboardDecoder _zhuyinKeyboardDecoder = new();
    private readonly PinyinReverseLookupService _reverseLookupService = new();
    private PluginInitContext? _context;
    private string _iconPath = @"Images\PinyinHelper.dark.png";

    public string Name => "Pinyin Helper";

    public string Description => "Look up pinyin spellings for Chinese text inside PowerToys Run.";

    public void Init(PluginInitContext context)
    {
        _context = context;
        _context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(_context.API.GetCurrentTheme());
    }

    public List<Result> Query(Query query)
    {
        var search = query.Search?.Trim() ?? string.Empty;
        if (search.Length == 0)
        {
            return [CreateHelpResult()];
        }

        if (!PinyinLookupService.ContainsChinese(search))
        {
            var zhuyinConversion = _zhuyinKeyboardDecoder.TryDecode(search);
            return zhuyinConversion is null
                ? [CreateEmptyStateResult(search)]
                : CreateZhuyinKeyboardResults(zhuyinConversion, _reverseLookupService.Convert(zhuyinConversion));
        }

        var conversion = _lookupService.Convert(search);
        if (conversion.Primary.Length == 0)
        {
            return [CreateEmptyStateResult(search)];
        }

        var results = new List<Result>
        {
            CreatePrimaryResult(search, conversion)
        };

        var breakdown = BuildBreakdown(conversion);
        if (breakdown.Length > 0)
        {
            results.Add(CreateBreakdownResult(breakdown));
        }

        if (HasAlternativePinyin(conversion))
        {
            results.Add(CreateDetailsResult(conversion.Details));
        }

        return results;
    }

    private List<Result> CreateZhuyinKeyboardResults(ZhuyinKeyboardResult conversion, PinyinReverseResult reverseLookup)
    {
        var results = new List<Result>
        {
            CreateReverseLookupResult(reverseLookup)
        };

        var breakdown = BuildReverseLookupBreakdown(reverseLookup);
        results.Add(CreateZhuyinPinyinResult(conversion, breakdown));

        return results;
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is not string text || string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return
        [
            new ContextMenuResult
            {
                PluginName = Name,
                Title = "複製",
                Glyph = "\xE8C8",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                AcceleratorKey = System.Windows.Input.Key.C,
                AcceleratorModifiers = System.Windows.Input.ModifierKeys.Control,
                Action = _ =>
                {
                    Clipboard.SetText(text);
                    return true;
                }
            }
        ];
    }

    public string GetTranslatedPluginTitle() => Name;

    public string GetTranslatedPluginDescription() => Description;

    private Result CreateHelpResult()
    {
        return new Result
        {
            Title = "輸入中文或注音鍵位",
            SubTitle = "例如：今天、su3 cl3、5j4 u4",
            IcoPath = _iconPath,
            Score = 10,
            Action = _ => false,
        };
    }

    private Result CreateEmptyStateResult(string search)
    {
        return new Result
        {
            Title = "查不到可轉換的拼音",
            SubTitle = $"請輸入中文，或英文模式注音鍵位，例如 su3 cl3。原始輸入: {search}",
            IcoPath = _iconPath,
            Score = 20,
            Action = _ => false,
        };
    }

    private Result CreateReverseLookupResult(PinyinReverseResult conversion)
    {
        var breakdown = BuildReverseLookupBreakdown(conversion);
        return new Result
        {
            Title = conversion.Text,
            SubTitle = breakdown.Length > 0 ? $"{breakdown} · 預設組字" : "預設組字",
            IcoPath = _iconPath,
            Score = 100,
            ContextData = conversion.Text,
            Action = _ =>
            {
                Clipboard.SetText(conversion.Text);
                return true;
            }
        };
    }

    private Result CreateZhuyinPinyinResult(ZhuyinKeyboardResult conversion, string breakdown)
    {
        return new Result
        {
            Title = conversion.Primary,
            SubTitle = string.IsNullOrWhiteSpace(breakdown)
                ? $"{conversion.Zhuyin} 的拼音"
                : $"{conversion.Zhuyin} 的拼音 · {breakdown}",
            IcoPath = _iconPath,
            Score = 90,
            ContextData = conversion.Primary,
            Action = _ =>
            {
                Clipboard.SetText(conversion.Primary);
                return false;
            }
        };
    }

    private Result CreatePrimaryResult(string search, PinyinResult conversion)
    {
        return new Result
        {
            Title = conversion.Primary,
            SubTitle = $"{search} 的整句拼音",
            IcoPath = _iconPath,
            Score = 100,
            ContextData = conversion.Primary,
            Action = _ =>
            {
                Clipboard.SetText(conversion.Primary);
                return true;
            }
        };
    }

    private Result CreateBreakdownResult(string breakdown)
    {
        return new Result
        {
            Title = breakdown,
            SubTitle = "逐字對照",
            IcoPath = _iconPath,
            Score = 90,
            ContextData = breakdown,
            Action = _ =>
            {
                Clipboard.SetText(breakdown);
                return false;
            }
        };
    }

    private Result CreateDetailsResult(string details)
    {
        return new Result
        {
            Title = "其他讀音",
            SubTitle = "多音字的其他讀音",
            IcoPath = _iconPath,
            Score = 80,
            ContextData = details,
            Action = _ =>
            {
                Clipboard.SetText(details);
                return false;
            }
        };
    }

    private static string BuildBreakdown(PinyinResult conversion)
    {
        return string.Join("  ", conversion.Tokens.Select(token => $"{token.Text}:{token.Primary}"));
    }

    private static string BuildReverseLookupBreakdown(PinyinReverseResult conversion)
    {
        return string.Join("  ", conversion.Tokens.Select(token => $"{token.Text}:{token.Pinyin}"));
    }

    private static bool HasAlternativePinyin(PinyinResult conversion)
    {
        return conversion.Tokens.Any(token => token.Alternatives.Count > 1);
    }

    private void UpdateIconPath(Theme theme)
    {
        _iconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
            ? @"Images\PinyinHelper.light.png"
            : @"Images\PinyinHelper.dark.png";
    }

    private void OnThemeChanged(Theme currentTheme, Theme newTheme)
    {
        UpdateIconPath(newTheme);
    }
}
