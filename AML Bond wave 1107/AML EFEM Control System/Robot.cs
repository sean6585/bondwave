using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections;

namespace AML
{
    public enum RobotMotionPoint { Normal, Snake }
    public enum RobotMacroCommand { ReadPosition, ResetError, Home, SmartGet, GetStandby, GetExtend, GetPick, GetRetract, SmartGet6S, SmartPut, PutStandby, PutExtend, PutPlace, PutRetract, SmartPut6S, VacuumOn, VacuumOff, EdgeGripOn, EdgeGripOff, GetStatus, SetSpeed }
    public enum RobotInnerCommand { GetStatus, GoPosition, ReadPosition, ReadDI, ReadDO, WriteDO }
    public enum RobotArm { UnknownArm, LowerArm, UpperArm }
    public enum RobotType { Unknown, Vacuum, Contact, Grip }
    public enum TargetType { Unknown, Wafer, Frame }
    public enum VacuumStatus { Unknown, On, Off }
    public enum GripStatus { Unknown, On, Off }
    public enum ForkWaferPresent { Absent = 0, Present = 1, Unknown = 9 }

    //LR LD


    public partial class Robot : Form
    {
        // public List<StationInfo> StationInfo_List = new List<StationInfo>();
        // private BlockQueue<string> RobotCmdQueue = new BlockQueue<string>();
        //private int RobotIndex = 0;
        private int RobotIndex = 0;
        private string DeviceName = "";
        private BlockQueue<string> RobotRecQueue = new BlockQueue<string>();
        private BlockQueue<string[]> RobotCmdMarcoQueue = new BlockQueue<string[]>();
        private BlockQueue<string> RobotRecMarcoQueue = new BlockQueue<string>();
        private List<byte> Temp = new List<byte>();
        public event ThrowOutSocketReply ThrowOutSocketReplyEvent;
        private BlockQueue<RobotMacroCommandObject> RobotMacroCommandObjectQueue = new BlockQueue<RobotMacroCommandObject>();


        public bool Enable_Alarm = false;
        public int RobotAlarmOffset = 10000000;

