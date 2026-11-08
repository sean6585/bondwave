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


namespace AML
{


    public enum WaferState { NoWafer = 0, WaferMapped = 1, WaferCross = 2, WaferThick = 3, WaferThin = 4, WaferPositionError = 5, WaferUnknown = 9 };
    public enum LoadPortType { Wafer, Frame, MCLP }

    public enum DoorStatus { Close, Open, Unknown }

    public partial class LoadPort : Form
    {
        public enum EFEMBrand { Hirata, Other }

        public enum LPCommand { GetStatus, GetWaferSlot, ResetError, ReadFoupID, Home, Load, Unload, Clamp, Unclamp, Map, ResetLock }
        // public enum E84Command { E84_Enable, E84_LinkTest, E84_SetAuto, E84_SetManual, E84_Reset, E84_SetESOn, E84_SetESOff, E84_SetTimeout }
        public enum E84Command { E84_Enable, E84_SetAuto, E84_SetManual, E84_Reset, E84_SetESOn, E84_SetESOff, E84_SetTimeout }
        public enum ClampStatus { Unclamp, Clamp, Unknown }
        public enum ErrorStatus { Recoverable, NoError, Unrecoverable, Unknown }
        public enum FoupStatus { Present, Absent, Unknown }

        public enum ModeStatus { Online, Teaching, Maintain, Unknown }

        public enum WaferProtrusionSensorStatus { Shading, Lighting, Unknown }

        public LoadPort()
        {
            InitializeComponent();
        }
        public WaferState[] WaferPresent;
        public event ThrowOutSocketReply ThrowOutSocketReplyEvent;
        public AML_LogPage Log = new AML_LogPage();
        BlockQueue<byte[]> LoadPortQueue = new BlockQueue<byte[]>();
        BlockQueue<LPCmdObject> LPCmdObjectQueue = new BlockQueue<LPCmdObject>();

        int _LPNumber = 0;
        public int LPNumber
        {
            get
            {
                return _LPNumber;
            }
        }
        string _LPName;
        public string LPName
        {
            get
            {
                return _LPName;
            }
        }
        string[] IPC_AllCOM;
        EFEMBrand _Brand = EFEMBrand.Hirata;
        public EFEMBrand Brand
        {
            get
            {
                return _Brand;
            }
        }
        public LoadPortType LPType = LoadPortType.Wafer;
        int SlotCount = 1;
        public bool Enable_Alarm = false;
        public int LPAlarmOffset = 20000000;
        public int AllSlotCount
        {
            get
            {
                return SlotCount;
            }

        }

        SerialPort MainCom;
        SerialPort RFIDCom;
        SerialPort E84Com;
        Dictionary<string, string> LP_ErrorCode = new Dictionary<string, string>();
        public bool Busy
        {
            get
            {
                return _Busy;
            }

        }

        bool _lock = false;
        public bool Lock
        {
            get
            {
                return _lock;
            }
        }

        public void SetLock(bool value)
        {
            _lock = value;
        }
        void SetBusy(bool value)
        {
            _Busy = value;
        }
        public bool Error
        {
            get
            {
                if ((_Error == ErrorStatus.Unrecoverable) | (_Error == ErrorStatus.Unknown))
                    return true;
                else
                    return false;
            }
        }

        //void SetError(bool value)
        //{
        //    _error = value;
        //}

        bool _Busy = false;

        ModeStatus Mode = ModeStatus.Unknown;
        FoupStatus Foup = FoupStatus.Unknown;
        ClampStatus Clamp = ClampStatus.Unknown;
        DoorStatus Door = DoorStatus.Unknown;
        ErrorStatus _Error = ErrorStatus.Unknown;
        WaferProtrusionSensorStatus WaferProtrusionSensor = WaferProtrusionSensorStatus.Shading;

        public WaferProtrusionSensorStatus WaferProtrusion
        {
            get
            {
                return WaferProtrusionSensor;
            }
        }
        public DoorStatus Door_Status
        {
            get
            {
                return Door;
            }
        }

        /* 
        vClamp = 2   ' 0=unclamp, 1=clamp , 2=unknow
        vFoup = 2   ' 0=absent, 1=present , 2=unknow
        vDoor = 2   ' 0=open, 1=close , 2=unknow
        vFoupID = ""
        vError = 3  ' 0=no error, 1=recoverable , 2=non-recover , 3=unknow
        vMode = 3   ' 0=online, 1=teaching, 2=maintenance        
        */
        RfidReader _RFID = new RfidReader();
        E84Control _E84 = new E84Control();

        public RfidReader RFID
        {
            get
            {
                return _RFID;
            }
        }

