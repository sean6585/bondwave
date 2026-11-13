# Bondwave 通訊測試工具（唯讀）

確認 PC ↔ AGD301-ET-2D09-001 ↔ Z1 (APM-XB04ADK3) / Z2 (AVM90-15) 的通訊、編碼器、I/O、類比輸入。

**本工具不包含任何 MotorOn / MotorOff / Move / Jog / Home / Stop / 寫參數 的呼叫。**
手動查詢只接受 `名稱` 或 `名稱[索引]`（不可含 `=` `;` 空白），名稱必須在白名單內（`reference.par` 參數名稱 + 已知狀態名稱），並排除含 MotorOn/Begin/Stop/Home/Jog… 等動作字眼的名稱。
`BondwaveCommTest.exe --selftest` 會驗證這些規則（結果寫到輸出資料夾的 `selftest.txt`）。

## 建置 / 執行（VS Code 或命令列）

需要 .NET 8 SDK。VS Code 開啟 `tools/CommTest` 資料夾，`Ctrl+Shift+B` 建置、`F5` 執行（需 C# 擴充套件）。命令列：

```
dotnet run --project tools/CommTest/CommTest.csproj
```

## 上電前準備

1. 接控制器的網卡（目前是「乙太網路」Intel I210）設固定 IP，例如 `172.1.1.10` / `255.255.255.0`，不設閘道。
2. 確認 Z1 已有機械支撐或煞車（本工具不會讓馬達通電，但手推軸測試時 Z1 是垂直軸）。

## 測試步驟

| # | 動作 | 判定 |
|---|---|---|
| 1 | 控制器上電，按「Ping / 網卡檢查」 | 網卡狀態 Up、與控制器同網段；Ping OK |
| 2 | SDK 類別選 **AGC301**，按「連線」 | 綠燈「通訊正常」、心跳往返 ms 穩定；若失敗再試 AGM800（舊主程式用的類別）並記錄差異 |
| 3 | 看「參數比對」分頁 | 記錄 ProductType / FirmwareVersion / SerialNumber；紅底 = 與 .par 舊備份不同 |
| 4 | 兩軸 `MotorOnStatus` 應為 0、`ConFlt` 應為 0 | 若 ConFlt ≠ 0，記下 ConFaultMsg |
| 5 | 按「設基準點」後，手慢推 Z1 / Z2 | `Δ` 有變化、方向正確（往上推 Δ 增或減，記錄下來）、推已知距離換算解析度 |
| 6 | 手按各限位開關 | 對應 IN 格變綠、外框橘色標記；滑鼠移到格子上看 DInMode 是否有指定給該軸 |
| 7 | 輕壓 load cell | AI2 數值有變化 |

Log 同時寫到輸出資料夾 `logs/commtest_yyyyMMdd.txt`，可直接附在測試紀錄。
