# CLAUDE.md — Bondwave 實驗基台

> 給在這個 repo 工作的 Claude 讀。描述**硬體現況、安全邊界、已知問題**。
> 程式碼細節請直接看原始碼；這份文件記錄的是看程式碼看不出來的東西。
> 最後更新：2026-11-13

---

## 1. 這台機器在做什麼

實驗用 die bonding 基台，研究 **bond wave**（die 與 wafer 貼合時，接合波從接觸點向外擴散的現象）。

核心動作：A 軸（大 Z）把 bond head 移到接觸位置 → B 軸（音圈）切力控模式，依 force profile 施壓 → load cell 回授實際力量 → 未來用 SWIR 相機即時觀察 bond wave 擴散。

---

## 2. 硬體拓撲（**已變更，與 repo 內舊文件不符**）

```
IPC (Windows)
├── Ethernet ──────── Agito AGD301-ET-2D09-001   馬達控制器
│                      ├── Axis A ── Z1 (型號已更換，見下)
│                      └── Axis B ── Z2 = AVM90-15 音圈馬達
├── Modbus TCP ────── 真空/壓力控制器 172.1.1.100:502
└── PCIe ──────────── Euresys Coaxlink Quad CXP-12 Value (PC3623)
                       └── CXP ×2 ── Cheetah+ 640 CXP SWIR 相機
```

### 2.1 控制器 AGD301-ET-2D09-001

- 3 軸控制器 + **內建 3 組驅動放大器**（這點與 AGC301 不同，AGC301 要外接驅動器）
- 控制迴路 16 kHz / 放大器 PWM 32 kHz
- 每軸連續 9 Arms、峰值 18.2 Arms（3 軸合計限 20 Arms）
- 電源 12–90 VDC
- **類比輸入 4 路 16-bit**（`-001` 變體才是 16-bit，`-2D05` 是 12-bit）
- 隔離輸入 27 / 隔離輸出 17 / 雙向差動 I/O (RS422) 8
- 編碼器 3 port，軟體可設 AquadB / Sin-Cos 1Vpp / BiSS-C / EnDat2.2

**SDK**：`AAMotion.dll` (Agito.AAMotion 6.0.2 NuGet)，程式用 `AGM800` 類別連線。
`AGM800` 是 SDK 的通用入口類別，**不是**控制器型號（AGM800 實際上是另一款 8 軸 Central-i 主控制器）。
套件內含 `net40` / `net48` / `net6.0` / `net8.0` 版本 DLL。

### 2.2 軸配置

| | **Axis A — Z1** | **Axis B — Z2** |
|---|---|---|
| 型號 | `APM-XB04ADK3` ⚠️ **新換，規格未確認** | `AVM90-15`（Akribis 音圈馬達） |
| 功能 | 大 Z，粗定位、進退料 | 小 Z，力控壓合 |
| `MotorType` (.par 舊值) | 4（三相，需 hall） | 2（Voice Coil，**不需 hall**） |
| `ContCL` / `PeakCL` | 1630 / 4800 mA | 4000 / 14000 mA |
| `EncType` / `EncRes` | 6 / 10000 | 6 / 1 |
| `FwdPLim` / `RevPLim` | **±1.6e9 = 等於沒設** ⚠️ | ±16,000,000 |
| 接線現況 | I/O 接 **X4 General I/O**，其餘接 Axis A port | IO / ENC / Motor M1-M3 / PE 全接 Axis B |

> ⚠️ `.par` 的舊值是**前一顆 Z1** 的設定。新馬達 `APM-XB04ADK3` 的相數、額定電流、編碼器型式**尚未確認**，不可沿用舊參數直接通電。

### 2.3 AGD301 接頭（來自應用筆記，**非完整接頭表**）

每軸有 ENC / I/O / Motor Power / BRK-HALLS 四組。

**Motor Power（A4）**

| Pin | 信號 |
|---|---|
| 1 | Phase A (M1) |
| 2 | Phase B (M2) |
| 3 | Phase C (M3) — **音圈馬達此腳 NC** |
| 4 | PE |

**Encoder（A1）**

| Pin | 名稱 | AqB | Sin/Cos | 絕對式 |
|---|---|---|---|---|
| 1 | 5V | 編碼器電源（每接頭限 0.5 A） | | |
| 2 | GND | | | |
| 3/4 | Encoder_1± | — | — | CLOCK± |
| 5/6 | Encoder_2± | A± | SIN± | |
| 7/8 | Encoder_3± | B± | COS± | |
| 9/10 | Encoder_4± | Z± | Z± | DATA± |

對接頭料號：ENC = 3M `101203000PE` + 外殼 `10320-3210-000`；Motor Power = Wurth `691364100003`（pitch 3.5 mm）。

