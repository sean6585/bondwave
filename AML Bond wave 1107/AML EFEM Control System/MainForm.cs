using AAMotion;
using EasyModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;



namespace AML
{
    public partial class MainForm : Form
    {
        AGM800 ControllerInstance;
        public bool hasShownMessage = false;
        public AML_LogPage Amotor_Log =new AML_LogPage();
        public AML_LogPage Bmotor_Log = new AML_LogPage();
        public AML_LogPage Vacuum_Log = new AML_LogPage();
        //初始化chart 
         //Chart chart = this.Controls[0] as Chart; //B motor的chart
         //Series series = chart1.Series[0];
         //Chart Force_chart = this.Controls[0] as Chart; //Auto mode的chart
         //Series Force_series = chart2.Series[0];

        ModbusClient modbusClient = new ModbusClient("172.1.1.100", 502);
        public MainForm()
        {
            InitializeComponent();
            Init();
            ControllerInstance = new AGM800(50); //Use update interval of 20ms   //Default is 50ms

            this.chart1.MouseWheel += Chart_MouseWheel;
            this.chart2.MouseWheel += Auto_Chart_MouseWheel;
            var chartArea = chart1.ChartAreas[0];
            var AutochartArea = chart2.ChartAreas[0];

            //======================================設定圖表刻度為0~1200ms 每200ms為一個區間======================================
            //chartArea.AxisX.IntervalType = DateTimeIntervalType.Milliseconds;
            //chartArea.AxisX.Interval = 200; // 設置刻度間隔為200毫秒
            //chartArea.AxisX.Minimum = 0; // 設置最小值為0
            //chartArea.AxisX.Maximum = 1400; // 設置最大值為1200
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(0, 200, "0", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(200, 400, "200", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(400, 600, "400", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(600, 800, "600", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(800, 1000, "800", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(1000, 1200, "1000", 0, LabelMarkStyle.None, GridTickTypes.Gridline));
            //chartArea.AxisX.CustomLabels.Add(new CustomLabel(1200, 1400, "1200", 0, LabelMarkStyle.None, GridTickTypes.Gridline));

        }
        private bool Check_ExeOpen()
        {
            Process aProcess = Process.GetCurrentProcess();
            string aProcName = aProcess.ProcessName;
            if (Process.GetProcessesByName(aProcName).Length > 1)
                return true;
            else
                return false;
        }
        public void Init()
        {
            try
            {
                this.notifyIcon1.Visible = false;
                //this.ShowInTaskbar = false;

                if (Check_ExeOpen())
                {
                    MessageBox.Show(this.Text + "已開啟，請確認右下角工具列Icon");
                    Process p = Process.GetCurrentProcess();
                    p.Kill();
                    return;
                }

                label_XML_Path.Text = Directory.GetCurrentDirectory() + "\\Setting.xml";

                string LogFolder = XML.Read("Setting/MainForm/Logfolder", "D:\\AML");
                int LogCountLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogCountLimit", "200"));
                int LogFolderLimit = Convert.ToInt32(XML.Read("Setting/MainForm/LogFolderLimit", "30"));
                amL_LogPage1.Initial("SystemTop", false, false, LogFolder, LogCountLimit, LogFolderLimit);
                Amotor_Log.Initial("A motor", false, false, LogFolder, LogCountLimit, LogFolderLimit);
                Bmotor_Log.Initial("B motor", false, false, LogFolder, LogCountLimit, LogFolderLimit);
                Vacuum_Log.Initial("Vacuum", false, false, LogFolder, LogCountLimit, LogFolderLimit);
                Log.Initial(false, true, LogFolder, LogCountLimit, LogFolderLimit);
                Log.OutputSystemLog += new AML_LogTab.SystemLogEvent(WriteTopLog);
                Log.Add(Amotor_Log);
                Log.Add(Bmotor_Log);
                Log.Add(Vacuum_Log);
                
                //Core.Init(this, Version.Text);
                //Log.Add(Core.Log);


                //Log.Add(Core.Rb[0].Log);
                //if (Core.Rb[1] != null)
                //    Log.Add(Core.Rb[1].Log);

                //if (Core.Al[0] != null)
                //    Log.Add(Core.Al[0].Log);
                //if (Core.Al[1] != null)
                //    Log.Add(Core.Al[1].Log);                   

                //for (int i = Core.LP.Count; i > 0; i--)
                //    Log.Add(Core.LP[i - 1].Log);


                //Log.WriteSystemLog("SmartGetDelayms:" + Core.Rb[0].SmartGetDelayms + " ms");
                //Log.WriteSystemLog("RobotAlarmOffset:" + Core.Rb[0].RobotAlarmOffset);         

                //if (Core.LP.Count > 0)
                //    Log.WriteSystemLog("LPAlarmOffset:" + Core.LP[0].LPAlarmOffset);

                //if (Core.Al != null)
                //{
                //    Core.Al[0].AlignerAlarmOffset = Convert.ToInt32(XML.Read("Setting/EFEM/AlignerAlarmOffset", "30000000"));
                //    Log.WriteSystemLog("AlignerAlarmOffset:" + Core.Al[0].AlignerAlarmOffset);
                //}

                Log.WriteSystemLog("Software Open. Version:" + Version.Text);
              
            }
            catch(Exception ex)
            {
                Log.WriteSystemLog("Software Open Error. Version:" + Version.Text);
                Log.WriteSystemLog("Exception:" + ex.ToString());
                MessageBox.Show(ex.ToString());
            }
           
        }
        private bool _vacConnect = false;//確認真空閥是否連線
        public bool isVacConnect//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _vacConnect = value; }));
                    return;
                }
                _vacConnect = value;
                tbVacLState.Text = (value ? "Connected" : "DisConnected");
                tbVacLState.BackColor = (value ? Color.Green : Color.Red);   
            }
            get { return _vacConnect; }
        }

        private bool _controlerConnect = false;//確認controler是否連線
        public bool isControlerConnect//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _controlerConnect = value; }));
                    return;
                }
                _controlerConnect = value;
                tbLiveState.Text = (value ? "Connected" : "DisConnected");
                tbLiveState.BackColor = (value ? Color.Green : Color.Red);
                tbLiveState.Text = (value ? "Connected" : "DisConnected");
                tbLiveState.BackColor = (value ? Color.Green : Color.Red);
                tbLiveState.Text = (value ? "Connected" : "DisConnected");
                tbLiveState.BackColor = (value ? Color.Green : Color.Red);
            }
            get { return _controlerConnect; }
        }

        private bool _motorAconnected = false;//確認真空閥是否連線
        public bool isMotorAconnected//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _motorAconnected = value; }));
                    return;
                }
                _motorAconnected = value;
                tbAMotorStatus.Text = (value ? "Motor On" : "Motor Off");
                tbAMotorStatus.BackColor = (value ? Color.Green : Color.Red);
            }
            get { return _motorAconnected; }
        }

        private bool _motorBconnected = false;//確認真空閥是否連線
        public bool isMotorBconnected//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _motorBconnected = value; }));
                    return;
                }
                _motorBconnected = value;
                tbBMotorStatus.Text = (value ? "Motor On" : "Motor Off");
                tbBMotorStatus.BackColor = (value ? Color.Green : Color.Red);
            }
            get { return _motorBconnected; }
        }


        //tbAPos
        private string _tbAPosition;//確認真空閥是否連線
        public string tbAPosition//RS232無法判斷 所以只有GetStatus成功才當他連線成功
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbAPosition = tbAPos.Text = tbAutoAPos.Text = value; }));
                    return;
                }
                _tbAPosition = value;
                tbAPos.Text = value;
                tbAutoAPos.Text = value;
            }
            get { return _tbAPosition; }
        }

        //instant A position
        private string _tbAinsPosition;
        public string tbAinsPosition// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbAinsPosition= tbINPShow.Text = value; }));
                    return;
                }
                _tbAinsPosition = value;
                tbINPShow.Text = value;
            }
            get { return _tbAinsPosition; }
        }

        //instant A velocity
        private string _tbAVelocity;
        public string tbAVelocity// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbAVelocity = tbAVelshow.Text = value; }));
                    return;
                }
                _tbAVelocity = value;
                tbAVelshow.Text = value;
            }
            get { return _tbAVelocity; }
        }

        //instant A Current
        private string _tbACurrent;
        public string tbACurrent// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbACurrent = tbACurrShow.Text = value; }));
                    return;
                }
                _tbACurrent = value;
                tbACurrShow.Text = value;
            }
            get { return _tbACurrent; }
        }

        private string _tbState;
        public string tbState// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbState = tbStateShow.Text =  value; tbStateShow.BackColor = value == "Running" ? Color.OrangeRed : Color.Green; }));
                    return;
                }
                _tbState = value;
                tbStateShow.Text = value;
                tbStateShow.BackColor = value == "Running" ? Color.OrangeRed : Color.Green; // 執行中 = 紅色
            }
            get { return _tbState; }
        }

        private string _tbForceFeedback;
        public string tbForceFeedback// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbForceFeedback = value; tbFeedback.Text = value;
                        tbAutoFb.Text = value;
                    }));
                    return;
                }
                _tbForceFeedback = value;
                tbFeedback.Text = value;
                tbAutoFb.Text = value; // auto 頁面顯示
            }
            get { return _tbForceFeedback; }
        }

        private string _tbForceReferFeedback;
        public string forceReferFeedback// A motor position shown
        {
            set
            {
                if (InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { _tbForceReferFeedback = value; tbRefForce.Text = value; tbAutoForce.Text = value; }));
                    return;
                }
                _tbForceReferFeedback = value;
                tbRefForce.Text = value;
                tbAutoForce.Text = value; // auto 頁面顯示
            }
            get { return _tbForceReferFeedback; }
        }

        private void WriteTopLog(DateTime Time, string SubTitle, string Description)
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate () { WriteTopLog(Time, SubTitle, Description); }));
                return;
            }
            amL_LogPage1.Write(Time, SubTitle, Description);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult DR = MessageBox.Show("關閉" + this.Text + "?", "問題", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DR == DialogResult.Yes)
            {
                //Core.SocketClose();
                Log.WriteOperationLog("User Close Software");
                Log.WriteSystemLog("User Close Software");
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            else
            {

                //    e.Cancel = true;
                //}

                //e.Cancel = true;
                //notifyIcon1.Visible = true;
                //Hide();
                //ShowInTaskbar = false;
            }
        }
        private void LogOpenFolderBtn_Click(object sender, EventArgs e)
        {
            Process.Start(Log.FolderPath);
        }

        private void LogRefreshBtn_Click(object sender, EventArgs e)
        {
            LogTree.Nodes.Clear();
            if (HistorylogStart.Value.Date > HistorylogEnd.Value.Date)
            {
                MessageBox.Show("時間範圍選取錯誤");
      
                return;
            }
            double datecount = HistorylogEnd.Value.AddDays(1).Date.Subtract(HistorylogStart.Value.Date).TotalDays;
            if (datecount > 60)
            {
                MessageBox.Show("為避免資料量過大，選取時間範圍請在兩個月以內");
                return;
            }

            for (int i = 0; i < datecount + 1; i++)
            {
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(Log.FolderPath, HistorylogStart.Value.Date.AddDays(i).ToString("yyyyMMdd")));
                foreach (var dir in dirs)
                {
                    TreeNode TestNode = new TreeNode();
                    TestNode.Nodes.Add("");
                    TestNode.Text = dir.Substring(dir.LastIndexOf("\\") + 1);
                    LogTree.Nodes.Add(TestNode);
                }
            }
        }

        private void LogTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string file_path = Log.FolderPath + "\\" + e.Node.Text;
            List<string> dirs1 = new List<string>(Directory.EnumerateFiles(file_path));
            foreach (var dir1 in dirs1)
                e.Node.Nodes.Add(dir1.Substring(dir1.LastIndexOf("\\") + 1));
        }
        private void LogTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Nodes.Clear();
            e.Node.Nodes.Add("");
        }
        private void LogTree_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (LogTree.SelectedNode.Level == 1)
                {
                    string file = LogTree.SelectedNode.Text;
                    string folder = LogTree.SelectedNode.Parent.Text;
                    Process.Start(Log.FolderPath + "\\" + folder + "\\" + file);
                }
            }
            catch
            {
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = false;
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.Visible = true;
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private void label_XML_Path_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("notepad++.exe", label_XML_Path.Text);
            }
            catch
            {
                Process.Start("notepad.exe", label_XML_Path.Text);
            }
        }

        private void amL_LogPage1_TextChanged(object sender, EventArgs e)
        {

        }

        //====================================================AAmotion===================================================================//
        private System.Timers.Timer motorTimer;  //宣告獨立 thread timer
        private System.Timers.Timer chartTimer;  //宣告獨立 thread timer
        private System.Timers.Timer vacuumTimer;  //宣告獨立vacuum thread timer
        private double timeCounter = 0;
        private bool isWrite = false;
        private bool isConnect = true;
        private bool BPosionMode = false;
        public bool CheckAMotor() //確認有沒有跟機台連線的finction
        {
            if (!isControlerConnect || !isMotorAconnected)
            {
                MessageBox.Show("Please connect & press A motor on first.");
                Log.WriteSystemLog("Please connect & press A motor on first.");
                return false;
            }
            return true;
        }

        public bool CheckBMotor() //確認音圈馬達的function
        {
            if (!isControlerConnect || !isMotorBconnected)
            {
                MessageBox.Show("Please connect & press B motor on first.");
                Log.WriteSystemLog("Please connect & press B motor on first.");
                return false;
            }
            return true;
        }
        private void btnConnect_Click(object sender, EventArgs e) //確認機台連線
        {
            if (ControllerInstance.Connect(tbIPAddr.Text.ToString()))
            {
                MessageBox.Show("Controller is connected.");   // controller id :172.1.1.101
                isControlerConnect = true;
                // 設定計時器  timerUpdateStatus
                //timerUpdateStatus.Interval = 20; //timerUpdateStatus的時間區間=20ms刷新一次
                //timerUpdateStatus.Enabled = true; //timerUpdateStatus啟用
                //timerUpdateStatus.Start(); //timerUpdateStatus開始
                motorTimer = new System.Timers.Timer(20);  // 設定區間
                motorTimer.Elapsed += MotorTimer_Elapsed;
                motorTimer.Start();
                Log.WriteSystemLog("Controller is connected.");
                chartTimer = new System.Timers.Timer(20);  // 設定區間
                chartTimer.Elapsed += ChartTimer_Elapsed;         
            }
            else
            {
                MessageBox.Show("Fail to connect.");
                Log.WriteSystemLog("Controller Fail to connected.");
            }
        }

        private void MotorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //tbLiveState.BeginInvoke(new Action(()=> tbLiveState.Text = ControllerInstance.ConnectedCableType.ToString() == "0" ? "Disconnected" : "Connected"));  // check 是否連上)
            ////線性馬達A
            tbAPosition = ControllerInstance.A.Pos.ToString(); //線性馬達A positon
            if (int.Parse(tbAPosition) > 800000) //線性馬達A 極限位置 => 最多移動80公分
            {
                if (!hasShownMessage)
                {
                    ControllerInstance.A.Stop();
                    hasShownMessage = true;
                    MessageBox.Show("Out of A motor's limit.");
                }
            }
            ControllerInstance.SendBulkCommandString("APos;APosRef;AGendata[10]=20303", out string response); //刷狀態UI的function
            tbAinsPosition = response;  // A motor position shown
            tbAVelocity = ControllerInstance.A.VelRef.ToString(); //線性馬達A velocity 
            tbACurrent = ControllerInstance.A.Iq.ToString(); //線性馬達A current
            tbState = ControllerInstance.A.MotionStat.ToString() == "0" ? "Inposition" : "Running";

            //音圈馬達B force狀態  切換到pos mode :  instance.axis.ForceCurr_GotoPosMode () ;  切換到force  mode : instance.axis.ForceCurr_GotoForceMode ()   
            ControllerInstance.SendCommandString("BForceRef", out string BForceRefer);//紀錄音圈馬達的force reference的function
            forceReferFeedback = BForceRefer;
            //紀錄音圈馬達的force feedback的function
            ControllerInstance.SendCommandString("BForce", out string BForceFeedback);
            tbForceFeedback = BForceFeedback; // 音圈馬達force反饋            
        }

        private void ChartTimer_Elapsed(object sender, ElapsedEventArgs e)  // 這邊要去刷ui,  不同線程   =>  用 invoke 
        {
            //=======================================chart=======================================//         
            if (InvokeRequired)
                Invoke(new Action(UpdateChart));
            else
                UpdateChart();
        }

        private void UpdateChart()  // 作圖
        {
            timeCounter++; // 兩張 chart 的 x座標
            //B motor的chart
            if (isWrite && (!isBChartStop)) //力控按鈕被按下 B motor的chart才會開始跑 && 若是按了暫停圖表的按鈕，即停止生成圖表
            {
                chart1.Series[0].Points.AddXY(timeCounter, Convert.ToInt32(tbFeedback.Text)); // 開始作圖
            }

            //Auto mode的chart
            if (isControlerConnect && (!isAutoChartStop)) //確認與機台正在連線才開始生成chart
            {
                chart2.Series[0].Points.AddXY(timeCounter, Convert.ToInt32(tbAutoFb.Text));
            }
        }

        private void tbIPAddr_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDisconnect_Click(object sender, EventArgs e) //跟 MOTOR controller 斷線的按鈕
        {
            ControllerInstance.Disconnect();
            isControlerConnect = false;
            isMotorAconnected = false;
            _motorBconnected = false;
            MessageBox.Show("Controller is Disonnected.");
            Log.WriteSystemLog("Controller is Disonnected.");
            tbBMotorStatus.Text = "Motor Off";
            tbBMotorStatus.BackColor = Color.Red;
        }
        
        private void timerUpdateStatus_Tick(object sender, EventArgs e) //timer為獨立線程, 用來刷新馬達位置、連線狀況、UI以及chart的timer
        {
            
            tbLiveState.Text = ControllerInstance.ConnectedCableType.ToString() == "0" ? "Disconnected" : "Connected";  // check 是否連上
            //線性馬達A連線狀態
            if (tbLiveState.Text == "Connected")
                tbLiveState.BackColor = Color.Green;
            else
                tbLiveState.BackColor = Color.Red;

            tbAMotorStatus.Text = ControllerInstance.A.MotorOnStatus.ToString() == "0" ? "Motor Off" : "Motor On";  // 1 = on 0 = off
            if (tbAMotorStatus.Text == "Motor On")
                tbAMotorStatus.BackColor = Color.Green;
            else
                tbAMotorStatus.BackColor = Color.Red;
            //  音圈馬達B 連線狀態
            tbBMotorStatus.Text = ControllerInstance.B.MotorOnStatus.ToString() == "0" ? "Motor Off" : "Motor On";
            if (tbBMotorStatus.Text == "Motor On")
                tbBMotorStatus.BackColor = Color.Green;
            else
                tbBMotorStatus.BackColor = Color.Red;
            //線性馬達A
            tbAPos.Text = ControllerInstance.A.Pos.ToString(); //線性馬達A positon
            tbAutoAPos.Text = ControllerInstance.A.Pos.ToString();
            if (int.Parse(tbAPos.Text) > 800000) //線性馬達A 極限位置 => 最多移動80公分
            {
                if (!hasShownMessage)
                {
                    ControllerInstance.A.Stop();
                    hasShownMessage = true;
                    MessageBox.Show("Out of A motor's limit.");
                }
            }
            ControllerInstance.SendBulkCommandString("APos;APosRef;AGendata[10]=20303", out string response); //刷狀態UI的function
            tbINPShow.Text = response;  // A motor position shown
            tbAVelshow.Text = ControllerInstance.A.VelRef.ToString(); //線性馬達A velocity 
            tbACurrShow.Text = ControllerInstance.A.Iq.ToString(); //線性馬達A current
            tbStateShow.Text = ControllerInstance.A.MotionStat.ToString() == "0" ? "Inposition" : "Running";
            if (tbStateShow.Text == "Running")
                tbStateShow.BackColor = Color.OrangeRed; // 執行中 = 紅色
            else
                tbStateShow.BackColor = Color.Green;

            //  音圈馬達B force狀態  切換到pos mode :  instance.axis.ForceCurr_GotoPosMode () ;  切換到force  mode : instance.axis.ForceCurr_GotoForceMode ()   
            ControllerInstance.SendCommandString("BForceRef", out string BForceRefer);//紀錄音圈馬達的force reference的function
            tbRefForce.Text = BForceRefer;
            tbAutoForce.Text = BForceRefer;  // auto 頁面顯示
            ControllerInstance.SendCommandString("BForce", out string BForceFeedback   );//紀錄音圈馬達的force feedback的function
            tbFeedback.Text = BForceFeedback; // 音圈馬達force反饋         
            tbAutoFb.Text = BForceFeedback; // 音圈馬達force反饋    
        }

        //===============================================A motor(線性)=================================================//
        private void btnAMotorOn_Click(object sender, EventArgs e)//線性馬達 A motor on的按鈕
        {
            try
            {
                ControllerInstance.A.MotorOn();
            }
            catch
            {
                Log.WriteOperationLog("Can't set A motor on, pls check.");
                return;
            }
            Log.WriteOperationLog("A motor is on.");
            isMotorAconnected = true;
        }

        private void btnAMotorOff_Click(object sender, EventArgs e)//線性馬達 A motor off的按鈕
        {
            try
            {
                ControllerInstance.A.MotorOff();
            }
            catch
            {
                Log.WriteOperationLog("Can't set A motor off, pls check.");
                return;
            }
            isMotorAconnected = false;
            Log.WriteOperationLog("A motor is off.");
        }

        private void btnAMoveAbs1_Click(object sender, EventArgs e) //線性馬達 A motor移動到p1位置
        {
            if (CheckAMotor())
            {
                if (ControllerInstance.A.MoveAbs(Convert.ToInt32(tbATrgt1.Text),500000)) //(位置,速度)
                {
                    Log.WriteOperationLog("A motor is moving.");
                }
                else
                {
                    Log.WriteSystemLog("A motor move fail.");
                }
            }
                
        }

        private void btnAMoveAbs2_Click(object sender, EventArgs e)//線性馬達 A motor移動到p2位置
        {
            if (CheckAMotor())
            {
                if (ControllerInstance.A.MoveAbs(Convert.ToInt32(tbATrgt2.Text), 500000))
                {
                    Log.WriteOperationLog("A motor is moving.");
                }
                else
                {
                    Log.WriteSystemLog("A motor move fail.");
                }
            }
        }

        private void btnAMovePTP1_Click(object sender, EventArgs e)//線性馬達 A motor往復移動p1 to p2
        {
            if (CheckAMotor())
            {
                ControllerInstance.A.MoveAbs(Convert.ToInt32(tbATrgt1.Text));
                
                ControllerInstance.A.MoveAbsRepetitive(Convert.ToInt32(tbATrgt2.Text), Convert.ToInt32(tbAWait1.Text));
            }              
        }

        private void btnAMovePTP2_Click(object sender, EventArgs e)//線性馬達 A motor往復移動p2 to p1
        {
            if (CheckAMotor())
            {
                ControllerInstance.A.MoveAbs(Convert.ToInt32(tbATrgt2.Text));
                ControllerInstance.A.MoveAbsRepetitive(Convert.ToInt32(tbATrgt1.Text), Convert.ToInt32(tbAWait2.Text));
            }
        }

        private void btnAReal_Click(object sender, EventArgs e)//線性馬達 A motor相對移動
        {
            if (CheckAMotor())
            {
                ControllerInstance.A.MoveRel((Convert.ToInt32(tbAReal.Text)));
            }     
        }

        private void btnAAbs_Click(object sender, EventArgs e)//線性馬達 A motor絕對值移動
        {
            if (CheckAMotor())
            {
                ControllerInstance.B.MoveAbs((Convert.ToInt32(tbAAbs.Text)));
            }             
        }

        private void bntAMovStop_Click(object sender, EventArgs e)//線性馬達 A motor 暫停所有移動
        {
            if (CheckAMotor())
                ControllerInstance.A.Stop();
        }

        private void bntAJogPositive_Click(object sender, EventArgs e) //線性馬達 A motor 往下JOG
        {
            if (CheckAMotor())
            {
                ControllerInstance.A.Jog((Convert.ToInt32(tbAJogVel.Text)));
            }       
        }

        private void bntAJogNegative_Click(object sender, EventArgs e) //線性馬達 A motor 往下上JOG
        {
            if (CheckAMotor())
            {
                ControllerInstance.A.Jog(-Convert.ToInt32(tbAJogVel.Text));
            }

        }
        private void btnAHome_Click(object sender, EventArgs e)//線性馬達 A motor 回home(原點)
        {
            if (CheckAMotor())
                ControllerInstance.A.MoveAbs(0,500000);
        }
        private void tbINPShow_TextChanged(object sender, EventArgs e)
        {

        }

        //==============================================B motor(音圈)==================================================//
        private void btnBMotorOn_Click(object sender, EventArgs e) //音圈馬達 B motor on
        {
            try
            {
                ControllerInstance.B.ForceCurr_GotoPosMode(); // 先轉position模式
                tB_Bmotion.Text = "Position Control";
                Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                BPosionMode = true;
                ControllerInstance.B.MotorOn();
                //if (ControllerInstance.B.OperationMode != 3)
                //{ // 3 = postion control mode / 4 =  force control
                //    ControllerInstance.B.ForceCurr_GotoPosMode(); // 先轉position模式
                //    tB_Bmotion.Text = "Position Control";
                //    Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                //}
                //else
                //{
                //    tB_Bmotion.Text = "Position Control";
                //    Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                //}
            }
            catch(Exception ex)
            {
                MessageBox.Show(" B motor 開啟失敗; 請檢查設定");
                Log.WriteSystemLog(" B motor 開啟失敗; 請檢查設定:/n" + ex.ToString());
            }
            isMotorBconnected = true;                
        }

        private void btnBMotorOff_Click(object sender, EventArgs e) //音圈馬達 B motor off
        {             
            try
            {
                ControllerInstance.B.ForceCurr_GotoPosMode(); // 先轉position模式
                BPosionMode = true;
                tB_Bmotion.Text = "Position Control";
                Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                ControllerInstance.B.MotorOff();
                //if (ControllerInstance.B.OperationMode != 3)
                //{ // 3 = postion control mode / 4 =  force control
                //    ControllerInstance.B.ForceCurr_GotoPosMode(); // 先轉position模式
                //    tB_Bmotion.Text = "Position Control";
                //    Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                //}
                //else
                //{
                //    tB_Bmotion.Text = "Position Control";
                //    Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(" B motor 關閉失敗; 請檢查設定");
                Log.WriteSystemLog(" B motor 關閉失敗; 請檢查設定:/n" + ex.ToString());
            }
            isMotorBconnected = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        bool isBChartStop = false;//確認音圈馬達chart是否暫停
        private void btnWait_Click(object sender, EventArgs e) // 暫停音圈馬達生成chart的按鈕
        {
            isBChartStop = true;
        }

        private void btnCont_Click(object sender, EventArgs e)//先清除音圈馬達的chart，再繼續生成chart  
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            foreach (var series in chart2.Series)
            {
                series.Points.Clear();
            }
            isBChartStop = false;
        }

        private void Chart_MouseWheel(object sender, MouseEventArgs e) // 縮放音圈馬達chart的  要加invoke
        {
            if (isBChartStop)//音圈馬達的chart需暫停才可以縮放
            {
                var result = chart1.HitTest(e.X, e.Y);
                if (result.ChartArea != null)
                {
                    // Zoom in or out based on mouse wheel direction
                    ZoomChart(result.ChartArea, e.Delta);
                }
            }
            else
            {
                Log.WriteSystemLog("B motor 圖表繪製中，請先暫停圖表繪製");
            }
        }
        private void ZoomChart(ChartArea chartArea, int delta)  // 圖表ZOOM   要加invoke
        {
            if (delta > 0) // Mouse wheel up
            {
                ZoomIn(chartArea);// Zoom in
            }
            else if (delta < 0) // Mouse wheel down
            {
                ZoomOut(chartArea);// Zoom out
            }
        }

        private void ZoomIn(ChartArea chartArea)//音圈馬達chart zoom in    要加invoke
        {
            double xMin = chartArea.AxisX.ScaleView.ViewMinimum;
            double xMax = chartArea.AxisX.ScaleView.ViewMaximum;
            double yMin = chartArea.AxisY.ScaleView.ViewMinimum;
            double yMax = chartArea.AxisY.ScaleView.ViewMaximum;

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            chartArea.AxisX.ScaleView.Zoom(xMin + xRange / 4, xMax - xRange / 4);
            chartArea.AxisY.ScaleView.Zoom(yMin + yRange / 4, yMax - yRange / 4);
        }

        private void ZoomOut(ChartArea chartArea) //音圈馬達chart zoom out   要加invoke
        {
            double xMin = chartArea.AxisX.ScaleView.ViewMinimum;
            double xMax = chartArea.AxisX.ScaleView.ViewMaximum;
            double yMin = chartArea.AxisY.ScaleView.ViewMinimum;
            double yMax = chartArea.AxisY.ScaleView.ViewMaximum;

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            chartArea.AxisX.ScaleView.Zoom(xMin - xRange / 2, xMax + xRange / 2);
            chartArea.AxisY.ScaleView.Zoom(yMin - yRange / 2, yMax + yRange / 2);
        }
        private async void tbnForcewrite_Click(object sender, EventArgs e) //音圈馬達力控按鈕
        {
            if (CheckBMotor())
            {
                chartTimer.Start();
                timeCounter = 0;
                isWrite = true;
                isBChartStop = false;
                //======================為了壓出跟BESI一樣的bonding profile而用JOG====================
                //try
                //{
                //    ControllerInstance.A.MoveAbs(594100, 5000000);
                //    await Task.Delay(2000);
                //    int AJogVel = Convert.ToInt32(tbAJogVel.Text);
                //    ControllerInstance.A.Jog(60);
                //    Log.WriteSystemLog("Picking Die.....");
                //    while (Convert.ToInt32(tbFeedback.Text) <= 100)
                //    {
                //        int timeout = 0;
                //        if (timeout > 300)
                //        {
                //            Log.WriteSystemLog("Touch down over time, Please check EQ Stats");
                //            return;
                //        }
                //        timeout++;
                //        await Task.Delay(10);
                //    }
                //    ControllerInstance.A.Stop();
                //    ControllerInstance.A.Jog(130);
                //    while (Convert.ToInt32(tbFeedback.Text) <= 130)
                //    {
                //        int timeout = 0;
                //        if (timeout > 300)
                //        {
                //            Log.WriteSystemLog("Touch down over time, Please check EQ Stats");
                //            return;
                //        }
                //        timeout++;
                //        await Task.Delay(10);
                //    }
                //    ControllerInstance.A.Stop();
                //    ControllerInstance.A.Jog(AJogVel);
                //    while (Convert.ToInt32(tbRefForce.Text) <= 290)
                //    {
                //        int timeout = 0;
                //        if (timeout > 300)
                //        {
                //            Log.WriteSystemLog("Touch down over time, Please check EQ Stats");
                //            return;
                //        }
                //        timeout++;
                //        await Task.Delay(10);
                //    }
                //    Log.WriteSystemLog("Touch down completed");
                //    ControllerInstance.A.Stop();
                //}
                //catch
                //{
                //    Log.WriteSystemLog("Touch down Error");
                //}
                ControllerInstance.B.ForceCurr_GotoForceMode();
                Log.WriteSystemLog("音圈動作中........");
                ControllerInstance.B.ForceCurr_SetForceCmdSource(1, new int[] { 1000, 1000, 1000, 1000, 1000, 1000 },
                    new int[] { Convert.ToInt32(tb11.Text), Convert.ToInt32(tb11.Text), Convert.ToInt32(tb21.Text), Convert.ToInt32(tb31.Text),  // hold time
                    Convert.ToInt32(tb41.Text), Convert.ToInt32(tb51.Text) },
                    new int[] { Convert.ToInt32(tb12.Text), Convert.ToInt32(tb12.Text), Convert.ToInt32(tb22.Text), Convert.ToInt32(tb32.Text),  // 對應力量
                    Convert.ToInt32(tb42.Text), Convert.ToInt32(tb52.Text) });
                int tottime = (Convert.ToInt32(tb11.Text) + Convert.ToInt32(tbx11.Text) + Convert.ToInt32(tbx21.Text) + Convert.ToInt32(tbx31.Text) + Convert.ToInt32(tbx41.Text) + Convert.ToInt32(tbx51.Text));
                await Task.Delay(tottime + 100);
                Log.WriteSystemLog("音圈行程結束");
                ControllerInstance.A.MoveAbs(0,500000);
                ControllerInstance.B.ForceCurr_GotoPosMode(); //壓完之後音圈馬達自動回home
                ControllerInstance.B.MoveAbs(0);
                isWrite = false;
                chartTimer.Stop(); // 停止畫圖計時器 thead
            }       
        }

        private  void btnBhome_Click(object sender, EventArgs e)//音圈馬達回home按鈕
        {
            if (CheckAMotor())
            {
                ControllerInstance.B.ForceCurr_GotoPosMode();
                ControllerInstance.B.MoveAbs(0);
            }
                
        }
        private void bntBMovStop_Click(object sender, EventArgs e)//音圈馬達停止動作按鈕
        {
            if (CheckAMotor())
                ControllerInstance.B.Stop();
        }

        private void btnclear_Click(object sender, EventArgs e)//音圈馬達清除chart按鈕
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            isWrite = false;
        }

        private void btnsave_Click(object sender, EventArgs e)//音圈馬達儲存chart圖片按鈕
        {
            using (Bitmap bitmap = new Bitmap(chart1.Width, chart1.Height))
            {
                chart1.DrawToBitmap(bitmap, new Rectangle(0, 0, chart1.Width, chart1.Height));

                Form previewForm = new Form
                {
                    Text = "Chart Preview",
                    Size = new Size(800, 600)
                };

                PictureBox pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = bitmap,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                previewForm.Controls.Add(pictureBox);

                previewForm.ShowDialog();
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = $@"C:\\Users\\User\\Desktop\\chart_{timestamp}.png"; 
            chart1.SaveImage(filePath, ChartImageFormat.Png);
            MessageBox.Show($"Chart has been exported as an image");
        }

        //==================================================Auto======================================================//
        private bool isAutoStop = false;//確認是否按下stop按鈕
        private async void btnAutorun_Click(object sender, EventArgs e)//Auto開始執行的按鈕
        {
           
            if (CheckAMotor() && CheckBMotor())
            {
                timeCounter = 0;
                timerExportCSV.Enabled = true; //負責匯出raw data的timer 啟用
                timerExportCSV.Interval = 1000; //raw data的timer區間為1000ms刷新一次
                timerExportCSV.Start();
                chartTimer.Start();
                while (!isAutoStop)
                {
                    //========================================吸Die的動作===================================
                    //try 
                    //{
                    //    ControllerInstance.A.MoveAbs(500000, 5000000);
                    //    await Task.Delay(3000);
                    //    ControllerInstance.A.MoveAbs(620000, 5000000);
                    //    Log.WriteSystemLog("Picking Die.....");
                    //    while (Convert.ToInt32(tbActPresure.Text) <= 85)
                    //    {
                    //        int timeout = 0;
                    //        if (timeout > 300)
                    //        {
                    //            Log.WriteSystemLog("Pick Die over time, Please check EQ Stats");
                    //            return;
                    //        }
                    //        timeout++;
                    //        await Task.Delay(100);
                    //    }
                    //    Log.WriteSystemLog("Pick Die completed");
                    //}
                    //catch 
                    //{
                    //    Log.WriteSystemLog("Pick Die Error");
                    //}
                    // 移動A motor


                    ControllerInstance.A.MoveAbs(0, 720000);
                    await Task.Delay(3000);
                    ControllerInstance.A.MoveAbs(210000, Convert.ToInt32(tbAmoVel.Text));
                    await Task.Delay(3000);
                    //ControllerInstance.A.MoveAbs(670000, 100000);
                    //await Task.Delay(3000);
                    //ControllerInstance.A.Stop();
                    CreateNewCSV();
                    //ControllerInstance.B.ForceCurr_SetSlowApproach(1000, 3000, 0, 1);

                    ControllerInstance.B.ForceCurr_GotoForceMode();
                    //ControllerInstance.B.ForceCurr_SetForceCmdSource(1, new int[] { 30, 30, 30, 30, 30, 30 },
                    //    new int[] { Convert.ToInt32(tbx11.Text), Convert.ToInt32(tbx11.Text), Convert.ToInt32(tbx21.Text), Convert.ToInt32(tbx31.Text),
                    //Convert.ToInt32(tbx41.Text), Convert.ToInt32(tbx51.Text) },
                    //    new int[] { Convert.ToInt32(tbx12.Text), Convert.ToInt32(tbx12.Text), Convert.ToInt32(tbx22.Text), Convert.ToInt32(tbx32.Text),
                    //Convert.ToInt32(tbx42.Text), Convert.ToInt32(tbx52.Text) });
                    //int tottime = (10000 + Convert.ToInt32(tbx11.Text) + Convert.ToInt32(tbx21.Text) + Convert.ToInt32(tbx31.Text) + Convert.ToInt32(tbx41.Text) + Convert.ToInt32(tbx51.Text));
                    //await Task.Delay(tottime + 100);

                    ControllerInstance.B.ForceCurr_SetForceCmdSource(1, new int[] { 30, 30},
                        new int[] { Convert.ToInt32(tbx11.Text), Convert.ToInt32(tbx11.Text)},
                        new int[] { Convert.ToInt32(tbx12.Text), Convert.ToInt32(tbx12.Text)});
                    int tottime = (10000 + Convert.ToInt32(tbx11.Text) + Convert.ToInt32(tbx21.Text));
                    await Task.Delay(tottime + 100);


                    ControllerInstance.B.ForceCurr_GotoPosMode();


                    //ControllerInstance.A.MoveAbs(0, 300000);
                    //await Task.Delay(2000);
                    ////ControllerInstance.A.MoveAbs(660000, 300000);//Convert.ToInt32(tbAmoVel.Text)
                    ////await Task.Delay(2000);
                    //ControllerInstance.A.MoveAbs(670000, 100000);  // 有時切不過去
                    //await Task.Delay(3000);
                    ////ControllerInstance.A.Stop();
                    //CreateNewCSV();  
                    //try
                    //{
                    //    ControllerInstance.B.ForceCurr_GotoForceMode();
                    //}
                    //catch
                    //{
                    //    Log.WriteSystemLog("不能切換ForceMode");
                    //}
                    //foreach (var series in chart2.Series)
                    //    series.Points.Clear();
                    //ControllerInstance.B.ForceCurr_SetForceCmdSource(1, new int[] { 1000, 1000, 1000, 1000, 1000, 1000 },
                    //    new int[] { Convert.ToInt32(tbx11.Text), Convert.ToInt32(tbx11.Text), Convert.ToInt32(tbx21.Text), Convert.ToInt32(tbx31.Text),
                    //Convert.ToInt32(tbx41.Text), Convert.ToInt32(tbx51.Text) },
                    //    new int[] { Convert.ToInt32(tbx12.Text), Convert.ToInt32(tbx12.Text), Convert.ToInt32(tbx22.Text), Convert.ToInt32(tbx32.Text),
                    //Convert.ToInt32(tbx42.Text), Convert.ToInt32(tbx52.Text) });
                    //int tottime = (2000 + Convert.ToInt32(tbx11.Text) + Convert.ToInt32(tbx21.Text) + Convert.ToInt32(tbx31.Text) + Convert.ToInt32(tbx41.Text) + Convert.ToInt32(tbx51.Text));
                    //await Task.Delay(tottime + 100);

                    //ControllerInstance.B.ForceCurr_GotoForceMode();
                    //Log.WriteSystemLog("音圈動作中........");
                    //ControllerInstance.B.ForceCurr_SetForceCmdSource(1, new int[] { 1000, 1000, 1000, 1000, 1000, 1000 },
                    //    new int[] { Convert.ToInt32(tb11.Text), Convert.ToInt32(tb11.Text), Convert.ToInt32(tb21.Text), Convert.ToInt32(tb31.Text),  // hold time
                    //Convert.ToInt32(tb41.Text), Convert.ToInt32(tb51.Text) },
                    //    new int[] { Convert.ToInt32(tb12.Text), Convert.ToInt32(tb12.Text), Convert.ToInt32(tb22.Text), Convert.ToInt32(tb32.Text),  // 對應力量
                    //Convert.ToInt32(tb42.Text), Convert.ToInt32(tb52.Text) });

                    ControllerInstance.B.ForceCurr_GotoPosMode();
                    isRecording = false;
                    ControllerInstance.B.MoveAbs(0);
                    ControllerInstance.A.MoveAbs(0, Convert.ToInt32(tbAmoVel.Text));
                    await Task.Delay(3000);
                }
                timerExportCSV.Stop();//若按下AutoStop 匯出raw data的timer即停止刷新
                // ControllerInstance.B.ForceCurr_GotoPosMode();
                isAutoStop = false;
                chartTimer.Stop();
            }       
        }

        private void btnAutoStop_Click(object sender, EventArgs e) //將A、B馬達動作都停止
        {
            if (CheckAMotor() || CheckBMotor())
            {
                isAutoStop = true;
                ControllerInstance.A.Stop();
                ControllerInstance.B.Stop();
            }              
        }
        private void btnAutoClear_Click(object sender, EventArgs e) //清除Auto mode的圖表
        {
            foreach (var series in chart2.Series)
            {
                series.Points.Clear();
            }
        }

        private void btnAutoSave_Click(object sender, EventArgs e) //保存Auto mode chart的圖片
        {
            using (Bitmap bitmap = new Bitmap(chart2.Width, chart2.Height))
            {
                chart2.DrawToBitmap(bitmap, new Rectangle(0, 0, chart2.Width, chart2.Height));
                Form previewForm = new Form
                {
                    Text = "Chart Preview",
                    Size = new Size(800, 600)
                };
                PictureBox pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = bitmap,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                previewForm.Controls.Add(pictureBox);
                previewForm.ShowDialog();
            }
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = $@"C:\\Users\\User\\Desktop\\Wave\\chart_{timestamp}.png";
            chart2.SaveImage(filePath, ChartImageFormat.Png);
            MessageBox.Show($"Chart has been exported as an image");
        }
        bool isAutoChartStop = false;//確認Auto mode chart是否暫停
        private void btnAutoWait_Click(object sender, EventArgs e)
        {
            isAutoChartStop = true;
        }

        private void btnAutoCont_Click(object sender, EventArgs e) // stop 作圖
        {
            foreach (var series in chart2.Series)
            {
                series.Points.Clear();
            }
            isAutoChartStop = false;
        }

        private void Auto_Chart_MouseWheel(object sender, MouseEventArgs e) // 縮放Auto mode chart的
        {
            if (isAutoChartStop)//Auto mode chart需暫停才可以縮放
            {
                var result = chart2.HitTest(e.X, e.Y);
                if (result.ChartArea != null)
                {
                    // Zoom in or out based on mouse wheel direction
                    ZoomChart(result.ChartArea, e.Delta);
                }
            }
            else
            {
                Log.WriteSystemLog("Auto mode 圖表繪製中，請先暫停圖表繪製");
            }
        }

        private void btnAutoHome_Click(object sender, EventArgs e) //將A、B馬達都回到home
        {
            if (CheckAMotor() && CheckBMotor())　　// Check 2 motor 都連上線
            {
                ControllerInstance.B.ForceCurr_GotoPosMode();
                ControllerInstance.B.MoveAbs(0);
                ControllerInstance.A.MoveAbs(0);
                isAutoStop = false;
            }
        }

        private bool isRecording = false;//確認是否正在紀錄raw data
        private readonly string directoryPath = $@"C:\\Users\\User\\Desktop\\DatraPoint"; //raw data檔案路徑
        private string currentFilePath;
        private void CreateNewCSV()
        {           
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // 每次開始新的行程時創建一個新的 CSV 檔案
            currentFilePath = Path.Combine(directoryPath, $"Time vs Force_{timestamp}.csv");
            isRecording = true;         
        }
        private void timerExportCSV_Tick(object sender, EventArgs e) //負責匯出raw data.csv的timer
        {
            if (isRecording)
            {
                var timestamp = DateTime.Now.ToString("o"); // ISO 8601 format for timestamp
                ControllerInstance.SendCommandString("BForceRef", out string Recresponse);
                var value = Recresponse;
                using (var writer = new StreamWriter(currentFilePath, append: true))
                {
                    writer.WriteLine($"{timestamp}, {value} g");
                }
            }
        }

        //===================================================Vaccum====================================================//
       
        private async void btnVcumConnect_Click(object sender, EventArgs e)//跟真空閥連線的按鈕
        {
            try
            {
                modbusClient.Connect();  //modbus connect
                MessageBox.Show("Pressure controller is connected.");
                isVacConnect = true;
                vacuumTimer = new System.Timers.Timer(20);  // 設定區間
                vacuumTimer.Elapsed += VacuumTimer_Elapsed;
                vacuumTimer.Start();
                Log.WriteSystemLog("Pressure controller is connected.");
            }
            catch
            {
                MessageBox.Show("Error, no connection between vacuum control systom.");
                Log.WriteSystemLog("Error, no connection between vacuum control systom, pls check system power.");
                return;
            }     
        }

        private void VacuumTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action(updateVacuumCon));
            else
                updateVacuumCon();
        }

        private void updateVacuumCon()
        {
            int startAddress = 0;
            try
            {
                int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters(startAddress, 1);   // 讀真空值
                tbActPresure.Text = readHoldingRegisters[0].ToString();
            }
            catch
            {
                Log.WriteSystemLog("Pressure controller did not connected.");
            }           
        }

        private void btnVcumDisconnect_Click(object sender, EventArgs e)
        {
            modbusClient.Disconnect();  //modbus disconnect
            isVacConnect = false;
            //tbVacLState.Text = "Disconnected";
            //tbVacLState.BackColor = Color.Red;
            vacuumTimer.Stop();// 停止更新
            MessageBox.Show("Pressure controller is disconnected.");           
        }

        private void timerVaccum_Tick(object sender, EventArgs e) // 用來刷新真空閥狀態UI的timer   請改system timer
        {
            //if (isVacConnect)
            //{
            //    tbVacLState.Text = "Connected";
            //    tbVacLState.BackColor = Color.Green;
            //}
            //else
            //{
            //    tbVacLState.Text = "Disconnected";
            //    tbVacLState.BackColor = Color.Red;
            //}

            //int startAddress = 10;
            //int[] readHoldingRegisters = modbusClient.ReadInputRegisters(startAddress, 1);   // 讀真空值
            //tbActPresure.Text = readHoldingRegisters[0].ToString();
        }

        private void tbLiveState_TextChanged(object sender, EventArgs e)
        {

        }
        //設定bond head真空閥所吸壓力的按鈕
        private void btnSetBHVac_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(0, Convert.ToInt32(tbSetBHVac.Text));   // 設定bond head 真空閥所吸壓
            Log.WriteSystemLog("Set Bond Head Vacuum = " + tbSetBHVac.Text + "kPa");
            Vacuum_Log.Write("Set Bond Head Vacuum = " + tbSetBHVac.Text + "kPa");
        }

        private void btnBmotionCtr_Click(object sender, EventArgs e)
        {
            if (BPosionMode)
            {
               // 3 = postion control mode / 4 =  force control
                ControllerInstance.B.ForceCurr_GotoPosMode(); // 先轉position模式
                tB_Bmotion.Text = "Position Control";
                Log.WriteSystemLog("音圈馬達控制模式: Position Control.");
            }
            else
            {
                ControllerInstance.B.ForceCurr_GotoForceMode(); // 轉 Force模式 
                tB_Bmotion.Text = "Force Control";
                Log.WriteSystemLog("音圈馬達控制模式: Force Control.");
                //isWrite = true;
            }
            BPosionMode = !BPosionMode;
        }

        private void btnBHVacOn_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(2, 1);   // 設定bond head 破真空 On 
            Log.WriteSystemLog("Set Bond Head pressue = ON!!");
            Vacuum_Log.Write("Set Bond Head pressure = ON!! ");
        }

        private void btnBHVacOff_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(2, 0);   // 設定bond head 真空閥 OFF 
            Log.WriteSystemLog("Set Bond Head Vacuum = OFF!!");
            Vacuum_Log.Write("Set Bond Head Vacuum = OFF!! ");
        }

        private void btnBHBlowOn_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(1, 1);   // 設定bond head 破真空 On 
            Log.WriteSystemLog("Set Bond Head pressue = ON!!");
            Vacuum_Log.Write("Set Bond Head pressure = ON!! ");
        }

        private void btnBHBlowOff_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(1, 0);   // 設定bond head 破真空 off
            Log.WriteSystemLog("Set Bond Head pressue = OFF!!");
            Vacuum_Log.Write("Set Bond Head pressure = OFF!! ");
        }

        private void btnCHVacOn_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(4, 1);   // 設定 Chuck 真空閥 On 
            Log.WriteSystemLog("Set Chuck Vacuum = ON!!");
            Vacuum_Log.Write("Set Chuck Vacuum = ON!! ");
        }

        private void btnCHVacOff_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(4, 0);   // 設定 Chuck 真空閥 Off 
            Log.WriteSystemLog("Set Chuck Vacuum = OFF!!");
            Vacuum_Log.Write("Set Chuck Vacuum = OFF!! ");
        }

        private void btnCHBlowOn_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(3, 1);   // 設定 Chuck 破真空 On 
            Log.WriteSystemLog("Set Chuck Pressue = ON!!");
            Vacuum_Log.Write("Set Chuck Pressue = ON!! ");
        }

        private void btnCHBlowOff_Click(object sender, EventArgs e)
        {
            modbusClient.WriteSingleRegister(3, 0);   // 設定 Chuck 破真空 OFF 
            Log.WriteSystemLog("Set Chuck Pressue = OFF!!");
            Vacuum_Log.Write("Set Chuck Pressue = OFF!! ");
        }

        private void tbActPresure_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