        public E84Control E84
        {
            get
            {
                return _E84;
            }
        }
        public void Init(string LP_Header, int LPNumber) //P1,P2
        {

            try
            {
                //int value = Convert.ToInt32("AA", 16);
                string LogFolder = XML.Read("Setting/MainForm/Logfolder", "D:\\AML");
                int LogCountLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogCountLimit", "200"));
                int LogFolderLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogFolderLimit", "30"));
                Log.Initial(LP_Header, false, true, LogFolder, LogCountLimit, LogFolderLimit);

                _LPName = LP_Header;
                //_LPNumber = Convert.ToInt32(LP_Header.Substring(1));
                _LPNumber = LPNumber;
                this.Text = LPName;
                label_LP.Text = LPName;
                comboBox_Brand.DataSource = Enum.GetNames(typeof(EFEMBrand));
                comboBox_Type.DataSource = Enum.GetNames(typeof(LoadPortType));
                listBox_LP_Command.DataSource = Enum.GetNames(typeof(LPCommand));
                listBox_E84_Command.DataSource = Enum.GetNames(typeof(E84Command));
                comboBox_E84_TP.DataSource = Enum.GetNames(typeof(AML.E84Control.E84TpnItem));



                LoadSetting(_LPNumber);
                //timer1sec.Enabled = true;
                RefreshWaferSlotUI();
                if (LPType != LoadPortType.MCLP)
                {
                    InitLPErrorCode();
                    GetStatus(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadPort設定有誤請開發者檢查於:" + LP_Header + Environment.NewLine + ex.ToString());
                MessageBox.Show("程式即將自動關閉.");
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            backgroundWorker_LP_Send.RunWorkerAsync();
            backgroundWorker_LP_Receive.RunWorkerAsync();
            Log.Write(LPName, "Init Complete.");
        }

        private void InitWaferSlot()
        {
            switch (LPType)
            {
                case LoadPortType.Frame:

                    SlotCount = 13;
                    DGV_Wafer.RowHeadersVisible = true;
                    DGV_Wafer.RowCount = SlotCount;
                    DGV_Wafer.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                    WaferPresent = new WaferState[SlotCount];
                    for (int i = 0; i < SlotCount; i++)
                    {
                        WaferPresent[SlotCount - i - 1] = WaferState.WaferUnknown;
                        DGV_Wafer.Rows[i].Cells["Column_Slot"].Value = (SlotCount - i).ToString();
                        DGV_Wafer.Rows[i].Cells["Column_State"].Value = WaferPresent[SlotCount - i - 1].ToString();
                    }
                    break;
                case LoadPortType.Wafer:

                    SlotCount = 25;
                    DGV_Wafer.RowHeadersVisible = true;
                    DGV_Wafer.RowCount = SlotCount;
                    DGV_Wafer.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                    WaferPresent = new WaferState[SlotCount];

                    for (int i = 0; i < SlotCount; i++)
                    {
                        WaferPresent[SlotCount - i - 1] = WaferState.WaferUnknown;
                        DGV_Wafer.Rows[i].Cells["Column_Slot"].Value = (SlotCount - i).ToString();
                        DGV_Wafer.Rows[i].Cells["Column_State"].Value = WaferPresent[SlotCount - i - 1].ToString();
                    }
                    break;
                default:
                    DGV_Wafer.Enabled = false;
                    break;
            }
        }

        private void InitLPErrorCode()
        {
            LP_ErrorCode.Add("00", "Normal");
            LP_ErrorCode.Add("10", "Clamp time over");
            LP_ErrorCode.Add("11", "Unclamp time over");
            LP_ErrorCode.Add("12", "Dock time over");
            LP_ErrorCode.Add("13", "Undock time over");
            LP_ErrorCode.Add("14", "Latch time over");
            LP_ErrorCode.Add("15", "Unlatch time over");
            LP_ErrorCode.Add("16", "Vacuum time over");
            LP_ErrorCode.Add("17", "Vacuum release time over ");
            LP_ErrorCode.Add("18", "Door open time over");
            LP_ErrorCode.Add("19", "Door close time over");
            LP_ErrorCode.Add("1A", "Mapping forward time over");
            LP_ErrorCode.Add("1B", "Mapping return time over");
            LP_ErrorCode.Add("1F", "communication error (depends on the specifications)");
            LP_ErrorCode.Add("20", "Home return time over");
            LP_ErrorCode.Add("21", "Loading time over");
            LP_ErrorCode.Add("22", "Unloading time over");
            LP_ErrorCode.Add("23", "Position time over");
            LP_ErrorCode.Add("40", "Mapping data error");
            LP_ErrorCode.Add("41", "Mode select error");
            LP_ErrorCode.Add("70", "clamp sensor error");
            LP_ErrorCode.Add("71", "Dock sensor error");
            LP_ErrorCode.Add("72", "Latch sensor error");
            LP_ErrorCode.Add("73", "Door sensor error");
            LP_ErrorCode.Add("74", "Mapping sensor error");
            LP_ErrorCode.Add("A0", "Wafer drop");
            LP_ErrorCode.Add("A1", "Wafer protrusion");
            LP_ErrorCode.Add("A2", "POUP mount error (Mount sensor)");
            LP_ErrorCode.Add("A3", "POUP mount error (Load sensor)");
            LP_ErrorCode.Add("A5", "Air pressure drop");
            LP_ErrorCode.Add("B0", "Host error (with no PIO input)");
            LP_ErrorCode.Add("C0", "Parameter error");
            LP_ErrorCode.Add("E0", "FAN alarm");
            LP_ErrorCode.Add("E2", "Encoder error");
            LP_ErrorCode.Add("E3", "Servo alarm");
            LP_ErrorCode.Add("FE", "Dock hand pinch error");
        }

        private void SendOutSocket(string data)
        {
            Log.Write(data);
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
        private void comboBox_RFIDPort_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
            CB.Items.Clear();

            for (int i = 0; i < IPC_AllCOM.Length; i++)
                CB.Items.Add(IPC_AllCOM[i]);
        }
        private void comboBox_E84Port_DropDown(object sender, EventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            CB.SelectedIndex = -1;
            CB.Items.Clear();

            for (int i = 0; i < IPC_AllCOM.Length; i++)
                CB.Items.Add(IPC_AllCOM[i]);
        }
        private void LoadPort_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
        private void LoadSetting(int LoadNo)
        {
            try
            {
                _Brand = (EFEMBrand)Enum.Parse(typeof(EFEMBrand), XML.Read("Setting/P" + LoadNo.ToString() + "/Brand", ""), false);
                LPType = (LoadPortType)Enum.Parse(typeof(LoadPortType), XML.Read("Setting/P" + LoadNo.ToString() + "/Type", ""), false);

                comboBox_Brand.Text = _Brand.ToString();
                comboBox_Type.Text = LPType.ToString();
                IPC_AllCOM = SerialPort.GetPortNames();

                for (int i = 0; i < IPC_AllCOM.Length; i++)
                {
                    comboBox_MainPort.Items.Add(IPC_AllCOM[i]);
                    comboBox_RFIDPort.Items.Add(IPC_AllCOM[i]);
                    comboBox_E84Port.Items.Add(IPC_AllCOM[i]);
                }

                if (MainCom != null)
                    MainCom.Close();
                if (RFIDCom != null)
                    RFIDCom.Close();
                if (E84Com != null)
                    E84Com.Close();

                Ian.SetComPortProperty(out MainCom, XML.Read("Setting/P" + LoadNo.ToString() + "/Main_COM", ""));
                MainCom.DtrEnable = true;
                MainCom.RtsEnable = true;
                MainCom.Open();

                if (XML.Read("Setting/" + LPName + "/RFID_Enable", "") == "1")
                {
                    Ian.SetComPortProperty(out RFIDCom, XML.Read("Setting/P" + LoadNo.ToString() + "/RFID_COM", ""));

                    _RFID.Intial(RfidReader.RfidReaderType.Omron, "1", RFIDCom);
                    _RFID.ReadComplete += RFID_ReadComplete;
                }
                else
                {
                    label_RFIDPort.Visible = false;
                    comboBox_RFIDPort.Visible = false;
                }

                if (XML.Read("Setting/P" + LoadNo.ToString() + "/E84_Enable", "") == "1")
                {
                    Ian.SetComPortProperty(out E84Com, XML.Read("Setting/P" + LoadNo.ToString() + "/E84_COM", ""));
                    _E84.Initial(_LPName, E84Com, Log);
                    _E84.E84Reply += E84_E84Reply;
                    _E84.E84Event += E84_E84Event;
                    _E84.E84Alarm += E84_E84Alarm;
                    _E84.OpenPort();
                }
                else
                {
                    Label_E84Port.Visible = false;
                    comboBox_E84Port.Visible = false;
                }

                MainCom.DataReceived += MainCom_DataReceived;
                //  RFIDCom.DataReceived += RFIDCom_DataReceived;
                //            E84Com.DataReceived += E84Com_DataReceived;

                comboBox_MainPort.Text = MainCom.PortName;
                comboBox_RFIDPort.Text = RFIDCom.PortName;
                comboBox_E84Port.Text = E84Com.PortName;
                InitWaferSlot();
            }
            catch (Exception ex)
            {
                Log.WriteSystemLog(LPName, ex.ToString());
                MessageBox.Show(ex.ToString());

            }
        }

        private void E84_E84Alarm(string Code, string Description)
        {
            //throw new NotImplementedException();
            //SendOutSocket()
            SetLock(true);
            Log.Write("Alarm", "ALID:" + Code + ";" + Description);
            Log.Write("Alarm", "Lock LP due to alarm");

            int ALID = Convert.ToInt32(Code) + LPAlarmOffset + (LPNumber * 1000000) + 840000;

            if (Enable_Alarm)
                SendOutSocket("Alm," + LPName + "," + ALID + "," + Description);
        }

        E84_COMPT_Check E84_COMPT_CLASS = new E84_COMPT_Check();
        private void E84_E84Event(E84Control.E84EventItem Event, bool OnOff, byte RawEvtBye)
        {
            //throw new NotImplementedException();

            switch (Event)
            {
                case E84Control.E84EventItem.Cs:
                    SetLock(OnOff);
                    break;
            }

            string Message = "Event," + LPName + ",E84,";

            switch (RawEvtBye)
            {
                case 0x8:
                    Message += "Evt1, Waiting Enable Command";
                    break;
                case 0x9:
                    SetLock(false);
                    E84_COMPT_CLASS = new E84_COMPT_Check();
                    Message += "Evt2,Standby for E84 Procedure";
                    break;
                case 0x10:
                    E84_COMPT_CLASS = new E84_COMPT_Check();
                    Message += "Evt3,GO ON , OHT Arrived";
                    break;
                case 0x11:
                    //SetLock(true); 
                    Message += "Evt4,CS ON";
                    break;
                case 0x12:
                    Message += "Evt5,VALID ON";
                    break;
                case 0x13:
                    Message += "Evt6,L_REQ ON";
                    break;
                case 0x14:
                    Message += "Evt7,U_REQ ON";
                    break;
                case 0x15:
                    Message += "Evt8,TR_REQ ON";
                    break;
                case 0x16:
                    Message += "Evt9,READY ON";
                    break;
                case 0x17:
                    //E84_COMPT_CLASS = new E84_COMPT_Check();
                    //E84_COMPT_CLASS.BusyOn = true;
                    Message += "Evt10,BUSY ON";
                    break;
                case 0x18:
                    Message += "Evt11,L_REQ OFF";
                    break;
                case 0x19:
                    Message += "Evt12,U_REQ OFF";
                    break;
                case 0x1A:
                    E84_COMPT_CLASS = new E84_COMPT_Check();
                    //  E84_COMPT_CLASS.BusyOff = true;
                    E84_COMPT_CLASS.COMPTOn = true;
                    Message += "Evt13,BUSY OFF, TR_REQ OFF, COMPT ON";

                    break;
                case 0x1B:
                    Message += "Evt71,TR_REQ OFF";
                    break;
                case 0x1C:
                    //COMPT ON
                    break;
                case 0x1D:
                    Message += "Evt14,READY OFF";
                    break;
                case 0x1E:
                    Message += "Evt72,VALID OFF";
                    break;
                case 0x1f:
                    //COMPT OFF
                    break;
                case 0x20:
                    // SetLock(false); 
                    E84_COMPT_CLASS.COMPTOff = true;
                    Message += "Evt15,VALID OFF, COMPT OFF, CS0 OFF";
                    break;
                case 0x21:
                    Message += "Evt16,GO OFF";
                    break;
                case 0x22:
                    //CONTINUE HAND-OFF
                    break;
                case 0x30:
                    Message += "Evt17,Detect LOAD start";
                    break;
                case 0x31:
                    Message += "Evt18,Presence Sensor On";
                    break;
                case 0x32:
                    Message += "Evt19,Placement Sensor On";
                    break;
                case 0x38:
                    Message += "Evt20,Detect UNLOAD start";
                    break;
                case 0x39:
                    Message += "Evt21,Placement Sensor Off";
                    break;
                case 0x3A:
                    Message += "Evt22,Presence Sensor Off";
                    break;
                case 0x72:
                    //KEY-RESET or DI-RESET
                    break;
                case 0x73:
                    Message += "Evt70,Auto Reset";
                    break;
                case 0x80:
                    Message += "Evt23,Error ! CS0 timeout";
                    E84_E84Alarm("23", "Error ! CS0 timeout");
                    break;
                case 0x81:
                    E84_E84Alarm("24", "Error !TD0 timeout");
                    Message += "Evt24,Error ! TD0 timeout";
                    break;
                case 0x82:
                    E84_E84Alarm("25", "Error ! TP1 timeout");
                    Message += "Evt25,Error ! TP1 timeout";
                    break;
                case 0x83:
                    E84_E84Alarm("26", "Error ! TP2 timeout");
                    Message += "Evt26,Error ! TP2 timeout";
                    break;
                case 0x84:
                    E84_E84Alarm("27", "Error ! TP3-L1 timeout");
                    Message += "Evt27,Error ! TP3-L1 timeout";
                    break;
                case 0x86:
                    E84_E84Alarm("28", "Error ! TP3-U1 timeout");
                    Message += "Evt28,Error ! TP3-U1 timeout";
                    break;
                case 0x88:
                    E84_E84Alarm("29", "Error ! TP4 timeout");
                    Message += "Evt29,Error ! TP4 timeout";
                    break;
                case 0x8B:
                    E84_E84Alarm("30", "Error ! TP5 timeout");
                    Message += "Evt30,Error ! TP5 timeout";
                    break;
                case 0xF1:
                    //ES ON
                    break;
                case 0xF2:
                    Message += "Evt33,Clamp On";
                    break;
                case 0xF3:
                    Message += "Evt34,Clamp Off";
                    break;
                case 0xF4:
                    Message += "Evt35,Clamp On Error";
                    break;
                case 0xF5:
                    Message += "Evt36,Light Curtain On";
                    break;
                case 0xF6:
                    Message += "Evt37,Light Curtain Off";
                    break;
                case 0xF7:
                    Message += "Evt38,Light Curtain On Error";
                    break;
                case 0xF8:
                    E84_E84Alarm("39", "Alarm Error");
                    Message += "Evt39,Alarm Error";
                    break;
                case 0xF9:
                    //ALARM OFF
                    break;
                case 0xFA:
                    //ES ON
                    break;
                case 0xFB:
                    E84_E84Alarm("40", "ES Error");
                    Message += "Evt40,ES Error";
                    break;
                case 0xFC:
                    E84_E84Alarm("41", "Power Error");
                    Message += "Evt41,Power Error";
                    break;
                case 0xFD:
                    Message += "Evt42,Manual Mode";
                    SetLock(false);
                    break;
                case 0xFE:
                    //Auto Mode 不報
                    break;
                case 0xFF:
                    // Environment Signal not Ready for Standby Condition(ext.BUSY not in OFF status...)
                    break;
                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA6:
                case 0xA7:
                case 0xA8:
                case 0xA9:
                case 0xAD:
                case 0xAF:
                case 0xB0:
                case 0xC0:
                case 0xC1:
                case 0xC2:
                case 0xC3:
                case 0xC4:
                case 0xC5:
                case 0xC6:
                case 0xC7:
                    //E84 Handshake Signal Violate Error (E84  信號違反錯誤類別) 0xa0 - 0xc7
                    E84_E84Alarm("31", "Error ! E84 Sensor Sequence Error:" + Convert.ToInt32(RawEvtBye).ToString());
                    Message += "Evt31,Error ! E84 Sensor Sequence Error:" + Convert.ToInt32(RawEvtBye).ToString();
                    break;
                case 0xD0:
                case 0xD1:
                case 0xD2:
                case 0xD3:
                case 0xD4:
                case 0xD5:
                case 0xD6:
                case 0xDC:
                case 0xDD:
                case 0xDE:
                case 0xDF:
                case 0xE0:
                case 0xE1:
                case 0xE2:
                case 0xE3:
                case 0xE4:
                case 0xE5:
                case 0xE6:
                case 0xE7:
                    //FOUP SENSOR ERROR (FOUP Sensor  信號錯誤類別)
                    E84_E84Alarm("32", "Error ! Foup Sensor Sequence Error:" + Convert.ToInt32(RawEvtBye).ToString());
                    Message += "Evt32,Error ! Foup Sensor Sequence Error:" + Convert.ToInt32(RawEvtBye).ToString();
                    break;
                default:
                    E84_E84Alarm("43", "Undefine : " + Convert.ToInt32(RawEvtBye).ToString());
                    Message += "Evt43,Undefine : " + Convert.ToInt32(RawEvtBye).ToString();
                    break;
            }
            SendOutSocket(Message);



            //if(Event == E84Control.E84EventItem.HandoffAvailable & !OnOff)
            if (E84_COMPT_CLASS.NeedCheckPlaceRemove)
            {
                if (E84.E84Mode == E84Control.E84AccessMode.Auto)
                {
                    if (Foup == FoupStatus.Present)
                        SendOutSocket("Event," + LPName + ",FoupPlace");

                    if (Foup == FoupStatus.Absent)
                        SendOutSocket("Event," + LPName + ",FoupRemove");
                }

                E84_COMPT_CLASS = new E84_COMPT_Check();
            }


            /*
            case &H8  'waiting start up command
                Message = "Evt1, Waiting Enable Command"
                'ProcessQueue.Enqueue("E84,LP" & Convert.ToString(portnumber) & ",Enable")
            case &H9
                Message = "Evt2,Standby for E84 Procedure"
                vE84_IsAuto = True
                vE84_Error = False
            case &H10
                Message = "Evt3,GO ON , OHT Arrived"
                vE84_IsOHT_Transfer = False   ' reset transfer
            case &H11
                Message = "Evt4,CS ON"
            case &H12
                Message = "Evt5,VALID ON"
                vE84_Error = False
            case &H13
                Message = "Evt6,L_REQ ON"
                vE84_IsAuto = True
                vE84_IsLoading = True
            case &H14
                Message = "Evt7,U_REQ ON"
                vE84_IsAuto = True
                vE84_IsLoading = False
            case &H15
                Message = "Evt8,TR_REQ ON"
                vE84_IsAuto = True
                vE84_IsOHT_Transfer = True
            case &H16
                Message = "Evt9,READY ON"
                vE84_IsAuto = True
            case &H17
                Message = "Evt10,BUSY ON"
                vE84_IsOHT_Transfer = True
                vE84_IsAuto = True
            case &H18
                Message = "Evt11,L_REQ OFF"
                vE84_IsLoading = True
            case &H19
                Message = "Evt12,U_REQ OFF"
                vE84_IsLoading = False
            case &H1A
                Message = "Evt13,BUSY OFF, TR_REQ OFF, COMPT ON"
                vE84_IsOHT_Transfer = True
            case &H1B
                Message = "Evt71,TR_REQ OFF"
            case &H1D
                Message = "Evt14,READY OFF"
            case &H1E
                Message = "Evt72,VALID OFF"
            case &H20
                Message = "Evt15,VALID OFF, COMPT OFF, CS0 OFF"
                vE84_Error = False
            case &H21
                Message = "Evt16,GO OFF"
            case &H30
                Message = "Evt17,Detect LOAD start"
                vE84_IsLoading = True
            case &H31
                Message = "Evt18,Presence Sensor On"
            case &H32
                Message = "Evt19,Placement Sensor On"
            case &H38
                Message = "Evt20,Detect UNLOAD start"
                vE84_IsLoading = False
            case &H39
                Message = "Evt21,Placement Sensor Off"
            case &H3A
                Message = "Evt22,Presence Sensor Off"
            case &H73
                Message = "Evt70,Auto Reset"
            case &H80
                Message = "Evt23,Error ! CS0 timeout"
                vE84_Error = True
            case &H81
                Message = "Evt24,Error ! TD0 timeout"
                vE84_Error = True
            case &H82
                Message = "Evt25,Error ! TP1 timeout"
                vE84_Error = True
            case &H83
                Message = "Evt26,Error ! TP2 timeout"
                vE84_Error = True
            case &H84
                Message = "Evt27,Error ! TP3-L1 timeout"
                vE84_Error = True
            case &H86
                Message = "Evt28,Error ! TP3-U1 timeout"
                vE84_Error = True
            case &H88
                Message = "Evt29,Error ! TP4 timeout"
                vE84_Error = True
            case &H8B
                Message = "Evt30,Error ! TP5 timeout"
                vE84_Error = True
            case &HA0, &HA1, &HA2, &HA3, &HA4, &HA5, &HA6, &HA7, &HA8, &HA9, &HAD, &HAF, &HB0, &HC0, &HC1, &HC2, &HC3, &HC4, &HC5, &HC6, &HC7
                Message = "Evt31,Error ! E84 Sensor Sequence Error"
                vE84_Error = True
            case &HD0, &HD1, &HD2, &HD3, &HD4, &HD5, &HD6, &HDC, &HDD, &HDE, &HDF, &HE0, &HE1, &HE2, &HE3, &HE4, &HE5, &HE6, &HE7
                Message = "Evt32,Error ! Foup Sensor Sequence Error"
                vE84_Error = True
            case &HF2
                Message = "Evt33,Clamp On"
            case &HF3
                Message = "Evt34,Clamp Off"
            case &HF4
                Message = "Evt35,Clamp On Error"
            case &HF5
                Message = "Evt36,Light Curtain On"
            case &HF6
                Message = "Evt37,Light Curtain Off"
            case &HF7
                Message = "Evt38,Light Curtain On Error"
            case &HF8
                Message = "Evt39,Alarm Error"
                vE84_Error = True
            case &HFB
                Message = "Evt40,ES Error"
                vE84_Error = True
            case &HFC
                Message = "Evt41,Power Error"
                vE84_Error = True
            case &HFD
                Message = "Evt42,Manual Mode"
                vE84_IsAuto = False
            case Else
                Message = "Evt43,Undefine : " + ByteArray(2).ToString
             */


        }
        private void E84_E84Reply(E84Control.E84CommandItem Action, bool Result)
        {
            // throw new NotImplementedException();
            string Message = "";

            switch (Action)
            {
                case E84Control.E84CommandItem.Auto: //2
                    Message += "E84_SetAuto," + LPName;
                    break;
                case E84Control.E84CommandItem.Manual: //42
                    Message += "E84_SetManual," + LPName;
                    break;
                case E84Control.E84CommandItem.Reset:
                    Message += "E84_Reset," + LPName;
                    break;
                case E84Control.E84CommandItem.Enable:
                    Message += "E84_Enable" + LPName;
                    break;

            }

            if (Result)
                Message += ",0";
            else
                Message += ",1";


            SendOutSocket(Message);
        }

        string _CarrierID = string.Empty;
        public string CarrierID
        {
            get
            {
                return _CarrierID;
            }
            set
            {
                _CarrierID = value;
            }
        }

        private void RFID_ReadComplete(string ID, bool Result)
        {
            string out_str = "ReadFoupID," + LPName + ",";

            if (Result)
            {
                out_str += "0," + ID;
                CarrierID = ID;
            }
            else
            {
                out_str += "1," + ID;
                CarrierID = string.Empty;
            }

            SendOutSocket(out_str);
        }

        private void E84Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //throw new NotImplementedException();
        }


        private void MainCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] raw_byte = new byte[128];
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)
            {
                Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                Array.Resize(ref raw_byte, length);
                LoadPortQueue.EnQueue(raw_byte);
            }
        }

