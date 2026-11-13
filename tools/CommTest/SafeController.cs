using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AAMotion;

namespace CommTest;

/// <summary>單軸狀態快照（null = 讀取失敗）。</summary>
public sealed class AxisSnap
{
    public int? Pos, PosErr, MotorOnStatus, MotionStat, OperationMode, LimitStat, ConFlt, StatReg, Force, Iq;
    public string ConFaultMsg;
}

/// <summary>一次輪詢的完整快照。由 worker thread 產生，UI 只讀。</summary>
public sealed class Snap
{
    public DateTime Time;
    public long PollCount;
    public bool? SdkIsConnected;
    public int? CableType, VBus, VLogic, PwrTemp, BoardTemp, DInPort, DInPortHigh;
    public int[] AInPort;
    public AxisSnap A = new(), B = new();
    public bool HeartbeatOk;
    public double HeartbeatMs = double.NaN;
    public string HeartbeatResponse;
    public int ConsecutiveFailures;
}

/// <summary>
/// 唯讀控制器包裝。
///
/// 安全設計：
///  1. 這個類別「沒有」任何 MotorOn / MotorOff / Move / Jog / Home / Stop / 寫參數 的方法，
///     也不把 SDK 物件暴露出去，所以 UI 不可能透過它讓馬達動作。
///  2. 手動查詢只接受「名稱」或「名稱[索引]」，不能有 = ; 空白（Agito 指令語法中 = 才是寫入），
///     且名稱必須在白名單內（.par 參數名稱 + 已知狀態名稱），並排除含動作字眼的名稱。
///  3. 所有 SDK 呼叫都在同一條專屬 thread 上執行（AAMotion 是否 thread-safe 未知）。
/// </summary>
public sealed class SafeController : IDisposable
{
    public enum Kind { AGC301, AGM800 }

    private readonly BlockingCollection<Action> _queue = new();
    private readonly Thread _worker;
    private volatile bool _stopping;

    private MotionController _ctrl;
    private ISingleAxis _a, _b;
    private IIOModule _io;
    private volatile bool _connected;
    private long _polls;
    private int _failures;
    private readonly HashSet<string> _loggedErrors = new();

    private readonly ConcurrentQueue<string> _log = new();
    private readonly HashSet<string> _allowedNames;

    public volatile Snap Latest;
    public bool Connected => _connected;
    public int PollIntervalMs { get; set; } = 100;

    // ---------------------------------------------------------------- 白名單

    private static readonly Regex QueryRx = new(@"^[A-Za-z][A-Za-z0-9_]*(\[\d{1,3}\])?$", RegexOptions.Compiled);

    /// <summary>名稱中出現這些字就拒絕（縱深防禦：就算白名單有漏，也不送出可能是動作的指令）。</summary>
    private static readonly string[] ForbiddenFragments =
    {
        "MotorOn", "MotorOff", "Begin", "Stop", "Abort", "Kill", "Home", "Jog", "Move", "Reset",
        "Save", "Load", "Boot", "Phase", "Commut", "Clear", "Exec", "Burn", "Flash", "Restore", "Run",
    };

    private static readonly string[] AxisStatusNames =
    {
        "Pos", "PosErr", "PosRef", "VelRef", "Iq", "Id", "MotionStat", "LimitStat", "ConFlt",
        "StatReg", "HomingStat", "InTargetStat", "Force", "ForceRef", "ForceErr", "OperationMode",
    };

    private static readonly string[] ControllerStatusNames =
    {
        "ADInPort", "ADInPortHigh", "AAInPort", "ADOutPort", "VBus", "VLogic", "PwrTemp", "BoardTemp", "MotorTemp",
    };

