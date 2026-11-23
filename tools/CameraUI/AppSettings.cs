using System.Text.Json;

namespace CameraUI;

/// <summary>相機 FPS 的控制方式（2026-11 時還不確定哪一種對這台 Cheetah+ 有效，所以做成可選 + 自動偵測）。</summary>
public enum FpsMethod
{
    /// <summary>不動觸發設定，只改 AcquisitionFrameTime。</summary>
    FrameTimeOnly,
    /// <summary>TriggerMode = Off（自由連拍），改 AcquisitionFrameTime。</summary>
    TriggerOffFrameTime,
    /// <summary>TriggerMode = On、TriggerSource = Timer，改 TimerFeature 指定的參數（µs）。</summary>
    TimerTrigger,
}

public sealed class AppSettings
{
    public double LiveFps { get; set; } = 120;
    public double LiveExposureUs { get; set; } = 5000;
    public double RecordFps { get; set; } = 1700;
    public double RecordExposureUs { get; set; } = 450;
    public double RecordSeconds { get; set; } = 5;
    public string RecordRoot { get; set; } = @"C:\W\Records";
    public string SnapshotRoot { get; set; } = @"C:\W\Snapshots";
    public FpsMethod Method { get; set; } = FpsMethod.FrameTimeOnly;
    /// <summary>Cheetah+ 的 Timer 參數：TimerLifeTime（週期？）/ TimerUpTime（脈波寬），2026-11-23 連線時讀到。</summary>
    public string TimerFeature { get; set; } = "TimerLifeTime";
    /// <summary>連線時設定的 PixelFormat。相機上電預設 Mono16（每張 655 KB，1700 fps = 1.1 GB/s），錄影用 Mono8。</summary>
    public string PixelFormat { get; set; } = "Mono8";
    /// <summary>即時模式的 buffer 數。</summary>
    public int LiveBuffers { get; set; } = 64;
    /// <summary>錄影 buffer 最多可用的實體記憶體比例（扣掉保留 4 GB 後）。</summary>
    public double RecordRamFraction { get; set; } = 0.7;

    private static string FilePath => Path.Combine(AppContext.BaseDirectory, "camera_settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
        catch { }
    }
}
