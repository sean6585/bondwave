using System.Text;

namespace CameraUI;

/// <summary>
/// 由每張影像的時間戳（ns）計算 FPS、間隔分布與缺張。
/// 演算法與 tools/RecorderCheck/Check-Recording.ps1 相同：
/// 標準週期 = 預期 FPS（若有）或間隔中位數；偏離超過 50% 視為異常，缺張數 = round(間隔/週期) - 1。
/// </summary>
public sealed class TimingStats
{
    public int Count;
    public double SpanSec;
    public double AvgFps;
    public double PeriodUs;
    public string PeriodSource = "";
    public double MinUs, MaxUs;
    public List<(double Us, int Count)> Histogram = new();
    public List<(long After, double Us, long Missing)> Gaps = new();
    public long MissingTotal;

    public bool Ok => Gaps.Count == 0 && Count >= 2;

    public static TimingStats Compute(IReadOnlyList<ulong> tsNs, double expectedFps = 0, double tolerancePct = 50)
    {
        var s = new TimingStats { Count = tsNs.Count };
        if (tsNs.Count < 2) return s;

        var d = new long[tsNs.Count - 1];
        for (int i = 1; i < tsNs.Count; i++) d[i - 1] = (long)(tsNs[i] - tsNs[i - 1]);

        long period;
        if (expectedFps > 0) { period = (long)(1e9 / expectedFps); s.PeriodSource = $"預期 {expectedFps:0.##} fps"; }
        else
        {
            var sorted = (long[])d.Clone(); Array.Sort(sorted);
            period = sorted[sorted.Length / 2];
            s.PeriodSource = "間隔中位數";
        }
        if (period <= 0) period = 1;
        s.PeriodUs = period / 1000.0;
        s.SpanSec = (tsNs[^1] - tsNs[0]) / 1e9;
        s.AvgFps = s.SpanSec > 0 ? (tsNs.Count - 1) / s.SpanSec : 0;

        double tol = period * tolerancePct / 100.0;
        var hist = new Dictionary<long, int>();
        long min = long.MaxValue, max = long.MinValue;
        for (int i = 0; i < d.Length; i++)
        {
            min = Math.Min(min, d[i]); max = Math.Max(max, d[i]);
            long us = (long)Math.Round(d[i] / 1000.0);
            hist[us] = hist.TryGetValue(us, out var c) ? c + 1 : 1;
            if (Math.Abs(d[i] - period) > tol)
            {
                long miss = Math.Max(0, (long)Math.Round(d[i] / (double)period) - 1);
                s.MissingTotal += miss;
                if (s.Gaps.Count < 1000) s.Gaps.Add((i + 1, d[i] / 1000.0, miss));
            }
        }
        s.MinUs = min / 1000.0; s.MaxUs = max / 1000.0;
        s.Histogram = hist.OrderByDescending(kv => kv.Value).Take(10).Select(kv => ((double)kv.Key, kv.Value)).ToList();
        return s;
    }

    public string Report(string title)
    {
        var sb = new StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine($"  張數        : {Count:N0}");
        if (Count < 2) { sb.AppendLine("  少於 2 張，無法計算"); return sb.ToString(); }
        sb.AppendLine($"  第一→最後   : {SpanSec:0.000000} s");
        sb.AppendLine($"  平均 FPS    : {AvgFps:0.00}");
        sb.AppendLine($"  標準週期    : {PeriodUs:0.0} µs（{PeriodSource}）");
        sb.AppendLine($"  間隔範圍    : {MinUs:0.0} ~ {MaxUs:0.0} µs");
        sb.AppendLine("  間隔分布（µs : 次數）");
        foreach (var (us, c) in Histogram) sb.AppendLine($"    {us,8:0} : {c:N0}");
        if (Ok) sb.AppendLine("  結果：沒有缺張 ✔");
        else
        {
            sb.AppendLine($"  結果：{Gaps.Count} 個異常間隔，推估缺 {MissingTotal:N0} 張 ✘");
            foreach (var g in Gaps.Take(20)) sb.AppendLine($"    第 {g.After} 張之後：{g.Us:0.0} µs（缺 {g.Missing}）");
            if (Gaps.Count > 20) sb.AppendLine("    ...");
        }
        return sb.ToString();
    }
}
