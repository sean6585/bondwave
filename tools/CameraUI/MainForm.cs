using System.Diagnostics;

namespace CameraUI;

/// <summary>
/// Bondwave 相機控制介面。
/// 左：影像（即時 / 回放），右：分頁。所有相機操作透過 CameraService（專屬執行緒），UI 不直接碰 SDK。
/// </summary>
public sealed class MainForm : Form
{
    private readonly AppSettings _cfg = AppSettings.Load();
    private readonly CameraService _cam = new();

    // ---------------- 影像區
    private readonly Panel _imgPanel = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Black };
    private readonly PictureBox _pic = new() { SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
    private readonly ComboBox _cbZoom = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly Label _lblImg = new() { AutoSize = true, ForeColor = Color.Gainsboro, Padding = new Padding(6, 6, 0, 0) };
    private Bitmap _bmp;
    private Frame _shownFrame;
    private double _focusSmooth;

    // ---------------- 狀態
    private readonly Label _lblState = new() { AutoSize = true, Dock = DockStyle.Top, Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold), Padding = new Padding(0, 4, 0, 4) };
    private readonly DataGridView _gridStatus = Grid();
    private readonly Button _btnConnect = Btn("連線相機");
    private readonly Button _btnDisconnect = Btn("斷線");

    // ---------------- 即時 / 單張
    private readonly NumericUpDown _nLiveFps = Num(1, 1800, 1, 0);
    private readonly NumericUpDown _nLiveExp = Num(10, 1_000_000, 10, 0);
    private readonly Button _btnLive = Btn("▶ 開始即時畫面");
    private readonly Button _btnStop = Btn("■ 停止");
    private readonly Button _btnSnap = Btn("📷 取單張 (TIFF)");
    private readonly TextBox _tbSnapDir = new() { Width = 260 };
    private readonly Label _lblSnap = new() { AutoSize = true, MaximumSize = new Size(420, 0) };

    // ---------------- 錄影
    private readonly NumericUpDown _nRecFps = Num(1, 1800, 1, 0);
    private readonly NumericUpDown _nRecExp = Num(10, 100_000, 10, 0);
    private readonly NumericUpDown _nRecSec = Num(0.1m, 600, 0.5m, 1);
    private readonly TextBox _tbRecName = new() { Width = 160 };
    private readonly TextBox _tbRecDir = new() { Width = 260 };
    private readonly CheckBox _chkBackToLive = new() { Text = "錄完自動回到即時畫面", AutoSize = true, Checked = true };
    private readonly Button _btnRec = Btn("● 開始錄影");
    private readonly Button _btnRecAbort = Btn("中止錄影");
    private readonly ProgressBar _prgRec = new() { Width = 400, Height = 18 };
    private readonly Label _lblRecPlan = new() { AutoSize = true, MaximumSize = new Size(440, 0), ForeColor = Color.DimGray };
    private readonly Label _lblRecProg = new() { AutoSize = true };
    private readonly TextBox _tbRecResult = MultiText();

    // ---------------- 回放 / 分析
    private readonly ListBox _lstRecs = new() { Height = 120, Width = 420 };
    private readonly Button _btnRefreshRecs = Btn("重新整理清單");
    private readonly Button _btnOpenRec = Btn("開啟其他資料夾…");
    private readonly Label _lblRecInfo = new() { AutoSize = true, MaximumSize = new Size(440, 0) };
    private readonly TrackBar _trk = new() { Width = 420, TickStyle = TickStyle.None, Minimum = 0, Maximum = 0 };
    private readonly Button _btnPlay = Btn("▶ 播放");
    private readonly Button _btnPrev = Btn("◀");
    private readonly Button _btnNext = Btn("▶|");
    private readonly NumericUpDown _nPlayFps = Num(1, 240, 5, 0);
    private readonly Button _btnAnalyze = Btn("分析 FPS / 缺張");
    private readonly Button _btnExportCur = Btn("匯出這張 TIFF");
    private readonly Button _btnExportRange = Btn("匯出範圍 TIFF…");
    private readonly NumericUpDown _nExpFrom = Num(0, int.MaxValue, 1, 0);
    private readonly NumericUpDown _nExpTo = Num(0, int.MaxValue, 1, 0);
    private readonly TextBox _tbAnalysis = MultiText();
    private readonly System.Windows.Forms.Timer _playTimer = new();
    private RecordingReader _reader;
    private CancellationTokenSource _bgCts;

    // ---------------- 相機參數
    private readonly ComboBox _cbMethod = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly TextBox _tbTimerFeat = new() { Width = 160 };
    private readonly Button _btnDetect = Btn("自動偵測 FPS 控制方式");
    private readonly TextBox _tbFeatFilter = new() { Width = 160 };
    private readonly Button _btnFeatList = Btn("列出");
    private readonly ListBox _lstFeat = new() { Width = 260, Height = 240 };
    private readonly TextBox _tbFeatValue = new() { Width = 160 };
    private readonly Button _btnFeatRead = Btn("讀");
    private readonly Button _btnFeatWrite = Btn("寫入");

    // ---------------- 其他
    private readonly TextBox _tbLog = MultiText();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TabPage _tpPlayback = new("回放 / 分析");
    private readonly System.Windows.Forms.Timer _uiTimer = new() { Interval = 16 };
    private readonly ToolTip _tip = new();
    private bool _busy;

    public MainForm()
    {
        Text = "Bondwave 相機控制 — Cheetah+ 640 CXP";
        Width = 1500; Height = 920;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft JhengHei UI", 9.5f);

        BuildLayout();
        LoadSettingsToUi();
        Wire();

        _uiTimer.Tick += (_, _) => OnUiTick();
        _uiTimer.Start();
        Log("相機控制介面啟動。請先關閉 eGrabber Studio（同一時間只能有一個程式控制相機），再按「連線相機」。");
        RefreshRecordingList();
    }

    // ================================================================ 版面

    private static Button Btn(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(3) };
    private static NumericUpDown Num(decimal min, decimal max, decimal inc, int dec) =>
        new() { Minimum = min, Maximum = max, Increment = inc, DecimalPlaces = dec, Width = 100, ThousandsSeparator = true };
    private static TextBox MultiText() => new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill,
        Font = new Font("Consolas", 9.5f),
    };
    private static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, ColumnHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = SystemColors.Window,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToResizeRows = false,
    };

    private static FlowLayoutPanel Row(params Control[] cs)
    {
        var p = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Top, Margin = new Padding(0, 2, 0, 2) };
        foreach (var c in cs)
        {
            if (c is Label l && l.AutoSize) l.Padding = new Padding(0, 6, 0, 0);
            p.Controls.Add(c);
        }
        return p;
    }
    private static Label L(string t, int w = 0) => w > 0
        ? new Label { Text = t, AutoSize = false, Width = w, Height = 26, TextAlign = ContentAlignment.MiddleLeft }
        : new Label { Text = t, AutoSize = true };
    private static Label Hint(string t) => new() { Text = t, AutoSize = true, MaximumSize = new Size(450, 0), ForeColor = Color.DimGray, Padding = new Padding(0, 4, 0, 4) };

    /// <summary>把一串控制項由上而下排進分頁（FlowLayout TopDown）。</summary>
    private static TabPage Page(string title, params Control[] cs)
    {
        var tp = new TabPage(title) { Padding = new Padding(8) };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        foreach (var c in cs) { c.Dock = DockStyle.None; flow.Controls.Add(c); }
        tp.Controls.Add(flow);
        return tp;
    }

    private void BuildLayout()
    {
        // 影像區
        _cbZoom.Items.AddRange(new object[] { "適應視窗", "100%", "200%", "300%" });
        _cbZoom.SelectedIndex = 0;
        var imgTop = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = Color.FromArgb(30, 30, 30) };
        imgTop.Controls.Add(new Label { Text = "縮放", AutoSize = true, ForeColor = Color.Gainsboro, Padding = new Padding(6, 8, 0, 0) });
        imgTop.Controls.Add(_cbZoom);
        imgTop.Controls.Add(_lblImg);
        _imgPanel.Controls.Add(_pic);
        var left = new Panel { Dock = DockStyle.Fill };
        left.Controls.Add(_imgPanel);
        left.Controls.Add(imgTop);

        // 狀態
        _gridStatus.Columns.Add("k", "項目");
        _gridStatus.Columns.Add("v", "值");
        _gridStatus.Columns[0].FillWeight = 40;
        foreach (var k in StatusRows) _gridStatus.Rows.Add(k, "—");
        var tpStatus = new TabPage("狀態") { Padding = new Padding(8) };
        tpStatus.Controls.Add(_gridStatus);
        tpStatus.Controls.Add(Row(_btnConnect, _btnDisconnect));
        tpStatus.Controls.Add(_lblState);

        // 即時 / 單張
        var tpLive = Page("即時 / 單張",
            Hint("對焦用的即時畫面。FPS 只要肉眼順暢即可（預設 120）；曝光可以比錄影時長，畫面較亮。"),
            Row(L("即時 FPS", 110), _nLiveFps),
            Row(L("即時曝光 (µs)", 110), _nLiveExp),
            Row(_btnLive, _btnStop),
            Hint("影像上方會顯示「對焦指標」：數字越大越清楚，轉動對焦環找最大值（只能相對比較）。"),
            new Label { Text = " ", AutoSize = true },
            Row(L("單張存到", 110), _tbSnapDir),
            Row(_btnSnap),
            _lblSnap);

        // 錄影
        var tpRec = Page("錄影",
            Hint("錄成 eGrabber container（與 Studio 同格式）。能放進 RAM 的張數會全部預先配置 buffer，保證不掉張；錄完自動分析 FPS / 缺張。"),
            Row(L("錄影 FPS", 110), _nRecFps),
            Row(L("錄影曝光 (µs)", 110), _nRecExp),
            Row(L("錄影秒數", 110), _nRecSec),
            Row(L("名稱（選填）", 110), _tbRecName),
            Row(L("存到", 110), _tbRecDir),
            _chkBackToLive,
            _lblRecPlan,
            Row(_btnRec, _btnRecAbort),
            _prgRec,
            _lblRecProg,
            new Panel { Height = 260, Width = 460, Controls = { _tbRecResult } });

        // 回放
        _tpPlayback.Padding = new Padding(8);
        var pbFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        foreach (var c in new Control[]
        {
            Hint("選一個錄影（雙擊開啟）。切到這個分頁時左邊顯示回放畫面。"),
            _lstRecs, Row(_btnRefreshRecs, _btnOpenRec), _lblRecInfo, _trk,
            Row(_btnPrev, _btnPlay, _btnNext, L("播放速度 (張/秒)"), _nPlayFps),
            Row(_btnAnalyze, _btnExportCur),
            Row(L("匯出 從"), _nExpFrom, L("到"), _nExpTo, _btnExportRange),
            new Panel { Height = 300, Width = 460, Controls = { _tbAnalysis } },
        }) { c.Dock = DockStyle.None; pbFlow.Controls.Add(c); }
        _tpPlayback.Controls.Add(pbFlow);

        // 相機參數
        _cbMethod.Items.AddRange(new object[]
        {
            "只改 AcquisitionFrameTime（不動觸發設定）",
            "TriggerMode=Off + AcquisitionFrameTime",
            "TriggerMode=On + Timer（改下面的 Timer 參數）",
        });
        var tpParams = Page("相機參數",
            Hint("FPS 控制方式：這台相機哪一種有效還沒確定。第一次上機請按「自動偵測」，它會依序試三種方式並實測 FPS，選第一個成功的。"),
            Row(L("FPS 控制方式", 110), _cbMethod),
            Row(L("Timer 參數名", 110), _tbTimerFeat),
            Row(_btnDetect),
            new Label { Text = " ", AutoSize = true },
            Hint("相機參數瀏覽（Remote device）：輸入關鍵字後按「列出」，點選後可讀 / 寫。改參數前請先停止擷取。"),
            Row(L("關鍵字"), _tbFeatFilter, _btnFeatList),
            _lstFeat,
            Row(L("值"), _tbFeatValue, _btnFeatRead, _btnFeatWrite));

        var tpLog = new TabPage("Log") { Padding = new Padding(4) };
        tpLog.Controls.Add(_tbLog);

        _tabs.TabPages.AddRange(new[] { tpStatus, tpLive, tpRec, _tpPlayback, tpParams, tpLog });

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(_tabs);
        Controls.Add(split);
        Shown += (_, _) => { split.SplitterDistance = Math.Max(300, ClientSize.Width - 500); ApplyZoom(); };
        _imgPanel.Resize += (_, _) => ApplyZoom();
    }

    private static readonly string[] StatusRows =
    {
        "連線狀態", "相機", "Grabber", "影像格式", "擷取狀態", "設定 FPS / 曝光", "實測 FPS", "累計張數", "即時缺張（間隔 >1.5 週期）",
        "觸發設定", "Buffer 數", "相機溫度", "可用記憶體", "錄影磁碟剩餘", "最後錯誤",
    };

    // ================================================================ 設定 <-> UI

    private void LoadSettingsToUi()
    {
        _nLiveFps.Value = (decimal)_cfg.LiveFps;
        _nLiveExp.Value = (decimal)_cfg.LiveExposureUs;
        _nRecFps.Value = (decimal)_cfg.RecordFps;
        _nRecExp.Value = (decimal)_cfg.RecordExposureUs;
        _nRecSec.Value = (decimal)_cfg.RecordSeconds;
        _tbRecDir.Text = _cfg.RecordRoot;
        _tbSnapDir.Text = _cfg.SnapshotRoot;
        _tbTimerFeat.Text = _cfg.TimerFeature;
        _cbMethod.SelectedIndex = (int)_cfg.Method;
        _nPlayFps.Value = 30;
    }

    private void SaveUiToSettings()
    {
        _cfg.LiveFps = (double)_nLiveFps.Value;
        _cfg.LiveExposureUs = (double)_nLiveExp.Value;
        _cfg.RecordFps = (double)_nRecFps.Value;
        _cfg.RecordExposureUs = (double)_nRecExp.Value;
        _cfg.RecordSeconds = (double)_nRecSec.Value;
        _cfg.RecordRoot = _tbRecDir.Text.Trim();
        _cfg.SnapshotRoot = _tbSnapDir.Text.Trim();
        _cfg.TimerFeature = _tbTimerFeat.Text.Trim();
        _cfg.Method = (FpsMethod)Math.Max(0, _cbMethod.SelectedIndex);
        _cfg.Save();
    }

    // ================================================================ 事件

    private void Wire()
    {
        _btnConnect.Click += async (_, _) => await Guard("連線", async () =>
        {
            await _cam.ConnectAsync(_cfg.PixelFormat);
            _nExpFrom.Maximum = _nExpTo.Maximum = int.MaxValue;
        });
        _btnDisconnect.Click += async (_, _) => await Guard("斷線", () => _cam.DisconnectAsync());
        _btnLive.Click += async (_, _) => await StartLive();
        _btnStop.Click += async (_, _) => await Guard("停止", () => _cam.StopAsync());
        _btnSnap.Click += async (_, _) => await Snapshot();
        _btnRec.Click += async (_, _) => await Record();
        _btnRecAbort.Click += (_, _) => { _cam.AbortRecording(); Log("要求中止錄影…"); };
        foreach (var n in new[] { _nRecFps, _nRecExp, _nRecSec }) n.ValueChanged += (_, _) => UpdateRecPlan();
        _tbRecDir.TextChanged += (_, _) => UpdateRecPlan();
        UpdateRecPlan();

        _cbZoom.SelectedIndexChanged += (_, _) => ApplyZoom();
        _tabs.SelectedIndexChanged += (_, _) => { if (_tabs.SelectedTab == _tpPlayback && _reader != null) ShowPlaybackFrame(_trk.Value); };

        _btnRefreshRecs.Click += (_, _) => RefreshRecordingList();
        _lstRecs.DoubleClick += (_, _) => { if (_lstRecs.SelectedItem is string s) OpenRecording(Path.Combine(_tbRecDir.Text.Trim(), s)); };
        _btnOpenRec.Click += (_, _) =>
        {
            using var d = new FolderBrowserDialog { Description = "選擇錄影資料夾（含 index / data 檔）", SelectedPath = _tbRecDir.Text };
            if (d.ShowDialog(this) == DialogResult.OK) OpenRecording(d.SelectedPath);
        };
        _trk.ValueChanged += (_, _) => ShowPlaybackFrame(_trk.Value);
        _btnPrev.Click += (_, _) => { if (_trk.Value > _trk.Minimum) _trk.Value--; };
        _btnNext.Click += (_, _) => { if (_trk.Value < _trk.Maximum) _trk.Value++; };
        _btnPlay.Click += (_, _) => TogglePlay();
        _playTimer.Tick += (_, _) =>
        {
            if (_reader == null || _trk.Value >= _trk.Maximum) { TogglePlay(false); return; }
            _trk.Value = Math.Min(_trk.Maximum, _trk.Value + 1);
        };
        _nPlayFps.ValueChanged += (_, _) => _playTimer.Interval = Math.Max(1, (int)(1000 / (double)_nPlayFps.Value));
        _btnAnalyze.Click += async (_, _) => await AnalyzeCurrent();
        _btnExportCur.Click += (_, _) => ExportCurrent();
        _btnExportRange.Click += async (_, _) => await ExportRange();

        _btnDetect.Click += async (_, _) => await DetectMethod();
        _btnFeatList.Click += async (_, _) => await Guard("列出參數", async () =>
        {
            var list = await _cam.ListFeaturesAsync(_tbFeatFilter.Text);
            _lstFeat.Items.Clear();
            _lstFeat.Items.AddRange(list.Cast<object>().ToArray());
        });
        _tbFeatFilter.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _btnFeatList.PerformClick(); } };
        _lstFeat.SelectedIndexChanged += async (_, _) => await ReadSelectedFeature();
        _btnFeatRead.Click += async (_, _) => await ReadSelectedFeature();
        _btnFeatWrite.Click += async (_, _) => await Guard("寫入參數", async () =>
        {
            if (_lstFeat.SelectedItem is not string f) return;
            var back = await _cam.SetFeatureAsync(f, _tbFeatValue.Text.Trim());
            _tbFeatValue.Text = back;
        });

        _tip.SetToolTip(_btnDetect, "依序測試：只改 FrameTime → Trigger Off + FrameTime → Trigger On + Timer，各量 1.5 秒");
        _tip.SetToolTip(_nRecExp, "1700 fps 時週期 588 µs，曝光要小於約 570 µs");
    }

    private async Task Guard(string what, Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        try { await action(); }
        catch (Exception ex) { Log($"{what} 失敗：{CameraService.Describe(ex)}"); MessageBox.Show(this, CameraService.Describe(ex), what + " 失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { _busy = false; }
    }

    private Task StartLive() => Guard("即時畫面", async () =>
    {
        SaveUiToSettings();
        await _cam.StartLiveAsync(_cfg.LiveFps, _cfg.LiveExposureUs, _cfg.Method, _cfg.TimerFeature, _cfg.LiveBuffers);
        _tabs.SelectedIndex = 1;
    });

    private Task Snapshot() => Guard("取單張", async () =>
    {
        SaveUiToSettings();
        var path = Path.Combine(_cfg.SnapshotRoot, $"snap_{DateTime.Now:yyyyMMdd_HHmmss_fff}.tif");
        var saved = await _cam.SnapshotAsync(path);
        _lblSnap.Text = "已存：" + saved;
    });

    private void UpdateRecPlan()
    {
        double fps = (double)_nRecFps.Value, sec = (double)_nRecSec.Value;
        long frames = (long)Math.Ceiling(fps * sec);
        long payload = _cam.PayloadSize > 0 ? _cam.PayloadSize : 640 * 512;
        double gb = frames * (double)payload / 1e9;
        double availGb = CameraService.AvailablePhysicalBytes() / 1e9;
        double ramGb = Math.Max(0, availGb - 4) * _cfg.RecordRamFraction;
        double periodUs = 1e6 / fps;
        string disk = "";
        try { var d = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(_tbRecDir.Text.Trim()))!); disk = $"，磁碟 {d.Name} 剩 {d.AvailableFreeSpace / 1e9:0} GB"; } catch { }
        _lblRecPlan.Text =
            $"將錄 {frames:N0} 張（{gb:0.00} GB，資料率 {fps * payload / 1e6:0} MB/s）{disk}。\n" +
            $"可當 buffer 的 RAM 約 {ramGb:0.0} GB → " + (gb <= ramGb ? "全部放得進 RAM ✔ 不會掉張" : $"放不下，超出部分靠磁碟即時寫（SATA 約 344 MB/s，可能掉張）") +
            $"\n週期 {periodUs:0.0} µs，曝光必須 < {periodUs * 0.97:0} µs" + ((double)_nRecExp.Value > periodUs * 0.97 ? "  ⚠ 目前曝光太長" : "");
    }

    private async Task Record()
    {
        if (_busy) return;
        SaveUiToSettings();
        bool wasLive = _cam.State == CamState.Live;
        _busy = true;
        _tbRecResult.Text = "";
        _prgRec.Value = 0;
        try
        {
            var r = await _cam.RecordAsync(_cfg.RecordFps, _cfg.RecordExposureUs, _cfg.RecordSeconds, _cfg.Method, _cfg.TimerFeature,
                                           _cfg.RecordRoot, _tbRecName.Text.Trim(), _cfg.RecordRamFraction);
            var head = r.Error != null ? $"✘ 錄影失敗：{r.Error}" : r.Aborted ? "⚠ 錄影已中止" : "✔ 錄影完成";
            var text = $"{head}\r\n資料夾：{r.Path}\r\n寫入 {r.Written:N0} / {r.Target:N0} 張，耗時 {r.WallSec:0.0} s\r\n\r\n" +
                       (r.Stats != null ? r.Stats.Report("時間戳分析（相機實際拍攝時間）").Replace("\n", "\r\n") : "");
            _tbRecResult.Text = text;
            RefreshRecordingList();
            if (r.Error == null && r.Written > 0 && Directory.Exists(r.Path)) OpenRecording(r.Path, silent: true);
        }
        catch (Exception ex) { _tbRecResult.Text = "✘ " + CameraService.Describe(ex); }
        finally { _busy = false; }

        if (wasLive && _chkBackToLive.Checked && _cam.State == CamState.Idle) await StartLive();
    }

    private Task DetectMethod() => Guard("偵測 FPS 控制方式", async () =>
    {
        SaveUiToSettings();
        if (_cam.State == CamState.Live) await _cam.StopAsync();
        Log($"開始偵測：目標 {_cfg.RecordFps} fps、曝光 {_cfg.RecordExposureUs} µs…");
        var results = await _cam.DetectFpsMethodAsync(_cfg.RecordFps, _cfg.RecordExposureUs, _cfg.TimerFeature);
        var ok = results.FirstOrDefault(r => r.Success);
        var msg = string.Join("\r\n", results.Select(r => $"{(r.Success ? "✔" : "✘")} {r.Method}: {r.Detail}"));
        if (ok != null)
        {
            _cbMethod.SelectedIndex = (int)ok.Method;
            SaveUiToSettings();
            MessageBox.Show(this, msg + $"\r\n\r\n已改用：{_cbMethod.Text}", "偵測結果");
        }
        else MessageBox.Show(this, msg + "\r\n\r\n三種方式都沒有達到目標 FPS。請把 Log 分頁內容給工程師判斷。", "偵測結果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    });

    private async Task ReadSelectedFeature()
    {
        if (_lstFeat.SelectedItem is not string f) return;
        try { _tbFeatValue.Text = await _cam.GetFeatureAsync(f); }
        catch (Exception ex) { _tbFeatValue.Text = "[讀取失敗] " + CameraService.Describe(ex); }
    }

    // ================================================================ 回放

    private void RefreshRecordingList()
    {
        _lstRecs.Items.Clear();
        try
        {
            var root = _tbRecDir.Text.Trim();
            if (!Directory.Exists(root)) return;
            foreach (var d in new DirectoryInfo(root).GetDirectories().Where(d => File.Exists(Path.Combine(d.FullName, "index")))
                                                     .OrderByDescending(d => d.LastWriteTime))
                _lstRecs.Items.Add(d.Name);
        }
        catch (Exception ex) { Log("列出錄影失敗：" + ex.Message); }
    }

    private void OpenRecording(string dir, bool silent = false)
    {
        TogglePlay(false);
        try
        {
            _reader?.Dispose();
            _reader = null;
            _reader = new RecordingReader(dir);
            _trk.Minimum = 0;
            _trk.Maximum = (int)Math.Max(0, _reader.Count - 1);
            _trk.Value = 0;
            _nExpFrom.Maximum = _nExpTo.Maximum = Math.Max(0, _reader.Count - 1);
            _nExpFrom.Value = 0; _nExpTo.Value = _nExpTo.Maximum;
            _lblRecInfo.Text = $"{Path.GetFileName(dir)}：{_reader.Count:N0} 張，{_reader.Width}x{_reader.Height}" +
                               (_reader.ChapterInfo.Length > 0 ? $"\n{_reader.ChapterInfo}" : "");
            _tbAnalysis.Text = "";
            var sum = Path.Combine(dir, "summary.txt");
            if (File.Exists(sum)) _tbAnalysis.Text = File.ReadAllText(sum).Replace("\n", "\r\n");
            if (!silent) _tabs.SelectedTab = _tpPlayback;
            ShowPlaybackFrame(0);
            Log($"開啟錄影 {dir}（{_reader.Count:N0} 張）");
        }
        catch (Exception ex)
        {
            _reader = null;
            Log("開啟錄影失敗：" + CameraService.Describe(ex));
            if (!silent) MessageBox.Show(this, CameraService.Describe(ex), "開啟錄影失敗");
        }
    }

    private ulong _t0Playback;

    private void ShowPlaybackFrame(int index)
    {
        if (_reader == null || _reader.Count == 0) return;
        try
        {
            if (index == 0 || _t0Playback == 0) _t0Playback = _reader.ReadInfo(0).timestamp;
            var f = _reader.ReadFrame(index);
            if (_tabs.SelectedTab == _tpPlayback) Render(f, $"回放 #{index:N0}/{_reader.Count - 1:N0}  t = {(f.TimestampNs - _t0Playback) / 1e6:0.000} ms");
        }
        catch (Exception ex) { Log("讀取影像失敗：" + CameraService.Describe(ex)); TogglePlay(false); }
    }

    private void TogglePlay(bool? on = null)
    {
        bool play = on ?? !_playTimer.Enabled;
        if (play && _reader == null) return;
        _playTimer.Interval = Math.Max(1, (int)(1000 / (double)_nPlayFps.Value));
        _playTimer.Enabled = play;
        _btnPlay.Text = play ? "❚❚ 暫停" : "▶ 播放";
    }

    private async Task AnalyzeCurrent()
    {
        if (_reader == null) return;
        var dir = _reader.Path;
        _bgCts?.Cancel(); _bgCts = new CancellationTokenSource();
        var ct = _bgCts.Token;
        _tbAnalysis.Text = "讀取時間戳中…";
        try
        {
            var text = await Task.Run(() =>
            {
                using var r = new RecordingReader(dir);
                var ts = r.ReadAllTimestamps(p => BeginInvoke(() => _tbAnalysis.Text = $"讀取時間戳中… {p:P0}"), ct);
                return TimingStats.Compute(ts).Report($"{Path.GetFileName(dir)}").Replace("\n", "\r\n");
            }, ct);
            _tbAnalysis.Text = text;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _tbAnalysis.Text = "分析失敗：" + CameraService.Describe(ex); }
    }

    private void ExportCurrent()
    {
        if (_reader == null) return;
        try
        {
            var f = _reader.ReadFrame(_trk.Value);
            var dir = Path.Combine(_reader.Path, "export");
            var path = Path.Combine(dir, $"frame_{_trk.Value:D6}.tif");
            Tiff.WriteMono8(path, f, $"record={_trk.Value} ts_ns={f.TimestampNs}");
            Log("已匯出 " + path);
            MessageBox.Show(this, path, "已匯出");
        }
        catch (Exception ex) { MessageBox.Show(this, CameraService.Describe(ex), "匯出失敗"); }
    }

    private async Task ExportRange()
    {
        if (_reader == null) return;
        long from = (long)_nExpFrom.Value, to = (long)_nExpTo.Value;
        var dir = Path.Combine(_reader.Path, $"export_{from}_{to}");
        if (MessageBox.Show(this, $"匯出 {to - from + 1:N0} 張 TIFF（約 {(to - from + 1) * _reader.Width * (double)_reader.Height / 1e9:0.00} GB）到\n{dir}？",
                "匯出範圍", MessageBoxButtons.OKCancel) != DialogResult.OK) return;
        var src = _reader.Path;
        _bgCts?.Cancel(); _bgCts = new CancellationTokenSource();
        var ct = _bgCts.Token;
        try
        {
            int n = await Task.Run(() =>
            {
                using var r = new RecordingReader(src);
                return r.ExportTiff(dir, from, to, p => BeginInvoke(() => _tbAnalysis.Text = $"匯出中… {p:P0}"), ct);
            }, ct);
            _tbAnalysis.Text = $"已匯出 {n:N0} 張到 {dir}";
            Log(_tbAnalysis.Text);
        }
        catch (Exception ex) { _tbAnalysis.Text = "匯出失敗：" + CameraService.Describe(ex); }
    }

    // ================================================================ 影像顯示

    private void ApplyZoom()
    {
        if (_bmp == null) { _pic.Size = _imgPanel.ClientSize; return; }
        if (_cbZoom.SelectedIndex <= 0)
        {
            _pic.SizeMode = PictureBoxSizeMode.Zoom;
            _pic.Location = Point.Empty;
            _pic.Size = _imgPanel.ClientSize;
        }
        else
        {
            int z = _cbZoom.SelectedIndex; // 1→100%, 2→200%, 3→300%
            _pic.SizeMode = PictureBoxSizeMode.StretchImage;
            _pic.Size = new Size(_bmp.Width * z, _bmp.Height * z);
        }
    }

    private void Render(Frame f, string caption)
    {
        bool sizeChanged = _bmp == null || _bmp.Width != f.Width || _bmp.Height != f.Height;
        var bmp = Imaging.ToBitmap(f, sizeChanged ? null : _bmp);
        if (!ReferenceEquals(bmp, _bmp)) { var old = _bmp; _bmp = bmp; _pic.Image = bmp; old?.Dispose(); ApplyZoom(); }
        else _pic.Invalidate();
        _shownFrame = f;

        double focus = Imaging.FocusScore(f);
        _focusSmooth = _focusSmooth <= 0 ? focus : _focusSmooth * 0.8 + focus * 0.2;
        var (mean, sat) = Imaging.Brightness(f);
        _lblImg.Text = $"{caption}   |   對焦指標 {_focusSmooth:0.0}   平均亮度 {mean:0}   飽和 {sat:0.0}%";
        _lblImg.ForeColor = sat > 1 ? Color.Orange : Color.Gainsboro;
    }

    // ================================================================ 週期更新

    private long _lastStatusMs;
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private void OnUiTick()
    {
        while (_cam.TryDequeueLog(out var line)) AppendLog(line);

        // 畫面：回放分頁以外顯示即時影像
        if (_tabs.SelectedTab != _tpPlayback)
        {
            var f = _cam.LatestFrame;
            if (f != null && !ReferenceEquals(f, _shownFrame))
            {
                var cap = _cam.State == CamState.Recording ? $"錄影中（預覽每 0.1 s 一張） #{f.Index:N0}" : $"即時 #{f.Index:N0}";
                Render(f, cap);
            }
        }

        // 錄影進度
        if (_cam.State == CamState.Recording && _cam.RecordTarget > 0)
        {
            long w = _cam.RecordWritten, t = _cam.RecordTarget;
            _prgRec.Value = (int)Math.Clamp(100.0 * w / t, 0, 100);
            double camSec = t / Math.Max(1, _cam.AppliedFps);
            _lblRecProg.Text = $"已寫入 {w:N0} / {t:N0} 張，經過 {_cam.RecordElapsedSec:0.0} s（相機約 {camSec:0.0} s 拍完，之後等磁碟寫完）";
        }

        long now = _clock.ElapsedMilliseconds;
        if (now - _lastStatusMs < 250) return;
        _lastStatusMs = now;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var st = _cam.State;
        (string text, Color color) = st switch
        {
            CamState.Disconnected => ("● 未連線", Color.DimGray),
            CamState.Idle => ("● 已連線（待機）", Color.SteelBlue),
            CamState.Live => ("● 即時畫面中", Color.ForestGreen),
            CamState.Recording => ("● 錄影中", Color.Crimson),
            _ => (st.ToString(), Color.Black),
        };
        _lblState.Text = text; _lblState.ForeColor = color;

        string disk = "";
        try { var d = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(_tbRecDir.Text.Trim()))!); disk = $"{d.Name} {d.AvailableFreeSpace / 1e9:0} GB"; } catch { }
        var vals = new[]
        {
            st == CamState.Disconnected ? "未連線" : "已連線",
            _cam.CameraName,
            _cam.GrabberName,
            _cam.Width > 0 ? $"{_cam.Width}x{_cam.Height} {_cam.PixelFormat}，每張 {_cam.PayloadSize:N0} bytes" : "",
            st.ToString(),
            _cam.AppliedFps > 0 ? $"{_cam.AppliedFps:0.##} fps / {_cam.AppliedExposureUs:0.#} µs" : "",
            st is CamState.Live or CamState.Recording ? $"{_cam.MeasuredFps:0.0} fps" : "",
            _cam.FrameCount.ToString("N0"),
            _cam.LiveGaps.ToString("N0"),
            _cam.TriggerInfo,
            _cam.BufferCount > 0 ? _cam.BufferCount.ToString("N0") : "",
            _cam.Temperature,
            $"{CameraService.AvailablePhysicalBytes() / 1e9:0.0} GB",
            disk,
            _cam.LastError,
        };
        for (int i = 0; i < vals.Length; i++)
        {
            var c = _gridStatus.Rows[i].Cells[1];
            if (!Equals(c.Value, vals[i])) c.Value = vals[i];
        }

        bool conn = st != CamState.Disconnected;
        _btnConnect.Enabled = !conn && !_busy;
        _btnDisconnect.Enabled = conn && st != CamState.Recording;
        _btnLive.Enabled = conn && st != CamState.Recording && !_busy;
        _btnStop.Enabled = st == CamState.Live;
        _btnSnap.Enabled = conn && st != CamState.Recording && !_busy;
        _btnRec.Enabled = conn && st != CamState.Recording && !_busy;
        _btnRecAbort.Enabled = st == CamState.Recording;
        _btnDetect.Enabled = conn && st != CamState.Recording && !_busy;
        _btnFeatWrite.Enabled = st == CamState.Idle;
        if (st != CamState.Recording && _cam.RecordTarget == 0) _lblRecProg.Text = "";
    }

    private void Log(string s) => AppendLog($"{DateTime.Now:HH:mm:ss.fff}  {s}");

    private void AppendLog(string line)
    {
        _tbLog.AppendText(line + "\r\n");
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, $"camera_{DateTime.Now:yyyyMMdd}.txt"), line + Environment.NewLine);
        }
        catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_cam.State == CamState.Recording &&
            MessageBox.Show(this, "錄影中，確定要中止並關閉？", "關閉", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }
        SaveUiToSettings();
        _uiTimer.Stop(); _playTimer.Stop();
        _bgCts?.Cancel();
        _reader?.Dispose();
        _cam.Dispose();
        base.OnFormClosing(e);
    }
}
