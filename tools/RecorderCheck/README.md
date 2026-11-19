# 錄影缺張檢查（eGrabber Recorder）

檢查 eGrabber Studio 錄下來的 container 有沒有掉張，並算出實際 FPS。只讀檔案，不改 container。

## 用法

```
powershell -ExecutionPolicy Bypass -File tools\RecorderCheck\Check-Recording.ps1
powershell -ExecutionPolicy Bypass -File tools\RecorderCheck\Check-Recording.ps1 -Path D:\rec\run01 -ExpectedFps 1700
```

| 參數 | 預設 | 說明 |
|---|---|---|
| `-Path` | `C:\W` | container 資料夾（裡面有 `index`、`data`） |
| `-ExpectedFps` | 不填 | 預期 FPS；不填就用間隔中位數當標準週期 |
| `-TolerancePct` | 50 | 間隔偏離標準週期超過幾 % 算異常 |

回傳碼：`0` = 沒有缺張，`1` = 有異常間隔（或檔案格式不符）。

## 判讀

- **第一→最後** 應約等於 (張數 − 1) × 週期。1700 fps、16,000 張 ≈ 9.407 s。
- **間隔分布** 正常時集中在週期 ±1 µs（588 / 587 / 589 µs）。
- 缺一張時會出現約 2 倍週期的間隔（1176 µs），腳本會列出位置和推估缺張數。
- 一個 container 裡有多個 chapter 時，chapter 之間的停頓也會被列為異常。

## 格式說明

`index` 格式是從實際錄影檔反推的（eGrabber Studio 26.10，Mono8 640×512）：
檔頭 8 bytes，每張一筆 216 bytes，時間戳在紀錄 +0x28（ns）。
eGrabber 改版若改了格式，腳本會在檢查檔頭或長度時報錯，而不是給出錯誤結果。

## 驗證紀錄（2026-11-19）

- `C:\W`（SeanTest3）：16,000 張、9.407263 s、1700.71 fps，**0 缺張**
- 人工刪掉一張的複本：正確抓到「第 5000 張之後 1176 µs，缺 1 張」
