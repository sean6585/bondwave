# CLAUDE.md — Bondwave 實驗基台

> 給在這個 repo 工作的 Claude 讀。描述**硬體現況、安全邊界、已知問題**。
> 程式碼細節請直接看原始碼；這份文件記錄的是看程式碼看不出來的東西。
> 最後更新：2026-11-26（11/23 相機控制介面與實機測試 §2.6 / §8.3；11/26 韌體層保護選項 §3.4、更換 AGD301、新增 IAI 下相機滑軌 §2.0）

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
├── Modbus TCP ────── 真空/壓力控制器 172.1.1.100:502（2026-11-26 ping 不到）
├── EtherNet/IP ───── IAI PCON-CB-35PWAI-EP-0-0（下相機滑軌）IP 未知，2026-11-26 尚未連上
│                      └── IAI RCP6-TA4C-WA-35P-2.5-25-P3-S-B
└── PCIe ──────────── Euresys Coaxlink Quad CXP-12 Value (PC3623)
                       └── CXP ×2 ── Cheetah+ 640 CXP SWIR 相機
```

> **2026-11-26 換了一台 AGD301**：IP 一樣 `172.1.1.101`，但 MAC 不同（舊 `…0E-FA` → 新 `…43-48`）。§2.1–§2.4、§3.2 的「控制器現值」都是**舊那台**讀的，新控制器的參數要重新讀（`tools/CommTest --probe`）。

### 2.0 下相機滑軌（IAI，2026-11-26 加入，**尚未通訊、尚未設定**）

| 項目 | 型號 / 推讀（依 IAI 命名規則，待型錄確認） |
|---|---|
| 電動缸 | `RCP6-TA4C-WA-35P-2.5-25-P3-S-B`：RCP6 步進馬達 RoboCylinder、小型平台款、**WA = 免電池絕對式編碼器**、□35 步進馬達、**導程 2.5 mm**、**行程 25 mm**、P3 世代控制器、線長 3 m、**B = 附煞車** |
| 控制器 | `PCON-CB-35PWAI-EP-0-0`：PCON-CB（步進馬達控制器）、搭配 35P 馬達 + 免電池絕對式編碼器、**EP = EtherNet/IP**、24 V |
| 電腦端控制方式 | EtherNet/IP：**電腦要當 scanner（originator）**，用 C# EtherNet/IP 函式庫做 implicit I/O（Class 1）或 explicit messaging；控制器端要設定 fieldbus 運作模式（遠端 I/O / 位置直接指定等）與 IP。**不是 Modbus/TCP**，repo 內的 EasyModbus 不能直接用。設定 / 讀警報碼用 IAI 的 PC 軟體（IA-OS / RC PC Software）+ USB 轉接線，或教導器 |
| 現況 | 控制器 ALM 紅燈（警報碼未讀到）。從 IPC 的「乙太網路」掃 172.1.1.0/24 與 EtherNet/IP ListIdentity 廣播都**沒有回應** → 不在這條網路上，或 IP 在別的網段 |
| 安全 | 附煞車、可能垂直安裝；絕對位置重置 / 回原點時軸**會自己移動**，操作前淨空 |

### 2.1 控制器 AGD301-ET-2D09-001

- 3 軸控制器 + **內建 3 組驅動放大器**（這點與 AGC301 不同，AGC301 要外接驅動器）
- 控制迴路 16 kHz / 放大器 PWM 32 kHz
- 每軸連續 9 Arms、峰值 18.2 Arms（3 軸合計限 20 Arms）
- 電源 12–90 VDC
- **類比輸入 4 路 16-bit**（`-001` 變體才是 16-bit，`-2D05` 是 12-bit）
- 隔離輸入 27 / 隔離輸出 17 / 雙向差動 I/O (RS422) 8
- 編碼器 3 port，軟體可設 AquadB / Sin-Cos 1Vpp / BiSS-C / EnDat2.2

**SDK**：`AAMotion.dll` (Agito.AAMotion 6.0.2 NuGet)。套件內含 `net40` / `net48` / `net6.0` / `net8.0` 版本 DLL。

- **AGD301 應使用 `AGC301` 類別**：SDK changelog v5.1.1 寫明「Rename Class AGD301 to AGC301」。2026-11-13 實測 `new AGC301(50).Connect("172.1.1.101")` 成功（約 0.3 s）。
- 舊主程式用的是 `AGM800` 類別（另一款 8 軸 Central-i 控制器的類別），歷史上也能連，但不是對應型號。新程式請用 `AGC301`。
- 兩者都繼承 `MotionController`；`AGC301` 的欄位是 `A` / `B` / `C`（`ISingleAxis`）與 `IO`（`IIOModule`）。
- SDK 會在背景啟動 `AACommServer.exe`。程式結束時若沒呼叫 `CloseAACommServer()`，它會殘留並鎖住 bin 資料夾的檔案（重新建置會失敗）。

**控制器實測識別（2026-11-13，typed 屬性）**：ProductType=11、FirmwareVersion=4444、HardwareVersion=1001、BootFirmwareVersion=1013、NumberOfAxes=3、MaxContCurrent=12750 mA（≈9 Arms）、MaxPeakCurrent=25500 mA（≈18 Arms）、Bus 12–90 V、CurrentLoopSampleRate=16384 Hz、SerialNumber/ManufacturingDate=-1（未讀到）。上電時 VBus≈55.5 V、VLogic≈5.1 V。

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
>
> ⚠️ **2026-11-13 實測：控制器裡的 Z1 參數仍是舊值**（MotorType=4、ContCL=1630、PeakCL=4800、BrakeUsed=1、BrakeMode=2、FwdPLim/RevPLim=±1.6e9），與 `202408081637.par` 完全相同。換馬達後還沒改過。

**上電後的位置讀值（2026-11-13）**：Z1 Pos≈130,496,811、Z2 Pos≈130,636,895（兩軸都約 1.3 億 counts，原因未明：可能是絕對式編碼器讀值或計數器初值）。
- Z2 的 Pos 超過 `BFwdPLim=16,000,000`，所以 Z2 `LimitStat` 一上電就是 0x2（推測為軟體正向極限旗標）。
- 舊主程式的 Z1 保護條件 `Pos > 800000` 在這個讀值下會**一連線就觸發**。改主程式前要先釐清位置原點。

**手推編碼器（馬達 Off，2026-11-13）**：兩軸 Pos 都會跟著變化 → 編碼器接線有通。Z1 淨變化 +1,855 counts、Z2 淨變化 −162,935 counts。**方向與每 mm counts 尚未量測**（沒有記錄實際推的距離）。

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
- 控制器端參數（`AAInMode[2]=0x20003` = 軸 B、Force Feedback）：

  | 參數 | `202408081637.par` | **控制器現值（2026-11-13）** |
  |---|---|---|
  | `AAInGain[2]` | −13950（= −0.21286 /mV） | **+13950 ⚠ 正負號相反** |
  | `AAInOffset[2]` | −456（PCSuite 截圖顯示 −390 mV） | **0** |

  有人改過這兩個值（可能是調 TM3 時）。**正確的正負號確認前，不可進入力控模式**：符號錯會變成正回授，越壓越大力。
- 2026-11-13 讀到 AI2 原始值 = 0、`BForce` = 0（其他 AI 有小幅變動）→ TM3 可能沒上電或 AI#2 沒接到，待查。
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
- 相機參數用 **`AcquisitionFrameTime`（µs 週期，2026-11-19 使用者確認單位）**，不是 FrameRate。`FPS = 1,000,000 ÷ FrameTime`。約束：`ExposureTime ≤ AcquisitionFrameTime`。1700 fps → FrameTime 588。
- **`AcquisitionFrameTime = 0` 不是有效值**：`TriggerMode = Off`（自由連拍）時相機依 FrameTime 拍，設 0 會完全沒有影像。
- **相機上電預設**（2026-11-23 讀到）：`PixelFormat = Mono16`（每張 655,360 B，1700 fps = 1.1 GB/s）、`TriggerMode = On`、`TriggerSource = Timer`、`AcquisitionFrameTime = 0`、`ExposureTime = 500`。Timer 相關參數只有 `TimerUpTime`、`TimerLifeTime`（數值每次讀都不同，像計數器，不是週期設定）。**錄影前要先切 Mono8**（`tools/CameraUI` 連線時自動切）。
- ✅ **FPS 控制方式已確認（2026-11-23 實測）**：保持上電預設的 `TriggerMode=On` + `TriggerSource=Timer`，**只改 `AcquisitionFrameTime` 就決定週期**。`AcquisitionFrameTime=588` → 實測 1700.7 fps；`8333` → 120.0 fps。不需要改 Trigger。（FrameTime=0 時仍會出圖但 FPS 不受控，這解釋了之前「FrameTime 0/1 也有畫面」的現象。）
- 曝光與亮度：同一場景（2026-11-23）曝光 5000 µs 平均灰階約 13；**曝光 450 µs 時幾乎全黑**（平均 0.0、最大 8–10）。1700 fps 錄影需要更強的 IR 光源或相機增益。
- 設定順序：先降 `ExposureTime`、再縮 `AcquisitionFrameTime`；改完要在 Studio 停止 → 重新開始擷取。
- Memento 持續報 `CoaXPress Warning: Unexpected data width in pixel format (0x0007)` — grabber 不認得相機送的格式代碼，**未解**。目前設 Mono8 時資料量正確（327,680 bytes/frame）
- Memento 開 Verbose 時每張影像會產生數十行 log，1700 fps 下會吃 CPU/磁碟。正式錄影請調回 Warning / Info。
- C# 整合：`C:\Program Files\Euresys\eGrabber\clr\anycpu\EGrabber.NET.dll`（.NET 6+，含 `EGrabber` 與 `EGrabberRecorder`）；`clr\x86_64\EGrabber.NETFramework.dll` 給 .NET Framework。2026-11-23 的安裝**沒有 samples 資料夾**，API 以 `EGrabber.NET.xml` 與 `doc\egrabber-recorder-reference` 為準。已實作：`tools/CameraUI`（§8.3）。
  - `Pop(timeout)` 逾時丟 `GenTLError`（`GcError == GC_ERR_TIMEOUT`）；`ScopedBuffer` Dispose 時自動歸還 buffer
  - `Recorder` 要求容器資料夾**先存在**，否則 `INVALID_CONTAINER_PATH`；`CONTAINER_SIZE` 單位 bytes；`ReadInfo()` 不會自動跳下一筆（每次先設 `RECORD_INDEX`）

#### 2.6.1 1700 fps 錄影（2026-11-19 驗證通過）

**頻寬需求**（Mono8 640×512）：1700 × 327,680 B ≈ **557 MB/s**。

| 環節 | 實測 / 規格 | 結論 |
|---|---|---|
| CXP 鏈路 | 2×CXP-6 = 12.5 Gbps | 足夠 |
| Coaxlink PCIe | 實際連線 **Gen3 x8**（裝置管理員屬性） | 足夠 |
| grabber → RAM | Memento：`CIC lost = 0`、`frame store filled with 0.0 MB`、`SourceTag` 連號 | 不掉張 |
| **C: 磁碟** | `96FD25-S4TB-AD1`，**SATA SSD**，連續寫入實測 **344 MB/s**（8 GB、WriteThrough） | **不夠持續寫 557 MB/s** |
| RAM | 32 GB | 可當緩衝 |

**資料流**：相機 → Coaxlink（板上只有小 FIFO）→ DMA 到 **PC RAM 的 Buffer queue** → Recorder 邊錄邊寫 container。磁碟跟不上時影像在 RAM 排隊；RAM 塞滿才會掉張。

**驗證通過的設定**：
- eGrabber Studio → 左側第 2 個圖示（Buffer queue）→ Replay 1000 + **Additional 9000** = 10,000 buffers（≈3.3 GB）→ Reallocate（要先停止擷取）
- Recorder container 5000 MiB（= 16,000 張 = 1700 fps 下 9.4 s），目錄 `C:\W`
- 結果（chapter `SeanTest3`）：**16,000 張、第一→最後 9.407263 s、1700.71 fps、0 缺張**；間隔 588 µs ×15,782、587 ×183、589 ×34
- 寫完要約 15 s（磁碟速度），這是正常的，不代表掉張。之前用 2000 buffers 量到「約 1067 fps」其實是磁碟速度，不是相機速度。

**錄影長度估算**（推算，未實測）：RAM 累積速度 ≈ 557 − 344 ≈ 213 MB/s，約 20 GB 可用 buffer → 邊錄邊寫約 90 s；只存 RAM 約 36 s。要更長或連續錄影 → **加一顆 NVMe SSD**（PCIe 4.0、持續寫入 > 1 GB/s）專門錄影。

**時間戳**：
- Studio Playback（REC → Close container → Playback → ⓘ）顯示的 `Timestamp` 單位是 **ns**，實際精度 1 µs（尾數都是 000）。Record 編號從 0 開始。
- 時鐘是開機/擷取卡啟動後累計值（約 8.9e12 ns ≈ 2.5 h），**不是牆上時間**；只能拿來算差值。Memento 的 `readout started at ... us` 是同一個時鐘（µs）。
- 檢查缺張：`tools/RecorderCheck/Check-Recording.ps1`（見 §8.3）。

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
| A 軸硬體限位 | 2026-11-13 讀控制器：**沒有任何數位輸入（DInMode）指定給 Axis A**。Z1 限位即使接在 X4，控制器也不會當成限位 |
| B 軸硬體限位 | `ADInMode[7]=0x20009`、`ADInMode[8]=0x2000A` → 軸 B、功能 9 = **REV_LIMIT**、10 = **FWD_LIMIT**（功能碼見 §3.4 `eDInMode`；`ADInLog=192` 反相）。實測推 Z2 到底時 IN7 跳變、`BLimitStat` bit0 立起 → **IN7 = Z2 反向限位，控制器有認到**。IN8（正向）未測 |
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

### 3.4 控制器（韌體）層的「阻力過大」保護選項（2026-11-26 查證，**尚未設定、尚未實測**）

馬達本體（線性馬達 / 音圈）是被動元件，**沒有內建停止功能**；能自己停的是 AGD301 韌體（16 kHz 迴路，反應在 0.1 ms 等級，不經過 PC）。可用的機制：

| 機制 | 參數 / API | 觸發後行為 | .par 現值（舊 Z1 時代） |
|---|---|---|---|
| 電流上限（= 推力上限，物理硬上限） | `CurrLimFwd` / `CurrLimRev`、`PeakCL`、`ContCL` | 推力不會超過 Kf × I_lim | A/B 都是 4000 |
| 位置誤差過大 → 故障斷電 | `MaxPosErr` → `CON_FLT_HIGH_POS_ERR`(1020) | 被擋住時誤差累積超過門檻即 fault | 150000 counts（很大，等於很晚才觸發） |
| 卡死偵測 | `StallCfg` / `StallThPcnt` → `CON_FLT_MOTOR_STUCK`(1007) | fault | **`StallCfg=0`（關閉）**、50% |
| 過熱 / I²t | `MaxMotorTemp`、`CON_FLT_MOTOR_I2T`(1044) | fault | 80 °C |
| **接觸自動切力控**（最適合 bond） | `ForceCurr_SetForceModeSWTrigSrc(forcePosErrTh, forceAInTh)` | 位置誤差 > 門檻 **或** AI（load cell）> 門檻時，韌體自動從位置模式切到力控，壓力被限制在 force profile | 未設定 |
| 接觸自動切電流模式 | `ForceCurr_SetCurrModeSWTrigSrc(currPosErrTh, currCurrTh, currCurrThDir, currAInTh)` | 位置誤差 / 電流 / AI 超過門檻 → 改為定電流（定推力） | `BCurrPosTh=146000`，其餘 0 |
| 力量誤差保護 | `ForceCurr_SetForceRefFilt(forceRefFilt, maxForceErr, maxForceErrOL, forceRelFOn)` | 力控時誤差過大 → 保護 | `MaxForceErr=2000`、`MaxForceErrOL=50000`（預設） |
| 慢速接近 | `ForceCurr_SetSlowApproach(speedChgPos, speedChgDir, speedChgNew, speedChgOn)` | 到指定位置自動降速，降低撞擊力 | 未設定 |
| 硬體輸入立即停 | `DInMode` 功能 4 = STOP_MOTION、8 = ABORT_MOTION、11 = TORQUE_LIMIT_ON | 外部開關 / 比較器訊號 → 韌體直接停 | 未使用 |
| 類比輸入限扭力 | `AInMode` 7 / 8 = ANALOG_INPUT_REV/FWD_TORQUE_LIMIT | 用類比電壓即時限制電流 | 未使用 |

- `eDInMode`：0 USER_INPUT、1 DEDICATED_HW、2 MOTOR_ON、3 BEGIN、4 STOP_MOTION、5 CLEAR_IN_PULSES、6 ABORT_RESUME、7 ALARM_RESET、8 ABORT_MOTION、9 REV_LIMIT、10 FWD_LIMIT、11 TORQUE_LIMIT_ON、12 DYNAMIC_BRAKE、13 LOCK_STATIC_BRAKE、14 CONTROL_SET_CHANGE、15 ADD_FILTER、16–18 MODE_SWITCH、19 CLEAR_ABS_ENC、20 CHANGE_SPEED、21 HOME。
- `eAInMode`：0 USER、1 VELOCITY_CMD、2 CURRENT_CMD、3 FORCE_FEEDBACK、4 FORCE_CMD、5 JOYSTICK、6 TORQUE_COMP、7 REV_TORQUE_LIMIT、8 FWD_TORQUE_LIMIT。
- 來源與文件位置（repo 內有三份 CHM，**版本不同**）：
  - ✅ `AML Bond wave 1107/packages/Agito.AAMotion.6.0.2/content/AAMotion_Documentation.chm` — **v6.0.2，與程式用的 DLL 同版，查 API 用這份**。有 `ISingleAxis.ForceCurr_*` 各方法頁，但頁面**只有函式簽章**，說明欄只列出它寫入的控制器參數名（例如 `ForceCurr_SetForceModeSWTrigSrc` → `ForcePosErrTh, ForceAInTh`），對應屬性頁標示「Missing documentation」。
  - `AKRIBIS/AAMotion_Documentation.chm`、`AML EFEM Control System/AAMotion_Documentation.chm` — v4.21.8 舊版，沒有 ForceCurr。`eDInMode` / `eAInMode` / `ConFaultEnum` 兩版都有。
  - `AGMotionManual.docx` — 另一套 `AG_*` 函式（AGMotion namespace）的中文手冊，不是 ISingleAxis。
  - 從網路下載的 CHM 開啟後若內容空白：檔案右鍵 → 內容 → 勾「解除封鎖」。
- 結論：ForceCurr 各參數的**單位、比較方向、觸發後的確切行為官方 SDK 文件沒寫**，要向 Akribis 原廠要控制器端的參數 / 指令參考手冊（keyword reference），並小行程實測。
- 前提：用 AI（load cell）當觸發源之前，必須先修好 §2.4 的 AI2 = 0 與 `AAInGain[2]` 正負號問題。

### 3.5 指令時序注意

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
  - ⚠️ 這段程式碼**不在 GitHub repo 裡**（repo 只有 1107 snapshot），最新主程式還在別處，合併前別在 repo 版本上大改
- **2026-11-13** PC ↔ AGD301 通訊確認（`tools/CommTest`，AGC301 類別，心跳往返 < 1 ms）；兩軸 MotorOn=0、ConFlt=0 "No error"
- **2026-11-13** 手推兩軸，編碼器讀值有反應；Z2 限位 = IN7 已確認
- **2026-11-19** 相機 1700 fps 錄 16,000 張 0 缺張（§2.6.1）

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

- [ ] **Z1 (`APM-XB04ADK3`) 規格**：幾相？額定/峰值電流？編碼器型式？——決定 `MotorType`/`ContCL`/`PeakCL`/`EncType`（控制器目前仍是舊 Z1 值）
- [ ] **Z1 有無電磁煞車？**——決定能否 servo off 當緊急停止、決定 commissioning 安全程序。舊參數 `ABrakeUsed=1`，代表前一顆有煞車；手推 Z1 推得動 → 新 Z1 可能沒煞車或煞車沒接，待確認
- [x] ~~X4 上的 Z1 限位有沒有映射到 Axis A？~~ → **沒有**（2026-11-13 讀 DInMode，無任何輸入指定給 Axis A）。待辦：找出 Z1 限位接在哪個 IN、再設定 DInMode
- [ ] 推 Z1 時 DInPort **bit 28（IN29）** 也跳變，AGD301 只有 27 個隔離輸入 → 是哪個訊號？會不會是 Z1 限位？
- [ ] 兩軸**方向**與**每 mm counts**（手推已知距離量測）
- [ ] 上電 Pos ≈ 1.3 億 counts 的原因（絕對式編碼器？）、要不要做 Home / 設原點
- [ ] `AAInGain[2]` 正確正負號、`AAInOffset[2]` 為何被改成 0；AI2 讀值 = 0 的原因（TM3 電源 / 接線）
- [ ] `EncType = 6` 對應哪一種編碼器？（影響 ENC pin 3/4、9/10 的意義）
- [ ] 力量單位：UI 標 g，控制器是 force-unit。**兩者換算未確認**，所以過壓門檻那個 `500` 目前意義不明 —— 需用已知砝碼實測建立對照
- [ ] `AGendata[10]=20303` 每次輪詢都寫入的用途
- [ ] Modbus HR0 讀回的是設定值還是實際壓力
- [ ] 相機 pixel format `0x0007` 不相容問題（需問 Exosens 原廠或用 Xeneth 對照）
- [x] ~~1700 fps 時實際使用的觸發設定~~ → Trigger On + Timer（上電預設）+ `AcquisitionFrameTime=588`（§2.6）
- [ ] 1700 fps（曝光 ≤ 570 µs）畫面太暗：IR 光源加強 / 相機增益設定
- [ ] 長時間錄影：實測 RAM 緩衝上限（Buffer 再加大是否能鎖定成功）或加裝 NVMe
- [ ] **新 AGD301（2026-11-26 更換）參數重讀**，並與舊 `.par` 比對
- [ ] **IAI 下相機滑軌**（§2.0）：讀 ALM 警報碼並排除；確認 PCON-CB 的 EtherNet/IP IP 位址與實體接線；選定 C# EtherNet/IP 函式庫與控制器 fieldbus 運作模式

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

---

## 8. 開發環境與工具（2026-11-13 ~ 11-19 建立）

### 8.1 這台 PC

| 項目 | 內容 |
|---|---|
| 主機 | Windows 10 Pro，i9-14900，32 GB RAM，Intel UHD 770 |
| 磁碟 | C: `96FD25-S4TB-AD1` **SATA SSD** 3.7 TB（連續寫入 ~344 MB/s） |
| repo 位置 | `C:\Users\User\Bondwave`（登入帳號 `user` 對 `C:\Users\CHANGJSD\...` 只有讀取權限，所以 clone 在這裡） |
| 工具 | Git 2.56（winget 安裝，`C:\Program Files\Git\cmd\git.exe`，未加入目前 shell 的 PATH 時用完整路徑）、.NET 8 SDK 8.0.403 |
| GitHub | `sean6585/bondwave`；工作分支 `feature/comm-test-tool`；push 需 Git Credential Manager 瀏覽器登入（已登入過） |

### 8.2 網路

| 網卡 | 用途 | 設定 |
|---|---|---|
| 「乙太網路」Intel I210 | **接 AGD301** | 固定 IP `172.1.1.10/24`，無閘道（Windows 預設 DHCP 會變 169.254.x.x → SDK 回 `EthernetInterfaceMismatch`） |
| 「乙太網路 2」Intel I219-LM | 外網 | DHCP 192.168.123.x，不要動 |

AGD301：`172.1.1.101`（ping < 1 ms）。真空控制器 `172.1.1.100:502` 在同網段（本次未測）。

### 8.3 repo 內工具

**`tools/CommTest/` — 唯讀通訊測試（.NET 8 WinForms）**
- 用途：通電前/後確認 PC ↔ AGD301 ↔ Z1/Z2 通訊、編碼器、DIn/AIn、參數比對。
- **刻意唯讀**：程式碼內沒有 MotorOn/Move/Jog/Home/Stop/寫參數；手動查詢只接受白名單名稱（`.par` 參數名 + 狀態名）且不可含 `=`。`--selftest` 驗證白名單規則。**修改這個工具時要維持唯讀**。
- 執行：`dotnet run --project tools/CommTest/CommTest.csproj`；Log 在 `bin/Debug/net8.0-windows/logs/`（含 `PARAM` / `SNAP` / `RANGE` 行，gitignore 排除）。
- `--probe [IP]`（2026-11-26 新增）：不用點畫面，唯讀連線 → 識別資訊、參數（含 `MaxPosErr` / `StallCfg` / `ForcePosErrTh` 等保護參數）、一筆兩軸狀態 → 斷線，結果寫 `probe.txt`。**新 AGD301 還沒跑過**。
- 第一次連線常回 `AACommServerNotRunning`（SDK 剛啟動 `AACommServer.exe`、還沒在 `127.0.0.1:399` 監聽）。11/26 起遇到會自動等 2 秒重試一次。
- 已知：DInPort 的 IN n 對應 bit n−1（由 Z2 限位實測反推）；`ADInMode` 等值的高 16 位 = 軸（1=A、2=B）、低 16 位 = 功能碼（§3.4 `eDInMode`，已由 SDK 文件確認）。

**`tools/RecorderCheck/Check-Recording.ps1` — 錄影缺張檢查**
- `powershell -ExecutionPolicy Bypass -File tools\RecorderCheck\Check-Recording.ps1 [-Path C:\W] [-ExpectedFps 1700]`
- 讀 container 的 `index`，算 FPS、間隔分布、缺張位置。回傳 0 = 無缺張、1 = 有異常。
- `index` 格式為反推（eGrabber Studio 26.10）：檔頭 8 bytes（magic `0x6EBABEBB`）、每張 216 bytes、時間戳在 +0x28（ns）。格式不符會報錯，不會亂給結果。

**`tools/CameraUI/` — 相機控制介面 BondwaveCamera（.NET 8 WinForms，x64）**
- 即時畫面（對焦，預設 120 fps + 對焦指標）、單張 TIFF、錄影（預設 1700 fps，eGrabber container，可設秒數）、回放、FPS / 缺張分析、匯出 TIFF、相機參數讀寫。
- `CameraService` 獨佔 EGrabber 物件於專屬執行緒（符合 §7 的 Actor 原則），之後併入主控程式時直接搬。
- FPS 控制方式三選一 + 自動偵測；實測第一種（只改 `AcquisitionFrameTime`）就成功，已存為預設。
- 2026-11-23 實機測試（`--hwtest`）：連線 → 自動切 Mono8 → 偵測 1700.7 fps ✔ → 即時 120.0 fps 0 缺張 ✔ → 單張 TIFF ✔ → 錄影 2 s 3,402/3,402 張、1700.71 fps、0 缺張、讀回一致 ✔（磁碟寫完 3.3 s）。GUI 按鈕走的是同一個 `CameraService`，但 GUI 本身尚未由人實際操作驗證。詳見該資料夾 README。

### 8.4 給 AI 的工作提醒

- 這台 PC 的 shell 是 Windows PowerShell 5.1：`.ps1` 含中文要存成 **UTF-8 with BOM**；`git commit -F -` 搭配 here-string 會失敗，請用訊息檔 `-F <file>`。
- 執行中的 CommTest 會鎖住 `bin/`，建置前請使用者關閉視窗；殘留的 `AACommServer.exe` 是本工具產生的，可安全結束。
- 相機與 eGrabber Studio 的操作都由使用者在 GUI 上做；AI 只能讀截圖、Memento log 或錄影檔分析。
