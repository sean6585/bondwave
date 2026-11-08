# Bondwave 實驗基台 — AI Context

> 給 AI / 新進工程師閱讀的系統說明。描述「目前程式實際長什麼樣子」，所有結論都附檔案與行號。
> 改善方向見 [REFACTOR_PLAN.md](REFACTOR_PLAN.md)。
> 最後整理：2026-11-09（依據 `AML Bond wave 1107` 版本原始碼）

---

## 1. 系統目的

實驗用打 die（die bonding）基台，用來研究 **bond wave**（die 與 wafer/基板貼合時，接合波從接觸點向外擴散的現象）。

| 子系統 | 硬體 | 程式中的名稱 | 作用 |
|---|---|---|---|
| 大 Z 軸 | 線性馬達 | **A axis / A motor** | Bond head 大行程上下移動（接近、離開） |
| 小 Z 軸 | 音圈馬達 (Voice Coil) | **B axis / B motor** | 接觸後的力控（force control）壓合，力量曲線 = bonding profile |
| 力量回饋 | Load cell → 控制器 Analog Input #2 | `BForce` | 力量 feedback，單位 UI 標示為 g |
| 真空 / 吹氣 | Modbus TCP 壓力控制器 | `modbusClient` | Bond head 吸 die、Chuck 吸 wafer、破真空 |
| (未來) 下相機 | Bottom camera | 尚未實作 | 對位（die/wafer alignment） |
| (未來) SWIR 相機 | 短波紅外相機（可穿透矽） | 尚未實作 | 即時觀察 bond wave 擴散 |

---

## 2. 目錄結構

```
Bondwave/
├── AML Bond wave 1029.zip              # 舊版快照 (2024-10-29)，與 1107 版只差 MainForm.*
├── AML Bond wave 1107/                 # ★ 目前版本 (Visual Studio solution)
│   ├── AML EFEM Control System.sln
│   ├── packages/                       # NuGet: Agito.AAMotion 6.0.2, EasyModbusTCP 5.6.0, YamlDotNet 4.2.2
│   └── AML EFEM Control System/        # 專案本體 (namespace AML)
│       ├── Program.cs                  # 進入點 → Application.Run(new MainForm())
│       ├── MainForm.cs                 # ★ 所有 Bondwave 邏輯都在這一個檔 (~1430 行)
│       ├── MainForm.Designer.cs        # ★ UI 版面 (tab 頁、按鈕、chart、timer)
│       ├── ControllerConfig.xml        # AAMotion AG_ API 用設定 (IP 172.1.1.103, AGC301, axis 3,4,5) — 目前程式沒用到
│       ├── AGMotionManual.docx / AAMotion_Documentation.chm   # Akribis API 文件
│       ├── Robot.cs, LoadPort.cs, Aligner.cs, Core.cs, OCR.cs, Stage.cs, ...   # ✗ EFEM 遺留程式碼，未被使用
│       ├── Module/                     # ✗ EFEM 遺留 (Adam IO, E84, RFID, Barcode, Socket, XML, Ian 工具)
│       └── bin/Debug/                  # 執行檔與第三方 DLL、Setting.xml、LogError.txt
└── AKRIBIS/                            # Akribis 控制器參數備份 (.par)、韌體 (.hex)、PCSuite 截圖
```

### 2.1 重要事實
- 這個專案是從公司舊的 **AML EFEM Control System**（晶圓搬運 EFEM：Robot / LoadPort / Aligner）**複製改造**而來。專案名、namespace、notifyIcon 文字、Setting.xml 內容都還是 EFEM 的。
- **真正 Bondwave 有在跑的程式碼只有 `MainForm.cs` + `MainForm.Designer.cs`**。`Core.Init(...)` 等 EFEM 初始化在 `MainForm.cs:95-122` 已全部註解掉。其餘約 15,000 行 EFEM 程式碼仍會被編譯，但沒有任何地方實例化。
- 沒有 git 版控；版本靠資料夾名稱（1029、1107）與 zip。
- Target framework：**.NET Framework 4.5**，WinForms，AnyCPU。UI 版本字串 `V20241029.1`（`MainForm.Designer.cs:2122`）。

