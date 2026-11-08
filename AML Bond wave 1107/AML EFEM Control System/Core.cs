using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AML_Library;
using System.Diagnostics;
using System.Net.Sockets;

namespace AML
{
    public delegate void ThrowOutSocketReply(string Data);//接到資料
    public enum ControlState { Remote, Local }
    public partial class Core : UserControl
    {
        public Core()
        {
            InitializeComponent();
        }

        public List<LoadPort> LP = new List<LoadPort>();
        public AML_LogPage Log = new AML_LogPage();
        public Robot[] Rb = new Robot[2];
        public Aligner[] Al = new Aligner[2];

        ControlState _ControlState = ControlState.Local;
        ControlState ControlState
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate() { ControlState = value; });
                    return;
                }

                _ControlState = value;

                if (ControlState == ControlState.Local)
                {
                    button_Exit.Enabled = true;
                    button_Local.BackColor = Color.Lime;
                    button_Remote.BackColor = SystemColors.Control;
                    button_Robot1.Enabled = true;
                    button_Aligner1.Enabled = true;
                    button_Robot2.Enabled = true;
                    button_Aligner2.Enabled = true;
                    panel_LP.Enabled = true;
                    panel_IO.Enabled = true;

                }
                else
                {
                    button_Remote.BackColor = Color.Cyan;
                    button_Local.BackColor = SystemColors.Control;
                    if (Rb[0].Visible)
                        Rb[0].Hide();

                    if (Rb[1] != null && Rb[1].Visible)
                        Rb[1].Hide();

                    if (Al[0] != null && Al[0].Visible)
                        Al[0].Hide();

                    if (Al[1] != null && Al[1].Visible)
                        Al[1].Hide();

                    for (int i = 0; i < LP.Count; i++)
                    {
                        if (LP[i].Visible)
                            LP[i].Hide();
                    }

                    for (int i = 0; i < IO.Length; i++)
                    {
                        if (AdamFrom[i] != null && AdamFrom[i].Visible)
                            AdamFrom[i].Hide();
                    }

                    button_Exit.Enabled = false;
                    button_Robot1.Enabled = false;
                    button_Aligner1.Enabled = false;
                    button_Robot2.Enabled = false;
                    button_Aligner2.Enabled = false;
                    panel_LP.Enabled = false;
                    panel_IO.Enabled = false;
                }

                Log.WriteSystemLog("ControlState Set to:" + ControlState.ToString());
            }
            get
            {
                return _ControlState;
            }
        }


        string Version = "";
        private SocketServer Socket;
        public ServerState ConnectState
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate() { ConnectState = value; });
                    return;
                }
                switch (value)
                {
                    case ServerState.Closed:
                        ConnectiionLabel.BackColor = Color.Salmon;
                        break;
                    case ServerState.Listening:
                        ConnectiionLabel.BackColor = Color.Yellow;
                        break;
                    case ServerState.Connect:
                        ConnectiionLabel.BackColor = Color.Lime;
                        break;
                }
                ConnectiionLabel.Text = value.ToString();
                _connectstate = value;
            }
            get { return _connectstate; }
        }

        ServerState _connectstate = ServerState.Closed;
        MainForm MF;

        private Adam6000s[] _IO;
        public Adam6000s[] IO //Core控制IO請用此
        {
            get
            {
                return _IO;
            }
        }
        AdamIOForm[] AdamFrom;
        bool Enable_Alarm = false;
        public async void Init(MainForm _MF, string _version)
        {
            //var tcpClient = new TcpClient();//modbus連線
            //await tcpClient.ConnectAsync("172.1.1.100", 502);
            MF = _MF;
            Version = _version;
            string LogFolder = XML.Read("Setting/MainForm/Logfolder", "D:\\AML");
            int LogCountLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogCountLimit", "200"));
            int LogFolderLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogFolderLimit", "30"));
            Log.Initial("Core", false, true, LogFolder, LogCountLimit, LogFolderLimit);
            //  LogSocket.Initial("Socket", false, true, LogFolder, LogCountLimit, LogFolderLimit);

            //string[] token_port = XML.Read("Setting/EFEM/LoadPort", "P1,P2").Split(',');
            _IO = new Adam6000s[3];
            AdamFrom = new AdamIOForm[3];

            for (int i = 0; i < 3; i++)
            {
                if (XML.Read("Setting/Adam_" + (i + 1) + "/Enable", "0") == "1")
                {
                    string Adam_IO = "IO" + +(i + 1);
                    string Adam_IP = XML.Read("Setting/Adam_" + (i + 1) + "/IP", "");

                    if (Adam_IP == "")
                        continue;

                    string AdammSeries = XML.Read("Setting/Adam_" + (i + 1) + "/Type", "");

                    if (AdammSeries == "Adam6050")
                        _IO[i] = new Adam6000s(Adam6000s.AdammSeries.Adam6050, Log, Adam_IO, Adam_IP, 502, 5000);
                    else if (AdammSeries == "Adam6024")
                        _IO[i] = new Adam6000s(Adam6000s.AdammSeries.Adam6024, Log, Adam_IO, Adam_IP, 502, 5000);
                    else
                        continue;

                    _IO[i].AdamResult += new Adam6000s.AdamCmdResult(DO_CmdEventControl);
                    _IO[i].DI_Change += new Adam6000s.DI_ChangeEvent(DI_ChangeEventControl);
                    _IO[i].StartAdam();

                    Button Bt = new Button();
                    Bt.Dock = System.Windows.Forms.DockStyle.Right;
                    Bt.Location = new System.Drawing.Point(120, 0);
                    Bt.Name = "button_IO_" + (i + 1);
                    Bt.Size = new System.Drawing.Size(120, 120);
                    Bt.TabIndex = 1;
                    Bt.Text = "IO_" + (i + 1) + Environment.NewLine + Adam_IP;
                    Bt.UseVisualStyleBackColor = true;
                    Bt.Click += Bt_IO_Click;
                    Bt.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
                    //this.Controls.Add(Bt);

                    AdamFrom[i] = new AdamIOForm();
                    AdamFrom[i].Init(_IO[i]);

                    panel_IO.Controls.Add(Bt);
                }
            }

            for (int i = 8; i >= 1; i--)
            {
                try
                {
                    string _enable_lp = XML.Read("Setting/P" + i + "/Enable", "0");
                    string _Name_lp = XML.Read("Setting/P" + i + "/DeviceName", "LP0");
                    if (_enable_lp != "1")
                        continue;

                    int port = i;
                    Button Bt = new Button();
                    panel_LP.Controls.Add(Bt);

                    Bt.Dock = System.Windows.Forms.DockStyle.Left;
                    Bt.Location = new System.Drawing.Point(0, 0);
                    Bt.Name = "button_P" + port;
                    Bt.Size = new System.Drawing.Size(120, 108);
                    Bt.TabIndex = 100 + i;
                    Bt.Text = _Name_lp;
                    Bt.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
                    Bt.UseVisualStyleBackColor = true;

                    /*  
                  this.button1.Dock = System.Windows.Forms.DockStyle.Left;
                  this.button1.Location = new System.Drawing.Point(0, 0);
                  this.button1.Name = "button1";
                  this.button1.Size = new System.Drawing.Size(75, 108);
                  this.button1.TabIndex = 14;
                  this.button1.Text = "button1";
                  this.button1.UseVisualStyleBackColor = true;
                  */

                    //Label LB = new Label();
                    //LB.Location = new System.Drawing.Point(0, 120);
                    //LB.Name = "button_P" + port + "_Lock";
                    //LB.Size = new System.Drawing.Size(120, 120);
                    //LB.TabIndex = 1;
                    //LB.Text = "Lock";
                    //LB.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
                    //LB.BackColor = Color.LightPink;              
                    //Bt.Controls.Add(new Label());



                    // Bt.Click += new System.EventHandler(this.button_LoadPort_Click);

                    LoadPort _lp = new LoadPort();
                    _lp.Init(Bt.Text, i);
                    _lp.ThrowOutSocketReplyEvent += ThrowOutSocketReplyEvent;
                    LP.Add(_lp);



                    if (_lp.Brand == LoadPort.EFEMBrand.Hirata)
                    {
                        Bt.Text = _Name_lp + Environment.NewLine + "(" + _lp.LPType.ToString() + ")";
                        Bt.Click += new System.EventHandler(this.button_LoadPort_Click);
                    }
                    else
                    {
                        Bt.Text = _Name_lp + Environment.NewLine + "(" + _lp.Brand.ToString() + ")";
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            string _enable_al = XML.Read("Setting/Aligner/Enable", "0");
            string _al_type = XML.Read("Setting/Aligner/Type", "WaferNormal");
            if (_enable_al == "1")
            {
                Al[0] = new Aligner();
                Al[0].Init(_IO[1], _al_type, 0);
                Al[0].ThrowOutSocketReplyEvent += ThrowOutSocketReplyEvent;
            }
            else
            {
                button_Aligner1.Visible = false;
            }
            string _enable_al2 = XML.Read("Setting/Aligner2/Enable", "0");
            string _al2_type = XML.Read("Setting/Aligner2/Type", "WaferNormal");
            if (_enable_al2 == "1")
            {
                Al[1] = new Aligner();
                Al[1].Init(_IO[1], _al2_type, 1);
                Al[1].ThrowOutSocketReplyEvent += ThrowOutSocketReplyEvent;
            }
            else
            {
                button_Aligner2.Visible = false;
            }


            string _enable_rb = XML.Read("Setting/Robot/Enable", "0");
            string _rb_Name = XML.Read("Setting/Robot/DeviceName", "Robot");
            if (_enable_rb == "1")
            {
                Rb[0] = new Robot();
                Rb[0].Init(LP, Al, _rb_Name, 0);
                Rb[0].ThrowOutSocketReplyEvent += ThrowOutSocketReplyEvent;
            }

            string _enable_rb2 = XML.Read("Setting/Robot2/Enable", "0");
            string _rb2_Name = XML.Read("Setting/Robot2/DeviceName", "Robot2");
            if (_enable_rb2 == "1")
            {
                Rb[1] = new Robot();
                Rb[1].Init(LP, Al, _rb2_Name, 1);
                Rb[1].ThrowOutSocketReplyEvent += ThrowOutSocketReplyEvent;
            }
            else
            {
                button_Robot2.Visible = false;
            }

            for (int i = Rb[0].STATIONLIST.Count - 1; i >= 0; i--)
            {
                if (Rb[0].STATIONLIST[i].OtherStation)
                {
                    Stage s = new Stage();
                    s.Init(Rb[0].STATIONLIST[i]);
                    s.Dock = System.Windows.Forms.DockStyle.Left;
                    s.Location = new System.Drawing.Point(0, 0);
                    s.Name = Rb[0].STATIONLIST[i].StationName;
                    s.Size = new System.Drawing.Size(120, 135);
                    panel_StageSub.Controls.Add(s);
                }
            }

            if (_enable_rb2 == "1")
            {
                
                for (int i = Rb[1].STATIONLIST.Count - 1; i >= 0; i--)
                {
                    if (Rb[1].STATIONLIST[i].OtherStation)
                    {
                        Stage s = new Stage();
                        s.Init(Rb[1].STATIONLIST[i]);
                        s.Dock = System.Windows.Forms.DockStyle.Left;
                        s.Location = new System.Drawing.Point(0, 0);
                        s.Name = Rb[1].STATIONLIST[i].StationName;
                        s.Size = new System.Drawing.Size(120, 135);
                        panel_StageSub.Controls.Add(s);
                    }
                }
            }
            

            ControlState = ControlState.Local;


            // Socket = new SocketServer(SocketIP, Convert.ToInt32(SocketPort));// Communication with AML_UI   這邊也要設定檔開4個port給UI連線
            Socket = new SocketServer();
            Socket.DataArrivalEvent += new SocketServer.DataReceived_Delegate(DataArrivalEvent);
            Socket.ConnectEvent += new SocketServer.Connect_Delegate(ConnectEvent);
            Socket.CloseEvent += new SocketServer.Close_Delegate(CloseEvent);
            Socket.DisconnectEvent += new SocketServer.Disconnect_Delegate(DisconnetEvent);
            Socket.ErrorEvent += new SocketServer.SocketError_Delegate(ErrorEvent);
            Socket.ListeningEvent += new SocketServer.Listening_Delegate(ListeningEvent);


            if (XML.Read("Setting/EFEM/EnableAlarm", "-1") == "-1")
            {
                XML.CreateNode("Setting/EFEM", "EnableAlarm", "value", "1");
            }

            if (XML.Read("Setting/EFEM/EnableAlarm", "-1") == "1")
            {
                Enable_Alarm = true;

                if (XML.Read("Setting/EFEM/RobotAlarmOffset", "-1") == "-1")
                {
                    XML.CreateNode("Setting/EFEM", "RobotAlarmOffset", "value", "10000000");
                }

                if (XML.Read("Setting/EFEM/LPAlarmOffset", "-1") == "-1")
                {
                    XML.CreateNode("Setting/EFEM", "LPAlarmOffset", "value", "20000000");
                }

                if (XML.Read("Setting/EFEM/AlignerAlarmOffset", "-1") == "-1")
                {
                    XML.CreateNode("Setting/EFEM", "AlignerAlarmOffset", "value", "30000000");
                }

                Rb[0].RobotAlarmOffset = Convert.ToInt32(XML.Read("Setting/EFEM/RobotAlarmOffset", "10000000"));
                Rb[0].Enable_Alarm = true;
                //Log.WriteSystemLog("RobotAlarmOffset:" + Rb[0].RobotAlarmOffset);

                for (int i = 0; i < LP.Count; i++)
                {
                    LP[i].LPAlarmOffset = Convert.ToInt32(XML.Read("Setting/EFEM/LPAlarmOffset", "20000000"));
                    LP[i].Enable_Alarm = true;
                }

                // if(LP.Count > 0)
                // Log.WriteSystemLog("LPAlarmOffset:" + LP[0].LPAlarmOffset);

                if (Al != null)
                {
                    Al[0].AlignerAlarmOffset = Convert.ToInt32(XML.Read("Setting/EFEM/AlignerAlarmOffset", "30000000"));
                    Al[0].Enable_Alarm = true;
                    // Log.WriteSystemLog("AlignerAlarmOffset:" + Al.AlignerAlarmOffset);
                }
            }
            else
            {
                Enable_Alarm = false;
            }

            SocketListening();

        }



        private void Bt_IO_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            //  Bt.Name = "button_IO_" + (i+1);
            Button bt = (Button)sender;
            int no = Convert.ToInt32(bt.Name.Substring(bt.Name.Length - 1)) - 1;

            if (AdamFrom[no].Visible)
            {
                AdamFrom[no].Hide();
            }
            else
            {
                AdamFrom[no].Show();
            }

        }

        bool fisrtOpen = true;
        private async void DI_ChangeEventControl(string AdamName, int Index, bool Result)
        {
            Log.WriteSystemLog("Adam:" + AdamName + " DI-" + Index.ToString() + " state change to " + Result.ToString());
            switch (AdamName)
            {
                case "IO1":
                    {
                        if (!Result)
                        {
                            switch (Index)
                            {
                                case 0:
                                    Log.WriteSystemLog("EFEM_CDAPressureLow");
                                    break;
                                case 1:
                                    Log.WriteSystemLog("EFEM_VacuumLow");
                                    break;
                                case 2:
                                    Log.WriteSystemLog("EFEM_IonizerError");
                                    break;
                                case 3:
                                    Log.WriteSystemLog("EFEM_FFU_1_Error");
                                    break;
                                case 4:
                                    Log.WriteSystemLog("EFEM_FFU_2_Error");
                                    break;
                                case 5:
                                    Log.WriteSystemLog("EFEM_FFU_3_Error");
                                    break;
                                case 6:
                                    Log.WriteSystemLog("EFEM_Robot_Auto_Mode_Failed");
                                    break;
                                case 7:
                                    Log.WriteSystemLog("EFEM_Robot_TPen_Enable_Failed");
                                    break;
                                case 8:

                                    for (int i = 0; i < LP.Count; i++)
                                        LP[i].SetAllWaferUnknown();
                                    Log.WriteSystemLog("EFEM_Door_Interlock_Triggered");
                                    break;
                                case 9:

                                    for (int i = 0; i < LP.Count; i++)
                                        LP[i].SetAllWaferUnknown();
                                    Log.WriteSystemLog("EFEM_EMO_Triggered");
                                    break;
                                case 10:
                                    for (int i = 0; i < LP.Count; i++)
                                        LP[i].SetAllWaferUnknown();
                                    Log.WriteSystemLog("EFEM_Power_OFF");
                                    break;
                            }
                        }
                        else
                        {
                            switch (Index)
                            {
                                case 0:
                                    Log.WriteSystemLog("EFEM CDAPressureLow Recover");
                                    break;
                                case 1:
                                    Log.WriteSystemLog("EFEM VacuumLow Recover");
                                    break;
                                case 2:
                                    Log.WriteSystemLog("EFEM Ionizer Recover");
                                    break;
                                case 3:
                                    Log.WriteSystemLog("EFEM FFU1 Recover");
                                    break;
                                case 4:
                                    Log.WriteSystemLog("EFEM FFU2 Recover");
                                    break;
                                case 5:
                                    Log.WriteSystemLog("EFEM FFU3 Recover");
                                    break;
                                case 6:
                                    Log.WriteSystemLog("EFEM Robot Auto_Mode Recover");
                                    break;
                                case 7:
                                    Log.WriteSystemLog("EFEM Robot TPen Enable Recover");
                                    break;
                                case 8:
                                    Log.WriteSystemLog("EFEM door Open Recover");
                                    break;
                                case 9:
                                    Log.WriteSystemLog("EMO Press Recover");
                                    break;
                                case 10:

                                    if (!fisrtOpen)
                                    {
                                        await Task.Run(delegate()
                                        {
                                            Log.WriteSystemLog("EFEM Power Off Recover, Start to Do RecoverSetRobotSpeed after 10 seconds.");
                                            System.Threading.Thread.Sleep(10000);
                                            Rb[0].RecoverSetRobotSpeed();
                                            System.Threading.Thread.Sleep(1000);

                                        });
                                    }

                                    fisrtOpen = false;

                                    break;
                            }
                        }
                    }
                    break;
                default:

                    break;

            }
        }

        private void DO_CmdEventControl(string AdamName, string AdamCmd, bool Result)
        {
            // throw new NotImplementedException();

        }

        void ThrowOutSocketReplyEvent(string Data)
        {
            if (Socket != null)
            {
                if (ControlState == AML.ControlState.Remote)
                    SendData(Data);
            }
        }


        //=====Socket=====
        public void SocketListening()
        {
            ConnectState = ServerState.Closed;
            string SocketIP = XML.Read("Setting/EFEM/IP", "127.0.0.1");
            int SocketPort = Convert.ToInt32(XML.Read("Setting/EFEM/Port", "6000"));
            label_IP.Text = "IP: " + SocketIP;
            label_Port.Text = "Port: " + SocketPort.ToString();
            Socket.Listening(SocketIP, SocketPort);


        }
        public void SocketClose()
        {
            ConnectState = ServerState.Closed;
            Socket.Socket_Close();
        }

        private bool SendData(string SocketString)    // 回傳給AML_UI
        {
            if (ConnectState != ServerState.Connect)
                return false;

            SocketString = "~" + SocketString + "@";

            bool _result = Socket.SendData(SocketString);
            if (!_result)
                return false;

            Log.Write("Send", SocketString);
            return true;

        }
        private void ConnectEvent()
        {
            // Login = false;
            TempArrived = string.Empty;
            ConnectState = ServerState.Connect;
            Log.WriteSystemLog("System", "Socket connect successful");
        }
        private void DisconnetEvent()
        {
            TempArrived = string.Empty;
            ConnectState = ServerState.Closed;
            Log.WriteSystemLog("System", "Socket disconnect");
            SocketListening();
        }
        private void CloseEvent()
        {
            TempArrived = string.Empty;
            ConnectState = ServerState.Closed;
            Log.WriteSystemLog("System", "Socket Close sever");
        }
        private void ListeningEvent()
        {
            ConnectState = ServerState.Listening;
            Log.WriteSystemLog("System", "Server listening");
        }

        string TempArrived = string.Empty;
        private void DataArrivalEvent(string Data)    // AML_UI丟過來的指令
        {
            string[] Received_data;
            TempArrived += Data;

            if (Data[Data.Length - 1] != '@')
            {
                if (TempArrived[TempArrived.Length - 1] != '@')
                {
                    return;
                }
            }

            //if (TempArrived != string.Empty)
            //{
            //    if (TempArrived[TempArrived.Length - 1] != '@')
            //    {
            //        return;
            //    }
            //}

            Received_data = TempArrived.ToString().Split(new char[] { '~', '@' }, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 0; i < Received_data.Length; i++)
            {
                //if (Received_data[i].StartsWith("~"))
                {
                    Log.Write("Receive", "~" + Received_data[i] + "@");
                    try
                    {
                        string[] Command = Received_data[i].Split(',');

                        if (ControlState != ControlState.Remote & Command[1].ToUpper() != "API")
                        {
                            SendData(Received_data[i] + ",Not In Remote Mode");
                            TempArrived = string.Empty;
                            continue;
                        }

                        switch (Command[1].ToUpper())
                        {
                            case "API":
                                ApiRecHandler(Command);
                                break;
                            case "P1":
                            case "P2":
                            case "P3":
                            case "P4":
                            case "P5":
                            case "P6":
                            case "P7":
                            case "P8":
                                LPRecHandler(Command);
                                break;
                            case "ALIGNER":
                            case "ALIGNER1":
                                AlignerRecHandler(Command, 0);
                                break;
                            case "ALIGNER2":
                                AlignerRecHandler(Command, 1);
                                break;
                            case "ROBOT":
                            case "ROBOT1":
                                RobotHandler(Command, 0);
                                break;
                            case "ROBOT2":
                                RobotHandler(Command, 1);
                                break;
                            case "ReadBarcode":
                                OtherRecHandler(Command);
                                break;
                            default:
                                OtherRecHandler(Command);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ThrowOutSocketReplyEvent(Received_data[i] + ",1," + ex.Message.ToString());
                        Log.WriteSystemLog("System", "DataArrival Catch : " + Received_data[i] + "," + ex.Message.ToString());
                        TempArrived = string.Empty;
                    }
                }
                TempArrived = string.Empty;
            }
        }

        private void ApiRecHandler(string[] Command)
        {

            switch (Command[0].ToUpper())
            {
                case "VERSION":
                    SendData("Version,API," + Version);
                    break;
                case "HIDE":
                    MF.Invoke((MethodInvoker)delegate() { MF.Visible = false; });
                    SendData("Hide,API,0");
                    break;
                case "SHOW":
                    MF.Invoke((MethodInvoker)delegate() { MF.Visible = true; });
                    SendData("Show,API,0");
                    break;
                case "REMOTE":
                    SendData("Remote,API,0");
                    ControlState = ControlState.Remote;
                    break;
                case "LOCAL":
                    SendData("Local,API,0");
                    ControlState = ControlState.Local;
                    break;

                default:
                    throw new Exception("Unknown Command");
            }

        }
        private void LPRecHandler(string[] Command)
        {
            //int port = Convert.ToInt32(Command[1].Substring(1));
            //LoadPort _lp = GetLoadPort(port);          
            LoadPort _lp = null;

            for (int i = 0; i < LP.Count; i++)
            {
                if (LP[i].LPName != Command[1])
                    continue;
                _lp = LP[i];
                break;
            }

            string error_msg = string.Empty;

            if (_lp == null)
            {
                string out_fail = string.Empty;
                for (int i = 0; i < Command.Length; i++)
                {
                    out_fail += Command[i] + ",";
                }
                out_fail += "Cannot find this port in API.";
                SendData(out_fail);
                return;
            }


            switch (Command[0].ToUpper())
            {
                case "READFOUPID":
                    _lp.RFID.ReadRfid();
                    break;
                case "GETSTATUS": //GetStatus
                    _lp.GetStatus(true);
                    break;
                case "LOAD":
                    _lp.Cmd_Load(out error_msg);
                    break;
                case "UNLOAD":
                    _lp.Cmd_Unload(out error_msg);
                    break;
                case "HOME":
                    _lp.Cmd_Home(out error_msg);
                    break;
                case "GETWAFERSLOT":
                    _lp.Qry_WaferSlot(true);
                    break;
                case "MAP":
                    _lp.Cmd_Map(out error_msg);
                    break;
                case "RESETERROR":
                    _lp.CLR_Error();
                    break;
                case "E84_RESET":
                    if (_lp.E84.E84Mode != E84Control.E84AccessMode.Manual)
                    {
                        SendData("E84_Reset," + _lp.LPName + ",NG for Reset, not in manual or error state");
                        return;
                    }
                    _lp.E84.SendReset();
                    break;
                case "E84_SETAUTO":
                    _lp.E84.SetAuto();
                    break;
                case "E84_SETMANUAL":
                    _lp.E84.SetManual();
                    break;
                case "E84_ENABLE":
                    _lp.E84.SetEnable();
                    break;
                case "E84_SETESON":
                    _lp.E84.SetES(true);
                    break;
                case "E84_SETESOFF":
                    _lp.E84.SetES(false);
                    break;

                default:
                    throw new Exception("Unknown Command");
            }
        }
        private void RobotHandler(string[] Command, int Index)
        {
            switch (Command[0].ToUpper())
            {
                //case "STOP":                 
                //    break;
                case "RESETERROR":
                    Rb[Index].DoResetError(true);
                    break;
                case "HOME":
                    Rb[Index].DoHome(true);
                    break;
                case "GETSTATUS":
                    Rb[Index].GetStatus(true);
                    break;
                //
                case "SMARTGET":
                case "SMARTGET6S":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("SmartGet,Robot,Arm select error");
                            break;
                        }

                        Rb[Index].GetWafer(f, pos, slot, true);
                        break;
                    }
                case "SMARTPUT":
                case "SMARTPUT6S":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("SmartPut,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].PutWafer(f, pos, slot, true);
                        break;

                    }
                //
                case "GETSTANDBY":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("GetStandby,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].GetStandby(f, pos, slot, true);
                        break;
                    }
                case "PUTSTANDBY":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("PutStandby,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].PutStandby(f, pos, slot, true);
                        break;
                    }
                //
                case "GETEXTEND":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("GetExtend,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].GetExtend(f, pos, slot, true);
                        break;
                    }
                case "PUTEXTEND":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("PutExtend,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].PutExtend(f, pos, slot, true);
                        break;
                    }
                //
                case "GETPICK":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("GetPick,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].GetPick(f, pos, slot, true);
                        break;
                    }
                case "PUTPLACE":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("PutPlace,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].PutPlace(f, pos, slot, true);
                        break;
                    }
                //
                case "GETRETRACT":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("GetRetract,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].GetRetract(f, pos, slot, true);
                        break;
                    }
                case "PUTRETRACT":
                    {
                        Fork f;
                        string pos = Command[3];
                        int slot = Convert.ToInt32(Command[4]);

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("PutRetract,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].PutRetract(f, pos, slot, true);
                        break;
                    }
                //
                case "VACUUMON":
                    {
                        Fork f;

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("VacuumOn,Robot,Arm select error");
                            break;
                        }

                        Rb[Index].VacummControl(f, true, true);
                        break;
                    }
                case "VACUUMOFF":
                    {
                        Fork f;
                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("VacuumOff,Robot,Arm select error");
                            break;
                        }
                        Rb[Index].VacummControl(f, false, true);
                        break;
                    }

                case "EDGEGRIPON":
                    {
                        Fork f;

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("EdgeGripOn,Robot,Arm select error");
                            break;
                        }

                        Rb[Index].EdgeGripControl(f, true, true);
                        break;
                    }

                case "EDGEGRIPOFF":
                    {
                        Fork f;

                        if (Command[2] == "1")
                            f = Rb[Index].LOWERARM;
                        else if (Command[2] == "2")
                            f = Rb[Index].UPPERARM;
                        else
                            f = null;

                        if (f == null)
                        {
                            SendData("EdgeGripOff,Robot,Arm select error");
                            break;
                        }

                        Rb[Index].EdgeGripControl(f, false, true);
                        break;
                    }

                default:
                    throw new Exception("Unknown Command");
            }
        }
        private void AlignerRecHandler(string[] Command, int Index)
        {

            //{ ResetError, GetStatus, Home, Alignment, VacuumOn, VacuumOff,SetAngle }

            switch (Command[0].ToUpper())
            {
                case "HOME":
                    {
                        Al[Index].DoHome();
                        break;
                    }
                case "ALIGNMENT":
                    {
                        // Alignment,Aligner,Degree,0
                        int degree = Convert.ToInt32(Command[2]);
                        if (Al[Index].Aligner_Type == Aligner.AlignerType.FrameTurn)
                            Al[Index].RunCMD(Aligner.AlignerCommand.Alignment, degree);
                        else
                            Al[Index].Alignment(degree);
                        break;
                    }
                case "ALIGNERVACUUM":
                    //AlignerVacuum,Aligner,0
                    if (Command[2].ToUpper() == "1")
                    {
                        Al[Index].VacuumControl(true);
                    }
                    else if (Command[2].ToUpper() == "0")
                    {
                        Al[Index].VacuumControl(false);
                    }
                    break;
                case "GETSTATUS":
                    {
                        Al[Index].GetStatus();
                    }
                    break;
                case "RESETERROR":
                    {
                        Al[Index].ResetError();
                    }
                    break;
                case "TURN":
                    {
                        // Alignment,Aligner,Degree,0
                        int degree = Convert.ToInt32(Command[2]);
                        Al[Index].CmdTurn(degree.ToString(), true);
                        break;
                    }
                    break;
            }
        }
        private void OtherRecHandler(string[] Command)
        {
            switch (Command[0].ToUpper())
            {
                //RobotSpeed,Z-axisSpeed,Other-axisSpeed
                case "ROBOTSPEED":
                case "ROBOT1SPEED":
                    {
                        int z_speed = Convert.ToInt32(Command[1]);
                        int other_speed = Convert.ToInt32(Command[2]);
                        Rb[0].SetRobotSpeed(z_speed, other_speed, true);
                    }
                    break;
                case "ROBOT2SPEED":
                    {
                        int z_speed = Convert.ToInt32(Command[1]);
                        int other_speed = Convert.ToInt32(Command[2]);
                        Rb[1].SetRobotSpeed(z_speed, other_speed, true);
                    }
                    break;
                case "READWAFERID":
                    switch (Command[1].ToUpper())
                    {
                        case "TOPOCR":
                            if (!Al[0].ocr_Top.ReadOCR(true))
                            {
                                SendData("ReadWaferID,TopOCR,************");
                                return;
                            }
                            break;
                        case "BOTTOMOCR":
                            if (!Al[0].ocr_Bottom.ReadOCR(true))
                            {
                                SendData("ReadWaferID,BottomOCR,************");
                                return;
                            }
                            break;
                    }
                    break;
                case "READBARCODE":
                    if (Command[1] == "1")
                    {
                        string ttt = "";
                        Al[0].RunCMD(Aligner.AlignerCommand.ReadBarcode);
                    }
                    if (Command[1] == "2")
                    {
                        string ttt = "";
                        Al[1].RunCMD(Aligner.AlignerCommand.ReadBarcode);
                    }
                    break;
            }
        }
        private void ErrorEvent(string EX)
        {
            ConnectState = ServerState.Closed;
            Log.WriteSystemLog("System", "Socket disconnect:" + EX);
        }

        LoadPort GetLoadPort(int port)
        {
            for (int i = 0; i < LP.Count; i++)
            {
                if (LP[i].LPNumber == port)
                    return LP[i];
            }

            return null;

        }

        private void button_LoadPort_Click(object sender, EventArgs e)
        {
            Button Bt = (Button)sender;

            int port = Convert.ToInt32(Bt.Name.ToString().Substring(8, 1));
            LoadPort _lp = GetLoadPort(port);

            if (_lp.Visible)
                _lp.Hide();
            else
                _lp.Show();

        }

        private void button_Robot_Click(object sender, EventArgs e)
        {
            if (Rb[0].Visible)
                Rb[0].Hide();
            else
                Rb[0].Show();
        }
        private void button_Robot2_Click(object sender, EventArgs e)
        {
            if (Rb[1].Visible)
                Rb[1].Hide();
            else
                Rb[1].Show();
        }
        private void button_WaferAligner_Click(object sender, EventArgs e)
        {
            if (Al[0].Visible)
                Al[0].Hide();
            else
                Al[0].Show();
        }
        private void button_Aligner2_Click(object sender, EventArgs e)
        {
            if (Al[1].Visible)
                Al[1].Hide();
            else
                Al[1].Show();
        }
        private void button_Local_Click(object sender, EventArgs e)
        {
            Log.WriteOperationLog("User Set ControlState to " + ControlState.Local.ToString());
            ControlState = ControlState.Local;


        }

        private void button_Remote_Click(object sender, EventArgs e)
        {
            Log.WriteOperationLog("User Set ControlState to " + ControlState.Remote.ToString());
            ControlState = ControlState.Remote;


        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            DialogResult DR = MessageBox.Show("關閉AML EFEM Control System?", "問題", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DR == DialogResult.Yes)
            {
                try
                {
                    for (int i = 0; i < IO.Length; i++)
                        AdamFrom[i].ExitFunction = true;
                    for (int i = 0; i < Al.Length; i++)
                        Al[i].ocr_Top.ExitFunction = true;
                    for (int i = 0; i < Al.Length; i++)
                        Al[i].ocr_Bottom.ExitFunction = true;
                }
                catch
                {

                }

                SocketClose();
                Log.WriteOperationLog("User Close Software");
                Log.WriteSystemLog("User Close Software");
                Process p = Process.GetCurrentProcess();
                p.Kill();
                Environment.Exit(Environment.ExitCode);
            }

        }

        private void timer1sec_Tick(object sender, EventArgs e)
        {

        }


    }
}
