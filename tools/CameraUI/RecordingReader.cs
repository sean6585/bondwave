using Euresys.EGrabberRecorder;

namespace CameraUI;

/// <summary>
/// 讀 eGrabber Recorder container（本程式或 eGrabber Studio 錄的都可以）：
/// 逐張讀影像、讀全部時間戳做 FPS 分析、匯出 TIFF。
/// 只用 READ 模式開檔，不會修改錄影檔。不是 thread-safe，請在同一條執行緒使用。
/// </summary>
public sealed class RecordingReader : IDisposable
{
    private readonly RecorderLibrary _lib;
    private readonly Recorder _rec;

    public string Path { get; }
    public long Count { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string ChapterName { get; private set; } = "";
    public string ChapterInfo { get; private set; } = "";

    public RecordingReader(string containerDir)
    {
        Path = containerDir;
        _lib = new RecorderLibrary();
        try
        {
            _rec = _lib.OpenRecorder(containerDir, RECORDER_OPEN_MODE.READ, RECORDER_CLOSE_MODE.KEEP);
            Count = _rec.Get<long>(RECORDER_PARAMETER.RECORD_COUNT);
            if (Count > 0)
            {
                var info = ReadInfo(0);
                Width = (int)info.width; Height = (int)info.height;
            }
            try
            {
                if (_rec.Get<long>(RECORDER_PARAMETER.CHAPTER_COUNT) > 0)
                {
                    _rec.Set(RECORDER_PARAMETER.CHAPTER_INDEX, 0L);
                    ChapterName = _rec.Get<string>(RECORDER_PARAMETER.CHAPTER_NAME);
                    ChapterInfo = _rec.Get<string>(RECORDER_PARAMETER.CHAPTER_USER_INFO);
                }
            }
            catch { /* chapter 資訊不是必要的 */ }
        }
        catch
        {
            _lib.Dispose();
            throw;
        }
    }

    public RecorderBufferInfo ReadInfo(long index)
    {
        _rec.Set(RECORDER_PARAMETER.RECORD_INDEX, index);
        return _rec.ReadInfo();
    }

    public Frame ReadFrame(long index)
    {
        _rec.Set(RECORDER_PARAMETER.RECORD_INDEX, index);
        var data = _rec.Read(out RecorderBufferInfo info);
        int w = (int)info.width, h = (int)info.height, pitch = info.pitch > 0 ? (int)info.pitch : w;
        var f = new Frame { Width = w, Height = h, Pixels = new byte[w * h], TimestampNs = info.timestamp, Index = index };
        if (pitch == w) Array.Copy(data, f.Pixels, Math.Min(data.Length, w * h));
        else for (int y = 0; y < h; y++) Array.Copy(data, y * pitch, f.Pixels, y * w, w);
        return f;
    }

    /// <summary>讀全部時間戳（ns）。progress 回報 0..1。</summary>
    public ulong[] ReadAllTimestamps(Action<double> progress = null, CancellationToken ct = default)
    {
        var ts = new ulong[Count];
        for (long i = 0; i < Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            ts[i] = ReadInfo(i).timestamp;
            if (progress != null && (i & 1023) == 0) progress((double)i / Count);
        }
        progress?.Invoke(1);
        return ts;
    }

    /// <summary>把 [from, to] 範圍匯出成 TIFF 序列（自己寫檔，保證是原始像素）。</summary>
    public int ExportTiff(string dir, long from, long to, Action<double> progress = null, CancellationToken ct = default)
    {
        Directory.CreateDirectory(dir);
        from = Math.Clamp(from, 0, Count - 1); to = Math.Clamp(to, from, Count - 1);
        ulong t0 = ReadInfo(0).timestamp;
        int n = 0;
        for (long i = from; i <= to; i++)
        {
            ct.ThrowIfCancellationRequested();
            var f = ReadFrame(i);
            var relUs = (f.TimestampNs - t0) / 1000.0;
            Tiff.WriteMono8(System.IO.Path.Combine(dir, $"frame_{i:D6}.tif"), f, $"record={i} t_rel_us={relUs:0.0} ts_ns={f.TimestampNs}");
            n++;
            if (progress != null && (n & 63) == 0) progress((double)(i - from) / Math.Max(1, to - from));
        }
        progress?.Invoke(1);
        return n;
    }

    public void Dispose()
    {
        try { _rec?.Close(RECORDER_CLOSE_MODE.KEEP); } catch { }
        try { _rec?.Dispose(); } catch { }
        try { _lib?.Dispose(); } catch { }
    }
}
