# Bondwave 相機控制介面（BondwaveCamera）

Cheetah+ 640 CXP + Coaxlink Quad CXP-12 Value，用 eGrabber .NET（`C:\Program Files\Euresys\eGrabber\clr\anycpu\EGrabber.NET.dll`）。
錄影、回放、FPS 分析都在這個程式裡完成，不需要 eGrabber Studio。

## 執行

```
dotnet run --project tools/CameraUI/CameraUI.csproj
```

**先關掉 eGrabber Studio**：同一時間只能有一個程式控制相機。

## 功能

| 分頁 | 功能 |
|---|---|
| 狀態 | 連線 / 斷線；相機型號、影像格式、設定與實測 FPS、累計張數、即時缺張、觸發設定、buffer 數、相機溫度、可用 RAM、磁碟剩餘、最後錯誤 |
| 即時 / 單張 | 對焦用即時畫面（預設 120 fps、曝光 5000 µs，可調）；影像上方顯示**對焦指標**、平均亮度、飽和比例；取單張存 TIFF（8-bit 原始值） |
| 錄影 | 設定 FPS（預設 1700）、曝光、秒數；錄成 eGrabber container；錄完自動分析時間戳（FPS / 缺張），結果寫到該資料夾的 `summary.txt` |
| 回放 / 分析 | 列出錄影資料夾；拖動 / 播放（可調速度）；分析 FPS / 缺張；匯出單張或範圍 TIFF |
| 相機參數 | FPS 控制方式（3 種）＋**自動偵測**；相機參數瀏覽與讀寫 |
| Log | 所有操作與錯誤；同時寫到 `bin/.../logs/camera_yyyyMMdd.txt` |

### 錄影怎麼保證不掉張
- 錄影前算總張數；能放進 RAM 的話就**全部預先配置 buffer**（可用 RAM 扣 4 GB 後 × 70%），磁碟慢也不會丟。
- 放不下時盡量配大，超出部分靠磁碟邊錄邊寫（C: 是 SATA ≈ 344 MB/s，1700 fps 需要 557 MB/s，可能掉張）。
- `Start(N)` 讓 grabber 剛好收 N 張；錄完用每張的硬體時間戳（ns）檢查間隔，缺張會列出位置。
- 錄影畫面預覽每 0.1 s 更新一張，不影響錄影。

### FPS 控制方式（第一次上機要確認）
這台相機用哪個參數控制 FPS 還沒確定（見 CLAUDE.md §2.6），所以做成三選一：
1. 只改 `AcquisitionFrameTime`（不動觸發設定）
2. `TriggerMode=Off` + `AcquisitionFrameTime`
3. `TriggerMode=On`、`TriggerSource=Timer` + 改「Timer 參數名」指定的參數

「自動偵測」會依序試 1→2→3，每種實拍 1.5 秒量 FPS，選第一個誤差 < 3% 的並記住（`camera_settings.json`）。
連線時 Log 會列出所有 Trigger / Timer / FrameTime / Exposure 相關參數，偵測失敗時把 Log 給工程師判斷。

## 命令列

| 參數 | 用途 |
|---|---|
| `--selftest` | 不需相機：TIFF 讀寫、FPS/缺張統計、對焦指標、Recorder API 寫入→讀回 → `selftest.txt` |
| `--probe` | 只連線相機並列出參數後斷線（不取像、不改設定）→ `probe.txt` |
| `--shots` | 開發用：把每個分頁畫成 PNG（`shots/`） |
| `--hwtest` | 實機測試：連線(Mono8) → FPS 偵測 → 即時 120 fps → 單張 → 錄影 2 s @1700 → 讀回分析 → `hwtest.txt` |
| `--inspect <資料夾或 .tif>` | 列出錄影檔幾張影像 / TIFF 的平均、最大灰階 → `inspect.txt` |

## 已驗證 / 未驗證（2026-11-23）

- ✅ 建置成功；`--selftest` 全部通過（含 Recorder API 從 C# 寫入→讀回）
- ✅ 實機 `--hwtest`（Xenics Cheetah+ 640 CXP）：
  - 連線後 PixelFormat 自動 Mono16 → Mono8
  - FPS 偵測：**只改 `AcquisitionFrameTime`（Trigger 維持 On/Timer）就有效**，588 µs → 實測 1700.7 fps
  - 即時 120 fps：實測 120.0 fps、0 缺張
  - 單張 TIFF：OK
  - 錄影 2 s @1700：3,402/3,402 張、1700.71 fps、0 缺張；用 Recorder API 讀回一致；磁碟寫完共 3.3 s
- ⚠ 曝光 450 µs 時畫面幾乎全黑（平均灰階 0、最大 8–10）；曝光 5000 µs 時平均約 13。是光量問題不是程式問題，1700 fps 需要更強 IR 光源 / 增益
- ⏳ GUI 各按鈕走同一個 `CameraService`，但還沒有人實際在畫面上操作過一輪

## 注意
- Recorder 要求錄影資料夾**已存在**，否則回 `INVALID_CONTAINER_PATH`（程式已自動建立）。
- 相機的 `ExposureTime ≤ 週期`，程式會擋下曝光 > 週期 97% 的設定。改 FPS 時依方向調整設定順序（變快先縮曝光，變慢先拉週期）。
- 這個程式只控制相機，不含任何運動控制。`CameraService` 是獨立類別，之後可直接併入主控程式。
