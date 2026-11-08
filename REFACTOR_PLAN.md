# Bondwave 控制程式 — 改善計畫

> 現況分析見 [BONDWAVE_AI_CONTEXT.md](BONDWAVE_AI_CONTEXT.md)。
> 目標：從「一個 God Form + 亂跑的 timer」改成**分層、每個裝置一個擁有者執行緒、流程可取消、資料有共同時間軸**的架構，
> 為之後加入**下相機（對位）**與 **SWIR 相機（觀察 bond wave）**做準備。

---

## 0. 核心問題一句話

問題不是「單執行緒」，而是**沒有執行緒所有權（thread ownership）**：
同一個控制器物件被 UI thread 和 ThreadPool 同時呼叫，UI 控制項被拿來當資料儲存，流程靠 `Task.Delay` 猜時間。
加相機之後（高頻寬、需要和力量曲線時間對齊），這個結構一定撐不住。

---

## 1. 目標架構

```
┌───────────────────────────── UI Layer (WinForms / WPF) ─────────────────────────────┐
│  只做：顯示 snapshot、送 command、畫圖 (30 Hz 刷新)。不碰硬體、不 Sleep、不 Invoke 回圈 │
└───────────────▲───────────────────────────────┬─────────────────────────────────────┘
                │ IObservable<Snapshot> / 最新值  │ async command (Task)
┌───────────────┴───────────────────────────────▼─────────────────────────────────────┐
│ Application Layer                                                                    │
│   BondSequencer (state machine, async + CancellationToken, timeout, interlock)       │
│   RecipeService (YAML/JSON recipe)     RunRecorder (每次 bond 一個資料夾)            │
└──────┬──────────────────┬──────────────────┬───────────────────┬────────────────────┘
       │                  │                  │                   │
┌──────▼──────┐   ┌───────▼──────┐   ┌───────▼───────┐   ┌───────▼────────┐
│MotionService│   │VacuumService │   │CameraService  │   │CameraService   │
│ 專屬 thread │   │ 專屬 thread  │   │ Bottom (對位) │   │ SWIR (bondwave)│
│ 擁有 AGM800 │   │ 擁有 Modbus  │   │ SDK callback  │   │ SDK callback   │
│ cmd queue + │   │ cmd queue +  │   │ → ring buffer │   │ → ring buffer  │
│ 10ms poll   │   │ 50ms poll    │   │               │   │ → disk writer  │
└──────┬──────┘   └───────┬──────┘   └───────┬───────┘   └───────┬────────┘
       │ HAL interfaces: IMotionAxis / IForceAxis / IVacuum / ICamera  (+ Simulator 實作)
   Akribis AGD301      Modbus TCP          Camera SDK          Camera SDK
```

### 1.1 執行緒規則（寫進 code review checklist）
1. **每個硬體物件只屬於一條 thread**（Actor 模式）。其他人只能透過 `Channel<Command>` 丟命令，拿回 `Task<Result>`。
2. 硬體 thread 定期產生**不可變 snapshot**（`record MotionSnapshot(long TimestampTicks, int APos, int AVel, int BForce, int BForceRef, MotionState A, ...)`），用 `Volatile.Write` 存最新值 + 推進 recorder queue。
3. **UI 只用 UI-thread timer（30 Hz）讀最新 snapshot**，不再由背景 thread `Invoke` 進 UI。
4. 流程（Sequencer）是 `async Task`，所有等待都是「**等條件 + timeout + cancellation**」，例如 `await motion.WaitInPositionAsync(A, timeout: 5s, ct)`，禁止用 `Task.Delay(估計時間)` 當流程同步。
5. 寫檔、存影像只在 writer thread（`BlockingCollection`/`Channel` 消費者），不在硬體 thread 或 UI thread 做 I/O。
6. 時間戳統一用 `Stopwatch.GetTimestamp()`（單調時鐘），每次 run 記錄 `t0`。

---

## 2. 分階段計畫

