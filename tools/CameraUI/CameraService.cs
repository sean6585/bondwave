using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Euresys.EGrabber;
using Euresys.EGrabberRecorder;
using Action = System.Action;

namespace CameraUI;

public enum CamState { Disconnected, Idle, Live, Recording }

public sealed class RecordResult
{
    public string Path = "";
    public long Target, Written;
    public double WallSec;
    public bool Aborted;
    public string Error;
    public TimingStats Stats;
}

public sealed class DetectResult
{
    public FpsMethod Method;
    public string Detail = "";
    public int Frames;
    public double MeasuredFps;
    public bool Success;
}

/// <summary>
/// 相機服務：EGrabber 物件只在這個類別自己的一條執行緒上使用（所有指令都排進佇列）。
/// UI 只讀 <see cref="LatestFrame"/> 和 <see cref="GetStatus"/>，不直接碰 SDK。
/// 之後併入主控程式時，這個類別可以原封不動搬過去。
/// </summary>
public sealed class CameraService : IDisposable
{
    // ------------------------------------------------------------------ 執行緒 / 佇列
    private readonly BlockingCollection<Action> _queue = new();
    private readonly Thread _worker;
    private volatile bool _quit;
    private readonly ConcurrentQueue<string> _log = new();

    // ------------------------------------------------------------------ SDK 物件（只在 worker 上用）
    private EGenTL _gentl;
    private EGrabberDiscovery _discovery;
    private EGrabber _g;
    private bool? _tsNsSupported;
    private int _consecutivePopErrors;

    // ------------------------------------------------------------------ 狀態（UI 讀）
    private volatile CamState _state = CamState.Disconnected;
    public CamState State => _state;
    public string CameraName { get; private set; } = "";
    public string GrabberName { get; private set; } = "";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long PayloadSize { get; private set; }
    public string PixelFormat { get; private set; } = "";
    public string LastError { get; private set; } = "";

    public double AppliedFps { get; private set; }
    public double AppliedExposureUs { get; private set; }
    public string TriggerInfo { get; private set; } = "";
    public string Temperature { get; private set; } = "";
    public int BufferCount { get; private set; }

    // 即時統計
    private long _frames;
    private ulong _fpsT0; private long _fpsN0;
    public double MeasuredFps { get; private set; }
    public long FrameCount => Interlocked.Read(ref _frames);
    private ulong _lastTs;
    public long LiveGaps { get; private set; }
    private double _expectedPeriodNs;

    // 畫面
    private volatile Frame _latest;
    public Frame LatestFrame => _latest;
    private readonly Stopwatch _previewClock = Stopwatch.StartNew();
    private long _lastPreviewMs;
    private long _lastTempMs = -10000;

    // 單張
    private string _snapPath;
    private TaskCompletionSource<string> _snapTcs;

    // 錄影
    private RecorderLibrary _recLib;
    private Recorder _rec;
    private ulong[] _recTs;
    private long _recTarget, _recWritten;
    private string _recPath = "";
    private double _recFps;
    private readonly Stopwatch _recClock = new();
    private TaskCompletionSource<RecordResult> _recTcs;
    private volatile bool _recAbort;
    private long _recLastBufferMs;
    public long RecordTarget => _recTarget;
    public long RecordWritten => Interlocked.Read(ref _recWritten);
    public double RecordElapsedSec => _recClock.Elapsed.TotalSeconds;
    public string RecordPath => _recPath;

    public CameraService()
    {
        _worker = new Thread(WorkerLoop) { IsBackground = true, Name = "EGrabber-owner" };
        _worker.Start();
    }

    public bool TryDequeueLog(out string line) => _log.TryDequeue(out line);
    private void Log(string s) => _log.Enqueue($"{DateTime.Now:HH:mm:ss.fff}  {s}");
    private void Fail(string what, Exception ex)
    {
        LastError = $"{what}: {Describe(ex)}";
        Log("錯誤 " + LastError);
    }