        bool SaveSetting()
        {
            return true;
        }

        private void button_ReloadSetting_Click(object sender, EventArgs e)
        {
            LoadSetting(_LPNumber);
        }

        private void button_SaveTest_Click(object sender, EventArgs e)
        {

        }

        private void button_LP_Command_Click(object sender, EventArgs e)
        {
            LPCommand cmd = (LPCommand)Enum.Parse(typeof(LPCommand), listBox_LP_Command.Text, false);
            string error_msg = null;

            Log.WriteOperationLog(DateTime.Now, LPName + ": User press " + cmd.ToString());

            switch (cmd)
            {
                case LPCommand.ResetError:
                    CLR_Error();
                    break;
                case LPCommand.ReadFoupID:
                    _RFID.ReadRfid();
                    break;
                case LPCommand.GetStatus:
                    GetStatus(false);
                    break;
                case LPCommand.GetWaferSlot:
                    Qry_WaferSlot(false);
                    break;
                case LPCommand.Home:
                    Cmd_Home(out error_msg);
                    break;

                case LPCommand.Load:
                    Cmd_Load(out error_msg);
                    break;
                case LPCommand.Map:
                    Cmd_Map(out error_msg);
                    break;
                case LPCommand.Clamp:
                    Cmd_Clamp(out error_msg);
                    break;
                case LPCommand.Unclamp:
                    Cmd_Unclamp(out error_msg);
                    break;
                case LPCommand.Unload:
                    Cmd_Unload(out error_msg);
                    break;
                case LPCommand.ResetLock:
                    SetLock(false);
                    break;

                //case LPCommand.Stop:
                //    Cmd_Stop(out error_msg);
                //    break;
            }

            if (error_msg != null)
                MessageBox.Show(error_msg, LPName);

        }
        //=====COM port 通訊=====
        private string LP_CommandString = "0000";
        private const byte StartByte = 0x01;
        private const byte LastByte = 0x0D;
        private const int Wait = 0;
        private const int Receiving = 1;
        private const int CloseCom = 10;
        int ReceiveState = Wait;
        private List<byte> Temp = new List<byte>();

