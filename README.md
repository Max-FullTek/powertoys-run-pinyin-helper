# Pinyin Helper

Pinyin Helper 是 [Microsoft PowerToys](https://github.com/microsoft/PowerToys) Run 的中文拼音查詢外掛。PowerToys 是 Microsoft 開源的 Windows 工具集合，PowerToys Run 是其中的快速啟動器；這個外掛需要安裝 PowerToys 才能執行。

安裝後，在 PowerToys Run 輸入 `!` 加上中文或注音鍵位，就能快速查拼音。

## 安裝

1. 到 GitHub Releases 下載最新版 `PinyinHelper-v0.2.0.zip`。
2. 關閉 PowerToys，或至少關閉 PowerToys Run。
3. 解壓縮 zip。
4. 將解壓後的 `PinyinHelper` 資料夾放到：

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins
```

完成後路徑應該長這樣：

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PinyinHelper
```

5. 重新啟動 PowerToys。

## 使用

開啟 PowerToys Run，輸入：

```text
! 今天
```

你會看到：

```text
jin tian
今:jin  天:tian
```

遇到多音字時，會另外顯示其他讀音。

## 英文模式注音鍵位

你也可以不切換到注音輸入法，直接用英文模式輸入台灣注音鍵盤的按鍵：

```text
! su3 cl3
```

這會被解讀為：

```text
ㄋㄧˇ ㄏㄠˇ
```

第一列會顯示預設組字：

```text
你好
```

第二列會顯示拼音：

```text
ni hao
```

第一列的說明會顯示逐字對照：

```text
你:ni  好:hao
```

選字目前是快速預設結果，不做完整詞級選字。

## 聲調鍵

- `6`：二聲
- `3`：三聲
- `4`：四聲
- `7`：輕聲
- 不輸入聲調鍵：一聲

## 範例

```text
! 今天
! 重慶
! su3 cl3
! 5j4 u4
! sm3 xm3
```

## 解除安裝

刪除這個資料夾後重新啟動 PowerToys：

```text
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PinyinHelper
```

## 注意事項

- 需要 Windows 與 PowerToys Run。
- 目前是 PowerToys Run 外掛，不能直接安裝到 PowerToys Command Palette。
- 英文模式注音鍵位的預設組字可能不一定符合你想要的字，但拼音查詢仍可用。

## 引用與依賴

這個專案有引用下列開源專案與套件：

- [Microsoft PowerToys](https://github.com/microsoft/PowerToys)：外掛宿主與 PowerToys Run API。沒有安裝 PowerToys 時，這個外掛無法執行。
- [PinYinConverterCore 1.0.2](https://www.nuget.org/packages/PinYinConverterCore/)（[GitHub](https://github.com/netcorepal/PinYinConverterCore)）：提供中文轉拼音資料與 API。本專案會在發布檔中一併包含 `PinYinConverterCore.dll`。

開發與測試時另外使用：

- [xUnit](https://github.com/xunit/xunit)：單元測試框架。
- [coverlet.collector](https://github.com/coverlet-coverage/coverlet)：測試覆蓋率收集工具。
- [Microsoft.NET.Test.Sdk](https://github.com/microsoft/vstest)：.NET 測試執行工具。
