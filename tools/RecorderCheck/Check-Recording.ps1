<#
.SYNOPSIS
    檢查 eGrabber Recorder 錄影檔（container）有沒有缺張，並算出實際 FPS。

.DESCRIPTION
    讀 container 資料夾裡的 `index` 檔（每張影像一筆紀錄，含硬體時間戳 ns），
    計算第一張到最後一張的時間、平均 FPS、間隔分布，並找出異常間隔（= 缺張）。
    只讀檔案，不修改 container。

    index 格式是從實際錄影檔反推的（eGrabber Studio 26.10，Mono8 640x512 驗證過）：
      - 檔頭 8 bytes（magic 0x6EBABEBB + version 1）
      - 每筆紀錄 216 bytes：+0x00 影像 bytes、+0x08 寬、+0x18 高、+0x28 時間戳 (ns)
    若 eGrabber 版本更新導致格式改變，腳本會在檔頭或紀錄檢查時報錯，不會給出錯誤結果。

.PARAMETER Path
    container 資料夾（含 index / data 檔），預設 C:\W

.PARAMETER ExpectedFps
    預期 FPS（例如 1700）。不填則以間隔的中位數當作標準週期。

.PARAMETER TolerancePct
    間隔偏離標準週期多少百分比算異常，預設 50%（缺一張時間隔會變 2 倍）。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\Check-Recording.ps1
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\Check-Recording.ps1 -Path D:\rec\run01 -ExpectedFps 1700
#>
param(
    [string]$Path = 'C:\W',
    [double]$ExpectedFps = 0,
    [double]$TolerancePct = 50
)

$ErrorActionPreference = 'Stop'

$HeaderSize = 8
$RecordSize = 216
$Magic = [uint32]0x6EBABEBB
$OffSize = 0x00; $OffWidth = 0x08; $OffHeight = 0x18; $OffTimestamp = 0x28

$indexPath = Join-Path $Path 'index'
if (-not (Test-Path $indexPath)) { throw "找不到 $indexPath（-Path 要指向 container 資料夾）" }

$b = [IO.File]::ReadAllBytes($indexPath)
if ($b.Length -lt $HeaderSize + $RecordSize) { throw "index 檔太小（$($b.Length) bytes），沒有任何紀錄" }
if ([BitConverter]::ToUInt32($b, 0) -ne $Magic) {
    throw ("index 檔頭不符（0x{0:X8}），可能不是 eGrabber Recorder 檔或格式已改版" -f [BitConverter]::ToUInt32($b, 0))
}
if ((($b.Length - $HeaderSize) % $RecordSize) -ne 0) {
    throw "index 長度 $($b.Length) 不是 8 + 216×N，格式可能已改版"
}

$n = [int](($b.Length - $HeaderSize) / $RecordSize)
$ts = New-Object 'long[]' $n
$first = $HeaderSize
$imgBytes = [BitConverter]::ToInt64($b, $first + $OffSize)
$w = [BitConverter]::ToInt64($b, $first + $OffWidth)
$h = [BitConverter]::ToInt64($b, $first + $OffHeight)
$mismatch = 0
for ($i = 0; $i -lt $n; $i++) {
    $o = $HeaderSize + $i * $RecordSize
    if ([BitConverter]::ToInt64($b, $o + $OffSize) -ne $imgBytes -or [BitConverter]::ToInt64($b, $o + $OffWidth) -ne $w) { $mismatch++ }
    $ts[$i] = [BitConverter]::ToInt64($b, $o + $OffTimestamp)
}

Write-Host ""
Write-Host "Container : $Path"
Write-Host ("影像      : {0} 張，{1}x{2}，每張 {3:N0} bytes" -f $n, $w, $h, $imgBytes)
if ($mismatch -gt 0) { Write-Warning "$mismatch 筆紀錄的尺寸和第一筆不同（可能有多種設定混在同一個 container）" }
if ($n -lt 2) { Write-Host "只有 1 張，無法計算間隔"; exit 0 }

# 間隔
$d = New-Object 'long[]' ($n - 1)
for ($i = 1; $i -lt $n; $i++) { $d[$i - 1] = $ts[$i] - $ts[$i - 1] }

if ($ExpectedFps -gt 0) {
    $period = [long](1e9 / $ExpectedFps)
    $periodSrc = "依 -ExpectedFps $ExpectedFps"
} else {
    $sorted = [long[]]$d.Clone(); [Array]::Sort($sorted)
    $period = $sorted[[int]($sorted.Length / 2)]
    $periodSrc = "間隔中位數"
}
if ($period -le 0) { throw "標準週期算出來 <= 0，時間戳可能無效" }

$span = ($ts[$n - 1] - $ts[0]) / 1e9
$tol = $period * $TolerancePct / 100.0

# 分布 + 異常
$hist = @{}
$gaps = New-Object System.Collections.Generic.List[object]
$missing = 0
for ($i = 0; $i -lt $d.Length; $i++) {
    $us = [math]::Round($d[$i] / 1000.0)
    if ($hist.ContainsKey($us)) { $hist[$us]++ } else { $hist[$us] = 1 }
    if ([math]::Abs($d[$i] - $period) -gt $tol) {
        $est = [math]::Max(0, [math]::Round($d[$i] / [double]$period) - 1)
        $missing += $est
        $gaps.Add([pscustomobject]@{ '第幾張之後' = $i + 1; '間隔_us' = [math]::Round($d[$i] / 1000.0, 1); '推估缺張' = $est })
    }
}

Write-Host ("標準週期  : {0:N1} us（{1}）→ {2:N2} fps" -f ($period / 1000.0), $periodSrc, (1e9 / $period))
Write-Host ("第一→最後 : {0:N6} s（理論 {1:N6} s）" -f $span, (($n - 1) * $period / 1e9))
Write-Host ("平均 FPS  : {0:N2}" -f (($n - 1) / $span))
Write-Host ""
Write-Host "間隔分布（us : 次數，前 10 名）"
$hist.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 10 |
    ForEach-Object { Write-Host ("  {0,8} : {1}" -f $_.Key, $_.Value) }
Write-Host ""

if ($gaps.Count -eq 0) {
    Write-Host "結果：沒有缺張 ✔（所有間隔都在標準週期 ±$TolerancePct% 內）" -ForegroundColor Green
    exit 0
}

Write-Host ("結果：{0} 個異常間隔，推估缺 {1} 張 ✘" -f $gaps.Count, $missing) -ForegroundColor Red
Write-Host "（若一個 container 內有多個 chapter，chapter 之間的停頓也會被列為異常間隔）"
$gaps | Select-Object -First 30 | Format-Table -AutoSize | Out-String | Write-Host
if ($gaps.Count -gt 30) { Write-Host "... 只列前 30 筆" }
exit 1
