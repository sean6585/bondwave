namespace AML
{
    partial class Robot
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox_Setting = new System.Windows.Forms.GroupBox();
            this.comboBox_MainPort = new System.Windows.Forms.ComboBox();
            this.Label_MainPort = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_ArrivedPoint = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label_GoPoint = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.forkView_Upper = new AML.ForkView();
            this.label_StatusCode = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.RemoteLabel = new System.Windows.Forms.Label();
            this.label_Error = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.BusyLabel = new System.Windows.Forms.Label();
            this.forkView_Lower = new AML.ForkView();
            this.backgroundWorker_Robot_COM = new System.ComponentModel.BackgroundWorker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox_ControlParameter = new System.Windows.Forms.GroupBox();
            this.groupBox_Speed = new System.Windows.Forms.GroupBox();
            this.comboBox_Other_Speed = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_Z_Speed = new System.Windows.Forms.ComboBox();
            this.label_Zoffset = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label_Addr = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_Slot = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_Target = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_UseArm = new System.Windows.Forms.ComboBox();
            this.listBox_RobotMacroCommand = new System.Windows.Forms.ListBox();
            this.button_Robot_Command = new System.Windows.Forms.Button();
            this.label_Motion = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox_Setting.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_ControlParameter.SuspendLayout();
            this.groupBox_Speed.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Setting
            // 
            this.groupBox_Setting.Controls.Add(this.label_Motion);
            this.groupBox_Setting.Controls.Add(this.label13);
            this.groupBox_Setting.Controls.Add(this.comboBox_MainPort);
            this.groupBox_Setting.Controls.Add(this.Label_MainPort);
            this.groupBox_Setting.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox_Setting.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox_Setting.Location = new System.Drawing.Point(0, 0);
            this.groupBox_Setting.Name = "groupBox_Setting";
            this.groupBox_Setting.Size = new System.Drawing.Size(551, 64);
            this.groupBox_Setting.TabIndex = 100;
            this.groupBox_Setting.TabStop = false;
            this.groupBox_Setting.Text = "Setting";
            // 
            // comboBox_MainPort
            // 
            this.comboBox_MainPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MainPort.Enabled = false;
            this.comboBox_MainPort.FormattingEnabled = true;
            this.comboBox_MainPort.Location = new System.Drawing.Point(93, 24);
            this.comboBox_MainPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_MainPort.Name = "comboBox_MainPort";
            this.comboBox_MainPort.Size = new System.Drawing.Size(95, 29);
            this.comboBox_MainPort.TabIndex = 94;
            this.comboBox_MainPort.DropDown += new System.EventHandler(this.comboBox_MainPort_DropDown);
            // 
            // Label_MainPort
            // 
            this.Label_MainPort.AutoSize = true;
            this.Label_MainPort.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_MainPort.Location = new System.Drawing.Point(3, 29);
            this.Label_MainPort.Margin = new System.Windows.Forms.Padding(10);
            this.Label_MainPort.Name = "Label_MainPort";
            this.Label_MainPort.Size = new System.Drawing.Size(85, 21);
            this.Label_MainPort.TabIndex = 80;
            this.Label_MainPort.Text = "Main Port";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_ArrivedPoint);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label_GoPoint);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.forkView_Upper);
            this.groupBox1.Controls.Add(this.label_StatusCode);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.RemoteLabel);
            this.groupBox1.Controls.Add(this.label_Error);
            this.groupBox1.Controls.Add(this.Label6);
            this.groupBox1.Controls.Add(this.BusyLabel);
            this.groupBox1.Controls.Add(this.forkView_Lower);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(0, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(270, 586);
            this.groupBox1.TabIndex = 102;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // label_ArrivedPoint
            // 
            this.label_ArrivedPoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_ArrivedPoint.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_ArrivedPoint.Location = new System.Drawing.Point(138, 117);
            this.label_ArrivedPoint.Margin = new System.Windows.Forms.Padding(10);
            this.label_ArrivedPoint.Name = "label_ArrivedPoint";
            this.label_ArrivedPoint.Size = new System.Drawing.Size(100, 21);
            this.label_ArrivedPoint.TabIndex = 91;
            this.label_ArrivedPoint.Text = "Unknown";
            this.label_ArrivedPoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label8.Location = new System.Drawing.Point(9, 117);
            this.label8.Margin = new System.Windows.Forms.Padding(10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(114, 21);
            this.label8.TabIndex = 90;
            this.label8.Text = "Arrived Point:";
            // 
            // label_GoPoint
            // 
            this.label_GoPoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_GoPoint.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_GoPoint.Location = new System.Drawing.Point(138, 147);
            this.label_GoPoint.Margin = new System.Windows.Forms.Padding(10);
            this.label_GoPoint.Name = "label_GoPoint";
            this.label_GoPoint.Size = new System.Drawing.Size(100, 21);
            this.label_GoPoint.TabIndex = 89;
            this.label_GoPoint.Text = "Unknown";
            this.label_GoPoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label11.Location = new System.Drawing.Point(9, 147);
            this.label11.Margin = new System.Windows.Forms.Padding(10);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 21);
            this.label11.TabIndex = 88;
            this.label11.Text = "Go Point:";
            // 
            // forkView_Upper
            // 
            this.forkView_Upper.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.forkView_Upper.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.forkView_Upper.Location = new System.Drawing.Point(3, 301);
            this.forkView_Upper.Margin = new System.Windows.Forms.Padding(5);
            this.forkView_Upper.Name = "forkView_Upper";
            this.forkView_Upper.Size = new System.Drawing.Size(264, 133);
            this.forkView_Upper.TabIndex = 86;
            // 
            // label_StatusCode
            // 
            this.label_StatusCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_StatusCode.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_StatusCode.Location = new System.Drawing.Point(138, 56);
            this.label_StatusCode.Margin = new System.Windows.Forms.Padding(10);
            this.label_StatusCode.Name = "label_StatusCode";
            this.label_StatusCode.Size = new System.Drawing.Size(100, 21);
            this.label_StatusCode.TabIndex = 85;
            this.label_StatusCode.Text = "Unknown";
            this.label_StatusCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(9, 56);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 21);
            this.label1.TabIndex = 84;
            this.label1.Text = "Status Code";
            // 
            // RemoteLabel
            // 
            this.RemoteLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RemoteLabel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RemoteLabel.Location = new System.Drawing.Point(138, 86);
            this.RemoteLabel.Margin = new System.Windows.Forms.Padding(10);
            this.RemoteLabel.Name = "RemoteLabel";
            this.RemoteLabel.Size = new System.Drawing.Size(100, 21);
            this.RemoteLabel.TabIndex = 82;
            this.RemoteLabel.Text = "Unknown";
            this.RemoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Error
            // 
            this.label_Error.BackColor = System.Drawing.Color.Red;
            this.label_Error.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Error.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Error.Location = new System.Drawing.Point(138, 25);
            this.label_Error.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Error.Name = "label_Error";
            this.label_Error.Size = new System.Drawing.Size(100, 21);
            this.label_Error.TabIndex = 80;
            this.label_Error.Text = "ERROR";
            this.label_Error.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Error.Visible = false;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label6.Location = new System.Drawing.Point(9, 86);
            this.Label6.Margin = new System.Windows.Forms.Padding(10);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(55, 21);
            this.Label6.TabIndex = 74;
            this.Label6.Text = "Mode";
            // 
            // BusyLabel
            // 
            this.BusyLabel.BackColor = System.Drawing.Color.Yellow;
            this.BusyLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BusyLabel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BusyLabel.Location = new System.Drawing.Point(13, 25);
            this.BusyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BusyLabel.Name = "BusyLabel";
            this.BusyLabel.Size = new System.Drawing.Size(100, 21);
            this.BusyLabel.TabIndex = 40;
            this.BusyLabel.Text = "BUSY";
            this.BusyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BusyLabel.Visible = false;
            // 
            // forkView_Lower
            // 
            this.forkView_Lower.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.forkView_Lower.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.forkView_Lower.Location = new System.Drawing.Point(3, 434);
            this.forkView_Lower.Margin = new System.Windows.Forms.Padding(5);
            this.forkView_Lower.Name = "forkView_Lower";
            this.forkView_Lower.Size = new System.Drawing.Size(264, 149);
            this.forkView_Lower.TabIndex = 87;
            // 
            // backgroundWorker_Robot_COM
            // 
            this.backgroundWorker_Robot_COM.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Robot_COM_DoWork);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox_ControlParameter);
            this.groupBox2.Controls.Add(this.listBox_RobotMacroCommand);
            this.groupBox2.Controls.Add(this.button_Robot_Command);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox2.Location = new System.Drawing.Point(270, 64);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(274, 586);
            this.groupBox2.TabIndex = 103;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Robot Control";
            // 
            // groupBox_ControlParameter
            // 
            this.groupBox_ControlParameter.Controls.Add(this.groupBox_Speed);
            this.groupBox_ControlParameter.Controls.Add(this.label_Zoffset);
            this.groupBox_ControlParameter.Controls.Add(this.label9);
            this.groupBox_ControlParameter.Controls.Add(this.label_Addr);
            this.groupBox_ControlParameter.Controls.Add(this.label4);
            this.groupBox_ControlParameter.Controls.Add(this.comboBox_Slot);
            this.groupBox_ControlParameter.Controls.Add(this.label7);
            this.groupBox_ControlParameter.Controls.Add(this.comboBox_Target);
            this.groupBox_ControlParameter.Controls.Add(this.label5);
            this.groupBox_ControlParameter.Controls.Add(this.label2);
            this.groupBox_ControlParameter.Controls.Add(this.comboBox_UseArm);
            this.groupBox_ControlParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_ControlParameter.Location = new System.Drawing.Point(3, 323);
            this.groupBox_ControlParameter.Name = "groupBox_ControlParameter";
            this.groupBox_ControlParameter.Size = new System.Drawing.Size(268, 221);
            this.groupBox_ControlParameter.TabIndex = 2;
            this.groupBox_ControlParameter.TabStop = false;
            this.groupBox_ControlParameter.Text = "Parameter";
            // 
            // groupBox_Speed
            // 
            this.groupBox_Speed.Controls.Add(this.comboBox_Other_Speed);
            this.groupBox_Speed.Controls.Add(this.label10);
            this.groupBox_Speed.Controls.Add(this.label3);
            this.groupBox_Speed.Controls.Add(this.comboBox_Z_Speed);
            this.groupBox_Speed.Location = new System.Drawing.Point(3, 143);
            this.groupBox_Speed.Name = "groupBox_Speed";
            this.groupBox_Speed.Size = new System.Drawing.Size(259, 56);
            this.groupBox_Speed.TabIndex = 111;
            this.groupBox_Speed.TabStop = false;
            this.groupBox_Speed.Text = "Speed";
            // 
            // comboBox_Other_Speed
            // 
            this.comboBox_Other_Speed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Other_Speed.FormattingEnabled = true;
            this.comboBox_Other_Speed.Location = new System.Drawing.Point(183, 21);
            this.comboBox_Other_Speed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Other_Speed.Name = "comboBox_Other_Speed";
            this.comboBox_Other_Speed.Size = new System.Drawing.Size(68, 29);
            this.comboBox_Other_Speed.TabIndex = 115;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label10.Location = new System.Drawing.Point(126, 24);
            this.label10.Margin = new System.Windows.Forms.Padding(10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 21);
            this.label10.TabIndex = 114;
            this.label10.Text = "Other:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(13, 27);
            this.label3.Margin = new System.Windows.Forms.Padding(10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 21);
            this.label3.TabIndex = 112;
            this.label3.Text = "Z:";
            // 
            // comboBox_Z_Speed
            // 
            this.comboBox_Z_Speed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Z_Speed.FormattingEnabled = true;
            this.comboBox_Z_Speed.Location = new System.Drawing.Point(51, 21);
            this.comboBox_Z_Speed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Z_Speed.Name = "comboBox_Z_Speed";
            this.comboBox_Z_Speed.Size = new System.Drawing.Size(68, 29);
            this.comboBox_Z_Speed.TabIndex = 113;
            // 
            // label_Zoffset
            // 
            this.label_Zoffset.AutoSize = true;
            this.label_Zoffset.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Zoffset.Location = new System.Drawing.Point(210, 197);
            this.label_Zoffset.Margin = new System.Windows.Forms.Padding(10);
            this.label_Zoffset.Name = "label_Zoffset";
            this.label_Zoffset.Size = new System.Drawing.Size(20, 21);
            this.label_Zoffset.TabIndex = 110;
            this.label_Zoffset.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label9.Location = new System.Drawing.Point(142, 197);
            this.label9.Margin = new System.Windows.Forms.Padding(10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 21);
            this.label9.TabIndex = 109;
            this.label9.Text = "Z Offset";
            // 
            // label_Addr
            // 
            this.label_Addr.AutoSize = true;
            this.label_Addr.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Addr.Location = new System.Drawing.Point(66, 197);
            this.label_Addr.Margin = new System.Windows.Forms.Padding(10);
            this.label_Addr.Name = "label_Addr";
            this.label_Addr.Size = new System.Drawing.Size(20, 21);
            this.label_Addr.TabIndex = 108;
            this.label_Addr.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(10, 197);
            this.label4.Margin = new System.Windows.Forms.Padding(10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 21);
            this.label4.TabIndex = 107;
            this.label4.Text = "Addr";
            // 
            // comboBox_Slot
            // 
            this.comboBox_Slot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Slot.FormattingEnabled = true;
            this.comboBox_Slot.Location = new System.Drawing.Point(100, 106);
            this.comboBox_Slot.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Slot.Name = "comboBox_Slot";
            this.comboBox_Slot.Size = new System.Drawing.Size(132, 29);
            this.comboBox_Slot.TabIndex = 106;
            this.comboBox_Slot.SelectedIndexChanged += new System.EventHandler(this.comboBox_Slot_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.Location = new System.Drawing.Point(10, 109);
            this.label7.Margin = new System.Windows.Forms.Padding(10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 21);
            this.label7.TabIndex = 105;
            this.label7.Text = "Slot";
            // 
            // comboBox_Target
            // 
            this.comboBox_Target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Target.FormattingEnabled = true;
            this.comboBox_Target.Location = new System.Drawing.Point(100, 65);
            this.comboBox_Target.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Target.Name = "comboBox_Target";
            this.comboBox_Target.Size = new System.Drawing.Size(132, 29);
            this.comboBox_Target.TabIndex = 104;
            this.comboBox_Target.DropDown += new System.EventHandler(this.comboBox_Tartget_DropDown);
            this.comboBox_Target.SelectedIndexChanged += new System.EventHandler(this.comboBox_Tartget_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(10, 68);
            this.label5.Margin = new System.Windows.Forms.Padding(10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 21);
            this.label5.TabIndex = 103;
            this.label5.Text = "Target";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(10, 27);
            this.label2.Margin = new System.Windows.Forms.Padding(10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 21);
            this.label2.TabIndex = 101;
            this.label2.Text = "Use Arm";
            // 
            // comboBox_UseArm
            // 
            this.comboBox_UseArm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_UseArm.FormattingEnabled = true;
            this.comboBox_UseArm.Location = new System.Drawing.Point(100, 24);
            this.comboBox_UseArm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_UseArm.Name = "comboBox_UseArm";
            this.comboBox_UseArm.Size = new System.Drawing.Size(132, 29);
            this.comboBox_UseArm.TabIndex = 102;
            this.comboBox_UseArm.DropDown += new System.EventHandler(this.comboBox_UseArm_DropDown);
            this.comboBox_UseArm.SelectedIndexChanged += new System.EventHandler(this.comboBox_UseArm_SelectedIndexChanged);
            // 
            // listBox_RobotMacroCommand
            // 
            this.listBox_RobotMacroCommand.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox_RobotMacroCommand.FormattingEnabled = true;
            this.listBox_RobotMacroCommand.ItemHeight = 21;
            this.listBox_RobotMacroCommand.Location = new System.Drawing.Point(3, 25);
            this.listBox_RobotMacroCommand.Name = "listBox_RobotMacroCommand";
            this.listBox_RobotMacroCommand.Size = new System.Drawing.Size(268, 298);
            this.listBox_RobotMacroCommand.TabIndex = 0;
            this.listBox_RobotMacroCommand.SelectedIndexChanged += new System.EventHandler(this.listBox_RobotMacroCommand_SelectedIndexChanged);
            // 
            // button_Robot_Command
            // 
            this.button_Robot_Command.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button_Robot_Command.Location = new System.Drawing.Point(3, 544);
            this.button_Robot_Command.Name = "button_Robot_Command";
            this.button_Robot_Command.Size = new System.Drawing.Size(268, 39);
            this.button_Robot_Command.TabIndex = 1;
            this.button_Robot_Command.Text = "Robot Command";
            this.button_Robot_Command.UseVisualStyleBackColor = true;
            this.button_Robot_Command.Click += new System.EventHandler(this.button_Robot_Command_Click);
            // 
            // label_Motion
            // 
            this.label_Motion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Motion.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Motion.Location = new System.Drawing.Point(271, 29);
            this.label_Motion.Margin = new System.Windows.Forms.Padding(10);
            this.label_Motion.Name = "label_Motion";
            this.label_Motion.Size = new System.Drawing.Size(100, 21);
            this.label_Motion.TabIndex = 96;
            this.label_Motion.Text = "Unknown";
            this.label_Motion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label13.Location = new System.Drawing.Point(198, 29);
            this.label13.Margin = new System.Windows.Forms.Padding(10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 21);
            this.label13.TabIndex = 95;
            this.label13.Text = "Motion:";
            // 
            // Robot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 650);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_Setting);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Robot";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Robot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Robot_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.Robot_VisibleChanged);
            this.groupBox_Setting.ResumeLayout(false);
            this.groupBox_Setting.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox_ControlParameter.ResumeLayout(false);
            this.groupBox_ControlParameter.PerformLayout();
            this.groupBox_Speed.ResumeLayout(false);
            this.groupBox_Speed.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox_Setting;
        private System.Windows.Forms.ComboBox comboBox_MainPort;
        internal System.Windows.Forms.Label Label_MainPort;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.Label RemoteLabel;
        private System.Windows.Forms.Label label_Error;
        internal System.Windows.Forms.Label Label6;
        private System.Windows.Forms.Label BusyLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Robot_COM;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox listBox_RobotMacroCommand;
        private System.Windows.Forms.Button button_Robot_Command;
        internal System.Windows.Forms.Label label_StatusCode;
        internal System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox_ControlParameter;
        internal System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_UseArm;
        private System.Windows.Forms.ComboBox comboBox_Target;
        internal System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_Slot;
        internal System.Windows.Forms.Label label7;
        private ForkView forkView_Lower;
        private ForkView forkView_Upper;
        internal System.Windows.Forms.Label label_Addr;
        internal System.Windows.Forms.Label label4;
        internal System.Windows.Forms.Label label_Zoffset;
        internal System.Windows.Forms.Label label9;
        internal System.Windows.Forms.Label label_ArrivedPoint;
        internal System.Windows.Forms.Label label8;
        internal System.Windows.Forms.Label label_GoPoint;
        internal System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox_Speed;
        private System.Windows.Forms.ComboBox comboBox_Other_Speed;
        internal System.Windows.Forms.Label label10;
        internal System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_Z_Speed;
        internal System.Windows.Forms.Label label_Motion;
        internal System.Windows.Forms.Label label13;
    }
}