        private string _StatusCode;
        private string StatusCode
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { StatusCode = value; }));
                    return;
                }
                _StatusCode = value;
                label_StatusCode.Text = value;

            }
            get { return _StatusCode; }
        }
        //public bool UpperPresent;//目前沒用
        //public bool LowerPresent;
        //public bool Robot_Error;
        public Robot()
        {
            InitializeComponent();
        }
        private void SendOutSocket(string data)
        {
            //Log.WriteSystemLog("SocketSendOut check",  data);
            if (ThrowOutSocketReplyEvent != null)
                ThrowOutSocketReplyEvent(data);
        }
        public Label ConnectLabel = new Label();

        private bool _Connect;
        public bool Connect//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { Connect = value; }));
                    return;
                }
                ConnectLabel.BackColor = (value ? Color.LightGreen : Color.Red);
                _Connect = value;
            }
            get { return _Connect; }
        }

        private bool _Remote;
        public bool Remote
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { Remote = value; }));
                    return;
                }
                _Remote = value;
                RemoteLabel.BackColor = (value ? Color.Lime : Color.LightPink);
                RemoteLabel.Text = (value ? "Remote" : "Local");
            }
            get { return _Remote; }
        }

        private RobotMotionPoint MotionType = RobotMotionPoint.Normal;
        private string SnakeHomeType = "";

        /*
        <add key="Robot_Type" value="Hirata"/>
        <add key="Robot_ComPort" value="COM10"/>
        <add key="Robot_ComPort_Setting" value="19200,E,7,1"/>
        <add key="Robot_EE_Thickness" value="3"/>
        <add key="Robot_IP" value="192.168.0.2"/>
        <add key="Robot_Port" value="12800"/>
        <add key="Robot_SmartHandling" value="0"/>
        <add key="Robot_AutoVacuumOnBeforeGetStatus" value="0"/>        
        */
        SerialPort MainCom;
        public AML_LogPage Log = new AML_LogPage();

        //=====Robot狀態Flag=====
        private bool Moving = false;     //這是機台回報Status內的參數

        //=====COM port 通訊=====
        //private string CMD_string = "";
        //private string REC_string = "";
        private const byte StartByte = 0x02;
        private const byte EndByte = 0x03;
        private const int Idle = 0;
        private const int WaitReply = 1;
        private const int Receiving = 2;
        private const int WaitCheckSum = 3;
        private int ReceiveState = Idle;
        //string MarcoMoveCommand;
        public int Speed;
        public int Z_Speed;
        public int SmartGetDelayms = 1000;

        private const string RobotAddress = "001 ";

        private bool _busy;
        public bool Busy //真的在動
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { Busy = value; }));
                    return;
                }
                BusyLabel.Visible = value;
                _busy = value;
            }
            get { return _busy; }
        }

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

        List<LoadPort> LP;
        string[] IPC_AllCOM;

        List<Fork> Fork = new List<Fork>();

        Fork LowerArm = null;
        Fork UpperArm = null;

        public Fork LOWERARM
        {
            get
            {
                return LowerArm;
            }
        }
        public Fork UPPERARM
        {
            get
            {
                return UpperArm;
            }
        }

        List<StationInfo> StationList = new List<StationInfo>();

        public List<StationInfo> STATIONLIST
        {
            get
            {
                return StationList;
            }
        }

        private Aligner[] Aligner = new Aligner[2];
        // RobotType RobotType = RobotType.Unknown;
        public void Init(List<LoadPort> _LP, Aligner[] _AL, string _Device, int _Index)
        {

            try
            {
                LP = _LP;
                Aligner = _AL;
                DeviceName = _Device;
                RobotIndex = _Index;
                listBox_RobotMacroCommand.DataSource = Enum.GetNames(typeof(RobotMacroCommand));

                string LogFolder = XML.Read("Setting/MainForm/Logfolder", "D:\\AML");
                int LogCountLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogCountLimit", "200"));
                int LogFolderLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogFolderLimit", "30"));

                Speed = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Speed", "10"));
                Z_Speed = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Z_Speed", "10"));


                if (XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/SmartGetDelayms", "-1") == "-1")
                {
                    MessageBox.Show("自動產生SmartGetDelayms於XML，預設1000ms");
                    XML.CreateNode("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "", "SmartGetDelayms", "value", "1000");
                }

                SmartGetDelayms = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/SmartGetDelayms", "1000"));

                if (SmartGetDelayms <= 500)
                    SmartGetDelayms = 500;

                Log.Initial(DeviceName, false, true, LogFolder, LogCountLimit, LogFolderLimit);
                IPC_AllCOM = SerialPort.GetPortNames();

                comboBox_Z_Speed.Items.Add(1);
                comboBox_Other_Speed.Items.Add(1);
                for (int i = 5; i <= 100; i += 5)
                {
                    comboBox_Z_Speed.Items.Add(i);
                    comboBox_Other_Speed.Items.Add(i);
                }

                comboBox_Z_Speed.Text = Z_Speed.ToString();
                comboBox_Other_Speed.Text = Speed.ToString();

                groupBox_Speed.Visible = false;

                comboBox_MainPort.SelectedIndex = -1;
                comboBox_MainPort.Items.Clear();
                for (int i = 0; i < IPC_AllCOM.Length; i++)
                    comboBox_MainPort.Items.Add(IPC_AllCOM[i]);

                int armcount = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/ArmCount", "10"));

                MotionType = (RobotMotionPoint)Enum.Parse(typeof(RobotMotionPoint), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Motion", "Normal"), true);
                label_Motion.Text = MotionType.ToString();

                if (armcount == 1)
                {
                    Fork _f = new Fork();
                    _f.RobotArm = RobotArm.LowerArm;
                    _f.TargetType = (TargetType)Enum.Parse(typeof(TargetType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/LowerArmTarget", ""), true);
                    _f.RobotType = (RobotType)Enum.Parse(typeof(RobotType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/LowerArmType", ""), true);
                    Fork.Add(_f);
                    LowerArm = _f;
                }
                else if (armcount == 2)
                {
                    Fork _f = new Fork();
                    _f.RobotArm = RobotArm.LowerArm;
                    _f.TargetType = (TargetType)Enum.Parse(typeof(TargetType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/LowerArmTarget", ""), true);
                    _f.RobotType = (RobotType)Enum.Parse(typeof(RobotType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/LowerArmType", ""), true);
                    Fork.Add(_f);

                    Fork _f2 = new Fork();
                    _f2.RobotArm = RobotArm.UpperArm;
                    _f2.TargetType = (TargetType)Enum.Parse(typeof(TargetType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/UpperArmTarget", ""), true);
                    _f2.RobotType = (RobotType)Enum.Parse(typeof(RobotType), XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/UpperArmType", ""), true);
                    Fork.Add(_f2);

                    LowerArm = _f;
                    UpperArm = _f2;
                }
                else
                {
                    MessageBox.Show("Robot ArmCount設定有誤請開發者檢查設定檔");
                    MessageBox.Show("程式即將自動關閉.");
                    Process p = Process.GetCurrentProcess();
                    p.Kill();
                }

                for (int i = LP.Count - 1; i >= 0; i--)
                {
                    StationInfo s = new StationInfo();
                    s.StationName = LP[i].LPName;
                    s.LowerPutAddress = GetPositionID(LP[i].LPName, true, RobotArm.LowerArm);
                    s.MaxSlot = LP[i].AllSlotCount;
                    s.SlotPitch = 10;
                    StationList.Add(s);
                }

                for (int i = Aligner.Length - 1; i >= 0; i--)
                {
                    string temp = ((i == 1) ? "Aligner2" : "Aligner");
                    if (XML.Read("Setting/" + temp + "/Enable", "0") != "1")
                        continue;

                    if (Aligner[i] != null)
                    {
                        StationInfo s = new StationInfo();
                        s.StationName = Aligner[i].DeviceName;
                        s.LowerPutAddress = Convert.ToInt32(XML.Read("Setting/" + temp + "/PutAddress", "0"));
                        s.MaxSlot = 1;
                        s.SlotPitch = 0;
                        StationList.Add(s);
                    }
                }

                for (int i = 1; i <= 8; i++)
                {
                    if (XML.Read("Setting/Station_" + i + "/Enable", "0") != "1")
                        continue;

                    StationInfo s = new StationInfo();
                    s.OtherStation = true;
                    s.TargetType = (TargetType)Enum.Parse(typeof(TargetType), XML.Read("Setting/Station_" + i + "/Type", ""), true);
                    s.StationName = XML.Read("Setting/Station_" + i + "/StationName", "0");
                    s.LowerPutAddress = Convert.ToInt32(XML.Read("Setting/Station_" + i + "/PutAddress", "0"));
                    s.MaxSlot = Convert.ToInt32(XML.Read("Setting/Station_" + i + "/MaxSlot", "1"));
                    s.SlotPitch = Convert.ToInt32(XML.Read("Setting/Station_" + i + "/SlotPitch", "0"));
                    StationList.Add(s);
                }

                InitRobotErrorCode();

                if (LowerArm == null)
                    forkView_Lower.Visible = false;
                else
                    forkView_Lower.Init(LowerArm);


                if (UpperArm == null)
                    forkView_Upper.Visible = false;
                else
                    forkView_Upper.Init(UpperArm);

                Ian.SetComPortProperty(out MainCom, XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Main_COM", "COM10"));

                MainCom.DtrEnable = true;
                MainCom.RtsEnable = true;
                MainCom.DataReceived += MainCom_DataReceived;
                COM_Connect();
                SetRobotSpeed(Speed, Z_Speed, false);
                //timer1sec.Enabled = true;
                comboBox_MainPort.Text = MainCom.PortName;
                backgroundWorker_Robot_COM.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Robot設定有誤請開發者檢查於:" + Environment.NewLine + ex.ToString());
                MessageBox.Show("程式即將自動關閉.");
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }

            Log.Write(DeviceName.ToString(), "Init Complete.");
        }
        private void DataSend(string Command_String)
        {
            try
            {
                //if (Error)
                //    return;
                //byte[] Command_Byte;
                string Send_String = RobotAddress + Command_String;
                List<byte> Command_Byte = new List<byte>(Encoding.ASCII.GetBytes(Send_String));
                Command_Byte.Add(EndByte);//加上end            
                byte check_sum = CheckSum(Command_Byte.ToArray());//計算check sum
                Command_Byte.Insert(0, StartByte);//加上start
                Command_Byte.Add(check_sum);//加上check sum
                ReceiveState = WaitReply;
                RobotRecQueue.Clear();
                MainCom.Write(Command_Byte.ToArray(), 0, Command_Byte.Count);//送出指令
                Log.Write("Send", Send_String);
            }
            catch (Exception ex)
            {
                Log.Write("Exception", ex.ToString());
            }
        }
        private void DataCombine(byte[] raw_data)
        {
            for (int i = 0; i < raw_data.Length; i++)
            {
                switch (ReceiveState)
                {
                    case Idle://0
                        Log.Write(DeviceName, "收到異常字元:0x" + Convert.ToString(raw_data[i], 16).ToUpper() + " State: Idle");
                        continue;
                    case WaitReply://1
                        if (raw_data[i] != StartByte)
                        {
                            Log.Write(DeviceName, "收到異常字元:0x" + Convert.ToString(raw_data[i], 16).ToUpper() + " State: WaitReply");
                            continue;
                        }
                        Temp.Clear();
                        ReceiveState = Receiving;
                        break;
                    case Receiving://2
                        Temp.Add(raw_data[i]);
                        if (raw_data[i] == EndByte)
                            ReceiveState = WaitCheckSum;
                        break;
                    case WaitCheckSum://3
                        byte Check = CheckSum(Temp.ToArray());
                        string CompleteString = Encoding.ASCII.GetString(Temp.GetRange(0, Temp.Count - 1).ToArray());
                        ReceiveState = Idle;

                        if (raw_data[i] != Check)
                        {
                            Log.Write("Receive", CompleteString + ":drop");
                            Log.Write(DeviceName, "Check sum異常:0x" + Convert.ToString(raw_data[i], 16).ToUpper() + " 應為:0x" + Convert.ToString(Check, 16).ToUpper());
                        }
                        else if (CompleteString.Substring(0, 4) != RobotAddress)
                        {
                            Log.Write("Receive", CompleteString + ":drop");
                            Log.Write(DeviceName, "Robot address異常");
                        }
                        else
                        {
                            Log.Write("Receive", CompleteString);
                            RobotRecQueue.EnQueue(CompleteString.Substring(4));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private byte CheckSum(byte[] data)
        {
            byte return_data = 0x00;
            foreach (byte SingleByte in data)
                return_data ^= SingleByte;
            return return_data;
        }
        private void JobFail()
        {
            Error = true;
            ReceiveState = Idle;
            //RobotCmdQueue.Clear();
            RobotRecQueue.Clear();
            if (Busy)
            {
                RobotCmdMarcoQueue.Clear();
                RobotRecMarcoQueue.Clear();
            }
        }
        private void Status_update(string Status)
        {
            bool[] View = new bool[32];//有問題時可以回來看這裡
            byte[] StatusByte = Encoding.ASCII.GetBytes(Status);
            Array.Reverse(StatusByte);
            BitArray _Status = new BitArray(StatusByte);
            for (int i = 0; i < 32; i++)
            {
                View[i] = _Status[i];
            }
            Remote = _Status[0];
            Moving = _Status[26];

            StatusCode = Status;
        }
        public void COM_Connect()
        {
            Connect = false;
            MainCom.Close();
            MainCom.Open();
            MainComOpen = true;

        }
        public void COM_Disconnect()
        {
            Connect = false;
            MainCom.Close();
        }

        private bool CheckIsEndMotion(int TimeOutHomeMS = 30000)
        {
            bool _result = true;
            int count = 0;
            while (true)
            {
                Thread.Sleep(200);
                count += 200;
                _result &= DataSend_And_WaitReply("LS");

                if (!_result)
                    return false;

                if (!Moving)
                {
                    LastArrived_GP = LastGo_GP;
                    break;
                }

                if (count > TimeOutHomeMS)
                {
                    _result = false;
                    break;
                }
            }

            return !Moving;
        }

        private void backgroundWorker_Robot_COM_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);

            GetStatus(false);

            while (true)
            {
                RobotMacroCommandObject MacroCMD = RobotMacroCommandObjectQueue.DeQueue();

                if (MacroCMD.UserFork == null)
                {
                    Log.Write(DeviceName, "Start to Do Macro " + MacroCMD.Cmd.ToString());
                    Log.WriteSystemLog("Robot", "Start to Do Macro " + MacroCMD.Cmd.ToString());
                }
                else
                {
                    Log.Write(DeviceName, "Start to Do Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                    Log.WriteSystemLog("Robot", "Start to Do Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                }
                bool MarcoResult = true;
                string FailReason = "1";
                int ALID = 0;
                bool WaferDetectDone = true;
                MarcoResult &= DataSend_And_WaitReply("LID0");

                switch (MacroCMD.Cmd)
                {
                    case RobotMacroCommand.SetSpeed:
                        {
                            //"SP " + Move.ToString() + " " + Z_Axis.ToString()
                            MarcoResult &= DataSend_And_WaitReply("SP " + MacroCMD.OtherSpeed + " " + MacroCMD.ZSpeed);
                        }
                        break;
                    case RobotMacroCommand.SmartGet:
                    case RobotMacroCommand.SmartPut:
                    case RobotMacroCommand.SmartGet6S:
                    case RobotMacroCommand.SmartPut6S:
                        {
                            Busy = true;
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;
                            List<int> AddressList = new List<int>();
                            LoadPort _LP = GetLoadPort(MacroCMD.Station.StationName);
                            Fork UseFork;

                            if (_LP != null)
                            {
                                if (!_LP.GetStatus(false, true))
                                {
                                    ALID = 31;
                                    FailReason = _LP.LPName + " Getstatus Error Before GetPutWafer.";
                                    MarcoResult = false;
                                    break;
                                }
                            }

                            #region 1. Check Max Slot Choose Arm & Decide Address

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range.";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                UseFork = LowerArm;
                                if (MacroCMD.Cmd == RobotMacroCommand.SmartGet || MacroCMD.Cmd == RobotMacroCommand.SmartGet6S)
                                    addr = MacroCMD.Station.LowerGetAddress;
                                else if (MacroCMD.Cmd == RobotMacroCommand.SmartPut || MacroCMD.Cmd == RobotMacroCommand.SmartPut6S)
                                    addr = MacroCMD.Station.LowerPutAddress;
                                else
                                {
                                    MarcoResult = false;
                                    break;
                                }
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                UseFork = UpperArm;
                                if (MacroCMD.Cmd == RobotMacroCommand.SmartGet || MacroCMD.Cmd == RobotMacroCommand.SmartGet6S)
                                    addr = MacroCMD.Station.UpperGetAddress;
                                else if (MacroCMD.Cmd == RobotMacroCommand.SmartPut || MacroCMD.Cmd == RobotMacroCommand.SmartPut6S)
                                    addr = MacroCMD.Station.UpperPutAddress;
                                else
                                {
                                    MarcoResult = false;
                                    break;
                                }
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error.";
                                MarcoResult = false;
                                break;
                            }
                            #endregion
                            #region 2. Check Arm Status Wafer/Vaccum/Grip & LP Door is open
                            if (MacroCMD.Cmd == RobotMacroCommand.SmartGet || MacroCMD.Cmd == RobotMacroCommand.SmartGet6S)
                            {
                                if (UseFork.WaferPresent != ForkWaferPresent.Absent)
                                {
                                    ALID = 3;
                                    FailReason = "Arm WaferPresent is not Absent.";
                                    MarcoResult = false;
                                    break;
                                }

                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.Off)
                                    {
                                        ALID = 4;
                                        FailReason = "Arm VacuumStatus is not Off.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.GripStatus != GripStatus.Off)
                                    {
                                        ALID = 5;
                                        FailReason = "Arm GripStatus is not Off.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }

                                if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
                                {
                                    if (_LP.Door_Status != DoorStatus.Open)
                                    {
                                        ALID = 6;
                                        FailReason = _LP.LPName + " Door is not Open.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (_LP.WaferPresent[MacroCMD.Slot - 1] != WaferState.WaferMapped)
                                    {
                                        ALID = 7;
                                        FailReason = _LP.LPName + " Slot " + MacroCMD.Slot + " is not present.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (_LP.WaferProtrusion != LoadPort.WaferProtrusionSensorStatus.Lighting)
                                    {
                                        ALID = 32;
                                        FailReason = _LP.LPName + " WaferProtrusion Error Before GetPutWafer.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }

                                if (MacroCMD.Station.StationName.ToUpper() == "ALIGNER")
                                {
                                    if (Aligner == null)
                                    {
                                        ALID = 8;
                                        FailReason = "Aligner is not Defined.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (Aligner[0].IsWaferPresence != WaferPresence.Present)
                                    {
                                        ALID = 9;
                                        FailReason = "Aligner WaferPresence is not Present.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (Aligner[0].IsVaccumm != Vaccumm.Off)
                                    {
                                        ALID = 10;
                                        FailReason = "Aligner Vaccumm is not Off.";
                                        MarcoResult = false;
                                        break;
                                    }



                                }
                            }

                            if (MacroCMD.Cmd == RobotMacroCommand.SmartPut || MacroCMD.Cmd == RobotMacroCommand.SmartPut6S)
                            {
                                if (UseFork.WaferPresent != ForkWaferPresent.Present)
                                {
                                    ALID = 11;
                                    FailReason = "Arm WaferPresent is not Present.";
                                    MarcoResult = false;
                                    break;
                                }

                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.On)
                                    {
                                        ALID = 12;
                                        FailReason = "Arm VacuumStatus is not On.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.GripStatus != GripStatus.On)
                                    {
                                        ALID = 13;
                                        FailReason = "Arm GripStatus is not On.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }

                                if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
                                {
                                    if (_LP.Door_Status != DoorStatus.Open)
                                    {
                                        ALID = 6;
                                        FailReason = _LP.LPName + " Door is not Open.";
                                        MarcoResult = false;
                                        break;
                                    }
                                    if (_LP.WaferPresent[MacroCMD.Slot - 1] != WaferState.NoWafer)
                                    {
                                        ALID = 14;
                                        FailReason = _LP.LPName + " Slot " + MacroCMD.Slot + " is not absent.";
                                        MarcoResult = false;
                                        break;
                                    }
                                    if (_LP.WaferProtrusion != LoadPort.WaferProtrusionSensorStatus.Lighting)
                                    {
                                        ALID = 32;
                                        FailReason = _LP.LPName + " WaferProtrusion Error Before GetPutWafer.";
                                        MarcoResult = false;
                                        break;
                                    }
                                }

                                if (MacroCMD.Station.StationName.ToUpper() == "ALIGNER")
                                {
                                    if (Aligner == null)
                                    {
                                        ALID = 8;
                                        FailReason = "Aligner is not Defined.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (Aligner[0].IsWaferPresence != WaferPresence.Absent)
                                    {
                                        ALID = 15;
                                        FailReason = "Aligner WaferPresence is not Absent.";
                                        MarcoResult = false;
                                        break;
                                    }

                                    if (!Aligner[0].IsHome)
                                    {
                                        ALID = 16;
                                        FailReason = "Aligner is not home before transfer.";
                                        MarcoResult = false;
                                        break;
                                    }

                                }
                            }
                            #endregion
                            #region 3. Start Move

                            AddressList = MacroCMD.Station.ReplyAddressList(addr, MotionType);

                            if (MacroCMD.Cmd == RobotMacroCommand.SmartGet || MacroCMD.Cmd == RobotMacroCommand.SmartGet6S)
                            {
                                #region A. Standby Extend Up
                                int GetStep = (MotionType == RobotMotionPoint.Snake) ? 3 : 2;
                                for (int i = 0; i <= GetStep; i++)
                                {
                                    //Standby Extend Up
                                    MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                    if (!MarcoResult)
                                    {
                                        ALID = 17; //....
                                        FailReason = "Robot Step Move Error Before Moving(GP " + AddressList[i] + ")";
                                        break;
                                    }
                                    MarcoResult &= CheckIsEndMotion(30000);
                                    if (!MarcoResult)
                                    {
                                        ALID = 18;
                                        FailReason = "Robot Step Move Error After Moving";
                                        break;
                                    }
                                }
                                #endregion

                                #region B. 真空/Grip On
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                    }

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                    }
                                }

                                if (!MarcoResult)
                                {
                                    ALID = 19;
                                    FailReason = "Vaccumm/Grip On Control Error(SOB)";
                                    break;
                                }
                                #endregion

                                #region C. 確認DO是否正確
                                MarcoResult &= DataSend_And_WaitReply("LOD0");
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.On)
                                        MarcoResult = false;

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.GripStatus != GripStatus.On)
                                        MarcoResult = false;
                                }
                                if (!MarcoResult)
                                {
                                    ALID = 20;
                                    FailReason = "Vaccumm/Grip On Control Error(LOD0)";
                                    break;
                                }
                                #endregion

                                #region D. 確認DI狀態(WaferPresent)，若失敗要真空/Grip切Off並確認Wafer Present Off
                                Thread.Sleep(SmartGetDelayms); //一律等1 sec 
                                MarcoResult &= DataSend_And_WaitReply("LID0");

                                //偵測失敗
                                if (UseFork.WaferPresent != ForkWaferPresent.Present)
                                {
                                    WaferDetectDone = false;

                                    if (UseFork.RobotType == RobotType.Vacuum)
                                    {
                                        if (UseFork.RobotArm == RobotArm.LowerArm)
                                        {
                                            MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                            MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                        }
                                        else if (UseFork.RobotArm == RobotArm.UpperArm)
                                        {
                                            MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                            MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                        }
                                    }
                                    else if (UseFork.RobotType == RobotType.Grip)
                                    {
                                        if (UseFork.RobotArm == RobotArm.LowerArm)
                                        {
                                            MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                            MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                        }
                                        else if (UseFork.RobotArm == RobotArm.UpperArm)
                                        {
                                            MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                            MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                        }
                                    }

                                    if (!MarcoResult)
                                    {
                                        ALID = 21;
                                        FailReason = "WaferPresent error and recover error in Vaccumm/Grip Off(SOB)";
                                        break;
                                    }


                                    MarcoResult &= DataSend_And_WaitReply("LOD0");
                                    if (UseFork.RobotType == RobotType.Vacuum)
                                    {
                                        if (UseFork.VacuumStatus != VacuumStatus.Off)
                                            MarcoResult = false;

                                    }
                                    else if (UseFork.RobotType == RobotType.Grip)
                                    {
                                        if (UseFork.GripStatus != GripStatus.Off)
                                            MarcoResult = false;
                                    }

                                    if (!MarcoResult)
                                    {
                                        ALID = 22;
                                        FailReason = "WaferPresent error and recover error in Vaccumm/Grip Off(LOD0)";
                                        break;
                                    }


                                    MarcoResult &= DataSend_And_WaitReply("LID0");
                                    if (UseFork.WaferPresent != ForkWaferPresent.Absent)
                                    {
                                        ALID = 23;
                                        MarcoResult = false;
                                        FailReason = "WaferPresent error and after Vaccumm/Grip Off(LID0)";
                                    }

                                    if (!MarcoResult)
                                        break;
                                }
                                #endregion

                                #region E. Robot 收回
                                if (WaferDetectDone) //正常流程
                                {
                                    int GetStart = (MotionType == RobotMotionPoint.Snake) ? 4 : 3;
                                    GetStep = (MotionType == RobotMotionPoint.Snake) ? 5 : 3;
                                    for (int i = GetStart; i <= GetStep; i++)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                        if (!MarcoResult)
                                        {
                                            ALID = 24;
                                            FailReason = "Robot Step Move Error in retract(GP " + AddressList[i] + ")";
                                            break;
                                        }
                                        MarcoResult &= CheckIsEndMotion(30000);
                                        if (!MarcoResult)
                                        {
                                            ALID = 18;
                                            FailReason = "Robot Step Move Error After Moving";
                                            break;
                                        }
                                    }

                                    //偵測失敗
                                    MarcoResult &= DataSend_And_WaitReply("LID0");
                                    if (UseFork.WaferPresent != ForkWaferPresent.Present)
                                    {
                                        ALID = 25;
                                        MarcoResult = false;
                                        FailReason = "WaferPresent error after retract(LID0)";
                                        break;
                                    }

                                    if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
                                    {

                                        _LP.WaferPresent[MacroCMD.Slot - 1] = WaferState.NoWafer;
                                        _LP.SetLock(false);

                                    }
                                }
                                else // 走失敗流程放回
                                {
                                    int GetStart = (MotionType == RobotMotionPoint.Snake) ? 2 : 1;
                                    for (int i = GetStart; i >= 0; i--)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                        if (!MarcoResult)
                                        {
                                            ALID = 26;
                                            FailReason = "Robot Step Move Error in recover for wafer detect fail(GP " + AddressList[i] + ")";
                                            break;
                                        }
                                        MarcoResult &= CheckIsEndMotion(30000);
                                        if (!MarcoResult)
                                        {
                                            ALID = 18;
                                            FailReason = "Robot Step Move Error After Moving";
                                            break;
                                        }
                                    }
                                    ALID = 27;
                                    FailReason = "Wafer Detect Fail after get(AutoPutBack)";

                                    if (MarcoResult && _LP != null) //雖取失敗但完成動作
                                        _LP.SetLock(false);

                                    MarcoResult = false; //還是算失敗
                                }

                                if (!MarcoResult)
                                    break;

                                if (_LP != null)
                                {
                                    _LP.SetLock(false);

                                    if (_LP.Brand == LoadPort.EFEMBrand.Hirata)
                                        _LP.RefreshWaferSlotUI();
                                }

                                #endregion
                            }
                            else if (MacroCMD.Cmd == RobotMacroCommand.SmartPut || MacroCMD.Cmd == RobotMacroCommand.SmartPut6S)
                            {
                                #region A. Standby Extend
                                int PutStep = (MotionType == RobotMotionPoint.Snake) ? 2 : 1;
                                for (int i = 0; i <= PutStep; i++)
                                {
                                    //Standby Extend
                                    MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                    if (!MarcoResult)
                                    {
                                        ALID = 17;
                                        FailReason = "Robot Step Move Error Before Moving(GP " + AddressList[i] + ")";
                                        break;
                                    }
                                    MarcoResult &= CheckIsEndMotion(30000);
                                    if (!MarcoResult)
                                    {
                                        ALID = 18;
                                        FailReason = "Robot Step Move Error After Moving";
                                        break;
                                    }
                                }

                                if (!MarcoResult)
                                    break;

                                #endregion

                                #region B. 真空/Grip Off
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                    }

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                    }
                                }

                                if (!MarcoResult)
                                {
                                    FailReason = "Vaccumm/Grip Off Control Error(SOB)";
                                    break;
                                }
                                #endregion

                                #region C. 確認DO是否正確
                                MarcoResult &= DataSend_And_WaitReply("LOD0");
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.Off)
                                        MarcoResult = false;

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.GripStatus != GripStatus.Off)
                                        MarcoResult = false;
                                }
                                if (!MarcoResult)
                                {
                                    ALID = 28;
                                    FailReason = "Vaccumm/Grip Off Control Error(LOD0)";
                                    break;
                                }
                                #endregion

                                #region D. Down vaccumm and Retract
                                //for (int i = 2; i <= 3; i++)
                                //{
                                //    //Standby Extend
                                //    MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                //    if (!MarcoResult)
                                //    {
                                //        FailReason = "Robot Step Move Error Before Moving(GP " + AddressList[i] + ")";
                                //        break;
                                //    }
                                //    MarcoResult &= CheckIsEndMotion(30000);
                                //    if (!MarcoResult)
                                //    {
                                //        FailReason = "Robot Step Move Error After Moving";
                                //        break;
                                //    }
                                //}

                                //Place
                                int PutPlaceStep = (MotionType == RobotMotionPoint.Snake) ? 3 : 2;
                                MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[PutPlaceStep] + z_str);
                                if (!MarcoResult)
                                {
                                    ALID = 17;
                                    FailReason = "Robot Step Move Error Before Moving(GP " + AddressList[PutPlaceStep] + ")";
                                    break;
                                }
                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                {
                                    ALID = 18;
                                    FailReason = "Robot Step Move Error After Moving";
                                    break;
                                }

                                //真空吸吸看
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                    }

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                    }
                                }

                                if (!MarcoResult)
                                {
                                    ALID = 19;
                                    FailReason = "Vaccumm/Grip On Control Error(SOB)";
                                    break;
                                }

                                //確定有再吸
                                MarcoResult &= DataSend_And_WaitReply("LOD0");
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.On)
                                        MarcoResult = false;

                                }
                                else if (UseFork.RobotType == RobotType.Grip)
                                {
                                    if (UseFork.GripStatus != GripStatus.On)
                                        MarcoResult = false;
                                }
                                if (!MarcoResult)
                                {
                                    ALID = 20;
                                    FailReason = "Vaccumm/Grip On Control Error(LOD0)";
                                    break;
                                }

                                Thread.Sleep(1000); //一律等0.5 sec
                                MarcoResult &= DataSend_And_WaitReply("LID0");

                                //偵測失敗
                                if (UseFork.WaferPresent != ForkWaferPresent.Absent)
                                {
                                    MarcoResult = false;
                                    ALID = 29;
                                    FailReason = "Detect Wafer Still On Fork";
                                    break;
                                }


                                #region  真空/Grip Off
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.RobotArm == RobotArm.LowerArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                    }
                                    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                    {
                                        MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                        MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                    }

                                }
                                //else if (UseFork.RobotType == RobotType.Grip)
                                //{
                                //    if (UseFork.RobotArm == RobotArm.LowerArm)
                                //    {
                                //        MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                //        MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                //    }
                                //    else if (UseFork.RobotArm == RobotArm.UpperArm)
                                //    {
                                //        MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                //        MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                //    }
                                //}

                                if (!MarcoResult)
                                {
                                    ALID = 30;
                                    FailReason = "Vaccumm/Grip Off Control Error(SOB)";
                                    break;
                                }
                                #endregion

                                #region C. 確認DO是否正確
                                MarcoResult &= DataSend_And_WaitReply("LOD0");
                                if (UseFork.RobotType == RobotType.Vacuum)
                                {
                                    if (UseFork.VacuumStatus != VacuumStatus.Off)
                                        MarcoResult = false;

                                }
                                //else if (UseFork.RobotType == RobotType.Grip)
                                //{
                                //    if (UseFork.GripStatus != GripStatus.Off)
                                //        MarcoResult = false;
                                //}
                                if (!MarcoResult)
                                {
                                    ALID = 28;
                                    FailReason = "Vaccumm/Grip Off Control Error(LOD0)";
                                    break;
                                }
                                #endregion

                                int PutStart = (MotionType == RobotMotionPoint.Snake) ? 4 : 3;
                                PutStep = (MotionType == RobotMotionPoint.Snake) ? 5 : 3;
                                for (int i = PutStart; i <= PutStep; i++)
                                {
                                    MarcoResult &= DataSend_And_WaitReply("GP " + AddressList[i] + z_str);
                                    if (!MarcoResult)
                                    {
                                        ALID = 17;
                                        FailReason = "Robot Step Move Error Before Moving(GP " + AddressList[i] + ")";
                                        break;
                                    }
                                    MarcoResult &= CheckIsEndMotion(30000);
                                    if (!MarcoResult)
                                    {
                                        ALID = 18;
                                        FailReason = "Robot Step Move Error After Moving";
                                        break;
                                    }
                                }


                                if (!MarcoResult)
                                    break;

                                if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
                                {
                                    _LP.WaferPresent[MacroCMD.Slot - 1] = WaferState.WaferMapped;
                                    _LP.SetLock(false);

                                    if (_LP.Brand == LoadPort.EFEMBrand.Hirata)
                                        _LP.RefreshWaferSlotUI();
                                }

                                UseFork.WaferPresent = ForkWaferPresent.Absent;

                                #endregion
                            }
                            #endregion
                            Busy = false;
                        }
                        break;
                    case RobotMacroCommand.ReadPosition:
                        {
                            MarcoResult &= DataSend_And_WaitReply("LR");
                            if (!MarcoResult)
                                break;
                        }
                        break;
                    case RobotMacroCommand.ResetError:
                        {
                            MarcoResult &= DataSend_And_WaitReply("CL");
                            if (!MarcoResult)
                                break;
                        }
                        break;

                    case RobotMacroCommand.Home: //Done 
                        {
                            Busy = true;

                            //開始吸一口偵測
                            if (LowerArm.RobotType == RobotType.Vacuum)
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                if (MarcoResult)
                                    LowerArm.VacuumStatus = VacuumStatus.On;
                                else
                                    LowerArm.VacuumStatus = VacuumStatus.Unknown;

                                Thread.Sleep(1000);
                                MarcoResult &= DataSend_And_WaitReply("LID0");

                                //沒片的話解真空
                                if (LowerArm.WaferPresent == ForkWaferPresent.Absent)
                                {
                                    MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                    MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                    if (MarcoResult)
                                        LowerArm.VacuumStatus = VacuumStatus.Off;
                                    else
                                        LowerArm.VacuumStatus = VacuumStatus.Unknown;
                                }

                                if (!MarcoResult)
                                    break;
                            }

                            if ((UpperArm != null) && (UpperArm.RobotType == RobotType.Vacuum))
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                if (MarcoResult)
                                    UpperArm.VacuumStatus = VacuumStatus.On;
                                else
                                    UpperArm.VacuumStatus = VacuumStatus.Unknown;

                                Thread.Sleep(1000);
                                MarcoResult &= DataSend_And_WaitReply("LID0");

                                //沒片的話解真空
                                if (UpperArm.WaferPresent == ForkWaferPresent.Absent)
                                {
                                    MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                    MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                    if (MarcoResult)
                                        UpperArm.VacuumStatus = VacuumStatus.Off;
                                    else
                                        UpperArm.VacuumStatus = VacuumStatus.Unknown;
                                }

                                if (!MarcoResult)
                                    break;

                            }
                            //結束吸一口偵測

                            if (MotionType == RobotMotionPoint.Snake)
                            {
                                SnakeHomeType = "";
                                MarcoResult &= DataSend_And_WaitReply("LBD");
                                if (!MarcoResult)
                                    break;

                                switch (SnakeHomeType)
                                {
                                    case "0":
                                    case "4":
                                    case "8":
                                    case "12":
                                    case "16":
                                        break;
                                    case "1":
                                    case "2":
                                        MarcoResult &= DataSend_And_WaitReply("GP 1");
                                        break;
                                    case "5":
                                    case "6":
                                        MarcoResult &= DataSend_And_WaitReply("GP 3");
                                        break;
                                    case "9":
                                    case "10":
                                        MarcoResult &= DataSend_And_WaitReply("GP 4");
                                        break;
                                    case "13":
                                    case "14":
                                        MarcoResult &= DataSend_And_WaitReply("GP 2");
                                        break;
                                    case "17":
                                    case "18":
                                        MarcoResult &= DataSend_And_WaitReply("GP 5");
                                        break;
                                    default:
                                        MarcoResult = false;
                                        break;
                                }

                                if (!MarcoResult)
                                    break;
                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                    break;
                            }

                            MarcoResult &= DataSend_And_WaitReply("GP 1");
                            if (!MarcoResult)
                                break;

                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                                break;

                            MarcoResult &= DataSend_And_WaitReply("GP 0");
                            if (!MarcoResult)
                                break;

                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                                break;

                            for (int i = 0; i < LP.Count; i++)
                                LP[i].SetLock(false);

                            GetStatus(true);    // 做Home 自動上拋狀態

                            Busy = false;
                        }
                        break;
                    //
                    case RobotMacroCommand.GetStandby:
                        {
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;

                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                addr = MacroCMD.Station.LowerGetAddress;
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                addr = MacroCMD.Station.UpperGetAddress;
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;

                            MarcoResult &= DataSend_And_WaitReply(cmd);

                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }
                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;
                        }
                        break;
                    case RobotMacroCommand.PutStandby:
                        {

                            //001 GP 880 ( 0 0 10 0)
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                                addr = MacroCMD.Station.LowerPutAddress;
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                                addr = MacroCMD.Station.UpperPutAddress;
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;
                            MarcoResult &= DataSend_And_WaitReply(cmd);
                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }

                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;

                        }
                        break;
                    //
                    case RobotMacroCommand.GetExtend:
                        {
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;

                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                                addr = MacroCMD.Station.LowerGetAddress + 1;
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                                addr = MacroCMD.Station.UpperGetAddress + 1;
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;

                            MarcoResult &= DataSend_And_WaitReply(cmd);

                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }
                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;

                            if (MotionType == RobotMotionPoint.Snake)
                            {
                                Busy = true;
                                cmd = "GP " + (addr + 1) + z_str;

                                MarcoResult &= DataSend_And_WaitReply(cmd);

                                if (!MarcoResult)
                                {
                                    ALID = 17;
                                    FailReason = "Robot Step Move Error Before Moving";
                                    break;
                                }
                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                {
                                    ALID = 18;
                                    FailReason = "Robot Step Move Error After Moving";
                                    break;
                                }

                                Busy = false;
                            }
                        }
                        break;
                    case RobotMacroCommand.PutExtend:
                        {
                            //001 GP 880 ( 0 0 10 0)
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                                addr = MacroCMD.Station.LowerPutAddress + 1;
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                                addr = MacroCMD.Station.UpperPutAddress + 1;
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;
                            MarcoResult &= DataSend_And_WaitReply(cmd);
                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }

                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;

                            if (MotionType == RobotMotionPoint.Snake)
                            {
                                Busy = true;
                                cmd = "GP " + (addr + 1) + z_str;

                                MarcoResult &= DataSend_And_WaitReply(cmd);

                                if (!MarcoResult)
                                {
                                    ALID = 17;
                                    FailReason = "Robot Step Move Error Before Moving";
                                    break;
                                }
                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                {
                                    ALID = 18;
                                    FailReason = "Robot Step Move Error After Moving";
                                    break;
                                }

                                Busy = false;
                            }
                        }
                        break;
                    //
                    case RobotMacroCommand.GetPick:
                        {
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;

                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                if (MotionType == RobotMotionPoint.Snake)
                                    addr = MacroCMD.Station.LowerGetAddress + 3;
                                else
                                    addr = MacroCMD.Station.LowerGetAddress + 2;
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                if (MotionType == RobotMotionPoint.Snake)
                                    addr = MacroCMD.Station.UpperGetAddress + 3;
                                else
                                    addr = MacroCMD.Station.UpperGetAddress + 2;
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;

                            MarcoResult &= DataSend_And_WaitReply(cmd);

                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }
                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;
                        }
                        break;
                    case RobotMacroCommand.PutPlace:
                        {

                            //001 GP 880 ( 0 0 10 0)
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                if (MotionType == RobotMotionPoint.Snake)
                                    addr = MacroCMD.Station.LowerPutAddress + 3;
                                else
                                    addr = MacroCMD.Station.LowerPutAddress + 2;
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                if (MotionType == RobotMotionPoint.Snake)
                                    addr = MacroCMD.Station.UpperPutAddress + 3;
                                else
                                    addr = MacroCMD.Station.UpperPutAddress + 2;
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            Busy = true;
                            string cmd = "GP " + addr + z_str;
                            MarcoResult &= DataSend_And_WaitReply(cmd);
                            if (!MarcoResult)
                            {
                                ALID = 17;
                                FailReason = "Robot Step Move Error Before Moving";
                                break;
                            }

                            MarcoResult &= CheckIsEndMotion(30000);
                            if (!MarcoResult)
                            {
                                ALID = 18;
                                FailReason = "Robot Step Move Error After Moving";
                                break;
                            }

                            Busy = false;

                        }
                        break;
                    //
                    case RobotMacroCommand.GetRetract:
                        {
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            int pos = LastArrived_GP % 10;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                addr = MacroCMD.Station.LowerGetAddress;
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                addr = MacroCMD.Station.UpperGetAddress;
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            int startStep = 0;
                            int finalStep = 0;

                            if (MotionType == RobotMotionPoint.Snake)
                            {
                                if (pos == 2)
                                {
                                    startStep = 3;
                                    finalStep = 5;
                                }
                                else if (pos == 3)
                                {
                                    startStep = 4;
                                    finalStep = 5;
                                }
                                else
                                {
                                    ALID = 3;
                                    FailReason = "Robot Position Error";
                                    MarcoResult = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (pos == 1)
                                {
                                    startStep = 2;
                                    finalStep = 3;
                                }
                                else if (pos == 2)
                                {
                                    startStep = 3;
                                    finalStep = 3;
                                }
                                else
                                {
                                    ALID = 3;
                                    FailReason = "Robot Position Error";
                                    MarcoResult = false;
                                    break;
                                }
                            }

                            for (int i = startStep; i <= finalStep; i++)
                            {
                                Busy = true;
                                string cmd = "GP " + (addr + i).ToString() + z_str;

                                MarcoResult &= DataSend_And_WaitReply(cmd);
                                if (!MarcoResult)
                                {
                                    ALID = 17;
                                    FailReason = "Robot Step Move Error Before Moving";
                                    break;
                                }

                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                {
                                    ALID = 18;
                                    FailReason = "Robot Step Move Error After Moving";
                                    break;
                                }
                                Busy = false;
                            }
                        }
                        break;
                    case RobotMacroCommand.PutRetract:
                        {
                            //001 GP 880 ( 0 0 10 0)
                            int z = (MacroCMD.Slot - 1) * MacroCMD.Station.SlotPitch;
                            string z_str = " ( 0 0 " + z + " 0)";
                            int addr;

                            int pos = LastArrived_GP % 10;

                            if (MacroCMD.Slot > MacroCMD.Station.MaxSlot | MacroCMD.Slot <= 0)
                            {
                                ALID = 1;
                                FailReason = "Slot is out of range";
                                MarcoResult = false;
                                break;
                            }

                            if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                            {
                                addr = MacroCMD.Station.LowerPutAddress;
                            }
                            else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                            {
                                addr = MacroCMD.Station.UpperPutAddress;
                            }
                            else
                            {
                                ALID = 2;
                                FailReason = "Arm select is error";
                                MarcoResult = false;
                                break;
                            }

                            int startStep = 0;
                            int finalStep = 0;

                            if (MotionType == RobotMotionPoint.Snake)
                            {
                                if (pos == 2)
                                {
                                    startStep = 3;
                                    finalStep = 5;
                                }
                                else if (pos == 3)
                                {
                                    startStep = 4;
                                    finalStep = 5;
                                }
                                else
                                {
                                    ALID = 3;
                                    FailReason = "Robot Position Error";
                                    MarcoResult = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (pos == 1)
                                {
                                    startStep = 2;
                                    finalStep = 3;
                                }
                                else if (pos == 2)
                                {
                                    startStep = 3;
                                    finalStep = 3;
                                }
                                else
                                {
                                    ALID = 3;
                                    FailReason = "Robot Position Error";
                                    MarcoResult = false;
                                    break;
                                }
                            }

                            for (int i = startStep; i <= finalStep; i++)
                            {
                                Busy = true;
                                string cmd = "GP " + (addr + i).ToString() + z_str;
                                MarcoResult &= DataSend_And_WaitReply(cmd);
                                if (!MarcoResult)
                                {
                                    ALID = 17;
                                    FailReason = "Robot Step Move Error Before Moving";
                                    break;
                                }

                                MarcoResult &= CheckIsEndMotion(30000);
                                if (!MarcoResult)
                                {
                                    ALID = 18;
                                    FailReason = "Robot Step Move Error After Moving";
                                    break;
                                }
                                Busy = false;
                            }
                        }
                        break;
                    //
                    case RobotMacroCommand.EdgeGripOn:
                    case RobotMacroCommand.VacuumOn:
                        {
                            Busy = true;
                            if (MacroCMD.UserFork == LowerArm)
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                MarcoResult &= DataSend_And_WaitReply("SOB1 0");

                                if (MacroCMD.UserFork.RobotType == RobotType.Vacuum)
                                {
                                    if (MarcoResult)
                                        LowerArm.VacuumStatus = VacuumStatus.On;
                                    else
                                        LowerArm.VacuumStatus = VacuumStatus.Unknown;
                                }

                                if (MacroCMD.UserFork.RobotType == RobotType.Grip)
                                {
                                    if (MarcoResult)
                                        LowerArm.GripStatus = GripStatus.On;
                                    else
                                        LowerArm.GripStatus = GripStatus.Unknown;
                                }

                            }
                            else if (MacroCMD.UserFork == UpperArm)
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                MarcoResult &= DataSend_And_WaitReply("SOB3 0");

                                if (MacroCMD.UserFork.RobotType == RobotType.Vacuum)
                                {
                                    if (MarcoResult)
                                        UpperArm.VacuumStatus = VacuumStatus.On;
                                    else
                                        UpperArm.VacuumStatus = VacuumStatus.Unknown;
                                }

                                if (MacroCMD.UserFork.RobotType == RobotType.Grip)
                                {
                                    if (MarcoResult)
                                        UpperArm.GripStatus = GripStatus.On;
                                    else
                                        UpperArm.GripStatus = GripStatus.Unknown;
                                }
                            }

                            Thread.Sleep(1000);
                            MarcoResult &= DataSend_And_WaitReply("LID0");
                            //RefreshForkStatus();
                            Busy = false;
                        }
                        break;
                    case RobotMacroCommand.EdgeGripOff:
                    case RobotMacroCommand.VacuumOff:
                        {
                            Busy = true;
                            if (MacroCMD.UserFork == LowerArm)
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                if (MarcoResult)
                                    LowerArm.VacuumStatus = VacuumStatus.Off;
                                else
                                    LowerArm.VacuumStatus = VacuumStatus.Unknown;
                            }
                            else if (MacroCMD.UserFork == UpperArm)
                            {
                                MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                if (MarcoResult)
                                    UpperArm.VacuumStatus = VacuumStatus.Off;
                                else
                                    UpperArm.VacuumStatus = VacuumStatus.Unknown;
                            }
                            else
                            {
                                FailReason = "Use wrong arm.";
                                MarcoResult = false;
                            }
                            Thread.Sleep(1000);
                            //RefreshForkStatus();
                            Busy = false;
                        }
                        break;
                    case RobotMacroCommand.GetStatus:
                        {
                            MarcoResult &= DataSend_And_WaitReply("LS");
                            MarcoResult &= DataSend_And_WaitReply("LR");
                            MarcoResult &= DataSend_And_WaitReply("LOD0");

                            //開始吸一口偵測
                            if (LowerArm.RobotType == RobotType.Vacuum)
                            {
                                if (LowerArm.VacuumStatus != VacuumStatus.On)
                                {
                                    MarcoResult &= DataSend_And_WaitReply("SOB0 1");
                                    MarcoResult &= DataSend_And_WaitReply("SOB1 0");
                                    if (MarcoResult)
                                        LowerArm.VacuumStatus = VacuumStatus.On;
                                    else
                                        LowerArm.VacuumStatus = VacuumStatus.Unknown;

                                    Thread.Sleep(1000);
                                    MarcoResult &= DataSend_And_WaitReply("LID0");

                                    MarcoResult &= DataSend_And_WaitReply("SOB0 0");
                                    MarcoResult &= DataSend_And_WaitReply("SOB1 1");
                                    if (MarcoResult)
                                        LowerArm.VacuumStatus = VacuumStatus.Off;
                                    else
                                        LowerArm.VacuumStatus = VacuumStatus.Unknown;
                                }
                            }

                            if ((UpperArm != null) && (UpperArm.RobotType == RobotType.Vacuum))
                            {
                                if (UpperArm.VacuumStatus != VacuumStatus.On)
                                {
                                    MarcoResult &= DataSend_And_WaitReply("SOB2 1");
                                    MarcoResult &= DataSend_And_WaitReply("SOB3 0");
                                    if (MarcoResult)
                                        UpperArm.VacuumStatus = VacuumStatus.On;
                                    else
                                        UpperArm.VacuumStatus = VacuumStatus.Unknown;

                                    Thread.Sleep(1000);
                                    MarcoResult &= DataSend_And_WaitReply("LID0");

                                    MarcoResult &= DataSend_And_WaitReply("SOB2 0");
                                    MarcoResult &= DataSend_And_WaitReply("SOB3 1");
                                    if (MarcoResult)
                                        UpperArm.VacuumStatus = VacuumStatus.Off;
                                    else
                                        UpperArm.VacuumStatus = VacuumStatus.Unknown;
                                }
                            }
                            //結束吸一口偵測

                            if ((LowerArm.RobotType == RobotType.Contact) | ((UpperArm != null) && (UpperArm.RobotType == RobotType.Contact)))
                            {
                                MarcoResult &= DataSend_And_WaitReply("LID0");
                            }

                            MarcoResult &= DataSend_And_WaitReply("LID1");
                            //RefreshForkStatus();
                        }
                        break;
                }

                if (MarcoResult)
                {
                    if (MacroCMD.UserFork == null)
                    {
                        Log.Write(DeviceName, "Finish Macro " + MacroCMD.Cmd.ToString());
                        Log.WriteSystemLog(DeviceName, "Finish Macro " + MacroCMD.Cmd.ToString());
                    }
                    else
                    {
                        Log.Write(DeviceName, "Finish Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                        Log.WriteSystemLog(DeviceName, "Finish Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                    }
                }
                else
                {
                    if (MacroCMD.UserFork == null)
                    {
                        Log.Write(DeviceName, "Fail Macro " + MacroCMD.Cmd.ToString());
                        Log.WriteSystemLog(DeviceName, "Fail Macro " + MacroCMD.Cmd.ToString());
                    }
                    else
                    {
                        Log.Write(DeviceName, "Fail Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                        Log.WriteSystemLog(DeviceName, "Fail Macro " + MacroCMD.Cmd.ToString() + " by " + MacroCMD.UserFork.RobotArm.ToString());
                    }
                    //Error = true;
                }

                RefreshForkStatus();

                if (MacroCMD.Cmd == RobotMacroCommand.SmartGet || MacroCMD.Cmd == RobotMacroCommand.SmartGet6S || MacroCMD.Cmd == RobotMacroCommand.SmartPut || MacroCMD.Cmd == RobotMacroCommand.SmartPut6S)
                {
                    if (MacroCMD.Station.StationName.ToUpper() == "ALIGNER" && Aligner != null)
                    {
                        Aligner[0].GetStatus(false);

                        int count = 0;
                        while (!Aligner[0].GetStatusDone)
                        {
                            count += 50;
                            Thread.Sleep(50);
                            if (count > 10000)
                                break;
                        }
                    }
                }

                if (MacroCMD.FromSocket) //-- 暫時測試用註解
                {
                    switch (MacroCMD.Cmd)
                    {

                        case RobotMacroCommand.SetSpeed:
                            {
                                string ack = DeviceName + "Speed," + MacroCMD.ZSpeed + "," + MacroCMD.OtherSpeed + ",";

                                if (MarcoResult)
                                    ack += "0";
                                else
                                    ack += FailReason;

                                if (MarcoResult)
                                {
                                    XML.ReWrite("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Speed", "value", MacroCMD.OtherSpeed.ToString());
                                    XML.ReWrite("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Z_Speed", "value", MacroCMD.ZSpeed.ToString());
                                }

                                SendOutSocket(ack);
                            }
                            break;
                        case RobotMacroCommand.ResetError:
                        case RobotMacroCommand.Home:
                            {
                                if (MarcoResult)
                                    SendOutSocket(MacroCMD.Cmd.ToString() + "," + DeviceName + ",0");
                                else
                                    SendOutSocket(MacroCMD.Cmd.ToString() + "," + DeviceName + "," + FailReason);
                            }
                            break;
                        case RobotMacroCommand.GetStatus:
                            {
                                //GetStatus,Robot,0601,0,0
                                string ack;
                                if (MarcoResult)
                                {
                                    ack = "GetStatus," + DeviceName + "," + StatusCode + ",";
                                    if (UpperArm == null)
                                        ack += "9,";
                                    else
                                        ack += ((int)UpperArm.WaferPresent) + ",";
                                    ack += ((int)LowerArm.WaferPresent);
                                    SendOutSocket(ack);
                                }
                                else
                                    ack = "GetStatus," + DeviceName + "," + StatusCode + ",9,9";

                            }
                            break;
                        case RobotMacroCommand.EdgeGripOn:
                        case RobotMacroCommand.EdgeGripOff:
                        case RobotMacroCommand.VacuumOn:
                        case RobotMacroCommand.VacuumOff:
                            {
                                string ack = MacroCMD.Cmd.ToString() + "," + DeviceName + ",";
                                if (MacroCMD.UserFork.RobotArm == RobotArm.LowerArm)
                                    ack += "1,";
                                else if (MacroCMD.UserFork.RobotArm == RobotArm.UpperArm)
                                    ack += "2,";

                                if (MarcoResult)
                                    ack += "0";
                                else
                                    ack += FailReason;

                                SendOutSocket(ack);
                            }
                            break;
                        case RobotMacroCommand.GetStandby:
                        case RobotMacroCommand.PutStandby:
                        case RobotMacroCommand.GetExtend:
                        case RobotMacroCommand.PutExtend:
                        case RobotMacroCommand.GetPick:
                        case RobotMacroCommand.PutPlace:
                        case RobotMacroCommand.GetRetract:
                        case RobotMacroCommand.PutRetract:
                        case RobotMacroCommand.SmartGet:
                        case RobotMacroCommand.SmartPut:
                        case RobotMacroCommand.SmartGet6S:
                        case RobotMacroCommand.SmartPut6S:
                            {
                                string ack = MacroCMD.Cmd.ToString() + "," + DeviceName + ",";


                                if (MarcoResult)
                                    ack += "0";
                                else
                                    ack += FailReason;

                                SendOutSocket(ack);
                            }
                            break;

                    }

                    if ((Enable_Alarm) & (MarcoResult == false))
                    {
                        try
                        {
                            ALID += RobotAlarmOffset;
                            if (MacroCMD.UserFork.RobotArm != RobotArm.UnknownArm)
                                SendOutSocket("Alm," + DeviceName + "," + ALID + "," + FailReason + "," + MacroCMD.UserFork.RobotArm);
                            else
                                SendOutSocket("Alm," + DeviceName + "," + ALID + "," + FailReason);
                        }
                        catch
                        {

                        }
                    }
                }
                Thread.Sleep(10);
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

        int _LastGo_GP = -10;
        int LastGo_GP
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { LastGo_GP = value; }));
                    return;
                }
                _LastGo_GP = value;
                label_ArrivedPoint.Text = value.ToString();
            }
            get
            {
                return _LastGo_GP;
            }
        }

        int _LastArrived_GP = -10;
        int LastArrived_GP
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate() { LastArrived_GP = value; }));
                    return;
                }
                _LastArrived_GP = value;
                label_GoPoint.Text = value.ToString();
            }
            get
            {
                return _LastArrived_GP;
            }
        }

        private bool NeedHomeFirst()
        {
            if (LastGo_GP == -10 && LastArrived_GP == -10)
            {
                return true; //剛開
            }

            return false;
        }

        private bool CheckIsSave(RobotMacroCommand Cmd)
        {
            if (LastGo_GP == -10 && LastArrived_GP == -10)
            {
                return false; //剛開
            }

            if (LastArrived_GP == -1)
            {
                return false; //沒走成功
            }

            if (LastGo_GP == LastArrived_GP)
            {
                int pos = LastArrived_GP % 10;

                if (Cmd == RobotMacroCommand.GetStandby
                 || Cmd == RobotMacroCommand.SmartGet
                 || Cmd == RobotMacroCommand.SmartGet6S
                 || Cmd == RobotMacroCommand.PutStandby
                 || Cmd == RobotMacroCommand.SmartPut
                 || Cmd == RobotMacroCommand.SmartPut6S)
                {
                    if (MotionType == RobotMotionPoint.Snake)
                    {
                        if (pos == 0 || pos == 5)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (pos == 0 || pos == 3)
                            return true;
                        else
                            return false;
                    }
                }
                if (Cmd == RobotMacroCommand.GetExtend || Cmd == RobotMacroCommand.PutExtend)
                {
                    if (pos == 0)
                        return true;
                    else
                        return false;
                }
                if (Cmd == RobotMacroCommand.GetPick || Cmd == RobotMacroCommand.PutPlace)
                {
                    if (MotionType == RobotMotionPoint.Snake)
                    {
                        if (pos == 2)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (pos == 1)
                            return true;
                        else
                            return false;
                    }
                }
                if (Cmd == RobotMacroCommand.GetRetract || Cmd == RobotMacroCommand.PutRetract)
                {
                    if (MotionType == RobotMotionPoint.Snake)
                    {
                        if (pos == 2 || pos == 3)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (pos == 1 || pos == 2)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return false;
        }

        //private bool CheckCanDoHome()
        //{

        //    if (LastGo_GP == -10 & LastArrived_GP == -10)
        //    {
        //        return true;
        //    }


        //    return false;

        //}

        Dictionary<string, string> Robot_ErrorCode = new Dictionary<string, string>();

        private void InitRobotErrorCode()
        {
            Robot_ErrorCode.Add("00", "Normal status (No error)");
            Robot_ErrorCode.Add("09", "Positioning error (Refer to the error axis information.)");
            Robot_ErrorCode.Add("10", "Emergency stop status");
            Robot_ErrorCode.Add("20", "A-CAL does not end normally. (Refer to the error axis information.)");
            Robot_ErrorCode.Add("30", "Address is outside the limit.");
            Robot_ErrorCode.Add("31", "Positioning to END(??) point");
            Robot_ErrorCode.Add("40", "Position data is outside the limit area. (Refer to the error axis information.)");
            Robot_ErrorCode.Add("51", "Robot is in overrun status. (Refer to the error axis information.)");
            Robot_ErrorCode.Add("60", "Communication format error");
            Robot_ErrorCode.Add("61", "Communication command error");
            Robot_ErrorCode.Add("62", "Command which cannot be executed in current mode is received.");
            Robot_ErrorCode.Add("63", "System data (SG, SP) is destroyed.");
            Robot_ErrorCode.Add("64", "Position data is destroyed.");
            Robot_ErrorCode.Add("70", "Encoder battery is lowered.");
            Robot_ErrorCode.Add("80", "Another command is received during processing of command.");
            Robot_ErrorCode.Add("82", "Sensor input error (During sensor stop function is activated)");
            Robot_ErrorCode.Add("90", "Operating before A-CAL completion");
            Robot_ErrorCode.Add("96", "Positioning error (PASS PTP)");
            Robot_ErrorCode.Add("A0", "Driver error (Refer to the error axis information.)");
            Robot_ErrorCode.Add("B0", "Servo cannot be locked.");
        }
        private bool DataSend_And_WaitReply(string cmd)
        {

            string rec = "";
            DataSend(cmd);

            if (cmd.Substring(0, 2) == "GP")
            {
                LastArrived_GP = -1;
                string[] token = cmd.Split(' ');
                LastGo_GP = Convert.ToInt32(token[1]);
            }

            int i = 0;

            for (i = 0; i < 100; i++)
            {
                Thread.Sleep(10);
                if (RobotRecQueue == null)
                    continue;
                if (RobotRecQueue.Count() == 0)
                    continue;
                rec = RobotRecQueue.DeQueue();
                RobotRecQueue.Clear();
                break;
            }
            if (i >= 100)
            {
                Log.Write(DeviceName, "Robot 回應逾時(Timeout):" + cmd);
                //Connect = false;//判斷連線中斷
                JobFail();
                return false;
            }

            if (rec.Contains('E'))
            {
                Status_update(rec.Substring(1, 4));
                string ErrorCode = rec.Substring(5, 2);

                try
                {
                    Log.Write(DeviceName, "Robot 回應錯誤訊息:" + ErrorCode + ";Description:" + Robot_ErrorCode[ErrorCode]);//這邊補上從Hardcode找錯誤訊息
                }
                catch
                {
                    Log.Write(DeviceName, "Robot 回應錯誤訊息:" + ErrorCode + ";Description: Unknown ErrorCode");//這邊補上從Hardcode找錯誤訊息
                }
                JobFail();
                return false;
            }

            ReceiveHandlerNew(cmd, rec);
            return true;
        }
        private void ReceiveHandlerNew(string CMD, string REC)//理論上沒Error才會到這邊
        {
            try
            {
                string[] CMD_Token = CMD.Split(new string[] { " " }, StringSplitOptions.None);
                string[] REC_Token = REC.Split(new string[] { " " }, StringSplitOptions.None);
                BitArray IN;
                BitArray OUT;
                switch (CMD_Token[0])
                {
                    case "LS":
                        Status_update(REC_Token[0].Substring(0, 4));
                        break;
                    case "LOD0":
                        {
                            OUT = new BitArray(new byte[] { Convert.ToByte(REC_Token[0]) });

                            switch (LowerArm.RobotType)
                            {
                                case RobotType.Vacuum:
                                    if (!OUT[0] && OUT[1])
                                        LowerArm.VacuumStatus = VacuumStatus.Off;
                                    else if (OUT[0] && !OUT[1])
                                        LowerArm.VacuumStatus = VacuumStatus.On;
                                    else
                                        LowerArm.VacuumStatus = VacuumStatus.Unknown;

                                    if (UpperArm != null)
                                    {
                                        if (!OUT[2] && OUT[3])
                                            UpperArm.VacuumStatus = VacuumStatus.Off;
                                        else if (OUT[2] && !OUT[3])
                                            UpperArm.VacuumStatus = VacuumStatus.On;
                                        else
                                            UpperArm.VacuumStatus = VacuumStatus.Unknown;
                                    }
                                    break;
                                case RobotType.Grip:
                                    if (!OUT[0] && OUT[1])
                                        LowerArm.GripStatus = GripStatus.Off;
                                    else if (OUT[0] && !OUT[1])
                                        LowerArm.GripStatus = GripStatus.On;
                                    else
                                        LowerArm.GripStatus = GripStatus.Unknown;

                                    if (UpperArm != null)
                                    {
                                        if (!OUT[2] && OUT[3])
                                            UpperArm.GripStatus = GripStatus.Off;
                                        else if (OUT[2] && !OUT[3])
                                            UpperArm.GripStatus = GripStatus.On;
                                        else
                                            UpperArm.GripStatus = GripStatus.Unknown;
                                    }
                                    break;

                            }
                        }
                        break;
                    case "LID0":
                        IN = new BitArray(new byte[] { Convert.ToByte(REC_Token[0]) });

                        switch (LowerArm.RobotType)
                        {
                            case RobotType.Vacuum:
                                if (LowerArm.VacuumStatus == VacuumStatus.On)
                                {
                                    if (IN[0])
                                        LowerArm.WaferPresent = ForkWaferPresent.Absent;
                                    else
                                        LowerArm.WaferPresent = ForkWaferPresent.Present;
                                }
                                break;
                            case RobotType.Contact:
                                {
                                    if (IN[0] && IN[1])
                                        LowerArm.WaferPresent = ForkWaferPresent.Absent;
                                    else if ((!IN[0]) && (!IN[1]))
                                        LowerArm.WaferPresent = ForkWaferPresent.Present;
                                    else
                                        LowerArm.WaferPresent = ForkWaferPresent.Unknown;
                                }
                                break;
                            case RobotType.Grip:
                                break;
                        }

                        if (UpperArm != null)
                        {
                            switch (UpperArm.RobotType)
                            {
                                case RobotType.Vacuum:
                                    if (UpperArm.VacuumStatus == VacuumStatus.On)
                                    {
                                        if (IN[2])
                                            UpperArm.WaferPresent = ForkWaferPresent.Absent;
                                        else
                                            UpperArm.WaferPresent = ForkWaferPresent.Present;
                                    }
                                    break;
                                case RobotType.Contact:
                                    if (IN[2] && IN[3])
                                        UpperArm.WaferPresent = ForkWaferPresent.Absent;
                                    else if ((!IN[2]) && (!IN[3]))
                                        UpperArm.WaferPresent = ForkWaferPresent.Present;
                                    else
                                        UpperArm.WaferPresent = ForkWaferPresent.Unknown;
                                    break;
                                case RobotType.Grip:
                                    break;
                            }
                        }



                        //LowerPresent = IN[4];
                        //LowerGRIP = ((!IN[4]) && (!IN[5]) || IN[4]);
                        //UpperPresent = IN[?];
                        break;
                    case "LID1":
                        IN = new BitArray(new byte[] { Convert.ToByte(REC_Token[0]) });
                        if (!Connect)
                        {
                            Connect = true; //這邊作為連線與否判斷基準
                            //Set_Speed(Speed, Z_Speed);
                        }
                        break;
                    case "LD"://teachingdata
                        {

                        }
                        break;
                    case "LR"://position & speed
                        {
                            /*
                            2021-10-25 11:46:49.510   Robot : A > D : ReadPosition : 001 LR
                            2021-10-25 11:46:49.619   Robot : D > A : @001 648.8 196.15 122 270 0 5.2L 0#K
                            */
                            string _str = string.Empty;

                            for (int i = 0; i < 6; i++)
                            {
                                _str += REC_Token[i];
                                if (i < 5)
                                    _str += ",";
                            }

                            Log.Write(DeviceName, "ReadPosition:" + _str);
                        }
                        break;
                    case "LBD":
                        {
                            //2024-03-01 13:52:17.022   Robot : A > D : Home_Snake : 001 LBD
                            //2024-03-01 13:52:17.129   Robot : D > A : @001 0#"
                            SnakeHomeType = REC_Token[0];
                            Log.Write(DeviceName, "Check Snake Arm:" + SnakeHomeType);
                        }
                        break;
                    case "SP":
                        Status_update(REC_Token[0].Substring(0, 4));
                        Log.Write(DeviceName, "已重設Robot速度 " + CMD_Token[1].ToString() + " " + CMD_Token[2].ToString());
                        break;
                    case "SOB0":
                    case "SOB1":
                    case "SOB2":
                    case "SOB3":
                        //Thread.Sleep(500);
                        //Qry_WaferPresent();
                        break;
                    case "GP":
                        Status_update(REC_Token[0].Substring(0, 4));
                        break;
                    case "CL":
                        Status_update(REC_Token[0].Substring(0, 4));
                        JobFail();
                        Error = false;
                        Busy = false;
                        Log.Write(DeviceName, "已重設Robot狀態");
                        break;
                    default:
                        break;
                }
            }
            catch
            {

            }
        }
        private void MainCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //throw new NotImplementedException();
                byte[] raw_byte = new byte[128];
                SerialPort sp = (SerialPort)sender;
                if (sp.BytesToRead > 0)
                {
                    Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                    Array.Resize(ref raw_byte, length);
                    for (int i = 0; i < raw_byte.Length; i++)
                        Log.Write("Data", raw_byte[i].ToString());
                    DataCombine(raw_byte);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Exception", ex.ToString());
            }
        }


        //=====單動指令=====
        //The LD command is used to read the position data of the specified address of the specified robot.
        public void Qry_TeachingData(string address)
        {
            // RobotCmdQueue.EnQueue("LD" + address);
        }
        //The LR command is used to read the current position data of the robot.
        public void Qry_CurrentPosition()
        {
            //RobotCmdQueue.EnQueue("LR");
        }

        //=====巨集指令=====
        public bool PutWafer(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "SmartPut," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do SmartPut.");
                return false;
            }

            if (f.WaferPresent != ForkWaferPresent.Present)
            {
                if (from_socket)
                {
                    string ack = "SmartPut," + DeviceName + ",Wafer is not Present";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Wafer is not Present. Cannot do SmartPut.");
                return false;
            }

            //if (Error)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "SmartPut," + DeviceName + ",Robot is Error";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is Error. Cannot do SmartPut.");
            //    return false;
            //}

            //if (Busy)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "SmartPut," + DeviceName + ",Robot is busy";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is busy. Cannot do PutStandby.");
            //    return false;
            //}

            if (!CheckIsSave(RobotMacroCommand.SmartPut))
            {
                if (NeedHomeFirst())
                {
                    string ack = "SmartPut," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "SmartPut," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }


            // int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.SmartPut);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "SmartPut," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do SmartPut.");
                return false;
            }

            LoadPort _LP = GetLoadPort(pos);
            if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
            {
                if (_LP.Door_Status != DoorStatus.Open)
                {
                    if (from_socket)
                    {
                        string ack = "SmartPut," + DeviceName + "," + _LP.LPName + " Door is not Open";
                        SendOutSocket(ack);
                    }



                    Log.WriteSystemLog("Robot", _LP.LPName + " Door is not Open. Cannot do SmartPut.");
                    return false;
                }

                if (_LP.Busy)
                {
                    if (from_socket)
                    {
                        string ack = "SmartPut," + DeviceName + "," + _LP.LPName + " is busy now.";
                        SendOutSocket(ack);
                    }

                    Log.WriteSystemLog("Robot", _LP.LPName + " is busy now.");
                    return false;
                }

                string failreason = string.Empty;
                if (_LP.CheckAllWaferNormal(out failreason) == false)
                {
                    if (from_socket)
                    {
                        string ack = "SmartPut," + DeviceName + "," + _LP.LPName + ":" + failreason;
                        SendOutSocket(ack);
                    }

                    Log.WriteSystemLog("Robot", _LP.LPName + ":" + failreason);
                    return false;
                }

                _LP.SetLock(true);
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool GetWafer(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "SmartGet," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do SmartGet.");
                return false;
            }



            if (f.WaferPresent != ForkWaferPresent.Absent)
            {
                if (from_socket)
                {
                    string ack = "SmartGet," + DeviceName + ",Wafer is not Absent";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Wafer is not Absent. Cannot do SmartGet.");
                return false;
            }


            //if (Error)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "SmartGet," + DeviceName + ",Robot is Error";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is Error. Cannot do SmartGet.");
            //    return false;
            //}

            //if (Busy)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "SmartGet," + DeviceName + ",Robot is busy";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is busy. Cannot do PutStandby.");
            //    return false;
            //}
            if (!CheckIsSave((RobotMacroCommand.SmartGet)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "SmartGet," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "SmartGet," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }

            //int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.SmartGet);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "SmartGet," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do SmartGet.");
                return false;
            }

            LoadPort _LP = GetLoadPort(pos);
            if (_LP != null && _LP.Brand != LoadPort.EFEMBrand.Other)
            {
                if (_LP.Door_Status != DoorStatus.Open)
                {
                    if (from_socket)
                    {
                        string ack = "SmartGet," + DeviceName + "," + _LP.LPName + " Door is not Open";
                        SendOutSocket(ack);
                    }

                    Log.WriteSystemLog("Robot", _LP.LPName + " Door is not Open. Cannot do SmartGet.");
                    return false;
                }

                if (_LP.Busy)
                {
                    if (from_socket)
                    {
                        string ack = "SmartGet," + DeviceName + "," + _LP.LPName + " is busy now.";
                        SendOutSocket(ack);
                    }

                    Log.WriteSystemLog("Robot", _LP.LPName + " is busy now.");
                    return false;
                }

                string failreason = string.Empty;
                if (_LP.CheckAllWaferNormal(out failreason) == false)
                {
                    if (from_socket)
                    {
                        string ack = "SmartGet," + DeviceName + "," + _LP.LPName + ":" + failreason;
                        SendOutSocket(ack);
                    }

                    Log.WriteSystemLog("Robot", _LP.LPName + ":" + failreason);
                    return false;
                }

                _LP.SetLock(true);
            }

            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool PutStandby(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "PutStandby," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do PutStandby.");
                return false;
            }

            //if (Error)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "PutStandby," + DeviceName + ",Robot is Error";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is Error. Cannot do PutStandby.");
            //    return false;
            //}

            //if (Busy)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "PutStandby," + DeviceName + ",Robot is busy";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is busy. Cannot do PutStandby.");
            //    return false;
            //}

            if (!CheckIsSave((RobotMacroCommand.PutStandby)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "PutStandby," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "PutStandby," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }
            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.PutStandby);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            // obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "PutStandby," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do PutStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;

        }
        public bool GetStandby(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "GetStandby," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do GetStandby.");
                return false;
            }


            //if (Error)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "GetStandby," + DeviceName + ",Robot is Error";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is Error. Cannot do GetStandby.");
            //    return false;
            //}

            //if (Busy)
            //{
            //    if (from_socket)
            //    {
            //        string ack = "GetStandby," + DeviceName + ",Robot is busy";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is busy. Cannot do GetStandby.");
            //    return false;
            //}

            if (!CheckIsSave((RobotMacroCommand.GetStandby)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "GetStandby," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "GetStandby," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }

            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.GetStandby);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            //obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "GetStandby," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do GetStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool PutExtend(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "PutExtend," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do PutStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.PutExtend)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "PutExtend," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "PutExtend," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }
            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.PutExtend);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            // obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "PutExtend," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do PutStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;

        }
        public bool GetExtend(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "GetExtend," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do GetStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.GetExtend)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "GetExtend," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "GetExtend," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }

            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.GetExtend);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            //obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "GetStandby," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do GetStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool PutPlace(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "PutPlace," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do PutStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.PutPlace)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "PutPlace," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "PutPlace," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }
            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.PutPlace);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            // obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "PutPlace," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do PutStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;

        }
        public bool GetPick(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "GetPick," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do GetStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.GetPick)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "GetPick," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "GetPick," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }

            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.GetPick);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            //obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "GetPick," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do GetStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool PutRetract(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "PutRetract," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do PutStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.PutRetract)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "PutRetract," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "PutRetract," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }
            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.PutRetract);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            // obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "PutRetract," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do PutStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;

        }
        public bool GetRetract(Fork f, string pos, int slot, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "GetRetract," + DeviceName + ",Select Wrong Arm";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Select Wrong Arm. Cannot do GetStandby.");
                return false;
            }

            if (!CheckIsSave((RobotMacroCommand.GetRetract)))
            {
                if (NeedHomeFirst())
                {
                    string ack = "GetRetract," + DeviceName + ",Need Home at first";
                    Log.WriteSystemLog("Robot", "Need Home at first");
                    if (from_socket)
                        SendOutSocket(ack);
                }
                else
                {
                    string ack = "GetRetract," + DeviceName + ",Not In Save Position LastArrived_GP:" + LastArrived_GP;
                    Log.WriteSystemLog("Robot", "Not In Save Position LastArrived_GP: " + LastArrived_GP);
                    if (from_socket)
                        SendOutSocket(ack);
                }

                return false;
            }

            //  int posid = GetPositionID(pos, true, f.RobotArm);
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.GetRetract);
            obj.UserFork = f;
            obj.FromSocket = from_socket;
            //obj.PositionIDList.Add(posid);
            obj.Slot = slot;
            obj.Station = GetStation(pos);

            if (obj.Station == null)
            {
                if (from_socket)
                {
                    string ack = "GetRetract," + DeviceName + ",Station is Wrong";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Station is Wrong. Cannot do GetStandby.");
                return false;
            }
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }

        public bool RecoverSetRobotSpeed()
        {
            Speed = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Speed", "10"));
            Z_Speed = Convert.ToInt32(XML.Read("Setting/Robot" + ((RobotIndex == 0) ? "" : "2") + "/Z_Speed", "10"));
            return SetRobotSpeed(Speed, Z_Speed, false);
        }
        public bool SetRobotSpeed(int _ZSpeed, int _OtherSpeed, bool from_socket)
        {
            if (Busy)
            {
                if (from_socket)
                {
                    string ack = DeviceName + "Speed," + _ZSpeed + "," + _OtherSpeed + ",Robot is busy";
                    SendOutSocket(ack);
                }

                Log.WriteSystemLog("Robot", "Robot is busy. Cannot do SetSpeed.");
                return false;
            }

            if ((_ZSpeed > 100) | (_OtherSpeed > 100) | (_ZSpeed < 0) | (_OtherSpeed < 0))
            {
                if (from_socket)
                {
                    string ack = DeviceName + "Speed," + _ZSpeed + "," + _OtherSpeed + ",Speed Parameter is Wrong";
                    SendOutSocket(ack);
                }
                return false;
            }

            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.SetSpeed);
            obj.FromSocket = from_socket;
            obj.ZSpeed = _ZSpeed;
            obj.OtherSpeed = _OtherSpeed;

            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool DoHome(bool from_socket)
        {
            //if (Busy)
            //{
            //    if (from_socket)
            //    {
            //        //VacuumOn,Robot,(ArmID),0
            //        string ack = "Home," + DeviceName + ",Robot is busy";
            //        SendOutSocket(ack);
            //    }

            //    Log.WriteSystemLog("Robot", "Robot is busy. Cannot do Home.");
            //    return false;
            //}
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.Home);
            obj.FromSocket = from_socket;
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool DoResetError(bool from_socket)
        {
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.ResetError);
            obj.FromSocket = from_socket;
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }

        public bool DoReadPosition(bool from_socket)
        {
            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.ReadPosition);
            obj.FromSocket = from_socket;
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }

        public bool GetStatus(bool from_socket)
        {

            if (Busy)
            {
                //0601
                //GetStatus,Robot,0601,0,0
                Log.WriteSystemLog(DeviceName, "Robot is busy. Direct update memory data.");
                string ack = "GetStatus," + DeviceName + ",0401,";

                if (UpperArm == null)
                    ack += "9,";
                else
                    ack += ((int)UpperArm.WaferPresent) + ",";

                ack += (int)LowerArm.WaferPresent;
                SendOutSocket(ack);
                return true;
            }

            RobotMacroCommandObject obj = new RobotMacroCommandObject(RobotMacroCommand.GetStatus);
            obj.FromSocket = from_socket;
            RobotMacroCommandObjectQueue.EnQueue(obj);
            return true;
        }
        public bool VacummControl(Fork f, bool VacuumOn, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "";

                    if (VacuumOn)
                        ack += "VacuumOn," + DeviceName + ",";
                    else
                        ack += "VacuumOff," + DeviceName + ",";

                    if ((f.RobotArm != RobotArm.LowerArm) && (f.RobotArm != RobotArm.UpperArm))
                    {
                        ack += "9,Select wrong arm.";
                        SendOutSocket(ack);
                        return false;
                    }
                }

            }


            if (Busy)
            {
                if (from_socket)
                {
                    //VacuumOn,Robot,(ArmID),0
                    string ack = "";

                    if (VacuumOn)
                        ack += "VacuumOn," + DeviceName + ",";
                    else
                        ack += "VacuumOff," + DeviceName + ",";

                    if (f.RobotArm == RobotArm.LowerArm)
                        ack += "1,";
                    else if (f.RobotArm == RobotArm.UpperArm)
                        ack += "2,";

                    ack += "Robot is busy.";
                    SendOutSocket(ack);
                    return false;
                }

                Log.WriteSystemLog("Robot", "Robot is busy. Cannot do VacummControl.");
                return false;
            }


            RobotMacroCommandObject obj;
            if (VacuumOn)
                obj = new RobotMacroCommandObject(RobotMacroCommand.VacuumOn);
            else
                obj = new RobotMacroCommandObject(RobotMacroCommand.VacuumOff);

            obj.FromSocket = from_socket;
            obj.UserFork = f;
            RobotMacroCommandObjectQueue.EnQueue(obj);

            return true;
        }
        public bool EdgeGripControl(Fork f, bool GripOn, bool from_socket)
        {
            if (f == null)
            {
                if (from_socket)
                {
                    string ack = "";

                    if (GripOn)
                        ack += "EdgeGripOn," + DeviceName + ",";
                    else
                        ack += "EdgeGripOff," + DeviceName + ",";

                    if ((f.RobotArm != RobotArm.LowerArm) && (f.RobotArm != RobotArm.UpperArm))
                    {
                        ack += "9,Select wrong arm.";
                        SendOutSocket(ack);
                        return false;
                    }
                }
            }


            if (Busy)
            {
                if (from_socket)
                {
                    //VacuumOn,Robot,(ArmID),0
                    string ack = "";

                    if (GripOn)
                        ack += "EdgeGripOn," + DeviceName + ",";
                    else
                        ack += "EdgeGripOff," + DeviceName + ",";

                    if (f.RobotArm == RobotArm.LowerArm)
                        ack += "1,";
                    else if (f.RobotArm == RobotArm.UpperArm)
                        ack += "2,";

                    ack += "Robot is busy.";
                    SendOutSocket(ack);
                    return false;
                }

                Log.WriteSystemLog("Robot", "Robot is busy. Cannot do EdgeGripControl.");
                return false;
            }


            RobotMacroCommandObject obj;
            if (GripOn)
                obj = new RobotMacroCommandObject(RobotMacroCommand.EdgeGripOn);
            else
                obj = new RobotMacroCommandObject(RobotMacroCommand.EdgeGripOff);

            obj.FromSocket = from_socket;
            obj.UserFork = f;
            RobotMacroCommandObjectQueue.EnQueue(obj);

            return true;
        }


        int GetPositionID(string pos, bool is_put, RobotArm Arm)
        {
            int _result = -1;
            pos = pos.ToUpper();


            if (Arm == RobotArm.UnknownArm)
                return -1;

            if (pos == "ALIGNER")
            {
                if (Arm == RobotArm.LowerArm)
                {
                    if (is_put)
                        _result = 110;
                    else
                        _result = 310;
                }
                else if (Arm == RobotArm.UpperArm)
                {
                    if (is_put)
                        _result = 610;
                    else
                        _result = 810;
                }

                return _result;
            }


            if (pos.Substring(0, 1) == "P")
            {
                for (int i = 0; i < LP.Count; i++)
                {
                    if (LP[i].LPName == pos)
                    {
                        int p_num = LP[i].LPNumber;

                        if (Arm == RobotArm.LowerArm)
                        {
                            if (is_put)
                                _result = 170 + (p_num * 10);
                            else
                                _result = 370 + (p_num * 10);
                        }
                        else if (Arm == RobotArm.UpperArm)
                        {
                            if (is_put)
                                _result = 670 + (p_num * 10);
                            else
                                _result = 870 + (p_num * 10);
                        }

                        return _result;
                    }
                }
            }

            return -1;
        }
        private void button_Robot_Command_Click(object sender, EventArgs e)
        {
            RobotMacroCommand cmd = (RobotMacroCommand)Enum.Parse(typeof(RobotMacroCommand), listBox_RobotMacroCommand.Text, false);
            // string error_msg = null;
            Log.WriteOperationLog(DateTime.Now, "Robot: User press " + cmd.ToString());
            int slot;// = Convert.ToInt32( comboBox_Slot.Text);
            switch (cmd)
            {
                case RobotMacroCommand.ReadPosition:
                    {
                        DoReadPosition(false);
                    }
                    break;
                case RobotMacroCommand.ResetError:
                    {
                        DoResetError(false);
                    }
                    break;
                case RobotMacroCommand.SmartGet:
                case RobotMacroCommand.SmartGet6S:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            GetWafer(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            GetWafer(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.SmartPut:
                case RobotMacroCommand.SmartPut6S:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            PutWafer(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            PutWafer(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.Home:
                    {
                        DoHome(false);
                    }
                    break;
                //
                case RobotMacroCommand.GetStandby:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            GetStandby(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            GetStandby(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.PutStandby:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            PutStandby(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            PutStandby(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                //
                case RobotMacroCommand.GetExtend:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            GetExtend(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            GetExtend(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.PutExtend:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            PutExtend(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            PutExtend(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                //
                case RobotMacroCommand.GetPick:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            GetPick(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            GetPick(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.PutPlace:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            PutPlace(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            PutPlace(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                //
                case RobotMacroCommand.GetRetract:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            GetRetract(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            GetRetract(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                case RobotMacroCommand.PutRetract:
                    {
                        slot = Convert.ToInt32(comboBox_Slot.Text);
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            PutRetract(LowerArm, comboBox_Target.Text, slot, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            PutRetract(UpperArm, comboBox_Target.Text, slot, false);
                    }
                    break;
                //
                case RobotMacroCommand.EdgeGripOn:
                    {
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            EdgeGripControl(LowerArm, true, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            EdgeGripControl(UpperArm, true, false);
                    }
                    break;
                case RobotMacroCommand.EdgeGripOff:
                    {
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            EdgeGripControl(LowerArm, false, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            EdgeGripControl(UpperArm, false, false);
                    }
                    break;
                case RobotMacroCommand.VacuumOn:
                    {
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            VacummControl(LowerArm, true, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            VacummControl(UpperArm, true, false);
                    }
                    break;
                case RobotMacroCommand.VacuumOff:
                    {
                        if (comboBox_UseArm.Text == LowerArm.RobotArm.ToString())
                            VacummControl(LowerArm, false, false);
                        if (comboBox_UseArm.Text == UpperArm.RobotArm.ToString())
                            VacummControl(UpperArm, false, false);
                    }
                    break;
                case RobotMacroCommand.GetStatus:
                    GetStatus(false);
                    break;
                case RobotMacroCommand.SetSpeed:
                    {
                        if (comboBox_Other_Speed.Text == string.Empty | comboBox_Z_Speed.Text == string.Empty)
                        {
                            MessageBox.Show("Please select robot speed.");
                            return;
                        }


                        SetRobotSpeed(Convert.ToInt32(comboBox_Z_Speed.Text), Convert.ToInt32(comboBox_Other_Speed.Text), false);
                    }
                    break;
            }
        }
        private void Robot_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                // Qry_Status(false);
            }
        }
        private void Robot_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
        private void comboBox_MainPort_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
            CB.Items.Clear();
            for (int i = 0; i < IPC_AllCOM.Length; i++)
                CB.Items.Add(IPC_AllCOM[i]);
        }
        private void comboBox_UseArm_DropDown(object sender, EventArgs e)
        {
            comboBox_UseArm.Items.Clear();
            for (int i = Fork.Count; i > 0; i--)
                comboBox_UseArm.Items.Add(Fork[i - 1].RobotArm.ToString());
        }
        private void comboBox_Tartget_DropDown(object sender, EventArgs e)
        {
            comboBox_Target.Items.Clear();
            //for (int i = LP.Count; i >0; i--)
            //    comboBox_Tartget.Items.Add(LP[i-1].LPName);

            for (int i = 0; i < StationList.Count; i++)
                comboBox_Target.Items.Add(StationList[i].StationName);

        }
        private void RefreshForkStatus()
        {
            if (LowerArm != null)
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        forkView_Lower.RefreshUI(LowerArm);
                    }));
                }
                else
                {
                    forkView_Lower.RefreshUI(LowerArm);
                }
            }

            if (UpperArm != null)
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        forkView_Upper.RefreshUI(UpperArm);
                    }));
                }
                else
                {
                    forkView_Upper.RefreshUI(UpperArm);
                }
            }
        }

        private void comboBox_Tartget_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox_Slot.Items.Clear();

            StationInfo s = GetStation(comboBox_Target.Text);


            for (int i = s.MaxSlot; i >= 1; i--)
                comboBox_Slot.Items.Add(i);



            comboBox_Slot.SelectedIndex = 0;

            CheckAddr();
        }

        private StationInfo GetStation(string name)
        {
            for (int i = 0; i < StationList.Count; i++)
            {
                if (StationList[i].StationName.ToUpper() == name.ToUpper())
                    return StationList[i];
            }

            return null;
        }

        private LoadPort GetLoadPort(int portnum)
        {
            for (int i = 0; i < LP.Count; i++)
            {
                if (LP[i].LPNumber == portnum)
                    return LP[i];
            }

            return null;
        }

        private LoadPort GetLoadPort(string LPName)
        {
            for (int i = 0; i < LP.Count; i++)
            {
                if (LP[i].LPName == LPName)
                    return LP[i];
            }

            return null;
        }

        private void comboBox_UseArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckAddr();
        }

        private void comboBox_Slot_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckAddr();
        }

        private void listBox_RobotMacroCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckAddr();
            RobotMacroCommand RNC = (RobotMacroCommand)Enum.Parse(typeof(RobotMacroCommand), listBox_RobotMacroCommand.Text, true);

            if (RNC == RobotMacroCommand.SetSpeed)
                groupBox_Speed.Visible = true;
            else
                groupBox_Speed.Visible = false;
        }

        private void CheckAddr()
        {
            if (listBox_RobotMacroCommand.SelectedIndex == -1)
            {
                label_Addr.Text = "NA";
                label_Zoffset.Text = "NA";
                return;
            }

            if (comboBox_UseArm.SelectedIndex == -1)
            {
                label_Addr.Text = "NA";
                label_Zoffset.Text = "NA";
                return;
            }

            if (comboBox_Slot.SelectedIndex == -1)
            {
                label_Addr.Text = "NA";
                label_Zoffset.Text = "NA";
                return;
            }

            if (listBox_RobotMacroCommand.SelectedIndex == -1)
            {
                label_Addr.Text = "NA";
                label_Zoffset.Text = "NA";
                return;
            }

            if (comboBox_Target.SelectedIndex == -1)
            {
                label_Addr.Text = "NA";
                label_Zoffset.Text = "NA";
                return;
            }

            RobotMacroCommand RNC = (RobotMacroCommand)Enum.Parse(typeof(RobotMacroCommand), listBox_RobotMacroCommand.Text, true);

            // Get, Put, GetStandby, PutStandby,
            if ((RNC != RobotMacroCommand.SmartGet) & (RNC != RobotMacroCommand.SmartPut) & (RNC != RobotMacroCommand.GetStandby) & (RNC != RobotMacroCommand.PutStandby))
                return;

            StationInfo s = GetStation(comboBox_Target.Text);
            label_Addr.Text = s.LowerPutAddress.ToString();

            int slot = Convert.ToInt32(comboBox_Slot.Text);

            label_Zoffset.Text = (s.SlotPitch * (slot - 1)).ToString();
            //001 GP 880 ( 0 0 10 0)



        }

    }

    public class RobotMacroCommandObject
    {
        public RobotMacroCommand Cmd = RobotMacroCommand.GetStatus;
        public RobotMacroCommandObject(RobotMacroCommand _cmd)
        {
            Cmd = _cmd;
        }
        public bool FromSocket = false;
        public double offset = 0;
        public int Slot = 0;
        public Fork UserFork = null;
        public List<int> PositionIDList = new List<int>();
        public StationInfo Station = null;
        public int ZSpeed;
        public int OtherSpeed;
    }

    public class StationInfo
    {
        public string StationName;
        public int MaxSlot = 1;
        public int SlotPitch = 10;
        public bool OtherStation = false;

        public TargetType TargetType = TargetType.Unknown;

        public int LowerPutAddress;
        public int LowerGetAddress
        {
            get
            {
                return LowerPutAddress + 200;
            }
        }
        public int UpperPutAddress
        {
            get
            {
                return LowerPutAddress + 500;
            }
        }
        public int UpperGetAddress
        {
            get
            {
                return LowerPutAddress + 700;
            }
        }


        public List<int> ReplyAddressList(int addr, RobotMotionPoint Motion)
        {
            List<int> _result = new List<int>();

            if (Motion == RobotMotionPoint.Snake)
            {
                _result.Add(addr);
                _result.Add(addr + 1);
                _result.Add(addr + 2);
                _result.Add(addr + 3);
                _result.Add(addr + 4);
                _result.Add(addr + 5);
            }
            else
            {
                _result.Add(addr);
                _result.Add(addr + 1);
                _result.Add(addr + 2);
                _result.Add(addr + 3);
            }
            return _result;
        }



        /*
        <StationName value = "CHAMBER1" />
        <PutAddress value = "1110" />
        <MaxSlot value = "1" />
        <SlotPitch value = "10" />           
        */
    }

    public class Fork
    {
        public RobotArm RobotArm = RobotArm.UnknownArm;
        public RobotType RobotType = RobotType.Unknown;
        public TargetType TargetType = TargetType.Unknown;
        public ForkWaferPresent WaferPresent = ForkWaferPresent.Unknown;
        public VacuumStatus VacuumStatus = VacuumStatus.Unknown;
        public GripStatus GripStatus = GripStatus.Unknown;
    }

}
