namespace CommTest;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--selftest")) return SelfTest();
        // 唯讀探測：連線 → 讀識別資訊 / 參數 / 一筆狀態 → 斷線，結果寫到 probe.txt（不需點畫面）
        if (args.Contains("--probe"))
        {
            var ip = args.SkipWhile(a => a != "--probe").Skip(1).FirstOrDefault(a => !a.StartsWith("--")) ?? "172.1.1.101";
            return Probe(ip);
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    /// <summary>
    /// 唯讀探測。只用 SafeController（沒有任何寫入 / 動作 API），和 GUI 讀的是同一組參數。
    /// 用法：BondwaveCommTest.exe --probe [IP]   → probe.txt；回傳 0 = 連線成功
    /// </summary>
    private static int Probe(string ip)
    {
        var par = ParFile.Load(Path.Combine(AppContext.BaseDirectory, "reference.par"));
        var lines = new List<string>();
        int rc = 0;
        using (var c = new SafeController(par))
        {
            void Drain() { while (c.TryDequeueLog(out var l)) lines.Add(l); }
            try
            {
                if (!c.ConnectAsync(SafeController.Kind.AGC301, ip).Wait(TimeSpan.FromSeconds(30)) || !c.Connected)
                {
                    Drain(); lines.Add("RESULT 連線失敗"); rc = 1;
                }
                else
                {
                    foreach (var (n, v) in c.ReadIdentityAsync().Result) lines.Add($"IDENT {n,-22} = {v}");

                    var names = new List<string>();
                    foreach (var ax in new[] { "A", "B" })
                        foreach (var p in new[] { "MotorType", "EncType", "EncRes", "ContCL", "PeakCL", "CurrLimFwd", "CurrLimRev",
                                                  "FwdPLim", "RevPLim", "BrakeUsed", "BrakeMode", "BrakeRelTime", "BrakeLockTime",
                                                  "EmrgDec", "OperationMode", "MaxPosErr", "StallCfg", "StallThPcnt", "ProtectMask",
                                                  "MaxForceErr", "MaxForceErrOL", "CurrPosTh", "ForcePosErrTh", "ForceAInTh" })
                            names.Add(ax + p);
                    names.Add("ADInLog");
                    for (int i = 1; i <= 32; i++) if (par.Values.ContainsKey($"ADInMode[{i}]")) names.Add($"ADInMode[{i}]");
                    for (int i = 1; i <= 4; i++)
                        foreach (var p in new[] { "AAInMode", "AAInGain", "AAInOffset" })
                            if (par.Values.ContainsKey($"{p}[{i}]")) names.Add($"{p}[{i}]");

                    foreach (var (n, now) in c.ReadParamsAsync(names).Result)
                    {
                        par.Values.TryGetValue(n, out var old);
                        bool diff = old != null && !now.StartsWith("[") && !string.Equals(now.Trim().TrimEnd(';'), old.Trim(), StringComparison.OrdinalIgnoreCase);
                        lines.Add($"PARAM {n,-18} 現值={now,-14} .par={old ?? "(無)",-14} {(diff ? "≠ 不同" : "")}");
                    }

                    // 等一筆輪詢快照
                    for (int i = 0; i < 30 && c.Latest == null; i++) Thread.Sleep(100);
                    Thread.Sleep(300);
                    var s = c.Latest;
                    if (s != null)
                    {
                        static string Ax(AxisSnap a) => $"Pos={a.Pos} PosErr={a.PosErr} MotorOn={a.MotorOnStatus} MotionStat={a.MotionStat} OpMode={a.OperationMode} " +
                                                        $"LimitStat={(a.LimitStat is int l ? $"0x{l:X}" : "?")} ConFlt={a.ConFlt} ConFaultMsg=\"{a.ConFaultMsg}\" Force={a.Force} Iq={a.Iq}";
                        lines.Add($"SNAP 心跳={(s.HeartbeatOk ? "OK" : "FAIL")} {s.HeartbeatMs:0.0}ms VBus={s.VBus} VLogic={s.VLogic} PwrTemp={s.PwrTemp}");
                        lines.Add($"SNAP A(Z1) {Ax(s.A)}");
                        lines.Add($"SNAP B(Z2) {Ax(s.B)}");
                        lines.Add($"SNAP DInPort={(s.DInPort is int d ? $"0x{d:X8}" : "?")} AInPort=[{(s.AInPort == null ? "?" : string.Join(", ", s.AInPort))}]");
                    }
                    lines.Add("RESULT 連線成功");
                }
            }
            catch (Exception ex) { lines.Add("RESULT 例外：" + (ex.InnerException ?? ex).Message); rc = 1; }
            Drain();
        }
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "probe.txt"), lines);
        return rc;
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