    public static string Describe(Exception ex) => ex switch
    {
        GenTLError g => $"{g.GcError} {g.Description} {g.Message}".Trim(),
        RecorderError r => $"Recorder {r.Status} {r.Message}",
        _ => $"{ex.GetType().Name}: {ex.Message}",
    };

    // ================================================================== 公開指令（全部丟到 worker）

    public Task<bool> ConnectAsync(string pixelFormat = "Mono8") => Run(() => DoConnect(pixelFormat));
    public Task DisconnectAsync() => Run(() => { DoDisconnect(); return true; });
    public Task<bool> StartLiveAsync(double fps, double exposureUs, FpsMethod m, string timerFeature, int buffers)
        => Run(() => DoStartLive(fps, exposureUs, m, timerFeature, buffers));
    public Task StopAsync() => Run(() => { DoStopAcquisition(); return true; });

    /// <summary>存一張 TIFF。即時中直接取下一張；沒在擷取時會臨時啟動取一張。</summary>
    public Task<string> SnapshotAsync(string path)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(() =>
        {
            try
            {
                if (_g == null) throw new InvalidOperationException("相機未連線");
                if (_state == CamState.Recording) throw new InvalidOperationException("錄影中不能取單張");
                if (_state == CamState.Live) { _snapPath = path; _snapTcs = tcs; return; }
                tcs.SetResult(DoSingleGrab(path));
            }
            catch (Exception ex) { Fail("取單張", ex); tcs.TrySetException(ex); }
        });
        return tcs.Task;
    }

    public Task<RecordResult> RecordAsync(double fps, double exposureUs, double seconds, FpsMethod m, string timerFeature,
                                          string root, string name, double ramFraction)
    {
        var tcs = new TaskCompletionSource<RecordResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(() =>
        {
            try { DoStartRecording(fps, exposureUs, seconds, m, timerFeature, root, name, ramFraction, tcs); }
            catch (Exception ex)
            {
                Fail("開始錄影", ex);
                CleanupRecorder();
                try { _g?.Stop(); } catch { }
                if (_g != null) _state = CamState.Idle;
                tcs.TrySetResult(new RecordResult { Error = Describe(ex), Path = _recPath });
            }
        });
        return tcs.Task;
    }

    public void AbortRecording() => _recAbort = true;

    public Task<List<DetectResult>> DetectFpsMethodAsync(double fps, double exposureUs, string timerFeature)
        => Run(() => DoDetect(fps, exposureUs, timerFeature));

    public Task<string[]> ListFeaturesAsync(string filter) => Run(() =>
    {
        if (_g == null) return Array.Empty<string>();
        var all = _g.Remote.Features(true);
        return string.IsNullOrWhiteSpace(filter) ? all
            : all.Where(f => f.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
    });

    public Task<string> GetFeatureAsync(string name) => Run(() => ReadFeatureString(name));

    /// <summary>寫入相機（Remote）參數。只影響相機設定，不涉及運動控制。</summary>
    public Task<string> SetFeatureAsync(string name, string value) => Run(() =>
    {
        if (_g == null) throw new InvalidOperationException("相機未連線");
        if (_state != CamState.Idle) throw new InvalidOperationException("請先停止擷取再改參數");
        var r = _g.Remote;
        if (r.Command(name)) { r.Execute(name); Log($"Execute {name}"); return "(executed)"; }
        var intf = string.Join(",", r.Interfaces(name));
        if (intf.Contains("IInteger") && long.TryParse(value, out var l)) r.Set(name, l);
        else if (intf.Contains("IFloat") && double.TryParse(value, out var d)) r.Set(name, d);
        else if (intf.Contains("IBoolean") && bool.TryParse(value, out var b)) r.Set(name, b);
        else r.Set(name, value);
        var back = ReadFeatureString(name);
        Log($"Set {name} = {value} → 讀回 {back}");
        return back;
    });

    public void Dispose()
    {
        try { _recAbort = true; Run(() => { DoDisconnect(); return true; }).Wait(5000); } catch { }
        _quit = true;
        _queue.CompleteAdding();
        _worker.Join(3000);
    }

    // ================================================================== worker

    private void Post(Action a)
    {
        try { _queue.Add(a); }
        catch (InvalidOperationException) { }
    }

    private Task<T> Run<T>(Func<T> f)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(() =>
        {
            try { tcs.SetResult(f()); }
            catch (Exception ex) { Fail("指令", ex); tcs.TrySetException(ex); }
        });
        return tcs.Task;
    }

    private void WorkerLoop()
    {
        while (!_quit)
        {
            bool acquiring = _state is CamState.Live or CamState.Recording;
            try
            {
                if (!acquiring)
                {
                    if (_queue.TryTake(out var job, 100)) job();
                    else PeriodicIdle();
                    continue;
                }
                while (_queue.TryTake(out var job2)) job2();
                if (_state is not (CamState.Live or CamState.Recording)) continue;

                if (_state == CamState.Recording && _recAbort) { FinishRecording(aborted: true, error: null); continue; }

                NewBufferData nbd;
                try { nbd = _g.Pop(20); _consecutivePopErrors = 0; }
                catch (GenTLError e) when (e.GcError == GC_ERROR.GC_ERR_TIMEOUT)
                {
                    if (_state == CamState.Recording && _recClock.ElapsedMilliseconds - _recLastBufferMs > 3000)
                        FinishRecording(false, _recWritten == 0
                            ? "3 秒內沒有收到任何影像：相機沒有在送圖（檢查觸發 / FPS 控制方式）"
                            : "錄影中超過 3 秒沒有新影像");
                    continue;
                }
                catch (Exception ex)
                {
                    if (++_consecutivePopErrors >= 10) { Fail("取像（連續失敗，停止擷取）", ex); DoStopAcquisition(); }
                    continue;
                }

                bool recordingDone;
                using (var buf = new ScopedBuffer(_g, nbd)) recordingDone = OnBuffer(buf);
                // 要在 buffer 歸還（Dispose → push）之後才停止擷取 / 關閉錄影檔
                if (recordingDone) FinishRecording(false, null);
            }
            catch (InvalidOperationException) when (_queue.IsAddingCompleted) { break; }
            catch (Exception ex)
            {
                Fail("worker", ex);
                if (_state == CamState.Recording) FinishRecording(false, Describe(ex));
            }
        }
    }

    /// <returns>true = 錄影張數已滿，呼叫端要結束錄影</returns>
    private bool OnBuffer(ScopedBuffer buf)
    {
        ulong ts = TimestampNs(buf);
        long n = Interlocked.Increment(ref _frames);

        // 即時 FPS（每 0.5 s 更新）與缺張偵測
        if (_fpsN0 == 0) { _fpsT0 = ts; _fpsN0 = n; }
        else if (ts - _fpsT0 > 500_000_000UL)
        {
            MeasuredFps = (n - _fpsN0) / ((ts - _fpsT0) / 1e9);
            _fpsT0 = ts; _fpsN0 = n;
        }
        if (_lastTs != 0 && _expectedPeriodNs > 0 && ts - _lastTs > _expectedPeriodNs * 1.5) LiveGaps++;
        _lastTs = ts;

        if (_state == CamState.Recording)
        {
            _rec.Write(buf);
            _recTs[_recWritten] = ts;
            Interlocked.Increment(ref _recWritten);
            _recLastBufferMs = _recClock.ElapsedMilliseconds;
            if (_recWritten >= _recTarget) return true;
        }

        long now = _previewClock.ElapsedMilliseconds;
        bool wantPreview = now - _lastPreviewMs >= (_state == CamState.Recording ? 100 : 15);
        bool wantSnap = _snapTcs != null && _state == CamState.Live;
        if (wantPreview || wantSnap)
        {
            var f = CopyFrame(buf, ts, n);
            if (wantPreview) { _latest = f; _lastPreviewMs = now; }
            if (wantSnap)
            {
                var (path, tcs) = (_snapPath, _snapTcs);
                _snapTcs = null;
                string desc = Describe(f);
                Task.Run(() =>
                {
                    try { Tiff.WriteMono8(path, f, desc); tcs.SetResult(path); Log("單張已存 " + path); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                });
            }
        }
        return false;
    }

    private string Describe(Frame f) =>
        $"BondwaveCamera {CameraName}; ts_ns={f.TimestampNs}; exposure_us={AppliedExposureUs}; fps={AppliedFps}; {TriggerInfo}";

    private Frame CopyFrame(ScopedBuffer buf, ulong ts, long index)
    {
        var info = buf.GetInfo();
        int w = Convert.ToInt32(info.Width);
        int h = Convert.ToInt32(info.DeliveredHeight);
        if (h <= 0) h = Height;
        int pitch = Convert.ToInt32(info.LinePitch);
        if (pitch <= 0) pitch = w;
        return Frame.FromPointer(buf.Pixels, w, h, pitch, ts, index);
    }

    private ulong TimestampNs(ScopedBuffer buf)
    {
        if (_tsNsSupported != false)
        {
            try { var v = buf.GetInfo<ulong>(BUFFER_INFO_CMD.BUFFER_INFO_TIMESTAMP_NS); _tsNsSupported = true; return v; }
            catch { _tsNsSupported = false; Log("BUFFER_INFO_TIMESTAMP_NS 不支援，改用 µs 時間戳 ×1000"); }
        }
        try { return buf.GetInfo<ulong>(BUFFER_INFO_CMD.BUFFER_INFO_TIMESTAMP) * 1000UL; }
        catch { return (ulong)(Stopwatch.GetTimestamp() * (1e9 / Stopwatch.Frequency)); }
    }

    private void PeriodicIdle()
    {
        if (_g == null) return;
        long now = _previewClock.ElapsedMilliseconds;
        if (now - _lastTempMs < 2000) return;
        _lastTempMs = now;
        try { if (_g.Remote.Available("DeviceTemperature")) Temperature = ReadFeatureString("DeviceTemperature"); }
        catch { }
    }

    // ================================================================== 連線

    private bool DoConnect(string pixelFormat)
    {
        if (_g != null) return true;
        LastError = "";
        Log("載入 GenTL（Coaxlink）並搜尋相機...");
        _gentl = new EGenTL();
        _discovery = new EGrabberDiscovery(_gentl);
        _discovery.Discover(true);
        Log($"找到 grabber {_discovery.GrabberCount} 個、相機 {_discovery.CameraCount} 台");
        if (_discovery.CameraCount == 0)
        {
            DoDisconnect();
            throw new InvalidOperationException("找不到相機：確認相機上電、兩條 CXP 線都接上，且 eGrabber Studio 已關閉（同時只能一個程式控制相機）");
        }

        var cam = _discovery.GetCamera(0);
        _g = new EGrabber(cam, DEVICE_ACCESS_FLAGS.DEVICE_ACCESS_CONTROL);
        var gi = cam.EGrabbers.FirstOrDefault();
        GrabberName = gi != null ? $"{gi.InterfaceID} / {gi.DeviceID}" : "";

        CameraName = string.Join(" ", new[] { "DeviceVendorName", "DeviceModelName" }.Select(TryRead).Where(s => s.Length > 0));
        var sn = TryRead("DeviceSerialNumber");
        if (sn.Length > 0) CameraName += $" (SN {sn})";

        // 相機上電預設是 Mono16；本程式（顯示 / TIFF / 1700 fps 錄影頻寬）用 Mono8
        if (!string.IsNullOrEmpty(pixelFormat))
        {
            var cur = TryRead("PixelFormat");
            if (!cur.Equals(pixelFormat, StringComparison.OrdinalIgnoreCase))
            {
                try { _g.Remote.Set("PixelFormat", pixelFormat); Log($"PixelFormat {cur} → {TryRead("PixelFormat")}"); }
                catch (Exception ex) { Log($"⚠ 設定 PixelFormat={pixelFormat} 失敗：{Describe(ex)}"); }
            }
        }
        Width = (int)_g.Width; Height = (int)_g.Height;
        PayloadSize = (long)_g.PayloadSize; PixelFormat = _g.PixelFormat;
        Log($"相機：{CameraName}；Grabber：{GrabberName}");
        Log($"影像：{Width}x{Height} {PixelFormat}，每張 {PayloadSize:N0} bytes");
        if (!PixelFormat.Equals("Mono8", StringComparison.OrdinalIgnoreCase))
            Log($"⚠ PixelFormat 是 {PixelFormat}，本程式的顯示與 TIFF 只支援 Mono8");

        // 把跟 FPS 控制有關的參數全部列出來（第一次上機用來判斷 FPS 控制方式）
        var keys = new[] { "Trigger", "Timer", "AcquisitionFrame", "Exposure", "AcquisitionMode", "AcquisitionSource" };
        foreach (var f in _g.Remote.Features(true).Where(f => keys.Any(k => f.Contains(k, StringComparison.OrdinalIgnoreCase))))
            Log($"  參數 {f} = {TryRead(f)}");
        RefreshTriggerInfo();
        _state = CamState.Idle;
        return true;
    }

    private void DoDisconnect()
    {
        if (_state == CamState.Recording) FinishRecording(true, "斷線");
        DoStopAcquisition();
        try { _g?.Dispose(); } catch { }
        try { _discovery?.Dispose(); } catch { }
        try { _gentl?.Dispose(); } catch { }
        _g = null; _discovery = null; _gentl = null;
        _latest = null;
        if (_state != CamState.Disconnected) Log("相機已斷線");
        _state = CamState.Disconnected;
        CameraName = GrabberName = "";
    }

    // ================================================================== 參數

    private string TryRead(string f)
    {
        try { return _g.Remote.Available(f) ? ReadFeatureString(f) : ""; } catch { return ""; }
    }

    private string ReadFeatureString(string name)
    {
        if (_g == null) throw new InvalidOperationException("相機未連線");
        var r = _g.Remote;
        if (r.Command(name)) return "(command)";
        try { return r.Get<string>(name); }
        catch
        {
            var intf = string.Join(",", r.Interfaces(name));
            if (intf.Contains("IFloat")) return r.Get<double>(name).ToString("0.###");
            if (intf.Contains("IInteger")) return r.Get<long>(name).ToString();
            if (intf.Contains("IBoolean")) return r.Get<bool>(name).ToString();
            throw;
        }
    }

    private void SetNumber(string name, double value)
    {
        var r = _g.Remote;
        if (!r.Available(name)) throw new InvalidOperationException($"相機沒有參數 {name}");
        var intf = string.Join(",", r.Interfaces(name));
        if (intf.Contains("IFloat")) r.Set(name, value);
        else r.Set(name, (long)Math.Round(value));
    }

    private double GetNumber(string name)
    {
        var r = _g.Remote;
        var intf = string.Join(",", r.Interfaces(name));
        return intf.Contains("IFloat") ? r.Get<double>(name) : r.Get<long>(name);
    }

    private void SetEnum(string name, string value)
    {
        var r = _g.Remote;
        if (!r.Available(name)) throw new InvalidOperationException($"相機沒有參數 {name}");
        if (!string.Equals(ReadFeatureString(name), value, StringComparison.OrdinalIgnoreCase)) r.Set(name, value);
    }

    private void RefreshTriggerInfo()
    {
        TriggerInfo = $"TriggerMode={TryRead("TriggerMode")} TriggerSource={TryRead("TriggerSource")} " +
                      $"AcquisitionFrameTime={TryRead("AcquisitionFrameTime")} ExposureTime={TryRead("ExposureTime")}";
    }

    /// <summary>
    /// 套用 FPS 與曝光（必須在停止擷取時呼叫）。
    /// 順序：FPS 變快時先縮曝光再縮週期；變慢時先拉長週期再拉長曝光（相機要求 曝光 ≤ 週期）。
    /// </summary>
    private void ApplyRate(double fps, double exposureUs, FpsMethod m, string timerFeature)
    {
        if (fps <= 0) throw new ArgumentException("FPS 必須 > 0");
        double periodUs = 1e6 / fps;
        if (exposureUs <= 0 || exposureUs > periodUs * 0.97)
            throw new ArgumentException($"曝光 {exposureUs} µs 太長：{fps} fps 的週期只有 {periodUs:0} µs（曝光要 ≤ 週期的 97%）");

        string periodFeature = m == FpsMethod.TimerTrigger ? timerFeature : "AcquisitionFrameTime";
        if (m == FpsMethod.TriggerOffFrameTime) SetEnum("TriggerMode", "Off");
        if (m == FpsMethod.TimerTrigger)
        {
            SetEnum("TriggerMode", "On");
            SetEnum("TriggerSource", "Timer");
        }

        double curPeriod = 0;
        try { curPeriod = GetNumber(periodFeature); } catch { }
        if (curPeriod <= 0 || periodUs < curPeriod)
        {
            SetNumber("ExposureTime", exposureUs);
            SetNumber(periodFeature, periodUs);
        }
        else
        {
            SetNumber(periodFeature, periodUs);
            SetNumber("ExposureTime", exposureUs);
        }

        double backP = GetNumber(periodFeature), backE = GetNumber("ExposureTime");
        AppliedFps = backP > 0 ? 1e6 / backP : 0;
        AppliedExposureUs = backE;
        _expectedPeriodNs = backP * 1000;
        RefreshTriggerInfo();
        Log($"套用 [{m}] {periodFeature}={backP:0.###} µs（{AppliedFps:0.##} fps）、ExposureTime={backE:0.###} µs");
        if (Math.Abs(backP - periodUs) > 1.5) Log($"⚠ 相機把週期改成 {backP} µs（要求 {periodUs:0.#}），可能已到相機上限");
    }

    private void AllocBuffers(long count)
    {
        while (true)
        {
            try
            {
                _g.ReallocBuffers((ulong)count);
                BufferCount = (int)count;
                Log($"配置 buffer {count:N0} 個（{count * PayloadSize / 1e9:0.00} GB）");
                return;
            }
            catch (Exception ex) when (count > 256)
            {
                Log($"配置 {count:N0} 個 buffer 失敗（{Describe(ex)}），改用一半");
                count /= 2;
            }
        }
    }

    private void ResetCounters()
    {
        _frames = 0; _fpsN0 = 0; _fpsT0 = 0; _lastTs = 0; LiveGaps = 0; MeasuredFps = 0;
    }

    // ================================================================== 即時 / 單張

    private bool DoStartLive(double fps, double exposureUs, FpsMethod m, string timerFeature, int buffers)
    {
        if (_g == null) throw new InvalidOperationException("相機未連線");
        if (_state == CamState.Recording) throw new InvalidOperationException("錄影中");
        DoStopAcquisition();
        ApplyRate(fps, exposureUs, m, timerFeature);
        AllocBuffers(Math.Max(8, buffers));
        ResetCounters();
        _g.Start();
        _state = CamState.Live;
        Log("即時畫面開始");
        return true;
    }

    private void DoStopAcquisition()
    {
        if (_g == null) return;
        if (_state is CamState.Live or CamState.Recording)
        {
            try { _g.Stop(); } catch (Exception ex) { Log("Stop 例外：" + Describe(ex)); }
            if (_state == CamState.Live) Log("擷取停止");
        }
        if (_snapTcs != null) { _snapTcs.TrySetException(new InvalidOperationException("擷取已停止")); _snapTcs = null; }
        if (_state != CamState.Disconnected) _state = CamState.Idle;
    }

    private string DoSingleGrab(string path)
    {
        AllocBuffers(8);
        ResetCounters();
        _g.Start();
        try
        {
            var nbd = _g.Pop(3000);
            using var buf = new ScopedBuffer(_g, nbd);
            var f = CopyFrame(buf, TimestampNs(buf), 1);
            _latest = f;
            Tiff.WriteMono8(path, f, Describe(f));
            Log("單張已存 " + path);
            return path;
        }
        finally { try { _g.Stop(); } catch { } }
    }

    // ================================================================== 錄影

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength, dwMemoryLoad;
        public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile, ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
    }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX m);

    public static ulong AvailablePhysicalBytes()
    {
        var m = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        return GlobalMemoryStatusEx(ref m) ? m.ullAvailPhys : 0;
    }

    private void DoStartRecording(double fps, double exposureUs, double seconds, FpsMethod m, string timerFeature,
                                  string root, string name, double ramFraction, TaskCompletionSource<RecordResult> tcs)
    {
        if (_g == null) throw new InvalidOperationException("相機未連線");
        if (_state == CamState.Recording) throw new InvalidOperationException("已在錄影中");
        if (seconds <= 0) throw new ArgumentException("錄影秒數必須 > 0");

        DoStopAcquisition();
        ApplyRate(fps, exposureUs, m, timerFeature);

        long frames = (long)Math.Ceiling(seconds * AppliedFps);
        long containerBytes = frames * (PayloadSize + 4096) + (64L << 20);

        // 磁碟空間
        Directory.CreateDirectory(root);
        var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(root))!);
        if (drive.AvailableFreeSpace < containerBytes)
            throw new IOException($"磁碟 {drive.Name} 剩 {drive.AvailableFreeSpace / 1e9:0.0} GB，錄影需要 {containerBytes / 1e9:0.0} GB");

        // Buffer：能全部放進 RAM 就全放（保證不掉張）；放不下就盡量大，靠磁碟邊錄邊寫
        double avail = AvailablePhysicalBytes();
        long ramBuffers = (long)(Math.Max(0, avail - 4e9) * ramFraction / PayloadSize);
        long buffers = Math.Max(256, Math.Min(frames, ramBuffers));
        AllocBuffers(buffers);
        if (BufferCount < frames)
            Log($"⚠ buffer {BufferCount:N0} < 總張數 {frames:N0}：超出部分要靠磁碟即時寫入，若磁碟跟不上會掉張（錄完會檢查）");

        // 錄影檔
        var safeName = string.Concat((name ?? "").Where(c => char.IsLetterOrDigit(c) || c is '_' or '-'));
        _recPath = Path.Combine(root, $"{DateTime.Now:yyyyMMdd_HHmmss}_{AppliedFps:0}fps" + (safeName.Length > 0 ? "_" + safeName : ""));
        Directory.CreateDirectory(_recPath); // Recorder 要求資料夾已存在（否則 INVALID_CONTAINER_PATH）
        _recLib = new RecorderLibrary();
        _rec = _recLib.OpenRecorder(_recPath, RECORDER_OPEN_MODE.WRITE, RECORDER_CLOSE_MODE.TRIM);
        _rec.Set(RECORDER_PARAMETER.CONTAINER_SIZE, containerBytes);
        var chapterInfo = $"fps={AppliedFps:0.###};exposure_us={AppliedExposureUs:0.###};seconds={seconds};method={m};{TriggerInfo}";
        _rec.StartChapter(safeName.Length > 0 ? safeName : "rec", chapterInfo);

        _recTs = new ulong[frames];
        _recTarget = frames;
        _recWritten = 0;
        _recFps = AppliedFps;
        _recAbort = false;
        _recTcs = tcs;
        ResetCounters();
        _recClock.Restart();
        _recLastBufferMs = 0;

        _g.Start((ulong)frames);
        _state = CamState.Recording;
        Log($"開始錄影：{frames:N0} 張（{seconds} s × {AppliedFps:0.##} fps），container {containerBytes / 1e9:0.00} GB → {_recPath}");
    }

    private void FinishRecording(bool aborted, string error)
    {
        try { _g?.Stop(); } catch { }
        _recClock.Stop();
        long written = _recWritten;
        CleanupRecorder();

        var res = new RecordResult
        {
            Path = _recPath, Target = _recTarget, Written = written, WallSec = _recClock.Elapsed.TotalSeconds,
            Aborted = aborted, Error = error,
        };
        if (written >= 2) res.Stats = TimingStats.Compute(new ArraySegment<ulong>(_recTs, 0, (int)written), _recFps);
        _recTs = null;
        if (_g != null) _state = CamState.Idle;

        var head = aborted ? "錄影中止" : error != null ? "錄影失敗：" + error : "錄影完成";
        Log($"{head}：寫入 {written:N0}/{_recTarget:N0} 張，耗時 {res.WallSec:0.0} s");
        if (res.Stats != null) Log($"  平均 {res.Stats.AvgFps:0.00} fps，缺張 {res.Stats.MissingTotal}（{(res.Stats.Ok ? "OK" : "有缺張")}）");
        WriteSummary(res);
        _recTcs?.TrySetResult(res);
        _recTcs = null;
    }

    private void CleanupRecorder()
    {
        try { _rec?.Close(RECORDER_CLOSE_MODE.TRIM); } catch (Exception ex) { Log("關閉錄影檔例外：" + Describe(ex)); }
        try { _rec?.Dispose(); } catch { }
        try { _recLib?.Dispose(); } catch { }
        _rec = null; _recLib = null;
    }

    private void WriteSummary(RecordResult r)
    {
        try
        {
            if (string.IsNullOrEmpty(r.Path) || !Directory.Exists(r.Path)) return;
            var sb = new StringBuilder();
            sb.AppendLine($"BondwaveCamera recording summary  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Camera   : {CameraName}");
            sb.AppendLine($"Settings : {TriggerInfo}");
            sb.AppendLine($"Buffers  : {BufferCount:N0}");
            sb.AppendLine($"Written  : {r.Written:N0} / {r.Target:N0}   wall {r.WallSec:0.0} s   aborted={r.Aborted}   error={r.Error ?? "-"}");
            if (r.Stats != null) sb.Append(r.Stats.Report("Timing"));
            File.WriteAllText(Path.Combine(r.Path, "summary.txt"), sb.ToString(), Encoding.UTF8);
        }
        catch { }
    }

    // ================================================================== FPS 控制方式偵測

    private List<DetectResult> DoDetect(double fps, double exposureUs, string timerFeature)
    {
        if (_g == null) throw new InvalidOperationException("相機未連線");
        if (_state != CamState.Idle) throw new InvalidOperationException("請先停止擷取");
        var results = new List<DetectResult>();
        var methods = new List<FpsMethod> { FpsMethod.FrameTimeOnly, FpsMethod.TriggerOffFrameTime };
        if (_g.Remote.Available(timerFeature)) methods.Add(FpsMethod.TimerTrigger);
        else Log($"相機沒有 {timerFeature}，略過 Timer 方式（可在參數分頁查 Timer 相關參數名稱）");

        foreach (var m in methods)
        {
            var r = new DetectResult { Method = m };
            try
            {
                ApplyRate(fps, exposureUs, m, timerFeature);
                AllocBuffers(256);
                ResetCounters();
                var ts = new List<ulong>();
                _g.Start();
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 1500 && ts.Count < 3000)
                {
                    try
                    {
                        var nbd = _g.Pop(200);
                        using var buf = new ScopedBuffer(_g, nbd);
                        ts.Add(TimestampNs(buf));
                        if (ts.Count == 1) _latest = CopyFrame(buf, ts[0], 1);
                    }
                    catch (GenTLError e) when (e.GcError == GC_ERROR.GC_ERR_TIMEOUT) { }
                }
                _g.Stop();
                r.Frames = ts.Count;
                var st = TimingStats.Compute(ts);
                r.MeasuredFps = st.AvgFps;
                r.Success = ts.Count >= 10 && Math.Abs(st.AvgFps - AppliedFps) / AppliedFps < 0.03;
                r.Detail = $"{TriggerInfo}；收到 {ts.Count} 張，實測 {st.AvgFps:0.0} fps（設定 {AppliedFps:0.0}）";
            }
            catch (Exception ex)
            {
                try { _g.Stop(); } catch { }
                r.Detail = "失敗：" + Describe(ex);
            }
            Log($"偵測 [{m}] {(r.Success ? "✔" : "✘")} {r.Detail}");
            results.Add(r);
            if (r.Success) break;
        }
        _state = CamState.Idle;
        return results;
    }
}