> 另有文件使用 `X2` / `X3` 命名（Pulse-Direction 應用筆記），與 `A1/A3/A4` 命名並存，來源不一致。
> **接線前務必以機器實體絲印 + 官方 Product Manual 為準。**

### 2.4 力量回授鏈

```
Load cell → TM3 訊號放大器（創唯實業）→ AGD301 Analog Input #2 → BForce
```

- TM3 是**類比式**放大器，面板下 4 個旋鈕：`ZERO C`（零點粗調）、`ZERO F`（零點細調）、`SPAN C`（增益粗調）、`SPAN F`（增益細調）。調整順序：**先粗調再細調**。
- 出廠設定：零點 0V @ 0.0 mV/V；放大 10V @ 2.0 mV/V
- 控制器端另有 `AAInOffset = −390 mV`、`AAInGain[2] = −13950`（= −0.21286 /mV，**負值**）
- ⚠️ **力量訊號有兩層零點/增益**（TM3 硬體旋鈕 + 控制器參數）。校正時必須先決定歸零歸在哪一層，另一層不動，否則會互相抵銷或疊加誤差。

### 2.5 真空 Modbus 暫存器

| HR | 寫入 | 意義 |
|---|---|---|
| 0 | kPa | Bond Head 真空設定值（`updateVacuumCon` 也從 HR0 讀「實際壓力」，讀寫同一暫存器，**待確認是否真為實際值**） |
| 1 | 1/0 | Bond Head Blow（破真空） |
| 2 | 1/0 | Bond Head Vacuum |
| 3 | 1/0 | Chuck Blow |
| 4 | 1/0 | Chuck Vacuum |

### 2.6 相機（獨立子系統，尚未整合進主程式）

- Cheetah+ 640 CXP（Exosens/Xenics），SWIR 900–1700 nm，640×512，20 µm pixel，最高 1730 fps
- Frame grabber：Euresys Coaxlink Quad CXP-12 Value，韌體 variant `1-camera` rev 530
- **必須接兩條 CXP 線**：單條 PoCXP（17 W）餵不動相機（耗電 15.3 W + TE 製冷湧入電流），實測拔掉第二條相機就不亮。2× CXP-6 總頻寬與 1× CXP-12 相同（12.5 Gbps），不影響幀率
- 相機參數用 **`AcquisitionFrameTime`（µs 週期）**，不是 FrameRate。`FPS = 1,000,000 ÷ FrameTime`。約束：`ExposureTime ≤ AcquisitionFrameTime`
- Memento 持續報 `CoaXPress Warning: Unexpected data width in pixel format (0x0007)` — grabber 不認得相機送的格式代碼，**未解**。目前設 Mono8 時資料量正確（327,680 bytes/frame）
- C# 整合：eGrabber SDK 內含 .NET binding，範例在 `C:\Program Files\Euresys\eGrabber\samples\csharp\`
- 滿速磁碟頻寬約 **1.1 GB/s**，必須 NVMe

---

## 3. 安全規則（修改運動控制程式碼前必讀）

### 3.1 絕對不可以做的事

1. **不要在未確認 Z1 煞車狀態前，產生會呼叫 `MotorOn()` 或 phasing 的程式碼。**
   A 軸是垂直 Z 軸。若無電磁煞車，servo off 或 phasing 期間 bond head 會因重力自由落下。
   AGD301 有專用 **BRK 腳位**支援煞車控制 —— 若 Z1 有煞車線（通常標 BR+/BR−）必須接上。
2. **不要在未驗證新 Z1 規格前沿用舊的 `ContCL`/`PeakCL`。** 電流限制是保護馬達不被燒毀的唯一防線。
3. **不要移除或弱化任何既有的保護判斷**，即使它看起來寫得很醜。
4. **不要提高 `tbAJogVel` 或 Auto 流程的預設速度**。

### 3.2 已知的保護缺口

| 項目 | 現況 |
|---|---|
| A 軸韌體行程限位 | **關閉**（±1.6e9）。唯一保護是 20 ms 輪詢的 `Pos > 800000` 軟體判斷 |
| A 軸軟體保護 | `hasShownMessage` 觸發後**永不重置**，等於只保護第一次 |
| B 軸行程監控 | **完全沒有**。程式從未讀取 `B.Pos` |
| 過壓保護 | 已實作（監控 `BForce`），但**高速 Jog 時反應不及** |
| EMO 急停 | 未實作 / 未驗證 |
| Controller fault | `ConFlt` / `ConFaultMsg` 從未被讀取 |
| 韌體層力量保護 | `ForceCurr_SetForceRefFilt()` 從未呼叫，`MaxForceErr` 停留在預設值 |

### 3.3 過壓保護的根本限制

軟體保護的延遲鏈：20 ms 輪詢 + Ethernet 往返 + `Stop()` 減速時間 ≈ **100–300 ms**。
高速下壓時力量早已衝過門檻。