    public SafeController(ParFile par)
    {
        _allowedNames = new HashSet<string>(par.BaseNames, StringComparer.OrdinalIgnoreCase);
        foreach (var ax in new[] { "A", "B" })
            foreach (var n in AxisStatusNames) _allowedNames.Add(ax + n);
        foreach (var n in ControllerStatusNames) _allowedNames.Add(n);

        _worker = new Thread(WorkerLoop) { IsBackground = true, Name = "AAMotion-owner" };
        _worker.Start();
    }

    public bool IsAllowedQuery(string q, out string reason)
    {
        q = (q ?? "").Trim();
        if (!QueryRx.IsMatch(q))
        {
            reason = "格式不符：只能是「名稱」或「名稱[索引]」，不可含 = ; 空白 等字元";
            return false;
        }
        var baseName = q.Split('[')[0];
        foreach (var f in ForbiddenFragments)
        {
            if (baseName.Contains(f, StringComparison.OrdinalIgnoreCase))
            {
                reason = $"名稱含禁止字「{f}」（可能是動作指令），拒絕送出";
                return false;
            }
        }
        if (!_allowedNames.Contains(baseName))
        {
            reason = $"「{baseName}」不在白名單（.par 參數名稱或已知狀態名稱）";
            return false;
        }
        reason = "";
        return true;
    }

    // ---------------------------------------------------------------- 對外 API（全部丟到 worker 執行）

    public bool TryDequeueLog(out string line) => _log.TryDequeue(out line);

    public Task<bool> ConnectAsync(Kind kind, string ip) => RunOnWorker(() => DoConnect(kind, ip));

    public Task DisconnectAsync() => RunOnWorker(() => { DoDisconnect(); return true; });

    /// <summary>送出一個唯讀查詢，回傳控制器回應文字。</summary>
    public Task<string> QueryAsync(string q)
    {
        if (!IsAllowedQuery(q, out var why))
            return Task.FromResult("[拒絕] " + why);
        return RunOnWorker(() => DoQuery(q.Trim()));
    }

    /// <summary>讀控制器識別資訊（typed 屬性，唯讀）。</summary>
    public Task<List<(string Name, string Value)>> ReadIdentityAsync() => RunOnWorker(DoReadIdentity);

    /// <summary>批次查詢參數現值（每個名稱都經過白名單檢查）。</summary>
    public Task<List<(string Name, string Value)>> ReadParamsAsync(IEnumerable<string> names)
    {
        var list = names.ToList();
        return RunOnWorker(() =>
        {
            var r = new List<(string, string)>();
            foreach (var n in list)
                r.Add((n, IsAllowedQuery(n, out var why) ? DoQuery(n) : "[拒絕] " + why));
            return r;
        });
    }

    public void Dispose()
    {
        try { RunOnWorker(() => { DoDisconnect(); return true; }).Wait(5000); } catch { }
        // SDK 會在背景啟動 AACommServer.exe；程式結束時一併關掉，避免殘留程序鎖住檔案 / 佔住連線
        try
        {
            RunOnWorker(() =>
            {
                using var tmp = new AGC301();
                tmp.CloseAACommServer();
                return true;
            }).Wait(3000);
        }
        catch { }
        _stopping = true;
        _queue.CompleteAdding();
        _worker.Join(3000);
    }

    // ---------------------------------------------------------------- worker thread