### 2.2 外部依賴
| 依賴 | 來源 | 用途 |
|---|---|---|
| `AAMotion.dll` / `AAComm.dll` (Agito.AAMotion 6.0.2) | NuGet；套件另有 net48 / net6.0 / net8.0 版本 | Akribis 馬達控制器 API（類別 `AGM800`）|
| `AACommServer.exe` | AAMotion build target 自動複製 | AAMotion 的通訊伺服程式 |
| `EasyModbus.dll` | NuGet | 真空控制器 Modbus TCP |
| `AML_LogControl.dll` | **HintPath 指向 `..\..\RCMS\RCMS\`（不存在）**，實際只在 `bin/Debug/` 有一份 | 公司自製 log 元件：`AML_LogPage`、`AML_LogTab`、靜態 `Log` |
| `Advantech.*.dll`, `AML_Socket.dll` | `bin/Debug/` | EFEM 遺留，Bondwave 沒用 |
| `System.Windows.Forms.DataVisualization` | .NET | MS Chart 畫力量曲線 |

---

## 3. 硬體與通訊

| 裝置 | 介面 | 位址 | 程式位置 |
|---|---|---|---|
| Akribis 控制器 AGD301/AGC301 (FW 3.0.0.98-141 ALPHA) | Ethernet (AAComm) | **172.1.1.101**（UI `tbIPAddr` 預設值，`MainForm.Designer.cs:2100`）| `AGM800 ControllerInstance`，`MainForm.cs:19,35` |
| 真空/壓力控制器 | Modbus TCP | **172.1.1.100:502**（hard-code，`MainForm.cs:30`）| `ModbusClient modbusClient` |

> 注意：`ControllerConfig.xml` 寫 172.1.1.103 / AGC301，但目前程式用 `AGM800` 類別 + UI 輸入 IP 連線，不讀這個檔。程式使用 `AGM800` 類別連 AGD301/AGC301 控制器，是否為原廠建議用法待確認。

### 3.1 Akribis 軸參數重點（`AKRIBIS/202408081637.par`，最新備份）

| 參數 | A axis (線性, 大Z) | B axis (音圈, 小Z) | 說明 |
|---|---|---|---|
| MotorType | 4 | 2 | |
| EncType / EncRes | 6 / 10000 | 6 / 1 | |
| ContCL / PeakCL | 1630 / 4800 | 4000 / 14000 | 連續 / 峰值電流限制 (mA) |
| Accel / Decel | 500000 / 500000 | 10000000 / 10000000 | counts/s² |
| Speed（預設）| 100000 | 600000 | counts/s |
| EmrgDec | 2000000 | 1000000 | |
| FwdPLim / RevPLim | **±1,600,000,000（等於沒設）** | ±16,000,000 | 軟體行程極限 |
| OperationMode | 3 | 3 | 3 = Position mode，4 = Force mode（程式註解）|
| ForceKi / ForceGain | 0 / 0 | 10 / 50000 | B 軸力控 PI |
| ForceCmdSrc | 0 | 1 | 1 = 使用 ForceCmdVal/HTime/Slope 表格 profile |
| CurrPosTh | — | 146000 | |

**力量回饋設定**（`AKRIBIS/0814_1638.jpg` PCSuite 截圖 & `.par`）：
- Analog Input #2，Input Mode = **3 - Force Feedback**，Axis B，HW: B3 pin 7,8
- Filter 500 Hz，Offset −390 mV，Gain **−0.21286 force-units/mV**（= `AAInGain[2]=-13950` / 65536）
- Analog Input #1 (B 軸, General) offset −172 mV；#3、#4 為 A 軸 general input

---

## 4. 程式架構（現況）

### 4.1 整體 — 單一 God Form

```
Program.Main
  └─ MainForm (WinForms, UI thread)
       ├─ 欄位: AGM800 ControllerInstance, ModbusClient modbusClient, 3 個 AML_LogPage
       ├─ 狀態: 一堆 bool flag (isWrite, isAutoStop, isRecording, BPosionMode, isBChartStop, ...)
       │        + UI TextBox.Text 本身被當成資料來源 (例：chart 讀 tbFeedback.Text)
       ├─ System.Timers.Timer motorTimer  (20 ms, ThreadPool)  → 輪詢馬達狀態，Invoke 回 UI
       ├─ System.Timers.Timer chartTimer  (20 ms, ThreadPool)  → Invoke(UpdateChart)
       ├─ System.Timers.Timer vacuumTimer (20 ms, ThreadPool)  → Invoke(updateVacuumCon) ← Modbus I/O 在 UI thread 上執行
       ├─ WinForms Timer timerExportCSV   (1000 ms, UI thread) → 寫 CSV
       ├─ WinForms Timer timerUpdateStatus / timerVaccum       → 從未 Enable（死碼）
       └─ async void button handlers (Auto run, Force write) → 用 Task.Delay 估時間