**軟體保護只能當第二道防線。** 第一道必須在：

- 控制器韌體：`ForceCurr_SetSlowApproach` / `SetPosThForAutoSwToPosMode` / `SetForceModeSWTrigSrc` / `SetForceRefFilt`（SDK 都有，**全部未使用**）
- 硬體：限位開關、EMO 繼電器直接切馬達電源

寫過壓相關程式碼時，不要把軟體層講得像是唯一防線。

### 3.4 指令時序注意

`Stop()` 是**減速停止**，不是立即斷電。`Stop()` 之後緊接著下 `MoveAbs()`，在 controller 仍在減速時會被忽略 —— 必須等 `MotionStat == 0` 再下一個運動指令。
同理，`ForceCurr_GotoPosMode()` 切模式也需要時間生效。

---

## 4. 程式架構現況

### 4.1 核心問題

不是「單執行緒」，而是**沒有執行緒所有權**：

- `ControllerInstance` 同時被 ThreadPool（`motorTimer` callback）和 UI thread（按鈕、Auto 流程）呼叫，**沒有 lock**
- UI 控制項的 `.Text` 被當成資料來源（chart 讀 `tbFeedback.Text` 再 `Convert.ToInt32`）
- Auto 流程靠 `await Task.Delay(估計時間)` 串時序，不檢查控制器狀態

### 4.2 執行緒清單

| Timer | 型別 | 週期 | 執行緒 |
|---|---|---|---|
| `motorTimer` | `System.Timers.Timer` | 20 ms | ThreadPool |
| `chartTimer` | `System.Timers.Timer` | 20 ms | ThreadPool |
| `vacuumTimer` | `System.Timers.Timer` | 20 ms | ThreadPool → `Invoke` 回 UI 做 Modbus I/O |
| `timerExportCSV` | WinForms Timer | 1000 ms | UI thread |
| `timerUpdateStatus` | WinForms Timer | — | **從未 Start，死碼** |
| `timerVaccum` | WinForms Timer | — | Tick 內容全註解，**死碼** |

另：`new AGM800(50)` 代表 SDK 內部還有自己的 50 ms 狀態更新 timer。

### 4.3 已知 Bug（修改時順手處理，但**先問過再動**）

| 位置 | 問題 |
|---|---|
| `btnAAbs_Click` | A 頁的 "Go Abs" 實際呼叫 `ControllerInstance.B.MoveAbs()`，**動到 B 軸** |
| `btnBhome_Click` / `bntBMovStop_Click` | B 軸操作卻檢查 `CheckAMotor()` |
| `tbnForcewrite_Click` | 總時間用 Auto 頁的 `tbx11..tbx51` 加總，不是 B 頁的 `tb11..tb51` |
| `btnAutorun_Click` | force profile 只送第 1 段，UI 上 2–5 段無效但仍計入等待時間 |
| `timerExportCSV_Tick` | CSV 記錄 **`BForceRef`（命令值）而非 `BForce`（實際回授）** |
| `btnDisconnect_Click` | 未停 `motorTimer`，斷線後持續丟例外（被靜默吞掉） |
| `btnConnect_Click` | 每按一次就新建一組 timer，舊的沒 Dispose |
| `btnBMotorOn_Click` | 失敗時仍設 `isMotorBconnected = true` |
| `MainForm_FormClosing` | 直接 `Process.Kill()`，不 MotorOff、不關真空；選「否」沒設 `e.Cancel` |
| `btnAutoStop_Click` | 只設 flag，`Task.Delay` 不會被取消，3 秒後流程繼續 |
| `MotorTimer_Elapsed` | 無 try/catch；`System.Timers.Timer` 可重入 |
| 旗標 | `isWrite` / `isRecording` / `isAutoStop` 跨執行緒讀寫無 `volatile`/lock |

### 4.4 名詞對照

| 名稱 | 意義 |
|---|---|
| `BForce` | **Load cell 實際量到的力量**（AI#2 回授） |
| `BForceRef` | **力控迴路當前目標命令值**，由 force profile 決定，會隨時間階梯變化 |
| A motor / 大 Z | Axis A，線性馬達 |
| B motor / 小 Z / 音圈 | Axis B，voice coil，做 force control |
| OperationMode 3 / 4 | 3 = Position mode，4 = Force mode |
| Bond Head (BH) / Chuck (CH) | 吸 die 的頭 / 吸 wafer 的平台 |

### 4.5 `ForceCurr_SetForceCmdSource` 參數語意

```csharp
bool ForceCurr_SetForceCmdSource(
    int forceCmdSrc,      // 1 = 使用下面三個表格
    int[] forceCurrSlope, // 每段的力量變化斜率
    int[] forceCmdHTime,  // 每段 hold time (ms) — 對應 UI 的 tb11..tb51
    int[] ForceCmdVal);   // 每段目標力量      — 對應 UI 的 tb12..tb52
```