    private Task<T> RunOnWorker<T>(Func<T> f)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            _queue.Add(() =>
            {
                try { tcs.SetResult(f()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
        }
        catch (InvalidOperationException) { tcs.SetCanceled(); }
        return tcs.Task;
    }

    private void WorkerLoop()
    {
        var clock = Stopwatch.StartNew();
        long nextPoll = 0;
        while (!_stopping)
        {
            int wait = _connected ? (int)Math.Max(0, nextPoll - clock.ElapsedMilliseconds) : 200;
            Action job;
            try
            {
                if (_queue.TryTake(out job, wait)) { job(); continue; }
            }
            catch (InvalidOperationException) { break; } // CompleteAdding

            if (_connected && clock.ElapsedMilliseconds >= nextPoll)
            {
                try { Poll(); }
                catch (Exception ex) { LogOnce("Poll", ex); }
                nextPoll = clock.ElapsedMilliseconds + Math.Max(20, PollIntervalMs);
            }
        }
    }

    private bool DoConnect(Kind kind, string ip)
    {
        DoDisconnect();
        Log($"建立 {kind} 物件並連線 {ip} ...");
        MotionController c;
        try
        {
            c = kind == Kind.AGC301 ? new AGC301(50) : new AGM800(50);
        }
        catch (Exception ex)
        {
            Log($"建立 SDK 物件失敗：{ex.GetType().Name}: {ex.Message}");
            return false;
        }

        var sw = Stopwatch.StartNew();
        bool ok;
        try { ok = c.Connect(ip); }
        catch (Exception ex) { Log($"Connect 例外：{ex.GetType().Name}: {ex.Message}"); ok = false; }
        string err = "";
        try { err = $"ErrorCode={c.ErrorCode} {c.ErrorMessage}"; } catch { }
        Log($"Connect({ip}) => {ok}   耗時 {sw.ElapsedMilliseconds} ms   {err}");

        if (!ok)
        {
            try { c.Dispose(); } catch { }
            return false;
        }

        try { c.SetTimeOutForSending(500); } catch { }

        if (c is AGC301 g) { _a = g.A; _b = g.B; _io = g.IO; }
        else if (c is AGM800 m) { _a = m.A; _b = m.B; _io = m.IO_A; }
        _ctrl = c;
        _failures = 0;
        _polls = 0;
        _loggedErrors.Clear();
        _connected = true;
        return true;
    }

    private void DoDisconnect()
    {
        if (_ctrl == null) return;
        _connected = false;
        try { _ctrl.Disconnect(); Log("Disconnect()"); }
        catch (Exception ex) { Log($"Disconnect 例外：{ex.Message}"); }
        try { _ctrl.Dispose(); } catch { }
        _ctrl = null; _a = _b = null; _io = null;
        Latest = null;
    }

    private string DoQuery(string q)
    {
        if (_ctrl == null) return "[未連線]";
        try
        {
            var sw = Stopwatch.StartNew();
            bool ok = _ctrl.SendCommandString(q, out var resp);
            return ok ? (resp ?? "").Trim() : $"[失敗] {resp} ({sw.ElapsedMilliseconds} ms)";
        }
        catch (Exception ex) { return $"[例外] {ex.GetType().Name}: {ex.Message}"; }
    }

    private List<(string, string)> DoReadIdentity()
    {
        var r = new List<(string, string)>();
        if (_ctrl == null) return r;
        void Add(string n, Func<object> f)
        {
            try { r.Add((n, Convert.ToString(f()))); }
            catch (Exception ex) { r.Add((n, $"[例外] {ex.Message}")); }
        }
        var c = _ctrl;
        Add("ProductType", () => c.ProductType);
        Add("SerialNumber", () => c.SerialNumber);
        Add("HardwareVersion", () => c.HardwareVersion);
        Add("FirmwareVersion", () => c.FirmwareVersion);
        Add("BootFirmwareVersion", () => c.BootFirmwareVersion);
        Add("FPGAVersion", () => c.FPGAVersion);
        Add("ManufacturingDate", () => c.ManufacturingDate);
        Add("IsInBootMode", () => c.IsInBootMode);
        Add("NumberOfAxes", () => c.NumberOfAxes);
        Add("NumOfServoAxes", () => c.NumOfServoAxes);
        Add("MaxContCurrent", () => c.MaxContCurrent);
        Add("MaxPeakCurrent", () => c.MaxPeakCurrent);
        Add("MinimalBusVoltage", () => c.MinimalBusVoltage);
        Add("MaximalBusVoltage", () => c.MaximalBusVoltage);
        Add("CurrentLoopSampleRate", () => c.CurrentLoopSampleRate);
        Add("AnalogInputSampleRate", () => c.AnalogInputSampleRate);
        Add("ConnectedCableType", () => c.ConnectedCableType);
        return r;
    }

    private void Poll()
    {
        var s = new Snap { Time = DateTime.Now, PollCount = ++_polls };
        var c = _ctrl;

        s.SdkIsConnected = TryGet(() => c.IsConnected, "IsConnected");
        s.CableType = TryGet(() => c.ConnectedCableType, "ConnectedCableType");
        s.VBus = TryGet(() => c.VBus, "VBus");
        s.VLogic = TryGet(() => c.VLogic, "VLogic");
        s.PwrTemp = TryGet(() => c.PwrTemp, "PwrTemp");
        s.BoardTemp = TryGet(() => c.BoardTemp, "BoardTemp");

        FillAxis(s.A, _a, "A");
        FillAxis(s.B, _b, "B");

        if (_io != null)
        {
            s.DInPort = TryGet(() => _io.DInPort, "IO.DInPort");
            s.DInPortHigh = TryGet(() => _io.DInPortHigh, "IO.DInPortHigh");
            try { s.AInPort = (int[])_io.AInPort?.Clone(); } catch (Exception ex) { LogOnce("IO.AInPort", ex); }
        }

        // 心跳：真正跟控制器來回一次（typed 屬性可能只是 SDK 快取值，斷線時不一定會報錯）
        var sw = Stopwatch.StartNew();
        try
        {
            s.HeartbeatOk = c.SendCommandString("APos", out var resp);
            s.HeartbeatResponse = resp;
        }
        catch (Exception ex) { s.HeartbeatOk = false; s.HeartbeatResponse = ex.Message; }
        s.HeartbeatMs = sw.Elapsed.TotalMilliseconds;

        if (s.HeartbeatOk) _failures = 0;
        else if (++_failures == 1 || _failures % 50 == 0) Log($"心跳失敗（連續 {_failures} 次）：{s.HeartbeatResponse}");
        s.ConsecutiveFailures = _failures;

        Latest = s;
    }

    private void FillAxis(AxisSnap d, ISingleAxis ax, string n)
    {
        if (ax == null) return;
        d.Pos = TryGet(() => ax.Pos, n + ".Pos");
        d.PosErr = TryGet(() => ax.PosErr, n + ".PosErr");
        d.MotorOnStatus = TryGet(() => ax.MotorOnStatus, n + ".MotorOnStatus");
        d.MotionStat = TryGet(() => ax.MotionStat, n + ".MotionStat");
        d.OperationMode = TryGet(() => ax.OperationMode, n + ".OperationMode");
        d.LimitStat = TryGet(() => ax.LimitStat, n + ".LimitStat");
        d.ConFlt = TryGet(() => ax.ConFlt, n + ".ConFlt");
        d.StatReg = TryGet(() => ax.StatReg, n + ".StatReg");
        d.Force = TryGet(() => ax.Force, n + ".Force");
        d.Iq = TryGet(() => ax.Iq, n + ".Iq");
        try { d.ConFaultMsg = ax.ConFaultMsg; } catch (Exception ex) { LogOnce(n + ".ConFaultMsg", ex); }
    }

    private T? TryGet<T>(Func<T> f, string name) where T : struct
    {
        try { return f(); }
        catch (Exception ex) { LogOnce(name, ex); return null; }
    }

    private void LogOnce(string what, Exception ex)
    {
        var key = what + "|" + ex.GetType().Name + "|" + ex.Message;
        if (_loggedErrors.Add(key)) Log($"讀取 {what} 失敗：{ex.GetType().Name}: {ex.Message}（同樣錯誤之後不再重複記錄）");
    }

    private void Log(string s) => _log.Enqueue($"{DateTime.Now:HH:mm:ss.fff}  {s}");
}