### Phase 0 — 止血（1~2 週，不改架構）
目的：先讓現有機台安全、資料可信，同時建立開發基礎。

- [ ] `git init`，把 `bin/ obj/ .vs/ packages/` 加進 `.gitignore`，以 1107 版作為 initial commit（1029 zip 可另 tag 保存）
- [ ] **控制器端設定 A 軸 `FwdPLim/RevPLim`**（目前 ±1.6e9 等於沒設），用 PCSuite 寫入並 Save to Flash；軟體極限只當第二層
- [ ] 修 bug：`btnAAbs_Click` 動到 B 軸、B 頁 `CheckAMotor`、`tbnForcewrite` 總時間用錯欄位、Auto 只送第 1 段 profile
- [ ] 關閉程式：MotorOff A/B、關真空（或確認關真空是否安全 — 吸著 die 時可能不該關）、停 timer、`e.Cancel` 正確處理
- [ ] Auto run / Force write 按鈕執行中 disable，Stop 用 `CancellationTokenSource` 真正取消
- [ ] motorTimer 改 `AutoReset=false` + 在 callback 結尾再 Start（防重入）；Elapsed 內 try/catch 寫 log
- [ ] Disconnect 時停 timer；Connect 重複按不再新建 timer
- [ ] CSV 改記 **BForce（回饋）+ BForceRef + APos + 時間戳**，先提高到 ≥ 50 Hz 並用常駐 StreamWriter
- [ ] 移除 EFEM 死碼（Robot/LoadPort/Aligner/Core/OCR/Stage/ForkView/AlignerView/WaferAligner/AdamIOForm/Module/*，除了仍被用的 `XML.cs`），修正 `AML_LogControl.dll` HintPath（放到 `lib/` 資料夾）
- [ ] 所有 hard-code（IP、Modbus 位址、接觸位置 210000、速度、存檔路徑）搬到設定檔

### Phase 1 — 硬體抽象層 + 執行緒所有權（2~4 週）
- [ ] 新增類別庫專案 `Bondwave.Hardware`：
  ```csharp
  interface IMotionAxis  { Task MotorOnAsync(); Task MoveAbsAsync(int pos, int vel, CancellationToken ct);
                           Task WaitInPositionAsync(TimeSpan timeout, CancellationToken ct); Task StopAsync(); }
  interface IForceAxis : IMotionAxis { Task EnterForceModeAsync(); Task EnterPositionModeAsync();
                           Task RunForceProfileAsync(ForceProfile p, CancellationToken ct); }
  interface IVacuum      { Task SetAsync(VacuumChannel ch, bool on); Task SetBondHeadSetpointAsync(int kPa); }
  ```
- [ ] `AkribisMotionService`：專屬 thread 擁有 `AGM800`，內部 command queue；輪詢合併成**一次** `SendBulkCommandString`（APos;AVelRef;AIq;AMotionStat;BForce;BForceRef;BPos）減少往返
- [ ] `ModbusVacuumService`：專屬 thread，Modbus I/O 完全離開 UI
- [ ] **Simulator 實作**（`SimMotionService`, `SimVacuumService`）：沒有機台也能開發 UI/流程、寫單元測試
- [ ] 確認 AAMotion 執行緒安全性 & `AGM800(updateMs)` 內部 AllStatUpdate 的行為，決定是否直接讀 `axis.Pos`（快取值）或自己 bulk query
- [ ] 驗證 `AGM800` 類別用在 AGD301/AGC301 是否為原廠建議（詢問 Akribis）

### Phase 2 — Bond Sequencer + Recipe（2~3 週）
- [ ] Recipe 檔（YAML，專案已有 YamlDotNet）：
  ```yaml
  name: test_300g_30s
  approach: { a_safe_pos: 0, a_contact_pos: 210000, a_speed: 300000 }
  touchdown: { method: slow_approach, speed: 500, force_threshold_g: 20 }   # ForceCurr_SetSlowApproach
  force_profile:            # B 軸力量曲線
    - { force_g: 100, ramp_ms: 30, hold_ms: 2000 }
    - { force_g: 300, ramp_ms: 30, hold_ms: 30000 }
  release: { bh_vacuum_off: true, bh_blow_ms: 200 }
  capture: { swir: { fps: 200, pre_trigger_ms: 500, post_ms: 5000 } }
  ```
- [ ] `BondSequencer` 狀態機：`Idle → PickDie → MoveToSafe → Align(bottom cam) → Approach → Touchdown → ForceProfile → Release → Retract → Done / Aborted / Fault`
  - 每一步有 precondition（interlock：motor on、真空壓力到位、B 在 position mode…）、timeout、失敗處理
  - Abort 路徑統一：B 停 → 切 position mode → A 回安全位 → 依設定處理真空
- [ ] 接觸偵測改由**控制器端**完成：`ForceCurr_SetSlowApproach` / `SetPosThForAutoSwToPosMode` / `SetForceModeSWTrigSrc`，不要靠 PC 輪詢力量再下 Stop（PC 端 20 ms 延遲對音圈來說太慢）
- [ ] 力量 profile 結束改用控制器狀態判斷（MotionStat / GenData 旗標 / `WaitGenData`），而非估計總時間

### Phase 3 — 資料擷取與共同時間軸（2 週）
- [ ] 力量/位置曲線改用**控制器端 recorder**（`StartLogData`/`StopLogData` 或 `SlowLogger`，參數 `RecGap`）以伺服週期取樣，bond 結束後 upload；PC 輪詢只用於 UI 顯示
- [ ] 每次 bond 建立一個 run 資料夾：
  ```
  runs/2026-11-09_143012_<recipe>/
    recipe.yaml        # 當次實際參數 (含軟體版本、控制器 .par checksum)
    motion.csv         # t_us, APos, BPos, BForce, BForceRef, mode   (controller recorder)
    vacuum.csv         # t_us, BH_kPa, CH_state
    events.jsonl       # 狀態機事件：touchdown、profile_start、release… 皆帶時間戳
    swir/              # 影像序列 (每張帶 frame_id + timestamp)
    bottom/            # 對位影像與結果
  ```
- [ ] **時間同步策略**（最重要的設計決定）：
  1. 首選：控制器 Digital Out 在 `profile_start` 時打 pulse → **硬體觸發 SWIR 相機**（或接到相機 GPIO 做 frame 標記），影像與力量曲線以同一事件對齊
  2. 次選：相機 SDK 提供的 hardware timestamp + PC `Stopwatch` 對時，事件檔記錄對應關係
- [ ] 圖表換成 **ScottPlot**（或 LiveCharts2），資料來源是 recorder buffer 而不是 TextBox；X 軸為真實時間 (ms)

### Phase 4 — 相機整合（依相機型號，3~6 週）
- [ ] `ICamera`：`Configure(exposure, gain, roi, fps, triggerMode)`、`StartAcquisition()`、`IObservable<Frame>`、`Stop()`
- [ ] 每台相機一條擷取 thread（通常是 SDK callback），frame 進 **固定大小 ring buffer（預配置記憶體，避免 GC）**
  - 消費者 1：Disk writer thread（raw/TIFF 序列，SSD；SWIR 高 fps 時要估算頻寬 = 解析度 × bit depth × fps）
  - 消費者 2：Preview（降採樣到 15~30 fps 給 UI）
  - 消費者 3：(之後) 線上分析
- [ ] **Bottom camera**：對位流程 — 找 die / 基板 fiducial → 計算 X/Y/θ offset → (若之後有 XYθ 平台) 補正；先做「量測 + 記錄」即可
- [ ] **SWIR camera**：bond wave 觀察
  - 支援 pre-trigger（ring buffer 保留觸發前 N ms）
  - 背景 frame（接觸前）存一張，用於後續差分
  - 離線分析工具（Python 較適合）：差分影像 → bond wave front 擷取 → 擴散速度 / 面積 vs 時間，與 `motion.csv` 的力量曲線對齊；之後再考慮搬到線上
- [ ] 影像處理函式庫：OpenCvSharp4（C#）或另開 Python 分析流程讀 run 資料夾

### Phase 5 — 平台現代化（可與 Phase 1~4 並行或最後做）
- [ ] .NET Framework 4.5 → **.NET 8**（Agito.AAMotion 6.0.2 已附 net8.0 版 DLL；EasyModbusTCP 需確認，或改 NModbus — bin 裡已有 NModbus.dll）
- [ ] UI：維持 WinForms（遷移成本最低）或改 WPF + MVVM（資料綁定、影像顯示較好）。建議：**先 WinForms on .NET 8**，等 HAL/Sequencer 穩定後 UI 層再決定
- [ ] Logging：`AML_LogControl.dll` → Serilog / NLog（結構化 log、非同步寫檔、不再有 LogError.txt 裡的檔案鎖衝突）
- [ ] 設定：`Setting.xml`（EFEM 內容）→ `appsettings.json` + recipe 檔

---

## 3. 建議專案結構（重構後）

```
Bondwave/
├── src/
│   ├── Bondwave.Core/          # Domain: Recipe, ForceProfile, Snapshot, 狀態機定義 (無硬體依賴)
│   ├── Bondwave.Hardware/      # HAL 介面 + Akribis / Modbus / Camera 實作 + Simulator
│   ├── Bondwave.Sequencing/    # BondSequencer, interlocks
│   ├── Bondwave.Data/          # RunRecorder, writers (CSV/影像)
│   └── Bondwave.App/           # UI (WinForms/WPF)，只依賴上面的介面
├── tests/
│   ├── Bondwave.Core.Tests/
│   └── Bondwave.Sequencing.Tests/   # 用 Simulator 跑完整 bond 流程
├── analysis/                   # Python：bond wave 影像分析、曲線比對
├── config/ recipes/
├── vendor/                     # AAMotion、AML_LogControl 等第三方 DLL、.par 備份
└── docs/  (BONDWAVE_AI_CONTEXT.md, REFACTOR_PLAN.md)
```

---

## 4. 優先順序建議

| 優先 | 項目 | 理由 |
|---|---|---|
| ★★★ | Phase 0 全部（尤其 A 軸控制器極限、關閉流程、Go Abs bug）| 人身/設備安全，成本低 |
| ★★★ | Phase 1 Simulator | 之後所有開發不再綁機台 |
| ★★☆ | Phase 3 時間同步設計 | **要在買/接相機前決定**（是否需要硬體觸發線、相機要支援 trigger in） |
| ★★☆ | Phase 2 Sequencer + 控制器端接觸偵測 | 力控結果重現性的關鍵 |
| ★☆☆ | Phase 5 .NET 8 | 可延後，但越晚改 code 越多 |

---

## 5. 待確認問題（Open Questions）

1. SWIR 相機與下相機的**型號 / 介面**（GigE Vision、USB3 Vision、CoaXPress?）、需要的 fps 與解析度 → 決定磁碟頻寬與 SDK。
2. 相機是否支援**硬體觸發輸入**？控制器 Digital Out 是否有空閒腳位可接？
3. Bond head 之後會不會加 X/Y/θ 軸（對位補正）？會影響 HAL 與 Sequencer 設計。
4. 力量單位：UI 標 g，控制器是 force-units（gain −0.21286 /mV）— 是否已校正為 g？校正紀錄放哪？
5. Modbus HR0 讀回的是設定值還是實際壓力？真空控制器型號 / register map 文件。
6. `AGendata[10]=20303` 每次輪詢都寫入的用途？（可能是控制器 user program 的旗標）
7. 是否還需要 EFEM 相關功能（Robot/LoadPort 上下料）？若無即可整批刪除。
8. 控制器跑的是 FW **ALPHA** 版（3.0.0.98-141），是否有正式版可升級？
