using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AML
{
    public enum Vaccumm { Unknown, On, Off }
    public enum WaferPresence { Unknown, Present, Absent }
    public partial class Aligner : Form
    {

        public enum Status { Normal, Error }
        public enum Mode { Online, Maintenance }
        public enum DeviceStatus { Unknown, SupplyOk, Extraction, LOADReady }
        public enum OperatingState { Stopping, Operating }
        public enum SetAngleDone { Notyet, Done }
        string _errorCode = "00";
        Dictionary<string, string> Aligner_ErrorCode = new Dictionary<string, string>();

        private int DeviceIndex = 0;
        string _DeviceName = "";
        public string DeviceName
        {
            get
            {
                return _DeviceName;
            }
        }

        bool Home = false;

        public bool IsHome
        {
            get
            {
                return Home;
            }
        }

        public WaferPresence IsWaferPresence
        {
            get
            {
                return _WaferPresence;
            }
        }

        public Vaccumm IsVaccumm
        {
            get
            {
                return _Vaccumm;
            }
        }

        public enum NotchDetectionStatus { Detected, NotDetected, PositioningCompleted }

        Vaccumm _Vaccumm = Vaccumm.Off;
        Status _Status = Status.Error;
        Mode _Mode = Mode.Maintenance;
        DeviceStatus _DeviceStatus = DeviceStatus.Unknown;
        OperatingState _OperatingState = OperatingState.Stopping;
        NotchDetectionStatus _NotchDetectionStatus = NotchDetectionStatus.NotDetected;
        WaferPresence _WaferPresence = WaferPresence.Unknown;
        SetAngleDone _SetAngleDone = SetAngleDone.Notyet;

        public bool Enable_Alarm = false;
        public int AlignerAlarmOffset = 30000000;
        private void InitAlignerErrorCode()
        {
            Aligner_ErrorCode.Add("1", "Serial communication");
            Aligner_ErrorCode.Add("2", "Serial communication");
            Aligner_ErrorCode.Add("3", "Serial communication");
            Aligner_ErrorCode.Add("21", "Power element error");
            Aligner_ErrorCode.Add("22", "Current detection error ");
            Aligner_ErrorCode.Add("23", "Current detection error ");
            Aligner_ErrorCode.Add("24", "Current detection error ");
            Aligner_ErrorCode.Add("28", "Servo ON error");
            Aligner_ErrorCode.Add("41", "Overload 1");
            Aligner_ErrorCode.Add("51", "Amplifier overheat");
            Aligner_ErrorCode.Add("61", "Over voltage");
            Aligner_ErrorCode.Add("62", "Main circuit voltage low");
            Aligner_ErrorCode.Add("63", "Main power open-phase");
            Aligner_ErrorCode.Add("71", "Control power voltage ");
            Aligner_ErrorCode.Add("81", "A-/B-phase pulse signal error");
            Aligner_ErrorCode.Add("82", "Absolute signal disconnection");
            Aligner_ErrorCode.Add("84", "Communication error between ncoder and amplifier");
            Aligner_ErrorCode.Add("87", "CS disconnection");
            Aligner_ErrorCode.Add("85", "Encoder initial treatment error");
            Aligner_ErrorCode.Add("91", "Encoder command error  Wrong operation due to ");
            Aligner_ErrorCode.Add("92", "Encoder FORM error");
            Aligner_ErrorCode.Add("93", "Encoder SYNC error");
            Aligner_ErrorCode.Add("94", "Encoder CRC error");
            Aligner_ErrorCode.Add("A1", "Encoder error 1");
            Aligner_ErrorCode.Add("A2", "Absolute encoder battery error");
            Aligner_ErrorCode.Add("A3", "Encoder overheat");
            Aligner_ErrorCode.Add("A5", "Encoder error 3");
            Aligner_ErrorCode.Add("A6", "Encoder error 4");
            Aligner_ErrorCode.Add("A7", "Encoder error 5 ");
            Aligner_ErrorCode.Add("A8", "Encoder error 6");
            Aligner_ErrorCode.Add("A9", "Encoder malfunction");
            Aligner_ErrorCode.Add("B2", "Encoder error 2");
            Aligner_ErrorCode.Add("B3", "Absolute encoder turn count error");
            Aligner_ErrorCode.Add("B4", "Absolute encoder one rotation error");
            Aligner_ErrorCode.Add("B6", "Encoder memory error");
            Aligner_ErrorCode.Add("B5", "Over speed Multiple rotation generation error");
            Aligner_ErrorCode.Add("B7", "Acceleration error");
            Aligner_ErrorCode.Add("C1", "Over speed");
            Aligner_ErrorCode.Add("C2", " Speed control error");
            Aligner_ErrorCode.Add("C3", "Speed feedback error");
            Aligner_ErrorCode.Add("C4", "Speed attainment error");
            Aligner_ErrorCode.Add("D1", "Over position error");
            Aligner_ErrorCode.Add("D2", "Position instruction pulse frequency error 1");
            Aligner_ErrorCode.Add("D3", "Position instruction pulse frequency error 2");
            Aligner_ErrorCode.Add("DF", "Test mode finish");
            Aligner_ErrorCode.Add("E2", "EEPROM check sum error");
            Aligner_ErrorCode.Add("E3", "Internal RAM error  Power unit, amplifier internal");
            Aligner_ErrorCode.Add("E4", "Communication error between CPU and ASI");
            Aligner_ErrorCode.Add("E5", "Parameter error 1");
            Aligner_ErrorCode.Add("E6", "Parameter error 2");
            Aligner_ErrorCode.Add("E8", "Parameter error 3");
            Aligner_ErrorCode.Add("F1", "Task treatment error");
            Aligner_ErrorCode.Add("F2", "Initial time out");

            // Aligner_ErrorCode
            //LP_ErrorCode.Add("00", "Normal");
            //LP_ErrorCode.Add("10", "Clamp time over");
            //LP_ErrorCode.Add("11", "Unclamp time over");
            //LP_ErrorCode.Add("12", "Dock time over");
            //LP_ErrorCode.Add("13", "Undock time over");
            //LP_ErrorCode.Add("14", "Latch time over");
            //LP_ErrorCode.Add("15", "Unlatch time over");
            //LP_ErrorCode.Add("16", "Vacuum time over");
            //LP_ErrorCode.Add("17", "Vacuum release time over ");
            //LP_ErrorCode.Add("18", "Door open time over");
            //LP_ErrorCode.Add("19", "Door close time over");
            //LP_ErrorCode.Add("1A", "Mapping forward time over");
            //LP_ErrorCode.Add("1B", "Mapping return time over");
            //LP_ErrorCode.Add("1F", "communication error (depends on the specifications)");
            //LP_ErrorCode.Add("20", "Home return time over");
            //LP_ErrorCode.Add("21", "Loading time over");
            //LP_ErrorCode.Add("22", "Unloading time over");
            //LP_ErrorCode.Add("23", "Position time over");
            //LP_ErrorCode.Add("40", "Mapping data error");
            //LP_ErrorCode.Add("41", "Mode select error");
            //LP_ErrorCode.Add("70", "clamp sensor error");
            //LP_ErrorCode.Add("71", "Dock sensor error");
            //LP_ErrorCode.Add("72", "Latch sensor error");
            //LP_ErrorCode.Add("73", "Door sensor error");
            //LP_ErrorCode.Add("74", "Mapping sensor error");
            //LP_ErrorCode.Add("A0", "Wafer drop");
            //LP_ErrorCode.Add("A1", "Wafer protrusion");
            //LP_ErrorCode.Add("A2", "POUP mount error (Mount sensor)");
            //LP_ErrorCode.Add("A3", "POUP mount error (Load sensor)");
            //LP_ErrorCode.Add("A5", "Air pressure drop");
            //LP_ErrorCode.Add("B0", "Host error (with no PIO input)");
            //LP_ErrorCode.Add("C0", "Parameter error");
            //LP_ErrorCode.Add("E0", "FAN alarm");
            //LP_ErrorCode.Add("E2", "Encoder error");
            //LP_ErrorCode.Add("E3", "Servo alarm");
            //LP_ErrorCode.Add("FE", "Dock hand pinch error");
        }

        public Aligner()
        {
            InitializeComponent();
        }
        public enum AlignBrand { Hirata, Other }
        public enum AlignerType { WaferNormal, FrameClamp, FrameTurn }
        public enum AlignerCommand { ResetError, GetStatus, Home, Alignment, VacuumOn, VacuumOff, SetAngle, Turn, ReadBarcode, Connect, Disconnect }

        public event ThrowOutSocketReply ThrowOutSocketReplyEvent;
        public AML_LogPage Log = new AML_LogPage();
        BlockQueue<byte[]> AlignerRecQueue = new BlockQueue<byte[]>();
        BlockQueue<AlingerCmdObject> AlingerCmdObjectQueue = new BlockQueue<AlingerCmdObject>();

        SerialPort MainCom;
        string[] IPC_AllCOM;
        Adam6000s IO;

        private string IP = "127.0.0.1";
        private int Port = 9000;
        private SocketClient Client;
        int timeout_normal = 5000;
        int Frame_Aligner_BusyTime = 60000;
        private BarcodeReader CodeReader = new BarcodeReader();

        public AlignerType Aligner_Type = AlignerType.WaferNormal;

        private bool _error = false;
        private bool Error
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { Error = value; }));
                    return;
                }
                label_Error.Visible = value;
                _error = value;
            }
            get { return _error; }
        }

        bool _Busy = false;
        public bool Busy
        {
            get
            {
                return _Busy;
            }

        }

        bool _GetStatusDone = false;
        public bool GetStatusDone
        {
            get
            {
                return _GetStatusDone;
            }
        }

        bool _Lock = false;
        public bool Lock
        {
            get
            {
                return _Lock;
            }
        }

        bool _MainComOpen = false;
        bool MainComOpen
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { MainComOpen = value; }));
                    return;
                }
                _MainComOpen = value;
                Label_MainPort.BackColor = value ? Color.Lime : Color.Salmon;
            }
            get
            {
                return _MainComOpen;
            }
        }

        bool _AdamConnect = false;
        bool AdamConnect
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { AdamConnect = value; }));
                    return;
                }
                _AdamConnect = value;
                label_AdamIP.BackColor = value ? Color.Lime : Color.Salmon;
            }
            get
            {
                return _AdamConnect;
            }
        }


        public void SetLock(bool value)
        {
            _Lock = value;
        }
        private void SendOutSocket(string data)
        {
            if (ThrowOutSocketReplyEvent != null)
                ThrowOutSocketReplyEvent(data);
        }
        private void comboBox_Brand_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
        }

        private void comboBox_Type_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
        }

        private void comboBox_MainPort_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
            CB.Items.Clear();

            for (int i = 0; i < IPC_AllCOM.Length; i++)
                CB.Items.Add(IPC_AllCOM[i]);
        }
        public void Init(Adam6000s _IO, string _Type, int _Index)
        {
            try
            {
                IO = _IO;
                DeviceIndex = _Index;
                string temp = (_Index == 1) ? "2" : "";

                _DeviceName = XML.Read("Setting/Aligner" + temp + "/DeviceName", "Aligner");
                Enum.TryParse<AlignerType>(_Type, out Aligner_Type);

                string LogFolder = XML.Read("Setting/MainForm/Logfolder", "D:\\AML");
                int LogCountLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogCountLimit", "200"));
                int LogFolderLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogFolderLimit", "30"));
                Log.Initial(DeviceName, false, true, LogFolder, LogCountLimit, LogFolderLimit);
                //comboBox_Brand.DataSource = Enum.GetNames(typeof(AlignBrand));
                comboBox_Type.DataSource = Enum.GetNames(typeof(AlignerType));
                listBox_AlignerCommand.DataSource = Enum.GetNames(typeof(AlignerCommand));
                comboBox_Type.Text = Aligner_Type.ToString();
                IPC_AllCOM = SerialPort.GetPortNames();
                LoadSetting();
                //timer1sec.Enabled = true;
                GetStatus(false);
                //Qry_Status(false);

                if (Aligner_Type == AlignerType.FrameTurn)
                {
                    IP = XML.Read("Setting/Aligner" + temp + "/IP_Frame", "127.0.0.1");
                    Port = Convert.ToInt32(XML.Read("Setting/Aligner" + temp + "/Port_Frame", "5566"));
                    Frame_Aligner_BusyTime = Convert.ToInt32(XML.Read("Setting/Aligner" + temp + "/Frame_Aligner_BusyTime", "56")) * 1000;

                    string BarcodeCOM = XML.Read("Setting/Aligner" + temp + "/BarcodeCOM_Frame", "COM31");
                    CodeReader.Initial(5000, 1, BarcodeCOM);
                    CodeReader.ReadBarcodeComplete += CodeReader_ReadBarcodeComplete;

                    Client = new SocketClient(IP, Port);
                    Client.CloseEvent += new SocketClient.Close_Delegate(CloseEvent);
                    Client.ConnectEvent += new SocketClient.Connect_Delegate(ConnectEvent);
                    Client.DataArrivalEvent += new SocketClient.DataReceived_Delegate(DataArrivalEvent);
                    Client.DisconnectEvent += new SocketClient.Disconnect_Delegate(DisconnectEvent);
                    Client.ErrorEvent += new SocketClient.SocketError_Delegate(ErrorEvent);

                    SocketConnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadPort設定有誤請開發者檢查於:Aligner" + Environment.NewLine + ex.ToString());
                MessageBox.Show("程式即將自動關閉.");
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }

            backgroundWorker_Aligner_Send.RunWorkerAsync();
            backgroundWorker_Aligner_Receive.RunWorkerAsync();
            Log.Write(DeviceName, "Init Complete.");
        }

        private void LoadSetting()
        {
            try
            {
                for (int i = 0; i < IPC_AllCOM.Length; i++)
                {
                    comboBox_MainPort.Items.Add(IPC_AllCOM[i]);
                }

                tabControl1.SizeMode = TabSizeMode.Fixed;
                tabControl1.Appearance = TabAppearance.FlatButtons;
                tabControl1.ItemSize = new Size(0, 1);

                //	<Type value = "WaferNormal"/> <!-- WaferNormal, FrameClamp, FRAMETURN  -->

                string temp = (DeviceIndex == 1) ? "Aligner2" : "Aligner";

                _DeviceName = XML.Read("Setting/" + temp + "/DeviceName", "WaferNormal");
                if (XML.Read("Setting/" + temp + "/Type", "WaferNormal").ToUpper() == "FRAMECLAMP")
                {
                    tabControl1.SelectedIndex = 1;
                    Aligner_Type = AlignerType.FrameClamp;
                    Label_MainPort.Visible = false;
                    comboBox_MainPort.Visible = false;
                    label_AdamIP.Text = "Adam IP:" + IO.AdamIP;
                    IO.DI_Change += new Adam6000s.DI_ChangeEvent(DI_ChangeEventControl);
                }
                else if (XML.Read("Setting/" + temp + "/Type", "WaferNormal").ToUpper() == "FRAMETURN")
                {
                    tabControl1.SelectedIndex = 2;
                    Aligner_Type = AlignerType.FrameTurn;
                    Label_MainPort.Visible = false;
                    comboBox_MainPort.Visible = false;
                }
                else if (XML.Read("Setting/" + temp + "/Type", "WaferNormal").ToUpper() == "WAFERNORMAL")
                {
                    tabControl1.SelectedIndex = 0;
                    Aligner_Type = AlignerType.WaferNormal;
                    Ian.SetComPortProperty(out MainCom, XML.Read("Setting/" + temp + "/Main_COM", ""));
                    MainCom.DtrEnable = true;
                    MainCom.RtsEnable = true;
                    MainCom.Open();
                    MainComOpen = true;
                    MainCom.DataReceived += MainCom_DataReceived;
                    comboBox_MainPort.Text = MainCom.PortName;
                    InitAlignerErrorCode();
                }


                if (XML.Read("Setting/" + temp + "/Present_OCR_Top", "0") == "1")
                {
                    ocr_Top.Visible = true;
                    ocr_Top.Init("TopOCR", XML.Read("Setting/" + temp + "/IP_OCR_Top", "192.168.0.11"), Convert.ToInt32(XML.Read("Setting/" + temp + "/Port_OCR_Top", "2000")), Log);
                    ocr_Top.ThrowOutSocketReplyEvent += SendOutSocket;
                }

                if (XML.Read("Setting/" + temp + "/Present_OCR_Bottom", "0") == "1")
                {
                    ocr_Bottom.Visible = true;
                    ocr_Bottom.Init("BottomOCR", XML.Read("Setting/" + temp + "/IP_OCR_Bottom", "192.168.0.12"), Convert.ToInt32(XML.Read("Setting/" + temp + "/Port_OCR_Bottom", "2000")), Log);
                    ocr_Bottom.ThrowOutSocketReplyEvent += SendOutSocket;
                }

            }
            catch (Exception ex)
            {
                Log.WriteSystemLog(DeviceName, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        private void DI_ChangeEventControl(string AdamName, int Index, bool Result)
        {
            //throw new NotImplementedException();

            /*
            DI0	汽缸1縮(定位)
            DI1	汽缸1伸(定位)
            DI2	汽缸2縮(定位)
            DI3	汽缸2伸(定位)
            DI4	定位1(右)
            DI5	定位2(左)
            DI6	　
            DI7	　
            DI8	EQ1 DOOR ON
            DI9	EQ1 DOOR OFF
            DI10	EQ2 DOOR ON
            DI11	EQ2 DOOR OFF
            */


            if (Aligner_Type == AlignerType.FrameClamp)
            {
                if ((IO.DI[4] == true) & (IO.DI[5]) == true)
                    _WaferPresence = WaferPresence.Present;
                else if ((IO.DI[4] == false) & (IO.DI[5] == false))
                    _WaferPresence = WaferPresence.Absent;
                else if (IO.DI[4] != IO.DI[5])
                    _WaferPresence = WaferPresence.Unknown;

                Log.WriteSystemLog("Wafer Presence change to " + _WaferPresence.ToString());
            }
        }
        private void MainCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] raw_byte = new byte[128];
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)
            {
                Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                Array.Resize(ref raw_byte, length);
                AlignerRecQueue.EnQueue(raw_byte);
            }
        }
        private void Aligner_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
        private bool FrameAligner_ClampControl(bool OnOff)
        {
            if (Aligner_Type != AlignerType.FrameClamp)
                return false;


            /*
            DO0	汽缸1縮
            DO1	汽缸1伸
            DO2	汽缸2縮
            DO3	汽缸2伸
            DO4	RB HOME 3 or TR3_Interlock
            DO5	RB HOME 4 or TR4_Interlock 
            
            DI0	汽缸1縮(定位)
            DI1	汽缸1伸(定位)
            DI2	汽缸2縮(定位)
            DI3	汽缸2伸(定位)
            
            DI4	定位1(右)
            DI5	定位2(左)
            DI6	　
            DI7	　
            DI8	EQ1 DOOR ON
            DI9	EQ1 DOOR OFF
            DI10	EQ2 DOOR ON
            DI11	EQ2 DOOR OFF
            */

            if (IO.WriteDO(0, !OnOff) & IO.WriteDO(1, OnOff) & IO.WriteDO(2, !OnOff) & IO.WriteDO(3, OnOff))
            {

                Thread.Sleep(1000);

                if (IO.DI[0] != !OnOff) // DI0	汽缸1縮(定位)
                {
                    Log.Write("DI0	汽缸1縮(定位) 失敗");
                    return false;
                }

                if (IO.DI[1] != OnOff) // DI1	汽缸1伸(定位)
                {
                    Log.Write("DI1	汽缸1伸(定位) 失敗");
                    return false;
                }

                if (IO.DI[2] != !OnOff) //   DI2	汽缸2縮(定位)
                {
                    Log.Write("DI2	汽缸2縮(定位) 失敗");
                    return false;
                }

                if (IO.DI[3] != OnOff) //  DI3	汽缸2伸(定位)
                {
                    Log.Write("DI3	汽缸2伸(定位) 失敗");
                    return false;
                }

                return true;

            }
            else
            {
                return false;
            }
        }
        private void backgroundWorker_Aligner_Send_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
            while (true)
            {
                AlingerCmdObject MacroCmd = AlingerCmdObjectQueue.DeQueue();

                bool MacroDone = false;
                bool CanDoMacro = true;
                int count = 0;
                string FailReason = string.Empty;
                int ALID = 0;

                if (MacroCmd.Cmd == AlignerCommand.ReadBarcode)
                {
                    string ttemp = "";
                    MacroDone = ReadBarcode(out ttemp);
                    if (ttemp == "")
                        ttemp = "Error";
                    if (DeviceIndex == 1)
                        SendOutSocket("ReadBarcode,2," + ttemp);
                    else
                        SendOutSocket("ReadBarcode,1," + ttemp);
                }

                if (Error)
                {
                    if (MacroCmd.Cmd == AlignerCommand.GetStatus | MacroCmd.Cmd == AlignerCommand.ResetError)
                    {

                    }
                    else
                    {
                        CanDoMacro = false;
                        Log.Write(DeviceName, "因Aligner狀態異常，無法執行" + MacroCmd.Cmd.ToString());
                    }
                }

                if (Busy)
                {
                    if (MacroCmd.Cmd == AlignerCommand.GetStatus | MacroCmd.Cmd == AlignerCommand.ResetError)
                    {

                    }
                    else
                    {
                        CanDoMacro = false;
                        Log.Write(DeviceName, "因Aligner忙碌中，無法執行" + MacroCmd.Cmd.ToString());
                    }
                }

                if (CanDoMacro)
                {
                    Log.Write(DeviceName, "開始執行" + MacroCmd.Cmd.ToString());

                    switch (MacroCmd.Cmd)
                    {
                        case AlignerCommand.ResetError:
                            _Busy = true;
                            Error = true;

                            if (Aligner_Type == AlignerType.WaferNormal)
                            {
                                MainComDataSend("SET:RSET");
                                while (Error)
                                {
                                    count += 50;
                                    Thread.Sleep(50);
                                    if (count > 3000)
                                        break;
                                }

                                if (Error)
                                    break;
                            }

                            if (Aligner_Type == AlignerType.FrameClamp)
                            {
                                if (!IO.IsConnect)
                                {
                                    FailReason = "Adam is disconnected.";
                                    break;
                                }

                                if (!FrameAligner_ClampControl(false))
                                {
                                    FailReason = "ClampControl Off Fail";
                                    break;
                                }
                            }

                            if (Aligner_Type == AlignerType.FrameTurn)
                            {
                                CmdClearAlarm(true);
                                Error = false;
                            }

                            _Busy = false;
                            MacroDone = true;
                            break;
                        case AlignerCommand.Home:
                            _Busy = true;
                            Home = false;

                            if (Aligner_Type == AlignerType.WaferNormal)
                            {
                                MainComDataSend("MOV:ORGN");
                                while (!Home)
                                {
                                    count += 50;
                                    Thread.Sleep(50);
                                    if (count > 30000)
                                        break;
                                }

                                if (!Home)
                                    break;
                            }

                            if (Aligner_Type == AlignerType.FrameClamp)
                            {
                                if (!IO.IsConnect)
                                {
                                    FailReason = "Adam is disconnected.";
                                    break;
                                }
                                if (!FrameAligner_ClampControl(false))
                                {
                                    FailReason = "ClampControl Off Fail";
                                    break;
                                }
                            }

                            if (Aligner_Type == AlignerType.FrameTurn)
                            {
                                MacroCmd.FromSocket = true;
                                CmdHome(true);
                            }

                            _Busy = false;
                            MacroDone = true;
                            break;
                        case AlignerCommand.GetStatus:
                            {
                                _Busy = true;
                                _GetStatusDone = false;
                                if (Aligner_Type == AlignerType.WaferNormal)
                                {
                                    MainComDataSend("GET:STAS");

                                    while (!GetStatusDone)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 10000)
                                            break;
                                    }

                                    if (!GetStatusDone) // Fail
                                        break;

                                }

                                if (Aligner_Type == AlignerType.FrameClamp)
                                {
                                    Thread.Sleep(500);// 500ms刷新IO狀態
                                    _GetStatusDone = true;
                                }

                                if (Aligner_Type == AlignerType.FrameTurn)
                                {
                                    GetStatus(true, false);
                                }

                                _Busy = false;
                                MacroDone = true;
                            }
                            break;
                        case AlignerCommand.SetAngle:
                            {
                                _Busy = true;
                                if (Aligner_Type == AlignerType.WaferNormal)
                                {
                                    string send = "SET:OFSE" + (MacroCmd.Degree * 10).ToString().PadLeft(4, '0');
                                    MainComDataSend(send);
                                }

                                if (Aligner_Type == AlignerType.FrameClamp)
                                {
                                    _Busy = false;
                                    MacroDone = false;
                                }
                                _Busy = false;
                                MacroDone = true;
                            }
                            break;
                        case AlignerCommand.Alignment:
                            {

                                _Busy = true;

                                if (Aligner_Type == AlignerType.WaferNormal)
                                {
                                    _NotchDetectionStatus = NotchDetectionStatus.NotDetected;

                                    //1. SetAngle
                                    _SetAngleDone = SetAngleDone.Notyet;
                                    MainComDataSend("SET:OFSE" + (MacroCmd.Degree * 10).ToString().PadLeft(4, '0'));
                                    count = 0;
                                    while (_SetAngleDone != SetAngleDone.Done)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 3000)
                                            break;
                                    }
                                    if (_SetAngleDone != SetAngleDone.Done)
                                        break;

                                    //2. 真空
                                    _Vaccumm = Vaccumm.Unknown;
                                    MainComDataSend("MOV:ACCL");
                                    count = 0;
                                    while (_Vaccumm != Vaccumm.On)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 3000)
                                            break;
                                    }
                                    if (_Vaccumm != Vaccumm.On)
                                        break;

                                    //3. Align
                                    _NotchDetectionStatus = NotchDetectionStatus.NotDetected;
                                    MainComDataSend("MOV:ARLD");
                                    count = 0;
                                    while (_NotchDetectionStatus != NotchDetectionStatus.PositioningCompleted)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 60000)
                                            break;

                                        if (Error)
                                            break;
                                    }
                                    if (_NotchDetectionStatus != NotchDetectionStatus.PositioningCompleted)
                                        break;


                                    //4. Vac Off
                                    _Vaccumm = Vaccumm.Unknown;
                                    MainComDataSend("MOV:ACOP");
                                    count = 0;
                                    while (_Vaccumm != Vaccumm.Off)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 3000)
                                            break;
                                    }
                                    if (_Vaccumm != Vaccumm.Off)
                                        break;

                                    //5. GetStatus 
                                    MainComDataSend("GET:STAS");
                                    _NotchDetectionStatus = NotchDetectionStatus.NotDetected;
                                    count = 0;
                                    while (_NotchDetectionStatus != NotchDetectionStatus.PositioningCompleted)
                                    {
                                        count += 50;
                                        Thread.Sleep(50);
                                        if (count > 3000)
                                            break;
                                    }
                                    if (_NotchDetectionStatus != NotchDetectionStatus.PositioningCompleted)
                                        break;
                                }

                                if (Aligner_Type == AlignerType.FrameClamp)
                                {
                                    if (!IO.IsConnect)
                                    {
                                        FailReason = "Adam is disconnected.";
                                        break;
                                    }

                                    if (IsWaferPresence == WaferPresence.Unknown)
                                    {
                                        FailReason = "Wafer is unknown.";
                                        break;
                                    }

                                    if (!FrameAligner_ClampControl(false))
                                    {
                                        FailReason = "ClampControl Off Fail";
                                        break;
                                    }

                                    if (!FrameAligner_ClampControl(true))
                                    {
                                        FailReason = "ClampControl On Fail";
                                        break;
                                    }

                                    if (!FrameAligner_ClampControl(false))
                                    {
                                        FailReason = "ClampControl Off Fail";
                                        break;
                                    }
                                }

                                if (Aligner_Type == AlignerType.FrameTurn)
                                {
                                    MacroCmd.FromSocket = true;
                                    CmdTurn(MacroCmd.Degree.ToString(), true);
                                }

                                MacroDone = true;
                                _Busy = false;
                            }
                            break;
                        case AlignerCommand.Turn:
                            _Busy = true;
                            if (Aligner_Type == AlignerType.FrameTurn)
                            {
                                MacroCmd.FromSocket = true;
                                CmdTurn(MacroCmd.Degree.ToString().PadLeft(3, '0'), true);
                            }
                            MacroDone = true;
                            _Busy = false;
                            break;
                        case AlignerCommand.VacuumOn:
                            _Busy = true;

                            if (Aligner_Type == AlignerType.WaferNormal)
                            {
                                _Vaccumm = Vaccumm.Unknown;
                                MainComDataSend("MOV:ACCL");
                                count = 0;
                                while (_Vaccumm != Vaccumm.On)
                                {
                                    count += 50;
                                    Thread.Sleep(50);
                                    if (count > 3000)
                                        break;
                                }
                                if (_Vaccumm != Vaccumm.On)
                                    break;
                            }

                            if (Aligner_Type == AlignerType.FrameClamp)
                            {
                                if (!IO.IsConnect)
                                {
                                    FailReason = "Adam is disconnected.";
                                    break;
                                }

                                if (!FrameAligner_ClampControl(true))
                                {
                                    FailReason = "ClampControl On Fail";
                                    break;
                                }
                            }
                            if (Aligner_Type == AlignerType.FrameTurn)
                            {
                                CmdAlignment(true);
                            }
                            _Busy = false;
                            MacroDone = true;
                            break;
                        case AlignerCommand.VacuumOff:
                            _Busy = true;
                            if (Aligner_Type == AlignerType.WaferNormal)
                            {
                                _Vaccumm = Vaccumm.Unknown;
                                MainComDataSend("MOV:ACOP");
                                count = 0;
                                while (_Vaccumm != Vaccumm.Off)
                                {
                                    count += 50;
                                    Thread.Sleep(50);
                                    if (count > 3000)
                                        break;
                                }
                                if (_Vaccumm != Vaccumm.Off)
                                    break;
                            }

                            if (Aligner_Type == AlignerType.FrameClamp)
                            {
                                if (!IO.IsConnect)
                                {
                                    FailReason = "Adam is disconnected.";
                                    break;
                                }

                                if (!FrameAligner_ClampControl(false))
                                {
                                    FailReason = "ClampControl Off Fail";
                                    break;
                                }
                            }

                            if (Aligner_Type == AlignerType.FrameTurn)
                            {
                                CmdAlignment(true);
                            }
                            _Busy = false;
                            MacroDone = true;
                            break;
                    }
                }

                RefreshUI();

                if (MacroDone)
                {
                    _Busy = false;
                    Log.Write(DeviceName, "完成" + MacroCmd.Cmd.ToString());
                    Log.WriteSystemLog(DeviceName, "完成" + MacroCmd.Cmd.ToString());
                }
                else
                {
                    Log.Write(DeviceName, "失敗" + MacroCmd.Cmd.ToString());
                    Log.WriteSystemLog(DeviceName, "失敗" + MacroCmd.Cmd.ToString());
                }

                if (MacroCmd.FromSocket)
                {
                    switch (MacroCmd.Cmd)
                    {
                        case AlignerCommand.Alignment:
                            if (MacroDone)
                                SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + "," + MacroCmd.Degree + ",0");
                            else
                            {
                                if (Aligner_Type == AlignerType.WaferNormal)
                                    SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + "," + MacroCmd.Degree + "," + ErrorCodeReply(RecError));

                                if (Aligner_Type == AlignerType.FrameClamp)
                                    SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + "," + MacroCmd.Degree + "," + FailReason);
                            }
                            break;
                        case AlignerCommand.GetStatus:
                            if (MacroDone)
                            {
                                string status_str = MacroCmd.Cmd.ToString() + "," + DeviceName + "," + _Mode.ToString() + ",";
                                if (_WaferPresence == WaferPresence.Absent)
                                    status_str += "False,";
                                else
                                    status_str += "True,";

                                if (_Vaccumm == Vaccumm.Off)
                                    status_str += "False";
                                else
                                    status_str += "True";

                                SendOutSocket(status_str);
                            }
                            break;
                        case AlignerCommand.Home:
                        case AlignerCommand.ResetError:
                            if (MacroDone)
                                SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + ",0");
                            else
                            {
                                if (Aligner_Type == AlignerType.WaferNormal)
                                    SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + "," + ErrorCodeReply(RecError));
                                if (Aligner_Type == AlignerType.FrameClamp)
                                    SendOutSocket(MacroCmd.Cmd.ToString() + "," + DeviceName + "," + FailReason);
                            }
                            break;
                        case AlignerCommand.VacuumOn:
                            if (MacroDone)
                                SendOutSocket("AlignerVacuum," + DeviceName + ",1,0");
                            else
                            {
                                if (Aligner_Type == AlignerType.WaferNormal)
                                    SendOutSocket("AlignerVacuum," + DeviceName + ",1," + ErrorCodeReply(RecError));
                                if (Aligner_Type == AlignerType.FrameClamp)
                                    SendOutSocket("AlignerVacuum," + DeviceName + ",1," + FailReason);
                            }
                            break;
                        case AlignerCommand.VacuumOff:
                            if (MacroDone)
                                SendOutSocket("AlignerVacuum," + DeviceName + ",0,0");
                            else
                            {
                                if (Aligner_Type == AlignerType.WaferNormal)
                                    SendOutSocket("AlignerVacuum," + DeviceName + ",0," + ErrorCodeReply(RecError));
                                if (Aligner_Type == AlignerType.FrameClamp)
                                    SendOutSocket("AlignerVacuum," + DeviceName + ",0," + FailReason);
                            }
                            break;
                    }

                    if ((Enable_Alarm) & (MacroDone == false))
                    {
                        ALID += AlignerAlarmOffset;
                        SendOutSocket("Alm,Robot," + ALID + "," + FailReason);
                    }
                }

            }

        }
        private void backgroundWorker_Aligner_Receive_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
            byte[] Rec;
            while (true)
            {
                Rec = AlignerRecQueue.DeQueue(5000);

                if (Rec == null)
                {
                    if (ReceiveState != Wait)
                    {
                        Log.Write(DeviceName, "回應逾時(Timeout)" + Encoding.ASCII.GetString(Temp.ToArray()));
                        ReceiveState = Wait;
                    }
                }
                else
                {
                    string[] Rec_String = DataCombine(Rec);
                    if (Rec_String.Length > 0)
                    {
                        foreach (string RecHandleString in Rec_String)
                            ReceiveHandler(RecHandleString);
                    }
                }
                Thread.Sleep(10);
            }
        }
        private string[] DataCombine(byte[] raw_data)
        {
            List<string> FeedBack = new List<string>();
            for (int i = 0; i < raw_data.Length; i++)
            {
                switch (ReceiveState)
                {
                    case Wait://0
                        if (raw_data[i] != StartByte)
                        {
                            Log.Write(DeviceName, "Receive異常字元:0x" + Convert.ToString(raw_data[i], 16).ToUpper());
                            break;
                        }
                        ReceiveState = Receiving;
                        Temp.Clear();
                        break;
                    case Receiving://1
                        if (raw_data[i] != LastByte)
                        {
                            Temp.Add(raw_data[i]);
                            break;
                        }
                        //以下已經收到LastByte了
                        string CompleteString = Encoding.ASCII.GetString(Temp.ToArray());
                        int CheckSumIndex = CompleteString.IndexOf(';') + 1;
                        string CheckSumRec = CompleteString.Substring(CheckSumIndex);
                        byte[] Check = CheckSum(Temp.GetRange(0, Temp.Count - 2).ToArray());
                        if (Encoding.ASCII.GetString(Check) != CheckSumRec)
                        {
                            Log.Write(DeviceName, "Receive:" + CompleteString + ":drop");
                            Log.Write(DeviceName, "Receive Check sum異常:" + CheckSumRec + " 應為:" + Encoding.ASCII.GetString(Check));
                        }
                        else
                        {
                            Log.Write(DeviceName + "->Host", CompleteString);
                            FeedBack.Add(CompleteString.Substring(0, CompleteString.IndexOf(';')));
                        }
                        ReceiveState = Wait;
                        break;
                    default:
                        break;
                }
            }
            return FeedBack.ToArray();
        }

        string RecError = string.Empty;
        private void ReceiveHandler(string RecString)
        {
            if (RecString.Substring(0, 2) != "00" && RecString.Substring(0, 2) != "\0\0")
            {
                Log.Write(DeviceName, "異常發生 請參閱Alarm訊息");
                ErrorHandler(RecString.Substring(0, 2));
                return;
            }
            string RecType = RecString.Substring(4, 3);
            string RecInfo = RecString.Substring(8, 4);
            RecError = string.Empty;

            if (RecType == "GET")
            {
                switch (RecInfo)
                {
                    case "STAS":
                        {
                            //~0000GET:STAS/004000111110000;58*
                            string StatusData = RecString.Substring(RecString.IndexOf('/') + 1, 15);

                            if (StatusData[0] == '0')
                                _Status = Status.Normal;
                            else
                                _Status = Status.Error;

                            if (StatusData[1] == '0')
                                _Mode = Mode.Online;
                            else
                                _Mode = Mode.Maintenance;

                            if (StatusData[2] == '0')
                                _DeviceStatus = DeviceStatus.Unknown;
                            else if (StatusData[2] == '1')
                                _DeviceStatus = DeviceStatus.SupplyOk;
                            else if (StatusData[2] == '2')
                                _DeviceStatus = DeviceStatus.Extraction;
                            else if (StatusData[2] == '3')
                                _DeviceStatus = DeviceStatus.LOADReady;

                            if (StatusData[3] == '0')
                                _OperatingState = OperatingState.Stopping;
                            else if (StatusData[3] == '1')
                                _OperatingState = OperatingState.Operating;

                            _errorCode = StatusData.Substring(4, 2);

                            if (StatusData.Substring(6, 3) == "011")
                                _WaferPresence = WaferPresence.Absent;
                            else if (StatusData.Substring(6, 3) == "111")
                                _WaferPresence = WaferPresence.Present;
                            else
                                _WaferPresence = WaferPresence.Unknown;

                            if (StatusData[9] == '1')
                                Home = true;
                            else
                                Home = false;

                            if (StatusData[10] == '0')
                                _Vaccumm = Vaccumm.Off;
                            else if (StatusData[10] == '1')
                                _Vaccumm = Vaccumm.On;


                            if (StatusData.Substring(12, 3) == "100")
                                _NotchDetectionStatus = NotchDetectionStatus.Detected;
                            else if (StatusData.Substring(12, 3) == "200")
                                _NotchDetectionStatus = NotchDetectionStatus.PositioningCompleted;
                            else if (StatusData.Substring(12, 3) == "300")
                                _NotchDetectionStatus = NotchDetectionStatus.PositioningCompleted;
                            else
                                _NotchDetectionStatus = NotchDetectionStatus.NotDetected;

                            /*
                            A: Status   0: Normal  1: Error
                            B: Mode  0: Online  1: Maintenance
                            C: Device status  0: Unknown  1: Supply ok  2: Extraction  4: LOAD ready
                            D: Operating state  0: Stopping  1: Operating
                            EE: Error code  00: Normal  From 01: Error code
                            FFF: Wafer presence   011: Without wafer  111: With wafer
                            G: Origin status  0: Not origin  1: Origin status
                            H: Suction sensor status 0:OFF  1: ON (* In case of specification without suction, when a workpiece is detected, 1: ON.)
                            I: Lift status
                            0: Up position  1: Down position  2: Unknown (*In the speciation without lift, fixed to 0.)
                            JJJ: Notch detection status 
                            100:  Detected    200:  Positioning  completed  300:  Positioning 
                            completed  000: Not detected
                            */
                            _GetStatusDone = true;
                            RefreshUI();
                        }
                        break;

                }
                //Qry_Status(false);
            }
            else if (RecType == "MOV")
            {
                switch (RecInfo)
                {
                    case "ACCL":
                        Log.Write(DeviceName, "接受建立真空");
                        Log.WriteSystemLog(DeviceName, "接受建立真空");
                        break;
                    case "ACOP":
                        Log.Write(DeviceName, "接受解除真空");
                        Log.WriteSystemLog(DeviceName, "接受解除真空");
                        break;
                    case "ARLD":
                        Log.Write(DeviceName, "接受執行對位");
                        Log.WriteSystemLog(DeviceName, "接受執行對位");
                        break;
                    default:
                        Log.Write(DeviceName, "接受" + RecInfo);
                        Log.WriteSystemLog(DeviceName, "接受" + RecInfo);
                        break;
                }

                _Busy = true;

            }
            else if (RecType == "INF")
            {
                switch (RecInfo)
                {
                    case "RSET":
                        Error = false;
                        _Busy = false;
                        RecError = string.Empty;
                        Log.Write(DeviceName, "清除異常完成");
                        Log.WriteSystemLog(DeviceName, "清除異常完成");
                        break;
                    case "ORGN":
                        Home = true;
                        Error = false;
                        _Busy = false;
                        Log.Write(DeviceName, "原點復歸完成");
                        Log.WriteSystemLog(DeviceName, "原點復歸完成");

                        break;
                    case "OFSE":
                        _SetAngleDone = SetAngleDone.Done;
                        Log.Write(DeviceName, "角度設定完成");
                        Log.WriteSystemLog(DeviceName, "角度設定完成");
                        break;
                    case "ACCL":
                        _Vaccumm = Vaccumm.On;
                        Log.Write(DeviceName, "建立真空完成");
                        Log.WriteSystemLog(DeviceName, "建立真空完成");
                        break;
                    case "ACOP":
                        _Vaccumm = Vaccumm.Off;
                        Log.Write(DeviceName, "解除真空完成");
                        Log.WriteSystemLog(DeviceName, "解除真空完成");
                        break;
                    case "ARLD":
                        _NotchDetectionStatus = NotchDetectionStatus.PositioningCompleted;
                        Log.Write(DeviceName, "Align執行完成");
                        Log.WriteSystemLog(DeviceName, "Align執行完成");
                        break;
                    default:
                        Log.Write(DeviceName, "完成" + RecInfo);
                        Log.WriteSystemLog(DeviceName, "完成" + RecInfo);
                        break;
                }
                // _Busy = false;
            }
            else if (RecType == "ABS")
            {
                //"0000ABS:ARLD/D7"
                RecError = RecString.Substring(13, 2);
                Error = true;
                int ALID = 0;
                switch (RecInfo)
                {
                    case "RSET":
                        ALID = 501;
                        Log.Write(DeviceName, "清除異常完成失敗");
                        Log.WriteSystemLog(DeviceName, "清除異常完成");
                        break;
                    case "ORGN":
                        ALID = 502;
                        Log.Write(DeviceName, "Home 失敗");
                        Log.WriteSystemLog(DeviceName, "Home 失敗");
                        break;
                    case "OFSE":
                        ALID = 503;
                        _SetAngleDone = SetAngleDone.Done;
                        Log.Write(DeviceName, "角度設定失敗");
                        Log.WriteSystemLog(DeviceName, "角度設定失敗");
                        break;
                    case "ACCL":
                        ALID = 504;
                        Log.Write(DeviceName, "建立真空失敗");
                        Log.WriteSystemLog(DeviceName, "建立真空失敗");
                        break;
                    case "ACOP":
                        ALID = 505;
                        Log.Write(DeviceName, "解除真空失敗");
                        Log.WriteSystemLog(DeviceName, "解除真空失敗");
                        break;
                    case "ARLD":
                        ALID = 506;
                        _NotchDetectionStatus = NotchDetectionStatus.NotDetected;
                        Log.Write(DeviceName, "Align執行失敗");
                        Log.WriteSystemLog(DeviceName, "Align執行失敗");
                        break;
                    default:
                        ALID = 500;
                        Log.Write(DeviceName, RecInfo + "執行失敗");
                        Log.WriteSystemLog(DeviceName, RecInfo + "執行失敗");
                        break;
                }

                ALID += AlignerAlarmOffset;
                if (Enable_Alarm)
                    SendOutSocket("Alm,Aligner," + ALID + "," + RecInfo + " Fail");

                Log.Write(DeviceName, RecInfo + "執行失敗");
                Log.WriteSystemLog(DeviceName, RecInfo + "執行失敗");
            }
        }
        private void ErrorHandler(string ErrorCode)
        {
            try
            {
                Log.Write(DeviceName, "異常發生 請參閱Alarm訊息,ErrorCode:" + ErrorCode + ",Description:" + Aligner_ErrorCode[ErrorCode]);
                SendOutSocket("Alarm," + DeviceName + ",ErrorCode:" + ErrorCode + ",Description:" + Aligner_ErrorCode[ErrorCode]);

                int ALID = Convert.ToInt32(ErrorCode, 16) + AlignerAlarmOffset;
                if (Enable_Alarm)
                    SendOutSocket("Alm," + DeviceName + "," + ALID + "," + Aligner_ErrorCode[ErrorCode]);
            }
            catch
            {
                Log.Write(DeviceName, "異常發生 請參閱Alarm訊息,ErrorCode:" + ErrorCode + ",Description:Unknown Error");
                SendOutSocket("Alarm," + DeviceName + ",ErrorCode:" + ErrorCode + ",Description:Unknown Error");

                int ALID = Convert.ToInt32(ErrorCode, 16) + AlignerAlarmOffset;
                if (Enable_Alarm)
                    SendOutSocket("Alm," + DeviceName + "," + ALID + ",Unknown Error");
            }
        }
        private string ErrorCodeReply(string _error_str)
        {
            if (_error_str == string.Empty)
                return "1";

            string _result = "Error Code:" + _error_str + " Reason:"; //TBD
            return _result;
        }

        private void button_Aligner_Command_Click(object sender, EventArgs e)
        {
            AlignerCommand cmd = (AlignerCommand)Enum.Parse(typeof(AlignerCommand), listBox_AlignerCommand.Text, false);
            string error_msg = null;
            Log.WriteOperationLog(DateTime.Now, "Aligner: User press " + cmd.ToString());
            switch (cmd)
            {
                case AlignerCommand.ResetError:
                    ResetError();
                    break;
                case AlignerCommand.Home:
                    DoHome();
                    break;
                case AlignerCommand.GetStatus:
                    GetStatus(false);
                    break;
                case AlignerCommand.SetAngle:
                    RunCMD(AlignerCommand.SetAngle, Convert.ToInt32(numericUpDown_Degree.Value));
                    break;
                case AlignerCommand.Alignment:
                    Alignment(Convert.ToInt32(numericUpDown_Degree.Value));
                    break;
                case AlignerCommand.VacuumOn:
                    VacuumControl(true);
                    break;
                case AlignerCommand.VacuumOff:
                    VacuumControl(false);
                    break;
                case AlignerCommand.Turn:
                    RunCMD(AlignerCommand.Turn, Convert.ToInt32(numericUpDown_Degree.Value));
                    break;
                case AlignerCommand.ReadBarcode:
                    RunCMD(AlignerCommand.ReadBarcode);
                    break;
                case AlignerCommand.Connect:
                    SocketConnect();
                    break;
                case AlignerCommand.Disconnect:
                    SocketDisConnect();
                    break;
            }

            if (error_msg != null)
                MessageBox.Show(error_msg, "Aligner");
        }
        public bool ResetError()
        {
            RunCMD(AlignerCommand.ResetError);
            return true;
        }
        public bool DoHome()
        {
            if (Error)
            {
                SendOutSocket("Home,Aligner,In Error Status(Do ResetError to recover)");
                return false;
            }

            //if (Busy)
            //{
            //    SendOutSocket("Home,Aligner,In Busy Status(Wait for other command finish)");
            //    return false;
            //}

            if (Lock)
            {
                SendOutSocket("Home,Aligner,In Lock Status(Do Robot Home to release lock)");
                return false;
            }

            RunCMD(AlignerCommand.Home);
            return true;
        }
        public bool GetStatus(bool from_socket = true)
        {
            //if (Busy)
            //{
            //    SendOutSocket("GetStatus,Aligner,In Busy Status(Wait for other command finish)");
            //    return false;
            //}
            _GetStatusDone = false;
            RunCMD(AlignerCommand.GetStatus, 0, from_socket);
            return true;
        }
        public bool Alignment(int degree)
        {
            if (Error)
            {
                SendOutSocket("Alignment,Aligner," + degree + ",In Error Status(Do ResetError to recover)");
                return false;
            }

            //if (Busy)
            //{
            //    SendOutSocket("Alignment,Aligner," + degree + ",In Busy Status(Wait for other command finish)");
            //    return false;
            //}

            if (Lock)
            {
                SendOutSocket("Alignment,Aligner," + degree + ",In Lock Status(Do Robot Home to release lock)");
                return false;
            }

            RunCMD(AlignerCommand.Alignment, degree);
            return true;
        }
        public bool VacuumControl(bool On)
        {
            if (Error)
            {
                if (On)
                    SendOutSocket("AlignerVacuum,Aligner,1,In Error Status(Do ResetError to recover)");
                else
                    SendOutSocket("AlignerVacuum,Aligner,0,In Error Status(Do ResetError to recover)");

                return false;
            }

            //if (Busy)
            //{
            //    if (On)
            //        SendOutSocket("AlignerVacuum,Aligner,1,In Busy Status(Wait for other command finish)");
            //    else
            //        SendOutSocket("AlignerVacuum,Aligner,0,In Busy Status(Wait for other command finish)");

            //    return false;
            //}

            if (Lock)
            {
                if (On)
                    SendOutSocket("AlignerVacuum,Aligner,1,In Lock Status(Do Robot Home to release lock)");
                else
                    SendOutSocket("AlignerVacuum,Aligner,0,In Lock Status(Do Robot Home to release lock)");

                return false;
            }

            if (On)
                RunCMD(AlignerCommand.VacuumOn);
            else
                RunCMD(AlignerCommand.VacuumOff);

            return true;
        }

        public bool RunCMD(AlignerCommand _cmd, int degree = 0, bool _fromsocket = true)
        {
            //if (Busy | Error)
            //{
            //    if (_cmd != AlignerCommand.GetStatus & _cmd != AlignerCommand.ResetError)
            //    {
            //        return false;
            //    }
            //}

            AlingerCmdObject lco = new AlingerCmdObject();
            lco.Cmd = _cmd;
            lco.Degree = degree;
            lco.FromSocket = _fromsocket;
            AlingerCmdObjectQueue.EnQueue(lco);
            return true;
        }

        private string Aligner_CommandString = "0000";
        private const byte StartByte = 0x01;
        private const byte LastByte = 0x0D;
        private const int Wait = 0;
        private const int Receiving = 1;
        private const int CloseCom = 10;
        int ReceiveState = Wait;
        private List<byte> Temp = new List<byte>();

        private void MainComDataSend(string Command_String)
        {
            try
            {
                string Send_String = Aligner_CommandString + Command_String + ";";
                List<byte> Command_Byte = new List<Byte>(Encoding.ASCII.GetBytes(Send_String));
                byte[] check_sum = CheckSum(Command_Byte.ToArray());
                Command_Byte.Insert(0, StartByte);

                if (check_sum.Length == 1)
                    Command_Byte.Add(0x30);

                Command_Byte.AddRange(check_sum);
                Command_Byte.Add(LastByte);
                MainCom.Write(Command_Byte.ToArray(), 0, Command_Byte.Count);//送出指令
                Log.Write("Host->" + DeviceName, Send_String + Encoding.ASCII.GetString(check_sum));
            }
            catch (Exception ex)
            {
                Log.Write("Host->" + DeviceName, "Exception:" + ex.ToString());
            }
        }

        private byte[] CheckSum(byte[] data)
        {
            byte Sum = 0x00;
            foreach (byte SingleByte in data)
                Sum += SingleByte;
            return Encoding.ASCII.GetBytes(Convert.ToString(Sum, 16).ToUpper());
        }


        public class AlingerCmdObject
        {
            public AlignerCommand Cmd = AlignerCommand.GetStatus;
            public int Degree = 0;
            public bool FromSocket = false;
        }


        public void RefreshUI()
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    RefreshUI();
                }));
            }
            else
            {
                if (Aligner_Type == AlignerType.WaferNormal)
                {
                    label_Status.Text = _Status.ToString();
                    RemoteLabel.Text = _Mode.ToString();
                    label_Vac.Text = _Vaccumm.ToString();
                    label_DeviceStatus.Text = _DeviceStatus.ToString();
                    label_OperatingState.Text = _OperatingState.ToString();
                    label_NotchDetectionStatus.Text = _NotchDetectionStatus.ToString();
                    label_WaferPresent.Text = _WaferPresence.ToString();
                }

                if (Aligner_Type == AlignerType.FrameClamp)
                {
                    label_Status.Text = _Status.ToString();
                    RemoteLabel.Text = _Mode.ToString();
                    //label_Vac.Text = _Vaccumm.ToString();
                    label_DeviceStatus.Text = _DeviceStatus.ToString();
                    label_OperatingState.Text = _OperatingState.ToString();
                    label_NotchDetectionStatus.Text = _NotchDetectionStatus.ToString();
                    label_WaferPresent.Text = _WaferPresence.ToString();
                }
            }
        }

        void CodeReader_ReadBarcodeComplete(string Code, bool Result)
        {
            //throw new NotImplementedException();
            if (Result)
                Log.WriteSystemLog("Get Barcode:" + Code);
            else
                Log.WriteSystemLog("Get Barcode Fail.");
        }
        public void ReadBarcodeAsync()
        {
            CodeReader.ReadBarcodeAsync();
        }
        public bool ReadBarcode(out string barcode)
        {
            return CodeReader.ReadBarcode(out barcode);
        }

        public void SocketDisConnect()
        {
            //Connected = false;
            Client.Socket_Close();
            Log.Write("System", "FrameAligner 主動關閉連線");
        }
        public void SocketConnect()
        {
            Client.Socket_Connect(IP, Port);
            Log.Write("System", "FrameAligner 嘗試連線至" + IP + ":" + Port);
        }

        private bool Send_Data(string Send_Message)
        {
            //if ()
            //{
            bool _result = Client.SendData(Send_Message);

            if (_result)
                Log.Write("UI->" + DeviceName, Send_Message);

            return _result;
            //}
            //else
            //{
            //    return false;
            //}
            //return true;

        }
        private void ConnectEvent()
        {
            //Connected = true;
            Log.Write("System", "FrameAligner Connect!");

            CmdRemote(true);

        }
        private void CloseEvent()
        {
            //Connected = false;
            Log.WriteSystemLog("System", "FrameAligner Close!");
        }
        private void ErrorEvent(string ex)
        {
            //Connected = false;
            //SendAlarm(ALID_Disconnect, "Frame Aligner disconnect");
            Log.Write(DeviceName, "FrameAligner error:" + ex.ToString());
        }
        private void DisconnectEvent()
        {
            //Connected = false;
            //SendAlarm(ALID_Disconnect, "Frame Aligner disconnect");
            Log.Write(DeviceName, "FrameAligner Disconnect.");
        }
        private void DataArrivalEvent(string Data)
        {
            string[] Received_data = Data.ToString().Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Received_data.Length; i++)
            {
                Received_data[i] = Received_data[i].Trim();
                if (Received_data[i].Substring(0, 1) == "~")
                {
                    Log.Write(DeviceName + "->UI", Received_data[i]);
                    ReceivedAligner(Received_data[i].Substring(1));
                }
            }
        }
        private void ReceivedAligner(string v)
        {
            // throw new NotImplementedException();
            string[] token = v.Split(new string[] { "," }, StringSplitOptions.None);

            if (token.Length > 1)
            {
                switch (token[0].ToUpper())
                {
                    case "ALM":
                        {
                            string alid = "73" + token[1];//7310220
                            string altx = token[2];
                            //SetError(true);
                            //SendAlarm(alid, altx);
                            Log.WriteSystemLog(DeviceName, "Alarm:" + altx);
                        }
                        //GetStatus(true, false);
                        break;
                    case "EVT":
                        if (token[1].ToUpper() == "ALM")
                        {
                            switch (token[2].ToUpper())
                            {
                                case "HOME":
                                    //_HomeDone = true;
                                    //SetHome(true);
                                    //SetError(false);
                                    //SetBusy(false);
                                    _Lock = false;
                                    _Busy = false;
                                    Log.WriteSystemLog(DeviceName, "完成Home指令");
                                    break;
                                case "ALIGNMENT":
                                    //_AlignDone = true;
                                    //SetHome(false);
                                    //SetBusy(false);
                                    _Lock = false;
                                    _Busy = false;
                                    Log.WriteSystemLog(DeviceName, "完成Alignment指令");
                                    break;
                                default:
                                    if (token[2].ToUpper().IndexOf("TURN") == 0)
                                    {
                                        _Lock = false;
                                        _Busy = false;
                                        //_AlignDone = true;
                                        //_TurnDone = true;
                                        //SetHome(false);
                                        //SetBusy(false);
                                        //_degree = Convert.ToDouble(token[2].Substring(4));
                                        //Log.WriteSystemLog("FrameAligner", "完成Turn指令(包含Align)，角度為" + Degree);
                                        //if (AlignedEvnet != null)
                                        //    AlignedEvnet(AlignerAction.Aligned);
                                    }
                                    break;
                            }
                        }
                        //GetStatus(true, false);
                        break;
                    case "ACK":
                        switch (token[1].ToUpper())
                        {
                            case "REMOTE":
                                // _ControlState = FrameAligner_ControlState.Remote;
                                //SetControlState(FrameAligner_ControlState.Remote);                                
                                Log.WriteSystemLog(DeviceName, "切換至Remote");

                                break;
                            case "LOCAL":
                                //_ControlState = FrameAligner_ControlState.Local;
                                //SetControlState(FrameAligner_ControlState.Local);
                                Log.WriteSystemLog(DeviceName, "切換至Local");

                                break;
                            case "ALM":
                                switch (token[2].ToUpper())
                                {
                                    case "HOME":
                                    case "ALIGNMENT":
                                        if (token[3] == "0")
                                            Log.WriteSystemLog(DeviceName, "接受" + token[2] + "指令");
                                        else
                                            Log.WriteSystemLog(DeviceName, "拒絕" + token[2] + "指令");
                                        break;
                                    case "CLEARALARM":
                                        _error = false;
                                        _Busy = false;
                                        break;

                                    default:
                                        if (token[2].ToUpper().IndexOf("TURN") == 0)
                                        {
                                            double d = Convert.ToDouble(token[2].Substring(4));

                                            if (token[3] == "0")
                                                Log.WriteSystemLog(DeviceName, "接受Turn Table至角度" + d.ToString());
                                            else
                                                Log.WriteSystemLog(DeviceName, "拒絕Turn Table至角度" + d.ToString());

                                        }
                                        break;
                                }

                                break;
                        }
                        break;
                    case "ANS":
                        if ((token[1].ToUpper() == "ALM") & (token[2].ToUpper() == "STATUS"))
                        {
                            //FrameAligner_ControlState fc = (FrameAligner_ControlState)Enum.Parse(typeof(FrameAligner_ControlState), token[3], true);
                            //SetControlState(fc);

                            switch (token[4].ToUpper())
                            {
                                case "IDLE":
                                    _Mode = Mode.Online;
                                    _Lock = false;
                                    _Busy = false;
                                    break;
                                case "BUSY":
                                    //SetBusy(true);
                                    _Mode = Mode.Online;
                                    _Lock = true;
                                    _Busy = true;
                                    break;
                                case "ERROR":
                                    //SetError(true);
                                    _Mode = Mode.Online;
                                    _Lock = true;
                                    break;
                            }

                            //FrameAligner_FramePresent WP = (FrameAligner_FramePresent)Enum.Parse(typeof(FrameAligner_FramePresent), token[5], true);
                            //SetWaferPresent(WP);
                            if (token[5] == "Present")
                                _WaferPresence = WaferPresence.Present;
                            else
                                _WaferPresence = WaferPresence.Absent;

                            //_Clamp = (FrameAligner_Clamp)Enum.Parse(typeof(FrameAligner_Clamp), token[6], true);

                            if (token[6] == "Clamp")
                                _Vaccumm = AML.Vaccumm.On;
                            else
                                _Vaccumm = AML.Vaccumm.Off;

                            //if (token[7].ToUpper() != "UNKNOW")
                            //    _degree = Convert.ToDouble(token[7]);
                            //else
                            //    _degree = -1;                            

                            //_StatusDone = true;
                            Log.WriteSystemLog(DeviceName, "刷新狀態完成");
                        }

                        //~Ans,ALM,Status,A,B,C,D,E@
                        //Format
                        //A:Remote / Local
                        //B:Idle / Busy / Error
                        //C:Present / Absent / Unknow
                        //D:Clamp / UnClamp / Unknow
                        //E:0 / 90 / 180 / 270 / Unknow
                        break;
                }

            }

        }

        public void ResetBusy()
        {
            _Busy = false;
            Log.WriteSystemLog("EFEM", "手動清除Frame Aligner忙碌狀態");
        }
        public bool CmdRemote(bool wait_reply)
        {
            bool _result = false;
            // _ControlState = FrameAligner_ControlState.Unknown;
            //SetControlState(FrameAligner_ControlState.Unknown);
            _result = Send_Data("~Cmd,Remote@");

            Log.WriteSystemLog(DeviceName, "Do CmdRemote.");

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;

        }
        public bool CmdLocal(bool wait_reply)
        {
            bool _result = false;
            // _ControlState = FrameAligner_ControlState.Unknown;
            //SetControlState(FrameAligner_ControlState.Unknown);
            _result = Send_Data("~Cmd,Local@");

            Log.WriteSystemLog(DeviceName, "Do CmdLocal.");


            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }
        public bool CmdClearAlarm(bool wait_reply)
        {
            bool _result = false;

            //_ClearAlarmDone = false;
            _result = Send_Data("~Cmd,ALM,ClearAlarm@");

            Log.WriteSystemLog(DeviceName, "Do CmdClearAlarm.");

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }
        public bool CmdTurn(string turn_degree, bool wait_reply)
        {
            bool _result = false;

            if (Error)
            {
                Log.WriteSystemLog(DeviceName, "Error中無法執行，請先清除Clear Error & Home");
                return false;
            }
            if (Lock)
            {
                Log.WriteSystemLog(DeviceName, "Lock中無法操作");
                return false;
            }
            //_AlignDone = false;
            //_TurnDone = false;

            string d = turn_degree.ToString().PadLeft(3, '0');

            if ((d != "000") & (d != "090") & (d != "180") & (d != "270"))
            {
                Log.WriteSystemLog(DeviceName, "CmdTurn 無法接受角度" + d);
                return false;
            }

            _result = Send_Data("~Cmd,ALM,Turn" + d + "@");

            Log.WriteSystemLog(DeviceName, "Do CmdTurn degree:" + turn_degree);

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }
        public bool CmdHome(bool wait_reply)
        {
            bool _result = false;

            if (Error)
            {
                Log.WriteSystemLog(DeviceName, "Error中無法執行，請先清除Clear Error");
                return false;
            }

            //if (Busy)
            //{
            //    Log.WriteSystemLog(DeviceName, "Busy中無法執行");
            //    return false;
            //}

            if (Lock)
            {
                Log.WriteSystemLog(DeviceName, "Lock中無法操作");
                return false;
            }

            //_TurnDone = false;
            //_AlignDone = false;
            //_HomeDone = false;
            _result = Send_Data("~Cmd,ALM,Home@");


            //SetBusy(true);
            Log.WriteSystemLog(DeviceName, "Do CmdHome.");

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }
        public bool CmdAlignment(bool wait_reply)
        {
            bool _result = false;

            if (Error)
            {
                Log.WriteSystemLog(DeviceName, "Error中無法執行，請先清除Clear Error & Home");
                return false;
            }

            //if (Busy)
            //{
            //    Log.WriteSystemLog(DeviceName, "Busy中無法執行");
            //    return false;
            //}

            if (Lock)
            {
                Log.WriteSystemLog(DeviceName, "Lock中無法操作");
                return false;
            }

            //_AlignDone = false;

            //SetBusy(true);
            _result = Send_Data("~Cmd,ALM,Alignment@");

            Log.WriteSystemLog(DeviceName, "Do CmdAlignment.");

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }
        public bool GetStatus(bool wait_reply, bool s)
        {
            //MacroCmd.FromSocket = true;

            bool _result = false;

            //_StatusDone = false;
            _result = Send_Data("~Get,ALM,Status@");

            Log.WriteSystemLog(DeviceName, "Do GetStatus.");

            if (_result == false)
                return false;

            if (wait_reply)
            {
                int count = 0;
                while (_Busy)
                {
                    Thread.Sleep(10);
                    count += 10;
                    if (count > Frame_Aligner_BusyTime)
                        return false;
                }
            }

            return _result;
        }

    }


}