```

沒有分層：UI、裝置通訊、流程控制（sequence）、資料記錄全部混在 `MainForm.cs`。

### 4.2 執行緒模型（實際情況）
使用者印象是「單執行緒」，實際上更糟：是**沒有規劃的多執行緒**。
- `ControllerInstance`（AAMotion）同時被 **ThreadPool**（motorTimer callback）和 **UI thread**（按鈕事件、Auto 流程）呼叫，沒有 lock；AAMotion 是否 thread-safe 未知。
- `new AGM800(50)`（`MainForm.cs:35`）：AAMotion 內部本身也有 50 ms 的狀態更新 timer（AllStatUpdate），所以總共有 lib 內部 timer + motorTimer 兩條在打控制器。
- motorTimer 每個屬性 setter 都用同步 `Invoke` 回 UI（`MainForm.cs:135-328`）；UI 忙碌時 ThreadPool thread 被卡住，20 ms timer 會**重入（reentrant）**堆疊。
- `System.Timers.Timer` 的 Elapsed 中拋出的例外會被**靜默吞掉**（例：`int.Parse(tbAPosition)` 失敗）。
- vacuumTimer 雖在 ThreadPool 觸發，但立即 `Invoke` 回 UI 執行 Modbus 讀取 → 網路 I/O 每 20 ms 卡一次 UI thread。
- 從 ThreadPool thread 呼叫 `MessageBox.Show`（`MainForm.cs:530`）。

### 4.3 UI 分頁（`MainForm.Designer.cs`）

| Tab | 內容 |
|---|---|
| A motor | MotorOn/Off、Go P1/P2、Rep P1↔P2（往復）、Add Rel、Go Abs、Jog±（預設 270）、Home、Stop；顯示 Pos/Vel/Current/Motion State |
| B motor | MotorOn/Off、Mode 切換（Position/Force）、力量 profile 表 5 段（Hold time ms / Force），Write 執行、Home、Stop；chart1 力量曲線（Stop/Cont/Clear/Save img）|
| Vacuum | Connect/Disconnect、Bond Head Vacuum 設定值(kPa)、BH Vacuum On/Off、BH Blow On/Off、Chuck Vacuum On/Off、Chuck Blow On/Off、Actual Pressure |
| Auto | Auto run / Stop / Home、A motor Vel（預設 300000）、5 段 profile 表（tbx11..tbx52）、chart2 力量曲線 |
| Log / Setting | Log 檔瀏覽、Setting.xml 路徑、Controller IP + Connect/Disconnect |

### 4.4 主要功能流程

**連線** `btnConnect_Click` (`MainForm.cs:495`)
→ `ControllerInstance.Connect(ip)` → 建立 motorTimer(20ms) 並 Start、建立 chartTimer(20ms)。

**狀態輪詢** `MotorTimer_Elapsed` (`MainForm.cs:519-545`)，每 20 ms：
- `A.Pos` → 若 > 800000 counts 則 `A.Stop()` + MessageBox（軟體極限，註解寫「最多移動 80 公分」）
- `SendBulkCommandString("APos;APosRef;AGendata[10]=20303")`（每次輪詢都寫 GenData[10]，用途不明）
- `A.VelRef`, `A.Iq`, `A.MotionStat`（0 = Inposition）
- `SendCommandString("BForceRef")`, `SendCommandString("BForce")` → 力量 reference / feedback（字串）

**B 軸力控（手動）** `tbnForcewrite_Click` (`MainForm.cs:908-984`)
1. `B.ForceCurr_GotoForceMode()`
2. `B.ForceCurr_SetForceCmdSource(1, slopes[6], holdTimes[6], forces[6])`
   - 參數語意（由使用方式推斷，待以 CHM 文件確認）：`src=1` 表格模式；slope 全 1000；hold time 與 force 各 6 段，**第 1 段重複使用 tb11/tb12**（index 0 可能被忽略或為起始段）
3. `await Task.Delay(總時間 + 100)` — 純時間估計，不檢查控制器狀態
4. A 回 0、B 切回 position mode 並 `B.MoveAbs(0)`

**Auto run** `btnAutorun_Click` (`MainForm.cs:1041-1154`)，迴圈直到 `isAutoStop`：
1. `A.MoveAbs(0, 720000)`；Delay 3 s
2. `A.MoveAbs(210000, tbAmoVel)`（接觸位置 **hard-code 210000**）；Delay 3 s
3. `CreateNewCSV()` → 開始記錄
4. `B.ForceCurr_GotoForceMode()` → `SetForceCmdSource(1, {30,30}, {tbx11,tbx11}, {tbx12,tbx12})`（**只用第 1 段**，表上 2~5 段沒作用）
5. `await Task.Delay(10000 + tbx11 + tbx21 + 100)`
6. `B.ForceCurr_GotoPosMode()`（呼叫兩次）、`B.MoveAbs(0)`、`A.MoveAbs(0, vel)`；Delay 3 s

（另有大量被註解的版本：Jog 接觸偵測模仿 BESI bonding profile、真空吸 die 等待 `tbActPresure > 85`。）

**資料記錄** `timerExportCSV_Tick` (`MainForm.cs:1249`)：每 **1 秒**一筆，記錄的是 **`BForceRef`（命令值）不是 `BForce`（實際回饋）**，每筆都重新開檔 append。路徑 hard-code `C:\Users\User\Desktop\DatraPoint\`。

**真空 Modbus 暫存器對照**（`MainForm.cs:1265-1420`）

| Holding Register | 寫入 | 意義 |
|---|---|---|
| 0 | 設定值 (kPa) | Bond Head 真空設定值；`updateVacuumCon` 也從 HR0 讀「實際壓力」（讀寫同一暫存器，需確認是否真的是實際值）|
| 1 | 1/0 | Bond Head Blow（破真空）On/Off |
| 2 | 1/0 | Bond Head Vacuum On/Off |
| 3 | 1/0 | Chuck Blow On/Off |
| 4 | 1/0 | Chuck Vacuum On/Off |

### 4.5 AAMotion API 使用清單
已用：`AGM800(int updateMs)`, `Connect(ip)`, `Disconnect()`, `ConnectedCableType`, `SendCommandString`, `SendBulkCommandString`,
`axis.MotorOn/MotorOff/MoveAbs(pos[,vel])/MoveRel/MoveAbsRepetitive/Jog/Stop`,
`axis.Pos/VelRef/Iq/MotionStat/MotorOnStatus/OperationMode`,
`axis.ForceCurr_GotoForceMode/GotoPosMode/SetForceCmdSource`。

DLL 中存在但**尚未使用、對改善很重要**的 API：
- `ForceCurr_SetSlowApproach`（慢速接近 → 接觸偵測後自動切力控，已有一行被註解）
- `ForceCurr_SetPosThForAutoSwToPosMode`、`ForceCurr_SetForceModeSWTrigSrc`、`ForceCurr_SetForceTunePID`、`ForceCurr_SetForceRefFilt`
- `StartLogData` / `StopLogData`、`StartSLowLogProcess` / `UploadSlowLoggerData`（**控制器端高速資料記錄**，取代 1 Hz 輪詢）
- `Home*`（正式回原點流程，目前「Home」只是 `MoveAbs(0)`）
- `WaitGenData`、`EventSettings`（控制器事件 / 同步）

---

## 5. 已知 Bug 與風險（依嚴重度）

### 安全相關
1. **A 軸沒有硬性行程保護**：`AFwdPLim/ARevPLim = ±1.6e9`（等於關閉），唯一保護是 20 ms 輪詢的 `Pos > 800000` 軟體判斷（`MainForm.cs:524`），且只在第一次觸發（`hasShownMessage` 永不重置）。
2. 關閉程式 `MainForm_FormClosing` (`:345`)：直接 `Process.Kill()`，**不 MotorOff、不關真空**；選「否」時沒設 `e.Cancel = true`，視窗還是會關。
3. `btnAutoStop` 只設 flag + `Stop()`，Auto 流程中的 `Task.Delay` 不會被取消，3 秒後流程會繼續送下一個 move。
4. Auto run / Force write 可被重複按，產生兩個並行流程。

### 邏輯錯誤
5. `btnAAbs_Click` (`:730-736`)：A 頁面的 "Go Abs" **實際移動的是 B 軸**。
6. `btnBhome_Click` / `bntBMovStop_Click` (`:986-999`)：B 軸操作卻檢查 `CheckAMotor()`。
7. `tbnForcewrite_Click` 的總時間 (`:975`) 用 **Auto 頁的 tbx11..tbx51** 加總，不是 B 頁的 tb11..tb51。
8. Auto 的 profile 只送第 1 段 (`:1099-1102`)，UI 上的 2~5 段無效但會加進等待時間。
9. `btnDisconnect` 不停 motorTimer → 斷線後輪詢持續丟例外（被吞掉）；`_motorBconnected` 直接寫欄位繞過 setter。
10. 每按一次 Connect 就多建一組 timer（舊的沒 Dispose）。
11. `btnBMotorOn` 失敗時仍把 `isMotorBconnected = true`。
12. `isWrite`/`isRecording` 等 flag 跨 thread 讀寫無 `volatile`/lock。

### 資料品質
13. CSV 1 Hz、記錄命令值不是回饋值 → 無法分析 bond profile（音圈力控動態在 ms 級）。
14. Chart 從 `TextBox.Text` 解析數字作圖、點數無上限持續累積 → 長時間跑會越來越慢。
15. Chart 的 X 軸是 tick 計數（`timeCounter++`），不是實際時間戳，timer 抖動直接扭曲曲線。

### 維護性
16. Hard-code：IP、Modbus 位址、接觸位置 210000、速度、存檔路徑 `C:\Users\User\Desktop\...`。
17. ~15k 行 EFEM 死碼、大量註解掉的實驗程式碼、`AML_LogControl.dll` HintPath 錯誤。
18. 無版控、無單元測試、無模擬（simulation）模式，沒有硬體就無法開發。

---

## 6. 名詞對照

| 術語 | 說明 |
|---|---|
| A motor / 大Z | 線性馬達，大行程 Z |
| B motor / 小Z / 音圈 | Voice coil，做 force control |
| Force Ref / BForceRef | 力量命令值 |
| Feedback / BForce | 力量實際值（load cell，AI#2）|
| counts | 編碼器單位 |
| Position mode (3) / Force mode (4) | B 軸 OperationMode |
| Bond Head (BH) | 吸 die 的頭；Chuck (CH) = 吸 wafer 的平台 |
| Blow / 破真空 | 吹氣釋放 |
| Bond wave | 接合波；SWIR 可穿透矽觀察 |
