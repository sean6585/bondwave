using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CommTest;

/// <summary>
/// Bondwave 通訊測試主畫面（唯讀）。
/// 用途：通電前/通電後確認 PC ↔ AGD301 ↔ Z1/Z2 的通訊、編碼器、I/O、類比輸入。
/// 本畫面沒有任何讓馬達通電或移動的按鈕。
/// </summary>
public sealed class MainForm : Form
{
    private const string Z1Label = "A 軸 — Z1 (APM-XB04ADK3)";
    private const string Z2Label = "B 軸 — Z2 (AVM90-15)";

    private readonly ParFile _par;
    private readonly SafeController _ctl;

    // 頂部
    private readonly TextBox _tbIp = new() { Text = "172.1.1.101", Width = 120 };
    private readonly ComboBox _cbKind = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
    private readonly Button _btnPing = new() { Text = "Ping / 網卡檢查", AutoSize = true };
    private readonly Button _btnConnect = new() { Text = "連線", AutoSize = true };
    private readonly Button _btnDisconnect = new() { Text = "斷線", AutoSize = true, Enabled = false };
    private readonly Label _lblConn = new() { Text = "未連線", AutoSize = true, Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold), ForeColor = Color.DimGray, Padding = new Padding(10, 4, 0, 0) };
    private readonly Label _lblNet = new() { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(4) };

    // 狀態
    private readonly DataGridView _gridAxis = MakeGrid();
    private readonly DataGridView _gridCtrl = MakeGrid();
    private readonly Button _btnMark = new() { Text = "設基準點（只記在本程式，不寫入控制器）", AutoSize = true };

    // I/O
    private readonly Label[] _din = new Label[32];
    private readonly bool[] _dinLatched = new bool[32];
    private int? _lastDin;
    private readonly Label[] _ain = new Label[4];
    private readonly Button _btnClearLatch = new() { Text = "清除變化標記", AutoSize = true };
    private readonly ToolTip _tip = new();

    // 下方分頁
    private readonly DataGridView _gridParams = MakeGrid();
    private readonly Button _btnReadParams = new() { Text = "重新讀取參數 / 識別資訊", AutoSize = true, Enabled = false };
    private readonly TextBox _tbQuery = new() { Width = 220, Text = "AEncType" };
    private readonly Button _btnQuery = new() { Text = "查詢", AutoSize = true, Enabled = false };
    private readonly TextBox _tbQueryOut = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
    private readonly TextBox _tbLog = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };

    private readonly System.Windows.Forms.Timer _uiTimer = new() { Interval = 100 };
    private Font _bold;

    // 手推編碼器輔助（本地計算）
    private readonly AxisTrack _trackA = new(), _trackB = new();
    private Dictionary<string, string> _dinModeNow = new();

    private sealed class AxisTrack
    {
        public int? Mark, Min, Max;
        public void Update(int? pos)
        {
            if (pos is not int p) return;
            Mark ??= p;
            Min = Min is int mn ? Math.Min(mn, p) : p;
            Max = Max is int mx ? Math.Max(mx, p) : p;
        }
        public void Reset() { Mark = Min = Max = null; }
    }

    public MainForm()
    {
        Text = "Bondwave 通訊測試（唯讀）— AGD301 / Z1 / Z2";
        Width = 1280; Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft JhengHei UI", 9.5f);

        _par = ParFile.Load(Path.Combine(AppContext.BaseDirectory, "reference.par"));
        _ctl = new SafeController(_par);

        _cbKind.Items.Add("AGC301（AGD301 適用，SDK 官方類別）");
        _cbKind.Items.Add("AGM800（舊主程式使用的類別）");
        _cbKind.SelectedIndex = 0;

        BuildLayout();
        InitGrids();

        _btnPing.Click += async (_, _) => await PingAndCheckNic();
        _btnConnect.Click += async (_, _) => await Connect();
        _btnDisconnect.Click += async (_, _) => await Disconnect();
        _btnMark.Click += (_, _) =>
        {
            LogSnapshot(_ctl.Latest);
            // 重設前先把這段期間的位移紀錄下來（手推編碼器測試用）
            foreach (var (name, t, pos) in new[] { ("A(Z1)", _trackA, _ctl.Latest?.A.Pos), ("B(Z2)", _trackB, _ctl.Latest?.B.Pos) })
                if (t.Mark is int m && t.Min is int mn && t.Max is int mx)
                    AppendLog($"RANGE {name} 基準={m} 最小={mn} 最大={mx} 跨度={mx - mn} 目前Δ={(pos is int p ? p - m : 0)}");
            _trackA.Reset(); _trackB.Reset();
            AppendLog("設基準點（只記在本程式）");
        };
        _btnClearLatch.Click += (_, _) => Array.Clear(_dinLatched);
        _btnReadParams.Click += async (_, _) => await ReadParams();
        _btnQuery.Click += async (_, _) => await RunQuery();
        _tbQuery.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await RunQuery(); } };
        _uiTimer.Tick += (_, _) => RefreshUi();
        _uiTimer.Start();

        AppendLog(_par.Values.Count > 0
            ? $"已載入參考參數檔 reference.par（{_par.Values.Count} 筆），用於白名單與比對"
            : "找不到 reference.par，手動查詢只能用狀態名稱");
        AppendLog("本程式為唯讀：不包含 MotorOn / MotorOff / Move / Jog / Home / 寫參數 等任何呼叫。");
        CheckNic();
    }

    // ================================================================ 版面

    private void BuildLayout()
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6), WrapContents = false };
        top.Controls.AddRange(new Control[]
        {
            new Label { Text = "控制器 IP", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _tbIp,
            new Label { Text = "SDK 類別", AutoSize = true, Padding = new Padding(8, 6, 0, 0) }, _cbKind,
            _btnPing, _btnConnect, _btnDisconnect, _lblConn,
        });

        var banner = new Label
        {
            Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(0xE6, 0xF4, 0xEA), ForeColor = Color.FromArgb(0x1E, 0x6B, 0x34),
            Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold),
            Text = "唯讀模式 — 本程式不會讓馬達通電或移動，也不寫入任何控制器參數",
        };

        // 中間：左 = 軸狀態 + 控制器即時，右 = I/O
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        left.Controls.Add(Group("軸狀態（手推軸時看 Pos / Δ 是否變化、方向是否正確）", _gridAxis), 0, 0);
        left.Controls.Add(_btnMark, 0, 1);
        left.Controls.Add(Group("控制器 / 通訊即時狀態", _gridCtrl), 0, 2);

        var dinPanel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 8, RowCount = 4, AutoSize = true };
        for (int i = 0; i < 32; i++)
        {
            var l = new Label
            {
                Text = $"IN{i + 1}", Width = 58, Height = 30, TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Gainsboro, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(2),
            };
            _din[i] = l;
            dinPanel.Controls.Add(l, i % 8, i / 8);
        }
        var ainPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        for (int i = 0; i < 4; i++)
        {
            _ain[i] = new Label { Text = $"AI{i + 1}: —", Width = 120, Height = 26, TextAlign = ContentAlignment.MiddleLeft, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(2) };
            ainPanel.Controls.Add(_ain[i]);
        }
        var ioHelp = new Label
        {
            Dock = DockStyle.Top, AutoSize = true, MaximumSize = new Size(560, 0), ForeColor = Color.DimGray,
            Text = "綠 = ON、灰 = OFF；外框橘色 = 自上次清除後有變化過（手按限位開關後檢查）。\n" +
                   "假設 IN n 對應 DInPort 的 bit n-1；滑鼠移到格子上可看該輸入的 DInMode（軸/功能，推測解讀）。\n" +
                   "AI2 = 力量回授（load cell → TM3 → AI#2），原始值。",
        };
        var ioInner = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        ioInner.Controls.Add(ainPanel);
        ioInner.Controls.Add(new Label { Text = "類比輸入 AInPort", Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 10, 0, 2), Font = new Font(Font, FontStyle.Bold) });
        ioInner.Controls.Add(_btnClearLatch);
        _btnClearLatch.Dock = DockStyle.Top;
        ioInner.Controls.Add(dinPanel);
        ioInner.Controls.Add(ioHelp);
        var right = Group("I/O（數位輸入 DInPort / 類比輸入）", ioInner);

        var mid = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 640 };
        mid.Panel1.Controls.Add(left);
        mid.Panel2.Controls.Add(right);

        // 下方分頁
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var tpParams = new TabPage("參數比對（控制器現值 vs .par 2024-08-08 舊備份）");
        var pBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        pBar.Controls.Add(_btnReadParams);
        pBar.Controls.Add(new Label { AutoSize = true, ForeColor = Color.DarkRed, Padding = new Padding(8, 6, 0, 0), Text = "⚠ .par 是「前一顆 Z1」的設定，Z1 換成 APM-XB04ADK3 後不可直接沿用。紅底 = 現值與 .par 不同。" });
        tpParams.Controls.Add(_gridParams);
        tpParams.Controls.Add(pBar);

        var tpQuery = new TabPage("手動查詢（唯讀）");
        var qBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        qBar.Controls.AddRange(new Control[]
        {
            new Label { Text = "名稱", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _tbQuery, _btnQuery,
            new Label { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(8, 6, 0, 0), Text = "例：AEncType、BMotorType、ADInMode[7]、BForce。不可含 =（寫入）；只接受 .par 參數名稱與已知狀態名稱。" },
        });
        tpQuery.Controls.Add(_tbQueryOut);
        tpQuery.Controls.Add(qBar);

        var tpLog = new TabPage("Log");
        tpLog.Controls.Add(_tbLog);

        tabs.TabPages.AddRange(new[] { tpLog, tpParams, tpQuery });

        var main = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 520 };
        main.Panel1.Controls.Add(mid);
        main.Panel2.Controls.Add(tabs);

        Controls.Add(main);
        Controls.Add(_lblNet);
        _lblNet.Dock = DockStyle.Top;
        Controls.Add(banner);
        Controls.Add(top);
    }

    private static GroupBox Group(string title, Control inner)
    {
        var g = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(6) };
        inner.Dock = DockStyle.Fill;
        g.Controls.Add(inner);
        return g;
    }

    private static DataGridView MakeGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = SystemColors.Window,
    };

    private static readonly string[] AxisRows =
    {
        "Pos (counts)", "Δ 相對基準點", "基準後 最小 / 最大", "PosErr", "MotorOnStatus", "MotionStat",
        "OperationMode", "LimitStat", "ConFlt", "ConFaultMsg", "StatReg (hex)", "Force", "Iq",
    };

    private static readonly string[] CtrlRows =
    {
        "SDK IsConnected", "ConnectedCableType", "心跳 (SendCommandString APos)", "心跳往返 ms", "連續失敗次數",
        "輪詢次數", "最後更新", "VBus", "VLogic", "PwrTemp", "BoardTemp",
    };

    private void InitGrids()
    {
        _gridAxis.Columns.Add("item", "項目");
        _gridAxis.Columns.Add("a", Z1Label);
        _gridAxis.Columns.Add("b", Z2Label);
        _gridAxis.Columns[0].FillWeight = 60;
        foreach (var r in AxisRows) _gridAxis.Rows.Add(r, "—", "—");

        _gridCtrl.Columns.Add("item", "項目");
        _gridCtrl.Columns.Add("v", "值");
        foreach (var r in CtrlRows) _gridCtrl.Rows.Add(r, "—");

        _gridParams.Columns.Add("name", "名稱");
        _gridParams.Columns.Add("now", "控制器現值");
        _gridParams.Columns.Add("par", ".par 舊備份");
        _gridParams.Columns.Add("note", "說明");
    }

    // ================================================================ 動作

    private async Task Connect()
    {
        var ip = _tbIp.Text.Trim();
        if (!IPAddress.TryParse(ip, out _)) { MessageBox.Show(this, "IP 格式不正確"); return; }
        var kind = _cbKind.SelectedIndex == 0 ? SafeController.Kind.AGC301 : SafeController.Kind.AGM800;

        SetBusy(true, "連線中…");
        CheckNic();
        bool ok;
        try { ok = await _ctl.ConnectAsync(kind, ip); }
        catch (Exception ex) { AppendLog("連線例外：" + ex.Message); ok = false; }

        if (ok)
        {
            _trackA.Reset(); _trackB.Reset(); Array.Clear(_dinLatched); _lastDin = null;
            _snapshotPending = true;
            SetBusy(false, null);
            await ReadParams();
        }
        else
        {
            SetBusy(false, null);
            AppendLog("連線失敗。檢查：控制器有無上電、網路線、PC 網卡是否與控制器同網段（見上方網卡檢查）、IP 是否正確、防火牆。");
        }
    }

    private async Task Disconnect()
    {
        SetBusy(true, "斷線中…");
        try { await _ctl.DisconnectAsync(); } catch (Exception ex) { AppendLog("斷線例外：" + ex.Message); }
        SetBusy(false, null);
    }

    private void SetBusy(bool busy, string text)
    {
        _btnConnect.Enabled = !busy && !_ctl.Connected;
        _btnDisconnect.Enabled = !busy && _ctl.Connected;
        _btnReadParams.Enabled = !busy && _ctl.Connected;
        _btnQuery.Enabled = !busy && _ctl.Connected;
        _cbKind.Enabled = _tbIp.Enabled = !busy && !_ctl.Connected;
        if (text != null) { _lblConn.Text = text; _lblConn.ForeColor = Color.DarkOrange; }
    }

    private async Task ReadParams()
    {
        _btnReadParams.Enabled = false;
        _gridParams.Rows.Clear();
        try
        {
            foreach (var (n, v) in await _ctl.ReadIdentityAsync())
            {
                _gridParams.Rows.Add(n, v, "", "控制器識別（typed 屬性）");
                AppendLog($"IDENT {n,-22} = {v}");
            }

            var names = new List<(string Name, string Note)>();
            foreach (var ax in new[] { "A", "B" })
            {
                var who = ax == "A" ? "Z1" : "Z2";
                names.Add((ax + "MotorType", $"{who} 馬達型式（舊值 A=4 三相、B=2 音圈）"));
                names.Add((ax + "EncType", $"{who} 編碼器型式"));
                names.Add((ax + "EncRes", $"{who} 編碼器解析度"));
                names.Add((ax + "ContCL", $"{who} 連續電流限制 mA ⚠"));
                names.Add((ax + "PeakCL", $"{who} 峰值電流限制 mA ⚠"));
                names.Add((ax + "CurrLimFwd", $"{who}"));
                names.Add((ax + "CurrLimRev", $"{who}"));
                names.Add((ax + "FwdPLim", $"{who} 軟體正向行程極限"));
                names.Add((ax + "RevPLim", $"{who} 軟體負向行程極限"));
                names.Add((ax + "BrakeUsed", $"{who} 煞車啟用"));
                names.Add((ax + "BrakeMode", $"{who} 煞車模式"));
                names.Add((ax + "BrakeRelTime", $"{who}"));
                names.Add((ax + "BrakeLockTime", $"{who}"));
                names.Add((ax + "EmrgDec", $"{who} 緊急減速"));
                names.Add((ax + "OperationMode", $"{who} 3=Position 4=Force"));
            }
            names.Add(("ADInLog", "數位輸入反相遮罩"));
            for (int i = 1; i <= 32; i++)
                if (_par.Values.ContainsKey($"ADInMode[{i}]")) names.Add(($"ADInMode[{i}]", "DIn 功能（高16位=軸?、低16位=功能?，推測）"));
            for (int i = 1; i <= 4; i++)
            {
                if (_par.Values.ContainsKey($"AAInMode[{i}]")) names.Add(($"AAInMode[{i}]", i == 2 ? "AI#2 力量回授（舊值 0x20003）" : "AI 模式"));
                if (_par.Values.ContainsKey($"AAInGain[{i}]")) names.Add(($"AAInGain[{i}]", "AI 增益"));
                if (_par.Values.ContainsKey($"AAInOffset[{i}]")) names.Add(($"AAInOffset[{i}]", "AI 偏移"));
            }

            var res = await _ctl.ReadParamsAsync(names.Select(x => x.Name));
            var dinMode = new Dictionary<string, string>();
            for (int i = 0; i < res.Count; i++)
            {
                var (n, now) = res[i];
                _par.Values.TryGetValue(n, out var old);
                var note = names[i].Note;
                if (n.StartsWith("ADInMode[") || n.StartsWith("AAInMode["))
                    note += "  " + DecodeMode(now);
                if (n.StartsWith("ADInMode[")) dinMode[n] = now;
                int row = _gridParams.Rows.Add(n, now, old ?? "(無)", note);
                bool diff = old != null && !now.StartsWith("[") && !string.Equals(Normalize(now), Normalize(old), StringComparison.OrdinalIgnoreCase);
                if (diff) _gridParams.Rows[row].DefaultCellStyle.BackColor = Color.MistyRose;
                AppendLog($"PARAM {n,-18} 現值={now,-14} .par={old ?? "(無)",-14} {(diff ? "≠ 不同" : "")}  {note}");
            }
            _dinModeNow = dinMode;
            UpdateDinTooltips();
            AppendLog($"已讀取 {res.Count} 個參數（見「參數比對」分頁）");
        }
        catch (Exception ex) { AppendLog("讀參數例外：" + ex.Message); }
        finally { _btnReadParams.Enabled = _ctl.Connected; }
    }

    private static string Normalize(string s) => (s ?? "").Trim().TrimEnd(';');

    private static string DecodeMode(string v)
    {
        if (!long.TryParse(Normalize(v), out var x)) return "";
        long axis = x >> 16, func = x & 0xFFFF;
        var a = axis switch { 1 => "A", 2 => "B", 3 => "C", _ => axis.ToString() };
        return $"(0x{x:X} → 軸 {a}, 功能 {func}；推測)";
    }

    private void UpdateDinTooltips()
    {
        for (int i = 0; i < 32; i++)
        {
            var key = $"ADInMode[{i + 1}]";
            var tip = $"IN{i + 1}";
            if (_dinModeNow.TryGetValue(key, out var now)) tip += $"\n控制器現值 DInMode = {now} {DecodeMode(now)}";
            if (_par.Values.TryGetValue(key, out var old)) tip += $"\n.par 舊值 DInMode = {old} {DecodeMode(old)}";
            _tip.SetToolTip(_din[i], tip);
        }
    }

    private async Task RunQuery()
    {
        var q = _tbQuery.Text.Trim();
        if (q.Length == 0) return;
        if (!_ctl.IsAllowedQuery(q, out var why))
        {
            _tbQueryOut.AppendText($"{q}  →  [拒絕] {why}\r\n");
            return;
        }
        if (!_ctl.Connected) { _tbQueryOut.AppendText($"{q}  →  [未連線]\r\n"); return; }
        var r = await _ctl.QueryAsync(q);
        _par.Values.TryGetValue(q, out var old);
        _tbQueryOut.AppendText($"{DateTime.Now:HH:mm:ss}  {q}  →  {r}" + (old != null ? $"     (.par 舊值 {old})" : "") + "\r\n");
    }

    // ================================================================ 網路檢查

    private async Task PingAndCheckNic()
    {
        CheckNic();
        var ip = _tbIp.Text.Trim();
        if (!IPAddress.TryParse(ip, out _)) { AppendLog("IP 格式不正確"); return; }
        _btnPing.Enabled = false;
        try
        {
            using var p = new Ping();
            for (int i = 0; i < 3; i++)
            {
                var r = await p.SendPingAsync(ip, 1000);
                AppendLog(r.Status == IPStatus.Success ? $"Ping {ip}: OK  {r.RoundtripTime} ms" : $"Ping {ip}: {r.Status}");
            }
        }
        catch (Exception ex) { AppendLog($"Ping 例外：{ex.Message}"); }
        finally { _btnPing.Enabled = true; }
    }

    private void CheckNic()
    {
        if (!IPAddress.TryParse(_tbIp.Text.Trim(), out var target)) return;
        var lines = new List<string>();
        bool sameSubnet = false;
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                bool match = SameSubnet(ua.Address, target, ua.IPv4Mask);
                sameSubnet |= match && ni.OperationalStatus == OperationalStatus.Up;
                lines.Add($"{ni.Name} [{ni.OperationalStatus}] {ua.Address}/{ua.PrefixLength}" + (match ? " ← 與控制器同網段" : ""));
            }
        }
        _lblNet.Text = "PC 網卡：" + string.Join("    ", lines) +
                       (sameSubnet ? "" : $"\n⚠ 沒有任何「已連線」網卡與 {target} 同網段。請把接控制器的網卡設成固定 IP（例 172.1.1.10 / 255.255.255.0），控制器上電後網卡狀態才會變 Up。");
        _lblNet.ForeColor = sameSubnet ? Color.DimGray : Color.DarkRed;
    }

    private static bool SameSubnet(IPAddress a, IPAddress b, IPAddress mask)
    {
        if (mask == null) return false;
        var x = a.GetAddressBytes(); var y = b.GetAddressBytes(); var m = mask.GetAddressBytes();
        if (x.Length != 4 || y.Length != 4 || m.Length != 4) return false;
        for (int i = 0; i < 4; i++) if ((x[i] & m[i]) != (y[i] & m[i])) return false;
        return true;
    }

    // ================================================================ 畫面更新

    private void RefreshUi()
    {
        while (_ctl.TryDequeueLog(out var line)) AppendRaw(line);

        if (!_ctl.Connected)
        {
            if (!_btnConnect.Enabled && !_btnDisconnect.Enabled) return; // busy
            if (_lblConn.Text != "未連線") { _lblConn.Text = "未連線"; _lblConn.ForeColor = Color.DimGray; SetBusy(false, null); }
            return;
        }

        var s = _ctl.Latest;
        if (s == null) { _lblConn.Text = "已連線，等待第一筆資料…"; _lblConn.ForeColor = Color.DarkOrange; return; }

        var age = (DateTime.Now - s.Time).TotalMilliseconds;
        if (s.HeartbeatOk && age < 1500) { _lblConn.Text = $"● 通訊正常  {s.HeartbeatMs:0.0} ms"; _lblConn.ForeColor = Color.ForestGreen; }
        else { _lblConn.Text = $"● 通訊異常（連續失敗 {s.ConsecutiveFailures}）"; _lblConn.ForeColor = Color.Crimson; }
        if (!_btnDisconnect.Enabled) SetBusy(false, null);

        _trackA.Update(s.A.Pos);
        _trackB.Update(s.B.Pos);
        if (_snapshotPending && s.PollCount >= 5) { _snapshotPending = false; LogSnapshot(s); }
        FillAxisColumn(1, s.A, _trackA);
        FillAxisColumn(2, s.B, _trackB);

        SetCtrl(0, s.SdkIsConnected?.ToString() ?? "讀取失敗");
        SetCtrl(1, Fmt(s.CableType));
        SetCtrl(2, s.HeartbeatOk ? $"OK  → {s.HeartbeatResponse?.Trim()}" : $"失敗  {s.HeartbeatResponse}");
        SetCtrl(3, double.IsNaN(s.HeartbeatMs) ? "—" : s.HeartbeatMs.ToString("0.0"));
        SetCtrl(4, s.ConsecutiveFailures.ToString());
        SetCtrl(5, s.PollCount.ToString());
        SetCtrl(6, $"{s.Time:HH:mm:ss.fff}");
        SetCtrl(7, Fmt(s.VBus));
        SetCtrl(8, Fmt(s.VLogic));
        SetCtrl(9, Fmt(s.PwrTemp));
        SetCtrl(10, Fmt(s.BoardTemp));

        if (s.DInPort is int din)
        {
            if (_lastDin is int prev)
            {
                int changed = prev ^ din;
                for (int i = 0; i < 32; i++) if ((changed & (1 << i)) != 0) _dinLatched[i] = true;
            }
            _lastDin = din;
            for (int i = 0; i < 32; i++)
            {
                bool on = (din & (1 << i)) != 0;
                var l = _din[i];
                l.BackColor = on ? Color.LimeGreen : Color.Gainsboro;
                l.ForeColor = _dinLatched[i] ? Color.OrangeRed : Color.Black;
                l.Font = _dinLatched[i] ? (_bold ??= new Font(Font, FontStyle.Bold)) : Font;
                l.Text = _dinLatched[i] ? $"IN{i + 1}*" : $"IN{i + 1}";
            }
        }
        if (s.AInPort != null)
            for (int i = 0; i < 4; i++)
                _ain[i].Text = i < s.AInPort.Length ? $"AI{i + 1}: {s.AInPort[i]}" : $"AI{i + 1}: —";
    }

    private void FillAxisColumn(int col, AxisSnap a, AxisTrack t)
    {
        string delta = a.Pos is int p && t.Mark is int m ? (p - m).ToString("+#,0;-#,0;0") : "—";
        string range = t.Min is int mn && t.Max is int mx ? $"{mn:#,0} / {mx:#,0}  (跨度 {mx - mn:#,0})" : "—";
        var vals = new[]
        {
            a.Pos?.ToString("#,0") ?? "讀取失敗",
            delta,
            range,
            Fmt(a.PosErr),
            a.MotorOnStatus switch { 0 => "0 (Off)", 1 => "1 (ON!)", null => "讀取失敗", var x => x.ToString() },
            a.MotionStat switch { 0 => "0 (靜止/到位)", null => "讀取失敗", var x => x.ToString() },
            a.OperationMode switch { 3 => "3 (Position)", 4 => "4 (Force)", null => "讀取失敗", var x => x.ToString() },
            a.LimitStat is int ls ? $"{ls} (0x{ls:X})" : "讀取失敗",
            a.ConFlt is int cf ? (cf == 0 ? "0 (無故障)" : $"{cf} ⚠") : "讀取失敗",
            string.IsNullOrWhiteSpace(a.ConFaultMsg) ? "—" : a.ConFaultMsg,
            a.StatReg is int sr ? $"0x{sr:X8}" : "讀取失敗",
            Fmt(a.Force),
            Fmt(a.Iq),
        };
        for (int r = 0; r < vals.Length; r++)
        {
            var cell = _gridAxis.Rows[r].Cells[col];
            if (!Equals(cell.Value, vals[r])) cell.Value = vals[r];
        }
        // 醒目標示：馬達被打開 / 有故障
        _gridAxis.Rows[4].Cells[col].Style.BackColor = a.MotorOnStatus == 1 ? Color.Orange : Color.Empty;
        _gridAxis.Rows[8].Cells[col].Style.BackColor = a.ConFlt is int f && f != 0 ? Color.MistyRose : Color.Empty;
    }

    private bool _snapshotPending;

    /// <summary>把目前快照寫進 Log（連線後自動一次；也可按按鈕手動）。</summary>
    private void LogSnapshot(Snap s)
    {
        if (s == null) { AppendLog("SNAP 尚無資料"); return; }
        static string Ax(AxisSnap a) =>
            $"Pos={a.Pos?.ToString() ?? "?"} PosErr={a.PosErr?.ToString() ?? "?"} MotorOn={a.MotorOnStatus?.ToString() ?? "?"} " +
            $"MotionStat={a.MotionStat?.ToString() ?? "?"} OpMode={a.OperationMode?.ToString() ?? "?"} " +
            $"LimitStat={(a.LimitStat is int l ? $"0x{l:X}" : "?")} ConFlt={a.ConFlt?.ToString() ?? "?"} " +
            $"ConFaultMsg=\"{a.ConFaultMsg}\" StatReg={(a.StatReg is int r ? $"0x{r:X8}" : "?")} Force={a.Force?.ToString() ?? "?"} Iq={a.Iq?.ToString() ?? "?"}";
        AppendLog($"SNAP 控制器 IsConnected={s.SdkIsConnected} Cable={s.CableType} 心跳={(s.HeartbeatOk ? "OK" : "FAIL")} {s.HeartbeatMs:0.0}ms " +
                  $"VBus={s.VBus} VLogic={s.VLogic} PwrTemp={s.PwrTemp} BoardTemp={s.BoardTemp}");
        AppendLog($"SNAP A(Z1) {Ax(s.A)}");
        AppendLog($"SNAP B(Z2) {Ax(s.B)}");
        AppendLog($"SNAP DInPort={(s.DInPort is int d ? $"0x{d:X8}" : "?")} DInPortHigh={(s.DInPortHigh is int h ? $"0x{h:X}" : "?")} " +
                  $"AInPort=[{(s.AInPort == null ? "?" : string.Join(", ", s.AInPort))}]");
    }

    private void SetCtrl(int row, string v)
    {
        var c = _gridCtrl.Rows[row].Cells[1];
        if (!Equals(c.Value, v)) c.Value = v;
    }

    private static string Fmt(int? v) => v?.ToString() ?? "讀取失敗";

    private void AppendLog(string s) => AppendRaw($"{DateTime.Now:HH:mm:ss.fff}  {s}");

    private void AppendRaw(string line)
    {
        _tbLog.AppendText(line + "\r\n");
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, $"commtest_{DateTime.Now:yyyyMMdd}.txt"), line + Environment.NewLine);
        }
        catch { /* log 檔寫不進去不影響測試 */ }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _uiTimer.Stop();
        _ctl.Dispose(); // 只做 Disconnect，不對馬達下任何指令
        base.OnFormClosing(e);
    }
}
