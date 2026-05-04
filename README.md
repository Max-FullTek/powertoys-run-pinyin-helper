# Pinyin Helper

Pinyin Helper 是 PowerToys Run 的中文拼音查詢外掛。安裝後，在 PowerToys Run 輸入 `!` 加上中文或注音鍵位，就能快速查拼音。

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