        bool Socket_WaferSlot = false;
        bool Socket_Status = false;
        bool GetStatus_Reply = false;
        public bool GetStatus(bool _Socket_Status, bool wait_reply = false)
        {
            //if (Error)
            //{
            //    Log.Write(LPName, "It's Error. Cannot do Qry_Status");
            //    return false;
            //}

            //if (Busy)
            //{
            //    string out_str = "GetStatus," + LPName + ",";
            //    /*
            //    GetStatus: will Reply Mode,Error,Foup,Clamp,Door Status Description
            //    Example: GetStatus,P1,(Online/Teaching/Maintain/Unknow),
            //    (No Error/Recoverable/Unrecoverable/Unknow),
            //    (Present/Absent/Unknow),
            //    (Clamp/Unclamp/Unknow),
            //    (Open/Close/Unknow)
            //    */
            //    SendOutSocket(out_str);
            //    Log.Write(LPName, "It's Busy. Cannot do Qry_Status");
            //    return false;
            //}

            //if (Lock)
            //{
            //    Log.Write(LPName, "It's Lock. Cannot do Qry_Status");
            //    return false;
            //}

            if (!RunCMD(LPCommand.GetStatus))
                return false;
            Socket_Status = _Socket_Status;
            GetStatus_Reply = false;
            int count = 0;

            if (wait_reply)
            {
                while (!GetStatus_Reply)
                {
                    Thread.Sleep(50);
                    count += 50;
                    if (count >= 3000)
                        return false;
                }
            }



            //backgroundWorker_LP_Cmd.RunWorkerAsync(LPCommand.GetStatus);
            return true;
        }
        public bool Qry_WaferSlot(bool _Socket_WaferSlot)
        {
            if (Error)
            {
                Log.Write(LPName, "It's Error. Cannot do Qry_WaferSlot");
                return false;
            }

            if (Busy)
            {
                Log.Write(LPName, "It's Busy. Cannot do Qry_WaferSlot");
                return false;
            }

            if (Lock)
            {
                Log.Write(LPName, "It's Lock. Cannot do Qry_WaferSlot");
                return false;
            }

            if (!RunCMD(LPCommand.GetWaferSlot))
                return false;

            Socket_WaferSlot = _Socket_WaferSlot;
            //  backgroundWorker_LP_Cmd.RunWorkerAsync(LPCommand.GetWaferSlot);
            return true;
        }
        public bool CLR_Error()
        {
            //if (Error)
            //{
            //    Log.Write(LPName, "It's Error. Cannot do Cmd_Load");
            //    return false;
            //}

            //if (Busy)
            //{
            //    Log.Write(LPName, "It's Busy. Cannot do Cmd_Load");
            //    return false;
            //}

            //if (Lock)
            //{
            //    error_message = "It's Error. Cannot do Home";
            //    Log.Write(LPName, error_message);
            //    return false;
            //}


            if (!RunCMD(LPCommand.ResetError))
                return false;

            return true;

            //backgroundWorker_LP_Cmd.RunWorkerAsync(LPCommand.ResetError);
            //return true;
        }
        public bool Cmd_Load(out string error_message)
        {
            error_message = null;
            if (Error)
            {
                error_message = "It's Error. Cannot do Load";
                Log.Write(LPName, error_message);
                SendOutSocket("Load," + LPName + "," + error_message);
                return false;
            }

            if (Busy)
            {
                error_message = "It's Busy. Cannot do Load";
                Log.Write(LPName, error_message);
                SendOutSocket("Load," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Load";
                Log.Write(LPName, error_message);
                SendOutSocket("Load," + LPName + "," + error_message);
                return false;
            }

            if (Foup != FoupStatus.Present)
            {
                error_message = "Foup is not Present. Cannot do Load";
                Log.Write(LPName, error_message);
                SendOutSocket("Load," + LPName + "," + error_message);
                return false;
            }

            if (!RunCMD(LPCommand.Load))
                return false;
            //   backgroundWorker_LP_Cmd.RunWorkerAsync(LPCommand.Load);
            return true;
        }
        public bool Cmd_Unload(out string error_message)
        {
            error_message = null;
            if (Error)
            {
                error_message = "It's Error. Cannot do Unload";
                Log.Write(LPName, error_message);
                SendOutSocket("Unload," + LPName + "," + error_message);
                return false;
            }

            if (Busy)
            {
                error_message = "It's Error. Cannot do Unload";
                Log.Write(LPName, error_message);
                SendOutSocket("Unload," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Unload";
                Log.Write(LPName, error_message);
                SendOutSocket("Unload," + LPName + "," + error_message);
                return false;
            }

            if (Foup != FoupStatus.Present)
            {
                error_message = "Foup is not Present. Cannot do Unload";
                Log.Write(LPName, error_message);
                SendOutSocket("Unload," + LPName + "," + error_message);
                return false;
            }


            if (!RunCMD(LPCommand.Unload))
                return false;

            return true;
        }
        public bool Cmd_Clamp(out string error_message)
        {
            error_message = null;
            if (Error)
            {
                error_message = "It's Error. Cannot do Clamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Clamp," + LPName + "," + error_message);
                return false;
            }

            if (Busy)
            {
                error_message = "It's Busy. Cannot do Clamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Clamp," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Clamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Clamp," + LPName + "," + error_message);
                return false;
            }

            if (!RunCMD(LPCommand.Clamp))
                return false;
            return true; ;
        }
        public bool Cmd_Unclamp(out string error_message)
        {
            error_message = null;
            if (Error)
            {
                error_message = "It's Error. Cannot do Unlamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Unlamp," + LPName + "," + error_message);
                return false;
            }

            if (Busy)
            {
                error_message = "It's Busy. Cannot do Unlamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Unlamp," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Unlamp";
                Log.Write(LPName, error_message);
                SendOutSocket("Unlamp," + LPName + "," + error_message);
                return false;
            }

            if (!RunCMD(LPCommand.Unclamp))
                return false;
            return true;
        }
        public bool Cmd_Map(out string error_message)
        {
            error_message = null;
            if (Error)
            {
                error_message = "It's Error. Cannot do Map";
                Log.Write(LPName, error_message);
                SendOutSocket("Map," + LPName + "," + error_message);
                return false;
            }

            if (Busy)
            {
                error_message = "It's Busy. Cannot do Map";
                Log.Write(LPName, error_message);
                SendOutSocket("Map," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Map";
                Log.Write(LPName, error_message);
                SendOutSocket("Map," + LPName + "," + error_message);
                return false;
            }


            if (Foup != FoupStatus.Present)
            {
                error_message = "Foup is not Present. Cannot do Map";
                Log.Write(LPName, error_message);
                SendOutSocket("Map," + LPName + "," + error_message);
                return false;
            }


            if (!RunCMD(LPCommand.Map))
                return false;

            return true;
        }
        public bool Cmd_Home(out string error_message)
        {
            error_message = null;
            //if (Error)
            //{
            //    error_message = "It's Error. Cannot do Home";
            //    Log.Write(LPName, error_message);
            //    return false;
            //}

            if (Busy)
            {
                error_message = "It's Busy. Cannot do Home";
                Log.Write(LPName, error_message);
                SendOutSocket("Home," + LPName + "," + error_message);
                return false;
            }

            if (Lock)
            {
                error_message = "It's Lock. Cannot do Home";
                Log.Write(LPName, error_message);
                SendOutSocket("Home," + LPName + "," + error_message);
                return false;
            }
            if (!RunCMD(LPCommand.Home))
                return false;

            return true;
        }
        //public bool Cmd_Stop(out string error_message)
        //{
        //    error_message = null;


        //    if (!RunCMD(LPCommand.Stop))
        //        return false;

        //    return true;
        //}

        bool RunCMD(LPCommand _cmd)
        {
            if (Busy)
            {
                if (_cmd != LPCommand.GetStatus & _cmd != LPCommand.GetWaferSlot & _cmd != LPCommand.ReadFoupID & _cmd != LPCommand.ResetError)
                    return false;
            }

            LPCmdObject lco = new LPCmdObject(_cmd);
            LPCmdObjectQueue.EnQueue(lco);
            return true;
        }


        //public void ResetLock()
        //{
        //    SetLock(false);
        //}

        private void MainComDataSend(string Command_String)
        {
            try
            {
                string Send_String = LP_CommandString + Command_String + ";";
                List<byte> Command_Byte = new List<Byte>(Encoding.ASCII.GetBytes(Send_String));
                byte[] check_sum = CheckSum(Command_Byte.ToArray());
                Command_Byte.Insert(0, StartByte);
                Command_Byte.AddRange(check_sum);
                Command_Byte.Add(LastByte);
                MainCom.Write(Command_Byte.ToArray(), 0, Command_Byte.Count);//送出指令
                Log.Write("Host->LP", Send_String + Encoding.ASCII.GetString(check_sum));
            }
            catch (Exception ex)
            {
                Log.Write("Host->LP", "Exception:" + ex.ToString());
            }
        }

        private byte[] CheckSum(byte[] data)
        {
            byte Sum = 0x00;
            foreach (byte SingleByte in data)
                Sum += SingleByte;
            return Encoding.ASCII.GetBytes(Convert.ToString(Sum, 16).ToUpper());
        }

        private void button_E84_Command_Click(object sender, EventArgs e)
        {
            E84Command cmd = (E84Command)Enum.Parse(typeof(E84Command), listBox_E84_Command.Text, false);
            string error_msg = null;
            Log.WriteOperationLog(DateTime.Now, LPName + ": User press " + cmd.ToString());
            switch (cmd)
            {
                case E84Command.E84_Enable:
                    _E84.SetEnable();
                    break;
                //case E84Command.E84_LinkTest:
                //    _E84.SendHeartBeat();
                //    break;
                case E84Command.E84_Reset:
                    _E84.SendReset();
                    break;
                case E84Command.E84_SetAuto:
                    _E84.SetAuto();
                    break;
                case E84Command.E84_SetManual:
                    _E84.SetManual();
                    break;
                case E84Command.E84_SetESOn:
                    _E84.SetES(true);
                    break;
                case E84Command.E84_SetESOff:
                    _E84.SetES(false);
                    break;
                case E84Command.E84_SetTimeout:
                    {
                        AML.E84Control.E84TpnItem e84_cmd = (AML.E84Control.E84TpnItem)Enum.Parse(typeof(AML.E84Control.E84TpnItem), comboBox_E84_TP.Text, false);
                        byte t = Convert.ToByte(numericUpDown_TPsec.Value);
                        Log.Write(LPName, "E84_SetTimeout in " + comboBox_E84_TP.Text + " with value " + t + " seconds");
                        E84.SetTpTimeout(e84_cmd, t);
                    }
                    //E84.SetTpTimeout()
                    break;
            }
        }
        private void backgroundWorker_E84_DoWork(object sender, DoWorkEventArgs e)
        {

        }
        private void backgroundWorker_E84_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private void backgroundWorker_LP_Send_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
            while (true)
            {
                LPCmdObject cmd = LPCmdObjectQueue.DeQueue(5000);

                if (cmd != null)
                {
                    switch (cmd.Cmd)
                    {
                        case LPCommand.GetStatus:
                            MainComDataSend("GET:STAS");
                            break;
                        case LPCommand.GetWaferSlot:
                            MainComDataSend("GET:MDAT");
                            break;
                        case LPCommand.ResetError:
                            MainComDataSend("SET:RSET");
                            break;
                        case LPCommand.ReadFoupID:
                            _RFID.ReadRfid();
                            break;
                        case LPCommand.Home:
                            SetLock(true);
                            MainComDataSend("MOV:ORGN");
                            break;
                        case LPCommand.Load:
                            SetLock(true);
                            MainComDataSend("MOV:FPML");
                            break;
                        case LPCommand.Unload:
                            SetLock(true);
                            MainComDataSend("MOV:FPMU");
                            break;
                        case LPCommand.Clamp:
                            SetLock(true);
                            MainComDataSend("MOV:FCCL");
                            break;
                        case LPCommand.Unclamp:
                            SetLock(true);
                            MainComDataSend("MOV:FCOP");
                            break;
                        case LPCommand.Map:
                            SetLock(true);
                            MainComDataSend("MOV:MAPP");
                            break;
                        //case LPCommand.Stop:
                        //    MainComDataSend("MOV:STPP");
                        //    break;
                        case LPCommand.ResetLock:
                            // ResetLock();
                            SetLock(false);
                            break;
                    }
                }
            }
        }
        private void backgroundWorker_LP_Receive_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
            byte[] Rec;
            while (true)
            {
                Rec = LoadPortQueue.DeQueue(5000);

                if (Rec == null)
                {
                    if (ReceiveState != Wait)
                    {
                        Log.Write(LPName, "回應逾時(Timeout)" + Encoding.ASCII.GetString(Temp.ToArray()));
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
                            Log.Write(LPName, "Receive異常字元:0x" + Convert.ToString(raw_data[i], 16).ToUpper());
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
                            Log.Write(LPName, "Receive:" + CompleteString + ":drop");
                            Log.Write(LPName, "Receive Check sum異常:" + CheckSumRec + " 應為:" + Encoding.ASCII.GetString(Check));
                        }
                        else
                        {
                            Log.Write("Receive", CompleteString);
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
        private void ReceiveHandler(string RecString)
        {
            if (RecString.Substring(0, 2) != "00" && RecString.Substring(0, 2) != "\0\0")
            {
                Log.Write(LPName, "異常發生 請參閱Alarm訊息");
                ErrorHandler(RecString.Substring(0, 2));
                return;
            }
            string RecType = RecString.Substring(4, 3);
            string RecInfo = RecString.Substring(8, 4);
            if (RecType == "GET")
            {
                switch (RecInfo)
                {
                    case "STAS":
                        {
                            string StatusData = RecString.Substring(RecString.IndexOf('/') + 1, 20);

                            if (StatusData[0] == '0')
                                _Error = ErrorStatus.NoError;
                            else if (StatusData[0] == 'A')
                                _Error = ErrorStatus.Recoverable;
                            else if (StatusData[0] == 'B')
                                _Error = ErrorStatus.Unrecoverable;
                            else
                                _Error = ErrorStatus.Unknown;

                            //InternalBusy = (StatusData[3] != '0');
                            if (StatusData[1] == '0')
                                Mode = ModeStatus.Online;
                            else if (StatusData[1] == '1')
                                Mode = ModeStatus.Teaching;
                            else if (StatusData[1] == '2')
                                Mode = ModeStatus.Maintain;
                            else
                                Mode = ModeStatus.Unknown;

                            if (StatusData[6] == '0')
                                Foup = FoupStatus.Absent;
                            else if (StatusData[6] == '1')
                                Foup = FoupStatus.Present;
                            else
                                Foup = FoupStatus.Unknown;

                            /*
                            if (StatusData[12] == '1')  //用DOCK來看，10才是真的Door Open
                                Door = DoorStatus.Open;
                            else if (StatusData[12] == '0')
                                Door = DoorStatus.Close;
                            else
                                Door = DoorStatus.Unknown;
                            */

                            if (StatusData[10] == '0' & StatusData[12] == '1')  //12是Dock，10是Doorn
                                Door = DoorStatus.Open;
                            else if (StatusData[10] == '1' & StatusData[12] == '0')
                                Door = DoorStatus.Close;
                            else
                                Door = DoorStatus.Unknown;

                            if (StatusData[7] == '0')
                                Clamp = ClampStatus.Unclamp;
                            else if (StatusData[7] == '1')
                                Clamp = ClampStatus.Clamp;
                            else
                                Clamp = ClampStatus.Unknown;

                            if (StatusData[11] == '0')
                                WaferProtrusionSensor = WaferProtrusionSensorStatus.Shading;
                            else if (StatusData[11] == '1')
                                WaferProtrusionSensor = WaferProtrusionSensorStatus.Lighting;
                            else
                                WaferProtrusionSensor = WaferProtrusionSensorStatus.Unknown;

                            string ErrorCode = (StatusData.Substring(4, 2));
                            if (ErrorCode != "00")
                                ErrorHandler(ErrorCode);


                            Socket_WaferSlot = false;
                            string out_str = "GetStatus," + LPName;
                            out_str += "," + Mode.ToString();
                            out_str += "," + _Error.ToString();
                            out_str += "," + Foup.ToString();
                            out_str += "," + Clamp.ToString();
                            out_str += "," + Door.ToString();

                            if (Socket_Status)
                            {
                                SendOutSocket(out_str);
                            }


                            /*
                            GetStatus: will Reply Mode,Error,Foup,Clamp,Door Status Description
                            Example: GetStatus,P1,(Online/Teaching/Maintain/Unknow),
                            (No Error/Recoverable/Unrecoverable/Unknow),
                            (Present/Absent/Unknow),
                            (Clamp/Unclamp/Unknow),
                            (Open/Close/Unknow)
                            */

                            Log.Write(LPName, "更新Loadport狀態:" + out_str);
                            GetStatus_Reply = true;
                            //Log.WriteSystemLog(LPName, "更新Loadport狀態");
                        }
                        break;
                    case "MDAT":
                        {
                            string MappingData = RecString.Substring(RecString.IndexOf('/') + 1, SlotCount);
                            for (int i = 0; i < SlotCount; i++)
                            {
                                int ThisWafer = Convert.ToInt32(MappingData.Substring(i, 1));
                                //if (ThisWafer > 2)
                                //    ThisWafer = 2;

                                if (ThisWafer >= 0 & ThisWafer <= 5)
                                    WaferPresent[SlotCount - 1 - i] = (WaferState)ThisWafer;
                                else
                                    WaferPresent[SlotCount - 1 - i] = WaferState.WaferUnknown;
                            }

                            RefreshWaferSlotUI();
                            //GetCurrentWaferSlot,P1,1,1,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,1,1,1 25->1
                            if (Socket_WaferSlot)
                            {
                                Socket_WaferSlot = false;
                                string out_str = "GetWaferSlot," + LPName;
                                for (int i = SlotCount - 1; i >= 0; i--)
                                {
                                    //if (WaferPresent[i] == WaferState.NoWafer)
                                    //    out_str += ",0";
                                    //else if (WaferPresent[i] == WaferState.WaferMapped)
                                    //    out_str += ",1";
                                    //else
                                    //    out_str += ",9";

                                    out_str += "," + (int)WaferPresent[i];

                                }
                                SendOutSocket(out_str);
                            }

                            Log.Write(LPName, "取得MAPPING結果");
                        }
                        break;
                }
                //Qry_Status(false);
            }
            else if (RecType == "MOV")
            {/*
                switch(RecInfo)
                {
                    case "FPML":
                        UI.WriteLOG("EFEM", Name, "Loadport開始進貨");
                        break;
                    case "FPMU":
                        UI.WriteLOG("EFEM", Name, "Loadport開始出貨");
                        break;
                    case "MAPP":
                        UI.WriteLOG("EFEM", Name, "Loadport開始Mapping");
                        break;
                    case "ORGN":
                        UI.WriteLOG("EFEM", Name, "Loadport開始進行原點復歸");
                        break;
                }*/
            }
            else if (RecType == "INF")
            {
                switch (RecInfo)
                {
                    case "PDOF":
                        SetBusy(false);
                        Foup = FoupStatus.Absent;
                        for (int i = 0; i < SlotCount; i++)
                            WaferPresent[i] = WaferState.WaferUnknown;
                        RefreshWaferSlotUI();
                        if (E84.E84Mode == E84Control.E84AccessMode.Manual)
                            SendOutSocket("Event," + LPName + ",FoupRemove");
                        break;
                    case "PDON":
                        SetBusy(false);
                        Foup = FoupStatus.Present;
                        for (int i = 0; i < SlotCount; i++)
                            WaferPresent[i] = WaferState.WaferUnknown;
                        RefreshWaferSlotUI();
                        if (E84.E84Mode == E84Control.E84AccessMode.Manual)
                            SendOutSocket("Event," + LPName + ",FoupPlace");
                        break;
                    case "FPML":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Door = DoorStatus.Open;
                            string out_str = "Load," + LPName + ",0";
                            SendOutSocket(out_str);
                            Qry_WaferSlot(false);

                        }
                        break;
                    case "FPMU":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Door = DoorStatus.Close;
                            for (int i = 0; i < SlotCount; i++)
                                WaferPresent[i] = WaferState.WaferUnknown;
                            RefreshWaferSlotUI();
                            string out_str = "Unload," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "FCCL":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Clamp = ClampStatus.Clamp;
                            string out_str = "Clamp," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "FCOP":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Clamp = ClampStatus.Unclamp;
                            string out_str = "Unclamp," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "MAPP":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Qry_WaferSlot(false);
                            string out_str = "Map," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "ORGN":
                        {
                            SetBusy(false);
                            SetLock(false);
                            Clamp = ClampStatus.Unclamp;
                            Door = DoorStatus.Close;
                            string out_str = "Home," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "RSET":
                        {
                            SetBusy(false);
                            SetLock(false);
                            string out_str = "ResetError," + LPName + ",0";
                            SendOutSocket(out_str);
                        }
                        break;

                }
                GetStatus(false);
            }
            else if (RecType == "ABS")
            {
                _Error = ErrorStatus.Unknown;
                int ALID = 0;
                string ALTX = string.Empty;
                switch (RecInfo)
                {
                    case "FPML":
                        ALID = 501;
                        ALTX = "Load Fail";
                        Log.Write(LPName, "Load 失敗");
                        SendOutSocket("Load," + LPName + ",1");
                        break;
                    case "FPMU":
                        ALID = 502;
                        ALTX = "Unload Fail";
                        Log.Write(LPName, "Unload 失敗");
                        SendOutSocket("Unload," + LPName + ",1");
                        break;
                    case "MAPP":
                        ALID = 503;
                        ALTX = "Mapping Fail";
                        Log.Write(LPName, "Mapping 失敗");
                        SendOutSocket("Map," + LPName + ",1");
                        break;
                    case "ORGN":
                        ALID = 504;
                        ALTX = "Home Fail";
                        Log.Write(LPName, "Home 失敗");
                        SendOutSocket("Home," + LPName + ",1");
                        break;
                    case "FCCL":
                        {
                            ALID = 505;
                            ALTX = "Clamp Fail";
                            Clamp = ClampStatus.Unknown;
                            string out_str = "Clamp," + LPName + ",1";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "FCOP":
                        {
                            ALID = 506;
                            ALTX = "Unclamp Fail";
                            Clamp = ClampStatus.Unknown;
                            string out_str = "Unclamp," + LPName + ",1";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "RSET":
                        {
                            ALID = 507;
                            string out_str = "ResetError," + LPName + ",1";
                            SendOutSocket(out_str);
                        }
                        break;
                    case "ERRS":
                        Log.Write(LPName, "異常發生 請參閱Alarm訊息");
                        ErrorHandler(RecString.Substring(RecString.IndexOf('/') + 1, 2));
                        break;

                }

                ALID = ALID + LPAlarmOffset + (LPNumber * 1000000);
                if ((Enable_Alarm) && (ALID != 0))
                    SendOutSocket("Alm," + LPName + "," + ALID + "," + ALTX);



                GetStatus(false);

            }
            RefreshLPStatus();
        }
        private void ErrorHandler(string ErrorCode)
        {
            try
            {
                Log.Write(LPName, "異常發生 請參閱Alarm訊息,ErrorCode:" + ErrorCode + ", Description:" + LP_ErrorCode[ErrorCode]);
                SendOutSocket("Alarm," + LPName + ",ErrorCode:" + ErrorCode + LP_ErrorCode[ErrorCode]);
                int ALID = Convert.ToInt32(ErrorCode, 16) + LPAlarmOffset + (LPNumber * 1000000);

                if (Enable_Alarm)
                    SendOutSocket("Alm," + LPName + "," + ALID + "," + LP_ErrorCode[ErrorCode]);
            }
            catch
            {
                Log.Write(LPName, "異常發生 請參閱Alarm訊息,ErrorCode:" + ErrorCode + ", Description: Unknown ErrorCode");
                SendOutSocket("Alarm," + LPName + ",ErrorCode:" + ErrorCode + " Unknown ErrorCode");
                int ALID = LPAlarmOffset + (LPNumber * 1000000);

                if (Enable_Alarm)
                    SendOutSocket("Alm," + LPName + "," + ALID + ",Unknown Error");
            }
        }
        public void RefreshWaferSlotUI()
        {
            if (this.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate()
                {
                    RefreshWaferSlotUI();
                });
            }
            else
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    DGV_Wafer.Rows[i].Cells["Column_State"].Value = WaferPresent[SlotCount - i - 1].ToString();

                    switch (WaferPresent[SlotCount - i - 1])
                    {
                        case WaferState.NoWafer:
                            DGV_Wafer.Rows[i].Cells["Column_State"].Style.BackColor = Color.WhiteSmoke;
                            break;
                        case WaferState.WaferMapped:
                            DGV_Wafer.Rows[i].Cells["Column_State"].Style.BackColor = Color.RoyalBlue;
                            break;
                        case WaferState.WaferUnknown:
                            DGV_Wafer.Rows[i].Cells["Column_State"].Style.BackColor = Color.Gainsboro;
                            break;
                        default:
                            DGV_Wafer.Rows[i].Cells["Column_State"].Style.BackColor = Color.LightCoral;
                            break;
                    }
                }
            }
        }
        public void SetAllWaferUnknown()
        {
            for (int i = 0; i < SlotCount; i++)
                WaferPresent[i] = WaferState.WaferUnknown;
            RefreshWaferSlotUI();
        }

        public bool CheckAllWaferNormal(out string FailReason)
        {
            FailReason = "";

            for (int i = 0; i < SlotCount; i++)
            {
                if ((WaferPresent[i] != WaferState.NoWafer) && (WaferPresent[i] != WaferState.WaferMapped))
                {
                    FailReason = "Slot " + (i + 1) + " Status is " + WaferPresent[i].ToString();
                    return false;
                }
            }

            return true;
        }
        private void RefreshLPStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    label_Mode.Text = Mode.ToString();
                    label_Clamp.Text = Clamp.ToString();
                    label_Door.Text = Door.ToString();
                    label_Protrusion.Text = WaferProtrusion.ToString();
                });
            }
        }

        private void timer1sec_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (Error)
                    label_Error.Visible = true;//; !label_Error.Visible;
                else
                    label_Error.Visible = false;


                if (Lock)
                    label_Lock.Visible = true; //!LockLabel.Visible;
                else
                    label_Lock.Visible = false;

                if (Busy)
                    label_Busy.Visible = true; //!BusyLabel.Visible;
                else
                    label_Busy.Visible = false;

                if (MainCom.IsOpen)
                    Label_MainPort.BackColor = Color.Lime;
                else
                    Label_MainPort.BackColor = Color.Salmon;

                if (RFIDCom.IsOpen)
                    label_RFIDPort.BackColor = Color.Lime;
                else
                    label_RFIDPort.BackColor = Color.Salmon;

                if (E84Com.IsOpen)
                    Label_E84Port.BackColor = Color.Lime;
                else
                    Label_E84Port.BackColor = Color.Salmon;


                //SerialPort MainCom;
                //SerialPort RFIDCom;
                //SerialPort E84Com;
            }
        }

        private void LoadPort_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                GetStatus(false);
            }
        }
        public class LPCmdObject
        {
            public LPCommand Cmd = LPCommand.GetStatus;
            //public bool Execute = false;
            public LPCmdObject(LPCommand _cmd)
            {
                Cmd = _cmd;
            }
        }

        private void listBox_E84_Command_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_E84_Command.Text == E84Command.E84_SetTimeout.ToString())
            {
                comboBox_E84_TP.Visible = true;
                numericUpDown_TPsec.Visible = true;
                label_E84_TP.Visible = true;
            }
            else
            {
                comboBox_E84_TP.Visible = false;
                numericUpDown_TPsec.Visible = false;
                label_E84_TP.Visible = false;
            }
        }
    }

    public class E84_COMPT_Check
    {
        //public bool BusyOn = false;
        //public bool BusyOff = false;
        public bool COMPTOn = false;
        public bool COMPTOff = false;

        public bool NeedCheckPlaceRemove
        {
            get
            {
                return COMPTOn & COMPTOff;
            }
        }

    }




}