三個陣列長度必須一致，段數可變。

---

## 5. 目前進度與卡點

### 5.1 已完成

- 相機鏈路打通：eGrabber 認到 Cheetah+ 640，能取像（低幀率、長曝光下可見訊號）
- Z1、Z2 已實體接上 AGD301
- 過壓保護已實作（`TriggerOverLoadRetract`），含 A/B 軸回彈、`isAutoStop` 中止 Auto 迴圈

### 5.2 當前階段：**通電前的通訊測試**

未接煞車的情況下，以下**安全可做**（不會讓馬達通電）：

1. PCSuite 連線，確認韌體版本與 Identity
2. 讀取並記錄所有軸參數現值
3. **手推軸驗證編碼器**：位置讀數有無變化、方向對不對、解析度對不對
4. 手動觸發限位開關，驗證 I/O 狀態跳變
5. Z1 若為三相，慢推驗證 hall 狀態循環
6. 設定 `FwdPLim` / `RevPLim`

**在確認煞車或架好機械支撐前，不可進行**：`MotorOn`、phasing、任何 move/jog。

### 5.3 待確認（擋住下一步）

- [ ] **Z1 (`APM-XB04ADK3`) 規格**：幾相？額定/峰值電流？編碼器型式？——決定 `MotorType`/`ContCL`/`PeakCL`/`EncType`
- [ ] **Z1 有無電磁煞車？**——決定能否 servo off 當緊急停止、決定 commissioning 安全程序
- [ ] **X4 General I/O 上的 Z1 限位有沒有映射到 Axis A？** 若只是通用輸入，控制器不會自動當成硬體限位，Z1 等於完全沒有行程保護
- [ ] `EncType = 6` 對應哪一種編碼器？（影響 ENC pin 3/4、9/10 的意義）
- [ ] 力量單位：UI 標 g，控制器是 force-unit。**兩者換算未確認**，所以過壓門檻那個 `500` 目前意義不明 —— 需用已知砝碼實測建立對照
- [ ] `AGendata[10]=20303` 每次輪詢都寫入的用途
- [ ] Modbus HR0 讀回的是設定值還是實際壓力
- [ ] 相機 pixel format `0x0007` 不相容問題（需問 Exosens 原廠或用 Xeneth 對照）

---

## 6. 工作慣例

- **目標框架**：目前 .NET Framework 4.5。升到 **4.8** 是低風險且必要的一步（AAMotion 有 net48，eGrabber C# binding 可能需要 4.6+）。先 4.8，.NET 8 之後再說。
- **動運動控制程式碼前先問**。這是實體設備，錯誤會造成硬體損壞或人身危險。
- 修 bug 時**一次一個**，不要順手重構一大片。
- `.par` 檔（`AKRIBIS/` 底下）是控制器參數備份，改控制器參數前先備份當前版本。
- repo 內 `Robot.cs` / `LoadPort.cs` / `Aligner.cs` / `Core.cs` / `OCR.cs` / `Stage.cs` / `Module/*` 是從舊 EFEM 專案複製來的**死碼**，約 15k 行，未被實例化。可整批刪除，但 `XML.cs` 仍被 `MainForm.Init()` 使用，**要保留**。

---

## 7. 架構改善方向（長期）

核心原則：**每個硬體物件只屬於一條執行緒**（Actor 模式），其他人透過命令佇列投遞。

```
View (WinForms)
  ↕
ViewModel
  ↕
Application: BondSequencer / RecipeService / RunRecorder
  ↕
MotionService    VacuumService    CameraService
(專屬 thread)    (專屬 thread)    (SDK callback)
  ↕
HAL: IMotionAxis / IForceAxis / IVacuum / ICamera (+ Simulator)
```

**多指令處理策略**（不同命令不同對待）：

| 命令 | 策略 |
|---|---|
| E-Stop / Stop | Preempt — 清空佇列立即執行 |
| MoveAbs / Home | Reject if busy（UI 層同時 disable 按鈕） |
| Jog | Last-wins（速度 setpoint 覆寫） |
| Auto 流程 | 獨佔，執行中拒絕手動命令 |

**時間同步**（相機整合的關鍵設計決定，要在接相機觸發線前定案）：
控制器 Digital Out → Coaxlink HD26F I/O → grabber 產生 CXP trigger → 相機。
AGD301 的 PEG 響應延遲 < 1 µs (AqB encoder)。
frame timestamp 用 `buf.GetInfo<long>(BUFFER_INFO_TIMESTAMP)`，**不要用 `DateTime.Now`**。

> 注意：只有 AGM800-CI 支援同時啟用 PEG 和 Position Lock。AGC/AGD 系列一次只能用一個。
