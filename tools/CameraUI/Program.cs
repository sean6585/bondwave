namespace CameraUI;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // 不需要相機的自我檢查：TIFF 寫入 / 統計計算
        if (args.Contains("--selftest")) return SelfTest.Run();
        // 只連線 + 列出參數再斷線（不取像、不改任何設定），結果寫到 probe.txt
        if (args.Contains("--probe")) return Probe();
        // 開發用：把每個分頁畫成 PNG（不連相機），檢查版面
        if (args.Contains("--shots")) return Shots();
        // 實機測試：連線(Mono8) → FPS 控制方式偵測 → 即時 120 fps → 單張 → 錄影 2 s @1700 → 分析，結果寫 hwtest.txt
        if (args.Contains("--hwtest")) return HwTest();
        // 檢查錄影檔 / TIFF 的像素統計：--inspect <錄影資料夾或 .tif>
        if (args.Length >= 2 && args[0] == "--inspect") return Inspect(args[1]);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    private static int Shots()
    {
        ApplicationConfiguration.Initialize();
        var dir = Path.Combine(AppContext.BaseDirectory, "shots");
        Directory.CreateDirectory(dir);
        using var f = new MainForm { StartPosition = FormStartPosition.Manual, Location = new Point(-3000, 0) };
        f.Show();
        Application.DoEvents();
        var tabs = f.Controls.OfType<SplitContainer>().First().Panel2.Controls.OfType<TabControl>().First();
        for (int i = 0; i < tabs.TabCount; i++)
        {
            tabs.SelectedIndex = i;
            Application.DoEvents();
            using var bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height));
            bmp.Save(Path.Combine(dir, $"tab{i}.png"));
        }
        f.Close();
        return 0;
    }

    private static int HwTest()
    {
        var cfg = AppSettings.Load();
        var lines = new List<string>();
        void Out(string s) { lines.Add(s); Console.WriteLine(s); }
        int rc = 0;
        using (var cam = new CameraService())
        {
            void Drain() { while (cam.TryDequeueLog(out var l)) Out("  | " + l); }
            try
            {
                cam.ConnectAsync(cfg.PixelFormat).Wait(30000); Drain();
                Out($"[1] 連線 OK：{cam.CameraName}  {cam.Width}x{cam.Height} {cam.PixelFormat}");

                var det = cam.DetectFpsMethodAsync(cfg.RecordFps, cfg.RecordExposureUs, cfg.TimerFeature).Result; Drain();
                foreach (var d in det) Out($"[2] 偵測 {d.Method}: {(d.Success ? "OK" : "NG")} {d.Detail}");
                var ok = det.FirstOrDefault(d => d.Success);
                if (ok == null) { Out("[2] 沒有任何方式達到 1700 fps，停止測試"); return 2; }
                cfg.Method = ok.Method; cfg.Save();
                Out($"[2] 採用 {ok.Method}（已存到 camera_settings.json）");

                cam.StartLiveAsync(cfg.LiveFps, cfg.LiveExposureUs, cfg.Method, cfg.TimerFeature, cfg.LiveBuffers).Wait(10000);
                Thread.Sleep(2500); Drain();
                Out($"[3] 即時 {cfg.LiveFps} fps：實測 {cam.MeasuredFps:0.0} fps，{cam.FrameCount} 張，缺張 {cam.LiveGaps}");
                var f = cam.LatestFrame;
                if (f != null)
                {
                    var (mean, sat) = Imaging.Brightness(f);
                    Out($"[3] 畫面 {f.Width}x{f.Height} 平均亮度 {mean:0.0} 飽和 {sat:0.00}% 對焦指標 {Imaging.FocusScore(f):0.0}");
                }
                var snap = cam.SnapshotAsync(Path.Combine(cfg.SnapshotRoot, $"hwtest_{DateTime.Now:yyyyMMdd_HHmmss}.tif")).Result; Drain();
                Out($"[4] 單張：{snap}（{new FileInfo(snap).Length:N0} bytes）");
                cam.StopAsync().Wait(5000);

                var r = cam.RecordAsync(cfg.RecordFps, cfg.RecordExposureUs, 2, cfg.Method, cfg.TimerFeature,
                                        cfg.RecordRoot, "hwtest", cfg.RecordRamFraction).Result; Drain();
                Out($"[5] 錄影：{(r.Error ?? (r.Aborted ? "中止" : "完成"))}，{r.Written:N0}/{r.Target:N0} 張，耗時 {r.WallSec:0.0} s → {r.Path}");
                if (r.Stats != null) Out(r.Stats.Report("[5] 錄影中的時間戳"));

                if (r.Written > 0)
                {
                    using var rd = new RecordingReader(r.Path);
                    var st = TimingStats.Compute(rd.ReadAllTimestamps(), cfg.RecordFps);
                    var mid = rd.ReadFrame(rd.Count / 2);
                    Out($"[6] 讀回錄影檔：{rd.Count:N0} 張 {rd.Width}x{rd.Height}，{st.AvgFps:0.00} fps，缺 {st.MissingTotal} 張，中間一張平均亮度 {Imaging.Brightness(mid).Mean:0.0}");
                    if (!st.Ok || rd.Count != r.Target) rc = 3;
                }
                else rc = 3;
            }
            catch (Exception ex) { Drain(); Out("測試失敗：" + CameraService.Describe(ex.InnerException ?? ex)); rc = 1; }
            finally { try { cam.DisconnectAsync().Wait(10000); } catch { } Drain(); }
        }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "hwtest.txt"), lines);
        return rc;
    }

    private static int Inspect(string path)
    {
        static string S(Frame f)
        {
            long sum = 0; int max = 0, nz = 0;
            foreach (var p in f.Pixels) { sum += p; if (p > max) max = p; if (p > 0) nz++; }
            return $"mean={(double)sum / f.Pixels.Length:0.00} max={max} nonzero={100.0 * nz / f.Pixels.Length:0.0}%";
        }
        var lines = new List<string>();
        if (path.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)) lines.Add($"{path}: {S(Tiff.ReadMono8(path))}");
        else
        {
            using var r = new RecordingReader(path);
            foreach (var i in new long[] { 0, 1, r.Count / 4, r.Count / 2, r.Count - 1 })
                lines.Add($"record {i}: {S(r.ReadFrame(i))}");
        }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "inspect.txt"), lines);
        return 0;
    }

    private static int Probe()
    {
        var lines = new List<string>();
        int rc = 0;
        using (var cam = new CameraService())
        {
            try { cam.ConnectAsync(null).Wait(30000); } // null = 不改 PixelFormat（純讀取）
            catch (Exception ex) { lines.Add("連線失敗：" + CameraService.Describe(ex.InnerException ?? ex)); rc = 1; }
            Thread.Sleep(300);
            try { cam.DisconnectAsync().Wait(10000); } catch { }
            while (cam.TryDequeueLog(out var l)) lines.Add(l);
        }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "probe.txt"), lines);
        return rc;
    }
}
