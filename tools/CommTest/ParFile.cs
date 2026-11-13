using System.Text.RegularExpressions;

namespace CommTest;

/// <summary>讀取 PCSuite 匯出的 .par 參數檔（每行 Name=Value 或 Name[i]=Value）。</summary>
public sealed class ParFile
{
    private static readonly Regex LineRx =
        new(@"^(?<name>[A-Za-z][A-Za-z0-9_]*)(\[(?<idx>\d+)\])?=(?<val>.*)$", RegexOptions.Compiled);

    /// <summary>完整名稱（含索引，如 ADInMode[7]）→ 值</summary>
    public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>不含索引的參數名稱集合（如 ADInMode）</summary>
    public HashSet<string> BaseNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string SourcePath { get; private set; } = "";

    public static ParFile Load(string path)
    {
        var p = new ParFile { SourcePath = path };
        if (!File.Exists(path)) return p;
        foreach (var raw in File.ReadLines(path))
        {
            var m = LineRx.Match(raw.Trim());
            if (!m.Success) continue;
            var name = m.Groups["name"].Value;
            var full = m.Groups["idx"].Success ? $"{name}[{m.Groups["idx"].Value}]" : name;
            p.Values[full] = m.Groups["val"].Value.Trim();
            p.BaseNames.Add(name);
        }
        return p;
    }
}
