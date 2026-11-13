namespace CommTest;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--selftest")) return SelfTest();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    /// <summary>
    /// 不需要控制器的自我檢查：確認唯讀白名單會擋下寫入 / 動作指令。
    /// 用法：BondwaveCommTest.exe --selftest   （結果寫到 selftest.txt，回傳 0 = 全部通過）
    /// </summary>
    private static int SelfTest()
    {
        var par = ParFile.Load(Path.Combine(AppContext.BaseDirectory, "reference.par"));
        using var c = new SafeController(par);
        var lines = new List<string> { $"reference.par 參數 {par.Values.Count} 筆" };
        int fail = 0;

        void Expect(string q, bool allowed)
        {
            bool ok = c.IsAllowedQuery(q, out var why);
            bool pass = ok == allowed;
            if (!pass) fail++;
            lines.Add($"{(pass ? "PASS" : "FAIL")}  {(allowed ? "允許" : "拒絕")}  {q,-28} {why}");
        }

        // 應允許：讀取
        Expect("APos", true);
        Expect("BForce", true);
        Expect("AEncType", true);
        Expect("BMotorType", true);
        Expect("ADInMode[7]", true);
        Expect("AAInMode[2]", true);
        Expect("AFwdPLim", true);

        // 應拒絕：寫入 / 動作 / 多指令 / 不明名稱
        Expect("AMotorOn=1", false);
        Expect("AMotorOn", false);
        Expect("BMotorOff", false);
        Expect("ABegin", false);
        Expect("AStop", false);
        Expect("AJog=1000", false);
        Expect("AFwdPLim=100", false);
        Expect("APos;AMotorOn=1", false);
        Expect("APos AMotorOn", false);
        Expect("AGendata[10]=20303", false);
        Expect("AHomeStart", false);
        Expect("ASomethingUnknown", false);
        Expect("", false);

        lines.Add(fail == 0 ? "ALL PASS" : $"{fail} FAIL");
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "selftest.txt"), lines);
        return fail == 0 ? 0 : 1;
    }
}
