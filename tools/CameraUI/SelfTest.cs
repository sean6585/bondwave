namespace CameraUI;

/// <summary>
/// 不需要相機的自我檢查：BondwaveCamera.exe --selftest（結果寫到 selftest.txt，回傳 0 = 全部通過）。
/// 若有 C:\W 的錄影檔，也會用 eGrabber Recorder API 讀它並與已知結果比對。
/// </summary>
internal static class SelfTest
{
    public static int Run()
    {
        var lines = new List<string>();
        int fail = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (!ok) fail++;
            lines.Add($"{(ok ? "PASS" : "FAIL")}  {name}  {detail}");
        }

        // 1. TIFF 寫入 / 讀回
        try
        {
            var f = new Frame { Width = 640, Height = 512, Pixels = new byte[640 * 512] };
            for (int i = 0; i < f.Pixels.Length; i++) f.Pixels[i] = (byte)(i * 7 % 251);
            var path = Path.Combine(Path.GetTempPath(), $"bw_selftest_{Environment.ProcessId}.tif");
            Tiff.WriteMono8(path, f, "selftest");
            var back = Tiff.ReadMono8(path);
            File.Delete(path);
            Check("TIFF 寫入讀回", back.Width == 640 && back.Height == 512 && back.Pixels.SequenceEqual(f.Pixels));
        }
        catch (Exception ex) { Check("TIFF 寫入讀回", false, ex.Message); }

        // 2. 統計：連續 1700 fps、無缺張
        var ts = new List<ulong>();
        for (int i = 0; i < 16000; i++) ts.Add(1_000_000_000UL + (ulong)i * 588_000UL);
        var s = TimingStats.Compute(ts, 1700);
        Check("統計：完整序列無缺張", s.Ok && s.MissingTotal == 0, $"fps={s.AvgFps:0.00}");

        // 3. 統計：刪掉一張要抓到
        ts.RemoveAt(5000);
        s = TimingStats.Compute(ts, 1700);
        Check("統計：缺 1 張被偵測", !s.Ok && s.MissingTotal == 1 && s.Gaps[0].After == 5000, $"gaps={s.Gaps.Count}");

        // 4. 對焦指標：清楚 > 模糊
        var sharp = new Frame { Width = 64, Height = 64, Pixels = new byte[64 * 64] };
        var blur = new Frame { Width = 64, Height = 64, Pixels = new byte[64 * 64] };
        for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
        {
            sharp.Pixels[y * 64 + x] = (byte)(((x + y) & 1) * 255);
            blur.Pixels[y * 64 + x] = 128;
        }
        Check("對焦指標", Imaging.FocusScore(sharp) > Imaging.FocusScore(blur));

        // 5. 讀實際錄影檔（有的話）：用 eGrabber Recorder API，結果應與 Check-Recording.ps1 一致
        const string known = @"C:\W";
        if (File.Exists(Path.Combine(known, "index")))
        {
            try
            {
                using var r = new RecordingReader(known);
                var all = r.ReadAllTimestamps();
                var st = TimingStats.Compute(all);
                var f0 = r.ReadFrame(0);
                Check("Recorder API 讀 C:\\W", r.Count > 0 && f0.Pixels.Length == r.Width * r.Height,
                      $"count={r.Count} {r.Width}x{r.Height} span={st.SpanSec:0.000000}s fps={st.AvgFps:0.00} missing={st.MissingTotal}");
            }
            catch (Exception ex) { Check("Recorder API 讀 C:\\W", false, CameraService.Describe(ex)); }
        }
        else lines.Add("SKIP  C:\\W 沒有錄影檔");

        // 6. Recorder API 寫入→讀回（合成 200 張、588 µs 間隔），確認 native 函式庫與授權可用
        var tmp = Path.Combine(Path.GetTempPath(), $"bw_rec_selftest_{Environment.ProcessId}");
        try
        {
            Directory.CreateDirectory(tmp);
            using (var lib = new Euresys.EGrabberRecorder.RecorderLibrary())
            using (var rec = lib.OpenRecorder(tmp, Euresys.EGrabberRecorder.RECORDER_OPEN_MODE.WRITE, Euresys.EGrabberRecorder.RECORDER_CLOSE_MODE.TRIM))
            {
                rec.Set(Euresys.EGrabberRecorder.RECORDER_PARAMETER.CONTAINER_SIZE, 200L * (640 * 512 + 4096) + (16L << 20));
                rec.StartChapter("selftest", "synthetic");
                var px = new byte[640 * 512];
                for (int i = 0; i < 200; i++)
                {
                    px[0] = (byte)i;
                    var info = new Euresys.EGrabberRecorder.RecorderBufferInfo
                    {
                        size = (ulong)px.Length, pitch = 640, width = 640, height = 512,
                        pixelFormat = 0x01080001, // PFNC Mono8
                        partCount = 1, partSize = (ulong)px.Length,
                        timestamp = 5_000_000_000UL + (ulong)i * 588_000UL,
                    };
                    rec.Write(info, px);
                }
                rec.Close(Euresys.EGrabberRecorder.RECORDER_CLOSE_MODE.TRIM);
            }
            using var rr = new RecordingReader(tmp);
            var st = TimingStats.Compute(rr.ReadAllTimestamps());
            var f150 = rr.ReadFrame(150);
            Check("Recorder API 寫入→讀回", rr.Count == 200 && st.Ok && Math.Abs(st.AvgFps - 1700.68) < 0.1 && f150.Pixels[0] == 150,
                  $"count={rr.Count} fps={st.AvgFps:0.00} chapter={rr.ChapterName} pixel0={f150.Pixels[0]}");
        }
        catch (Exception ex) { Check("Recorder API 寫入→讀回", false, CameraService.Describe(ex)); }
        finally { try { Directory.Delete(tmp, true); } catch { } }

        lines.Add(fail == 0 ? "ALL PASS" : $"{fail} FAIL");
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "selftest.txt"), lines);
        return fail == 0 ? 0 : 1;
    }
}
