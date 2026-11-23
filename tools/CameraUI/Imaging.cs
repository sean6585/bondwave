using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CameraUI;

/// <summary>一張 Mono8 影像（緊密排列，pitch = width）。</summary>
public sealed class Frame
{
    public int Width, Height;
    public byte[] Pixels;          // Width * Height
    public ulong TimestampNs;      // 開機後 ns（與 eGrabber / 錄影檔同一個時鐘）
    public long Index;             // 序號（即時：累計張數；回放：record index）

    public static Frame FromPointer(IntPtr src, int width, int height, int pitch, ulong tsNs, long index)
    {
        var f = new Frame { Width = width, Height = height, Pixels = new byte[width * height], TimestampNs = tsNs, Index = index };
        if (pitch == width) Marshal.Copy(src, f.Pixels, 0, width * height);
        else for (int y = 0; y < height; y++) Marshal.Copy(src + y * pitch, f.Pixels, y * width, width);
        return f;
    }
}

public static class Imaging
{
    private static ColorPalette _gray;

    /// <summary>Mono8 → 8bpp 灰階 Bitmap（給 PictureBox 顯示）。</summary>
    public static Bitmap ToBitmap(Frame f, Bitmap reuse = null)
    {
        var bmp = reuse != null && reuse.Width == f.Width && reuse.Height == f.Height && reuse.PixelFormat == PixelFormat.Format8bppIndexed
            ? reuse
            : new Bitmap(f.Width, f.Height, PixelFormat.Format8bppIndexed);
        if (!ReferenceEquals(bmp, reuse))
        {
            if (_gray == null)
            {
                _gray = bmp.Palette;
                for (int i = 0; i < 256; i++) _gray.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = _gray;
        }
        var data = bmp.LockBits(new Rectangle(0, 0, f.Width, f.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        try
        {
            for (int y = 0; y < f.Height; y++)
                Marshal.Copy(f.Pixels, y * f.Width, data.Scan0 + y * data.Stride, f.Width);
        }
        finally { bmp.UnlockBits(data); }
        return bmp;
    }

    /// <summary>
    /// 對焦指標：Tenengrad 簡化版（水平+垂直梯度平方的平均），越大越清楚。
    /// 只取中央 50% 區域、隔點取樣，計算量很小。數值只能拿來「相對比較」。
    /// </summary>
    public static double FocusScore(Frame f)
    {
        int x0 = f.Width / 4, x1 = f.Width * 3 / 4, y0 = f.Height / 4, y1 = f.Height * 3 / 4;
        double sum = 0; long n = 0;
        var p = f.Pixels; int w = f.Width;
        for (int y = y0; y < y1 - 1; y += 2)
        {
            int row = y * w;
            for (int x = x0; x < x1 - 1; x += 2)
            {
                int c = p[row + x];
                int gx = p[row + x + 1] - c;
                int gy = p[row + w + x] - c;
                sum += gx * gx + gy * gy;
                n++;
            }
        }
        return n == 0 ? 0 : sum / n;
    }

    /// <summary>平均亮度與飽和比例（> 250 的像素比例），用來判斷曝光。</summary>
    public static (double Mean, double SaturatedPct) Brightness(Frame f)
    {
        long sum = 0, sat = 0;
        var p = f.Pixels;
        for (int i = 0; i < p.Length; i += 4) { sum += p[i]; if (p[i] > 250) sat++; }
        long n = (p.Length + 3) / 4;
        return (n == 0 ? 0 : (double)sum / n, n == 0 ? 0 : 100.0 * sat / n);
    }
}

/// <summary>
/// 最小 TIFF 寫入器：8-bit 灰階、未壓縮、單頁。
/// 不依賴 GDI+ 編碼器，確保存下來的就是相機原始像素值（不做任何轉換）。
/// </summary>
public static class Tiff
{
    public static void WriteMono8(string path, Frame f, string description = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var desc = System.Text.Encoding.ASCII.GetBytes((description ?? "") + "\0");
        const int entryCount = 11;
        int ifdOffset = 8;
        int ifdSize = 2 + entryCount * 12 + 4;
        int descOffset = ifdOffset + ifdSize;
        int dataOffset = descOffset + desc.Length;
        dataOffset = (dataOffset + 15) & ~15;
        int dataLen = f.Width * f.Height;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20);
        using var w = new BinaryWriter(fs);
        w.Write((byte)'I'); w.Write((byte)'I'); w.Write((ushort)42); w.Write((uint)ifdOffset);

        w.Write((ushort)entryCount);
        void Entry(ushort tag, ushort type, uint count, uint value) { w.Write(tag); w.Write(type); w.Write(count); w.Write(value); }
        // 型別：3 = SHORT, 4 = LONG, 2 = ASCII（tag 必須遞增）
        Entry(256, 4, 1, (uint)f.Width);            // ImageWidth
        Entry(257, 4, 1, (uint)f.Height);           // ImageLength
        Entry(258, 3, 1, 8);                        // BitsPerSample
        Entry(259, 3, 1, 1);                        // Compression = none
        Entry(262, 3, 1, 1);                        // Photometric = BlackIsZero
        Entry(270, 2, (uint)desc.Length, (uint)descOffset); // ImageDescription
        Entry(273, 4, 1, (uint)dataOffset);         // StripOffsets
        Entry(277, 3, 1, 1);                        // SamplesPerPixel
        Entry(278, 4, 1, (uint)f.Height);           // RowsPerStrip
        Entry(279, 4, 1, (uint)dataLen);            // StripByteCounts
        Entry(284, 3, 1, 1);                        // PlanarConfig = contiguous
        w.Write((uint)0);                           // 沒有下一個 IFD

        w.Write(desc);
        while (fs.Position < dataOffset) w.Write((byte)0);
        w.Write(f.Pixels, 0, dataLen);
    }

    /// <summary>讀回自己寫的 TIFF（自我檢查用）。</summary>
    public static Frame ReadMono8(string path)
    {
        var b = File.ReadAllBytes(path);
        int ifd = BitConverter.ToInt32(b, 4);
        int n = BitConverter.ToUInt16(b, ifd);
        int w = 0, h = 0, off = 0;
        for (int i = 0; i < n; i++)
        {
            int e = ifd + 2 + i * 12;
            int tag = BitConverter.ToUInt16(b, e);
            int val = BitConverter.ToInt32(b, e + 8);
            if (tag == 256) w = val; else if (tag == 257) h = val; else if (tag == 273) off = val;
        }
        var f = new Frame { Width = w, Height = h, Pixels = new byte[w * h] };
        Array.Copy(b, off, f.Pixels, 0, w * h);
        return f;
    }
}
