namespace AML
{
    partial class Aligner
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
            this.label_AdamIP = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_Type = new System.Windows.Forms.ComboBox();
            this.comboBox_MainPort = new System.Windows.Forms.ComboBox();
            this.Label_MainPort = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox_ControlParameter = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_Degree = new System.Windows.Forms.NumericUpDown();
            this.listBox_AlignerCommand = new System.Windows.Forms.ListBox();
            this.button_Aligner_Command = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.RemoteLabel = new System.Windows.Forms.Label();
            this.label_NotchDetectionStatus = new System.Windows.Forms.Label();
            this.label_Status = new System.Windows.Forms.Label();
            this.label_15 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label_DeviceStatus = new System.Windows.Forms.Label();
            this.label_Vac = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label_OperatingState = new System.Windows.Forms.Label();
            this.label_WaferPresent = new System.Windows.Forms.Label();
            this.label_14 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label_ClampStatus = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_FrameWaferPresent = new System.Windows.Forms.Label();
            this.ocr_Top = new AML.OCR();
            this.ocr_Bottom = new AML.OCR();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BusyLabel = new System.Windows.Forms.Label();
            this.label_Error = new System.Windows.Forms.Label();
            this.backgroundWorker_Aligner_Send = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker_Aligner_Receive = new System.ComponentModel.BackgroundWorker();
            this.groupBox_Setting.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_ControlParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Degree)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Setting
            // 
            this.groupBox_Setting.Controls.Add(this.label_AdamIP);
            this.groupBox_Setting.Controls.Add(this.label4);
            this.groupBox_Setting.Controls.Add(this.comboBox_Type);
            this.groupBox_Setting.Controls.Add(this.comboBox_MainPort);
            this.groupBox_Setting.Controls.Add(this.Label_MainPort);
            this.groupBox_Setting.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox_Setting.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox_Setting.Location = new System.Drawing.Point(0, 0);
            this.groupBox_Setting.Name = "groupBox_Setting";
            this.groupBox_Setting.Size = new System.Drawing.Size(674, 61);
            this.groupBox_Setting.TabIndex = 101;
            this.groupBox_Setting.TabStop = false;
            this.groupBox_Setting.Text = "Setting";
            // 
            // label_AdamIP
            // 
            this.label_AdamIP.AutoSize = true;
            this.label_AdamIP.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_AdamIP.Location = new System.Drawing.Point(420, 29);
            this.label_AdamIP.Margin = new System.Windows.Forms.Padding(10);
            this.label_AdamIP.Name = "label_AdamIP";
            this.label_AdamIP.Size = new System.Drawing.Size(29, 21);
            this.label_AdamIP.TabIndex = 101;
            this.label_AdamIP.Text = "IP:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(7, 29);
            this.label4.Margin = new System.Windows.Forms.Padding(10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 21);
            this.label4.TabIndex = 99;
            this.label4.Text = "Type";
            // 
            // comboBox_Type
            // 
            this.comboBox_Type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Type.Enabled = false;
            this.comboBox_Type.FormattingEnabled = true;
            this.comboBox_Type.Location = new System.Drawing.Point(68, 24);
            this.comboBox_Type.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Type.Name = "comboBox_Type";
            this.comboBox_Type.Size = new System.Drawing.Size(139, 29);
            this.comboBox_Type.TabIndex = 100;
            this.comboBox_Type.DropDown += new System.EventHandler(this.comboBox_Type_DropDown);
            // 
            // comboBox_MainPort
            // 
            this.comboBox_MainPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MainPort.Enabled = false;
            this.comboBox_MainPort.FormattingEnabled = true;
            this.comboBox_MainPort.Location = new System.Drawing.Point(311, 24);
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
            this.Label_MainPort.Location = new System.Drawing.Point(221, 29);
            this.Label_MainPort.Margin = new System.Windows.Forms.Padding(10);
            this.Label_MainPort.Name = "Label_MainPort";
            this.Label_MainPort.Size = new System.Drawing.Size(85, 21);
            this.Label_MainPort.TabIndex = 80;
            this.Label_MainPort.Text = "Main Port";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox_ControlParameter);
            this.groupBox2.Controls.Add(this.listBox_AlignerCommand);
            this.groupBox2.Controls.Add(this.button_Aligner_Command);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox2.Location = new System.Drawing.Point(411, 61);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(247, 534);
            this.groupBox2.TabIndex = 105;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Aligner Control";
            // 
            // groupBox_ControlParameter
            // 
            this.groupBox_ControlParameter.Controls.Add(this.label2);
            this.groupBox_ControlParameter.Controls.Add(this.numericUpDown_Degree);
            this.groupBox_ControlParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_ControlParameter.Location = new System.Drawing.Point(3, 365);
            this.groupBox_ControlParameter.Name = "groupBox_ControlParameter";
            this.groupBox_ControlParameter.Size = new System.Drawing.Size(241, 127);
            this.groupBox_ControlParameter.TabIndex = 2;
            this.groupBox_ControlParameter.TabStop = false;
            this.groupBox_ControlParameter.Text = "Parameter";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(13, 30);
            this.label2.Margin = new System.Windows.Forms.Padding(10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 21);
            this.label2.TabIndex = 86;
            this.label2.Text = "Degree";
            // 
            // numericUpDown_Degree
            // 
            this.numericUpDown_Degree.Location = new System.Drawing.Point(109, 28);
            this.numericUpDown_Degree.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown_Degree.Name = "numericUpDown_Degree";
            this.numericUpDown_Degree.Size = new System.Drawing.Size(120, 29);
            this.numericUpDown_Degree.TabIndex = 0;
            // 
            // listBox_AlignerCommand
            // 
            this.listBox_AlignerCommand.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox_AlignerCommand.FormattingEnabled = true;
            this.listBox_AlignerCommand.ItemHeight = 21;
            this.listBox_AlignerCommand.Location = new System.Drawing.Point(3, 25);
            this.listBox_AlignerCommand.Name = "listBox_AlignerCommand";
            this.listBox_AlignerCommand.Size = new System.Drawing.Size(241, 340);
            this.listBox_AlignerCommand.TabIndex = 0;
            // 
            // button_Aligner_Command
            // 
            this.button_Aligner_Command.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button_Aligner_Command.Location = new System.Drawing.Point(3, 492);
            this.button_Aligner_Command.Name = "button_Aligner_Command";
            this.button_Aligner_Command.Size = new System.Drawing.Size(241, 39);
            this.button_Aligner_Command.TabIndex = 1;
            this.button_Aligner_Command.Text = "Aligner Command";
            this.button_Aligner_Command.UseVisualStyleBackColor = true;
            this.button_Aligner_Command.Click += new System.EventHandler(this.button_Aligner_Command_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.Controls.Add(this.ocr_Top);
            this.groupBox1.Controls.Add(this.ocr_Bottom);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(0, 61);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(411, 534);
            this.groupBox1.TabIndex = 104;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(3, 68);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(405, 241);
            this.tabControl1.TabIndex = 100;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.Label6);
            this.tabPage1.Controls.Add(this.RemoteLabel);
            this.tabPage1.Controls.Add(this.label_NotchDetectionStatus);
            this.tabPage1.Controls.Add(this.label_Status);
            this.tabPage1.Controls.Add(this.label_15);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.label_DeviceStatus);
            this.tabPage1.Controls.Add(this.label_Vac);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label_OperatingState);
            this.tabPage1.Controls.Add(this.label_WaferPresent);
            this.tabPage1.Controls.Add(this.label_14);
            this.tabPage1.Location = new System.Drawing.Point(4, 33);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(397, 204);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(10, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 21);
            this.label1.TabIndex = 84;
            this.label1.Text = "Status";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label6.Location = new System.Drawing.Point(10, 35);
            this.Label6.Margin = new System.Windows.Forms.Padding(10);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(55, 21);
            this.Label6.TabIndex = 74;
            this.Label6.Text = "Mode";
            // 
            // RemoteLabel
            // 
            this.RemoteLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RemoteLabel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RemoteLabel.Location = new System.Drawing.Point(205, 35);
            this.RemoteLabel.Margin = new System.Windows.Forms.Padding(10);
            this.RemoteLabel.Name = "RemoteLabel";
            this.RemoteLabel.Size = new System.Drawing.Size(178, 21);
            this.RemoteLabel.TabIndex = 82;
            this.RemoteLabel.Text = "Unknown";
            this.RemoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_NotchDetectionStatus
            // 
            this.label_NotchDetectionStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_NotchDetectionStatus.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_NotchDetectionStatus.Location = new System.Drawing.Point(205, 178);
            this.label_NotchDetectionStatus.Margin = new System.Windows.Forms.Padding(10);
            this.label_NotchDetectionStatus.Name = "label_NotchDetectionStatus";
            this.label_NotchDetectionStatus.Size = new System.Drawing.Size(178, 21);
            this.label_NotchDetectionStatus.TabIndex = 97;
            this.label_NotchDetectionStatus.Text = "Unknown";
            this.label_NotchDetectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Status
            // 
            this.label_Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Status.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Status.Location = new System.Drawing.Point(205, 6);
            this.label_Status.Margin = new System.Windows.Forms.Padding(10);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(178, 21);
            this.label_Status.TabIndex = 85;
            this.label_Status.Text = "Unknown";
            this.label_Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_15
            // 
            this.label_15.AutoSize = true;
            this.label_15.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_15.Location = new System.Drawing.Point(10, 178);
            this.label_15.Margin = new System.Windows.Forms.Padding(10);
            this.label_15.Name = "label_15";
            this.label_15.Size = new System.Drawing.Size(187, 21);
            this.label_15.TabIndex = 96;
            this.label_15.Text = "Notch Detection Status";
            this.label_15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.Location = new System.Drawing.Point(10, 63);
            this.label7.Margin = new System.Windows.Forms.Padding(10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 21);
            this.label7.TabIndex = 88;
            this.label7.Text = "Vacuum";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_DeviceStatus
            // 
            this.label_DeviceStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_DeviceStatus.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_DeviceStatus.Location = new System.Drawing.Point(205, 120);
            this.label_DeviceStatus.Margin = new System.Windows.Forms.Padding(10);
            this.label_DeviceStatus.Name = "label_DeviceStatus";
            this.label_DeviceStatus.Size = new System.Drawing.Size(178, 21);
            this.label_DeviceStatus.TabIndex = 95;
            this.label_DeviceStatus.Text = "Unknown";
            this.label_DeviceStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Vac
            // 
            this.label_Vac.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Vac.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Vac.Location = new System.Drawing.Point(205, 63);
            this.label_Vac.Margin = new System.Windows.Forms.Padding(10);
            this.label_Vac.Name = "label_Vac";
            this.label_Vac.Size = new System.Drawing.Size(178, 21);
            this.label_Vac.TabIndex = 89;
            this.label_Vac.Text = "Unknown";
            this.label_Vac.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label13.Location = new System.Drawing.Point(10, 120);
            this.label13.Margin = new System.Windows.Forms.Padding(10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(109, 21);
            this.label13.TabIndex = 94;
            this.label13.Text = "DeviceStatus";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(10, 92);
            this.label5.Margin = new System.Windows.Forms.Padding(10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 21);
            this.label5.TabIndex = 90;
            this.label5.Text = "WaferPresent";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_OperatingState
            // 
            this.label_OperatingState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_OperatingState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_OperatingState.Location = new System.Drawing.Point(205, 150);
            this.label_OperatingState.Margin = new System.Windows.Forms.Padding(10);
            this.label_OperatingState.Name = "label_OperatingState";
            this.label_OperatingState.Size = new System.Drawing.Size(178, 21);
            this.label_OperatingState.TabIndex = 93;
            this.label_OperatingState.Text = "Unknown";
            this.label_OperatingState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_WaferPresent
            // 
            this.label_WaferPresent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_WaferPresent.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_WaferPresent.Location = new System.Drawing.Point(205, 92);
            this.label_WaferPresent.Margin = new System.Windows.Forms.Padding(10);
            this.label_WaferPresent.Name = "label_WaferPresent";
            this.label_WaferPresent.Size = new System.Drawing.Size(178, 21);
            this.label_WaferPresent.TabIndex = 91;
            this.label_WaferPresent.Text = "Unknown";
            this.label_WaferPresent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_14
            // 
            this.label_14.AutoSize = true;
            this.label_14.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_14.Location = new System.Drawing.Point(10, 150);
            this.label_14.Margin = new System.Windows.Forms.Padding(10);
            this.label_14.Name = "label_14";
            this.label_14.Size = new System.Drawing.Size(130, 21);
            this.label_14.TabIndex = 92;
            this.label_14.Text = "Operating State";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.panel3);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Controls.Add(this.label_ClampStatus);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label_FrameWaferPresent);
            this.tabPage2.Location = new System.Drawing.Point(4, 33);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(397, 204);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label8.Location = new System.Drawing.Point(11, 77);
            this.label8.Margin = new System.Windows.Forms.Padding(10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(137, 21);
            this.label8.TabIndex = 98;
            this.label8.Text = "WaferPresent IO";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(310, 77);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(74, 22);
            this.panel3.TabIndex = 97;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(206, 77);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(74, 22);
            this.panel2.TabIndex = 96;
            // 
            // label_ClampStatus
            // 
            this.label_ClampStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_ClampStatus.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_ClampStatus.Location = new System.Drawing.Point(206, 13);
            this.label_ClampStatus.Margin = new System.Windows.Forms.Padding(10);
            this.label_ClampStatus.Name = "label_ClampStatus";
            this.label_ClampStatus.Size = new System.Drawing.Size(178, 21);
            this.label_ClampStatus.TabIndex = 95;
            this.label_ClampStatus.Text = "Unknown";
            this.label_ClampStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label9.Location = new System.Drawing.Point(11, 13);
            this.label9.Margin = new System.Windows.Forms.Padding(10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(111, 21);
            this.label9.TabIndex = 94;
            this.label9.Text = "Clamp Status";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(11, 43);
            this.label3.Margin = new System.Windows.Forms.Padding(10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 21);
            this.label3.TabIndex = 92;
            this.label3.Text = "WaferPresent";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_FrameWaferPresent
            // 
            this.label_FrameWaferPresent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_FrameWaferPresent.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_FrameWaferPresent.Location = new System.Drawing.Point(206, 43);
            this.label_FrameWaferPresent.Margin = new System.Windows.Forms.Padding(10);
            this.label_FrameWaferPresent.Name = "label_FrameWaferPresent";
            this.label_FrameWaferPresent.Size = new System.Drawing.Size(178, 21);
            this.label_FrameWaferPresent.TabIndex = 93;
            this.label_FrameWaferPresent.Text = "Unknown";
            this.label_FrameWaferPresent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ocr_Top
            // 
            this.ocr_Top.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ocr_Top.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ocr_Top.Location = new System.Drawing.Point(3, 312);
            this.ocr_Top.Margin = new System.Windows.Forms.Padding(5);
            this.ocr_Top.Name = "ocr_Top";
            this.ocr_Top.Size = new System.Drawing.Size(405, 111);
            this.ocr_Top.TabIndex = 99;
            this.ocr_Top.Visible = false;
            // 
            // ocr_Bottom
            // 
            this.ocr_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ocr_Bottom.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ocr_Bottom.Location = new System.Drawing.Point(3, 423);
            this.ocr_Bottom.Margin = new System.Windows.Forms.Padding(5);
            this.ocr_Bottom.Name = "ocr_Bottom";
            this.ocr_Bottom.Size = new System.Drawing.Size(405, 108);
            this.ocr_Bottom.TabIndex = 98;
            this.ocr_Bottom.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BusyLabel);
            this.panel1.Controls.Add(this.label_Error);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(405, 43);
            this.panel1.TabIndex = 101;
            // 
            // BusyLabel
            // 
            this.BusyLabel.BackColor = System.Drawing.Color.Yellow;
            this.BusyLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BusyLabel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BusyLabel.Location = new System.Drawing.Point(10, 10);
            this.BusyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BusyLabel.Name = "BusyLabel";
            this.BusyLabel.Size = new System.Drawing.Size(100, 21);
            this.BusyLabel.TabIndex = 40;
            this.BusyLabel.Text = "BUSY";
            this.BusyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BusyLabel.Visible = false;
            // 
            // label_Error
            // 
            this.label_Error.BackColor = System.Drawing.Color.Red;
            this.label_Error.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Error.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Error.Location = new System.Drawing.Point(269, 10);
            this.label_Error.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Error.Name = "label_Error";
            this.label_Error.Size = new System.Drawing.Size(100, 21);
            this.label_Error.TabIndex = 80;
            this.label_Error.Text = "ERROR";
            this.label_Error.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Error.Visible = false;
            // 
            // backgroundWorker_Aligner_Send
            // 
            this.backgroundWorker_Aligner_Send.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Aligner_Send_DoWork);
            // 
            // backgroundWorker_Aligner_Receive
            // 
            this.backgroundWorker_Aligner_Receive.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Aligner_Receive_DoWork);
            // 
            // Aligner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 595);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_Setting);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Aligner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aligner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Aligner_FormClosing);
            this.groupBox_Setting.ResumeLayout(false);
            this.groupBox_Setting.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox_ControlParameter.ResumeLayout(false);
            this.groupBox_ControlParameter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Degree)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Setting;
        private System.Windows.Forms.ComboBox comboBox_MainPort;
        internal System.Windows.Forms.Label Label_MainPort;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox_ControlParameter;
        private System.Windows.Forms.ListBox listBox_AlignerCommand;
        private System.Windows.Forms.Button button_Aligner_Command;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.Label label_Status;
        internal System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label RemoteLabel;
        private System.Windows.Forms.Label label_Error;
        internal System.Windows.Forms.Label Label6;
        private System.Windows.Forms.Label BusyLabel;
        internal System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_Type;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Aligner_Send;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Aligner_Receive;
        private System.Windows.Forms.NumericUpDown numericUpDown_Degree;
        internal System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_WaferPresent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_Vac;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_NotchDetectionStatus;
        private System.Windows.Forms.Label label_15;
        internal System.Windows.Forms.Label label_DeviceStatus;
        internal System.Windows.Forms.Label label13;
        internal System.Windows.Forms.Label label_OperatingState;
        internal System.Windows.Forms.Label label_14;
        public OCR ocr_Bottom;
        public OCR ocr_Top;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.Label label_AdamIP;
        private System.Windows.Forms.Label label_ClampStatus;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_FrameWaferPresent;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
    }
}