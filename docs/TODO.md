# TODO

## Near-Term Validation

- Verify more real PowerToys Run examples for both supported input modes:
  - Chinese input: `! 今天`, `! 重慶`, `! 行`, `! 樂`
  - English-mode Zhuyin keyboard input: `! su3 cl3`, `! 5j4 u4`, `! sm3 xm3`
- Tune default character preferences in `PinyinReverseLookupService` when real usage exposes bad first-choice characters.
- Decide whether `PinYinConverterCore` data quality remains good enough after broader Traditional Chinese and polyphonic testing.

## Future Options

- Expand focused tests with more Traditional Chinese word-level examples
- Expand raw Zhuyin tests with less common finals and punctuation-heavy syllables
- Consider a real frequency dictionary if default character selection becomes important
- Consider support for compact raw Zhuyin input without spaces, such as `su3cl3`, using tone keys as syllable boundaries.
- Consider clipboard lookup for text that is already visible elsewhere, such as a future `!clip` flow.
- Explore a companion hotkey/tray app only if the PowerToys Run plugin flow is not enough.
- Consider separate dark/light icon tuning if the current shared icon is not readable enough
- Add GitHub Release notes/checklist once the remote repo is connected

## Command Palette Migration Notes

- The current plugin cannot be installed directly into PowerToys Command Palette.
- Current implementation targets PowerToys Run via `Wox.Plugin.IPlugin`, `plugin.json`, and the PowerToys Run plugin folder.
- Command Palette uses a different WinRT extension model with `IExtension`, `ICommandProvider`, and list/content pages.
- Keep query logic host-independent so future migration can reuse:
  - `PinyinLookupService`
  - `ZhuyinKeyboardDecoder`
  - `PinyinReverseLookupService`
  - focused tests
- Future shape should be:
  - `PinyinHelper.Core` for lookup/decoder/reverse lookup
  - `PinyinHelper.PowerToysRun` for the current Wox adapter
  - `PinyinHelper.CommandPalette` for a future CmdPal adapter
