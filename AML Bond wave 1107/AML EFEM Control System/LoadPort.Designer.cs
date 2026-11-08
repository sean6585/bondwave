namespace AML
{
    partial class LoadPort
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
            this.ClampLabelTitle = new System.Windows.Forms.Label();
            this.label_Lock = new System.Windows.Forms.Label();
            this.label_Busy = new System.Windows.Forms.Label();
            this.button_SaveTest = new System.Windows.Forms.Button();
            this.comboBox_E84Port = new System.Windows.Forms.ComboBox();
            this.comboBox_RFIDPort = new System.Windows.Forms.ComboBox();
            this.comboBox_MainPort = new System.Windows.Forms.ComboBox();
            this.comboBox_Type = new System.Windows.Forms.ComboBox();
            this.comboBox_Brand = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Label_E84Port = new System.Windows.Forms.Label();
            this.label_RFIDPort = new System.Windows.Forms.Label();
            this.Label_MainPort = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.DGV_Wafer = new System.Windows.Forms.DataGridView();
            this.Column_Slot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_State = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_ReloadSetting = new System.Windows.Forms.Button();
            this.groupBox_Setting = new System.Windows.Forms.GroupBox();
            this.label_LP = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_Protrusion = new System.Windows.Forms.Label();
            this.label_2 = new System.Windows.Forms.Label();
            this.label_Mode = new System.Windows.Forms.Label();
            this.label_Clamp = new System.Windows.Forms.Label();
            this.label_Door = new System.Windows.Forms.Label();
            this.label_CarrierID = new System.Windows.Forms.Label();
            this.label_Error = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listBox_LP_Command = new System.Windows.Forms.ListBox();
            this.button_LP_Command = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label_E84_TP = new System.Windows.Forms.Label();
            this.numericUpDown_TPsec = new System.Windows.Forms.NumericUpDown();
            this.comboBox_E84_TP = new System.Windows.Forms.ComboBox();
            this.listBox_E84_Command = new System.Windows.Forms.ListBox();
            this.button_E84_Command = new System.Windows.Forms.Button();
            this.backgroundWorker_E84 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker_LP_Send = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker_LP_Receive = new System.ComponentModel.BackgroundWorker();
            this.NoWaferTitleLabel = new System.Windows.Forms.Label();
            this.UnknownLabel = new System.Windows.Forms.Label();
            this.WaferMappedLabel = new System.Windows.Forms.Label();
            this.labelWaferMappedTitle = new System.Windows.Forms.Label();
            this.WaferErrorLabel = new System.Windows.Forms.Label();
            this.UnknownTitleLabel = new System.Windows.Forms.Label();
            this.WaferErrorTitleLabel = new System.Windows.Forms.Label();
            this.NowaferLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Wafer)).BeginInit();
            this.groupBox_Setting.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TPsec)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ClampLabelTitle
            // 
            this.ClampLabelTitle.AutoSize = true;
            this.ClampLabelTitle.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ClampLabelTitle.Location = new System.Drawing.Point(19, 164);
            this.ClampLabelTitle.Margin = new System.Windows.Forms.Padding(10);
            this.ClampLabelTitle.Name = "ClampLabelTitle";
            this.ClampLabelTitle.Size = new System.Drawing.Size(59, 21);
            this.ClampLabelTitle.TabIndex = 3;
            this.ClampLabelTitle.Text = "Clamp";
            this.ClampLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_Lock
            // 
            this.label_Lock.BackColor = System.Drawing.Color.LightPink;
            this.label_Lock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Lock.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Lock.Location = new System.Drawing.Point(271, 33);
            this.label_Lock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Lock.Name = "label_Lock";
            this.label_Lock.Size = new System.Drawing.Size(100, 21);
            this.label_Lock.TabIndex = 39;
            this.label_Lock.Text = "LOCK";
            this.label_Lock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Lock.Visible = false;
            // 
            // label_Busy
            // 
            this.label_Busy.BackColor = System.Drawing.Color.Yellow;
            this.label_Busy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Busy.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Busy.Location = new System.Drawing.Point(23, 33);
            this.label_Busy.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Busy.Name = "label_Busy";
            this.label_Busy.Size = new System.Drawing.Size(100, 21);
            this.label_Busy.TabIndex = 40;
            this.label_Busy.Text = "BUSY";
            this.label_Busy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Busy.Visible = false;
            // 
            // button_SaveTest
            // 
            this.button_SaveTest.Location = new System.Drawing.Point(23, 280);
            this.button_SaveTest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_SaveTest.Name = "button_SaveTest";
            this.button_SaveTest.Size = new System.Drawing.Size(181, 36);
            this.button_SaveTest.TabIndex = 97;
            this.button_SaveTest.Text = "Save And Test";
            this.button_SaveTest.UseVisualStyleBackColor = true;
            this.button_SaveTest.Visible = false;
            this.button_SaveTest.Click += new System.EventHandler(this.button_SaveTest_Click);
            // 
            // comboBox_E84Port
            // 
            this.comboBox_E84Port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_E84Port.Enabled = false;
            this.comboBox_E84Port.FormattingEnabled = true;
            this.comboBox_E84Port.Location = new System.Drawing.Point(631, 28);
            this.comboBox_E84Port.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_E84Port.Name = "comboBox_E84Port";
            this.comboBox_E84Port.Size = new System.Drawing.Size(95, 29);
            this.comboBox_E84Port.TabIndex = 96;
            this.comboBox_E84Port.DropDown += new System.EventHandler(this.comboBox_E84Port_DropDown);
            // 
            // comboBox_RFIDPort
            // 
            this.comboBox_RFIDPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_RFIDPort.Enabled = false;
            this.comboBox_RFIDPort.FormattingEnabled = true;
            this.comboBox_RFIDPort.Location = new System.Drawing.Point(412, 26);
            this.comboBox_RFIDPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_RFIDPort.Name = "comboBox_RFIDPort";
            this.comboBox_RFIDPort.Size = new System.Drawing.Size(95, 29);
            this.comboBox_RFIDPort.TabIndex = 95;
            this.comboBox_RFIDPort.DropDown += new System.EventHandler(this.comboBox_RFIDPort_DropDown);
            // 
            // comboBox_MainPort
            // 
            this.comboBox_MainPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MainPort.Enabled = false;
            this.comboBox_MainPort.FormattingEnabled = true;
            this.comboBox_MainPort.Location = new System.Drawing.Point(412, 68);
            this.comboBox_MainPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_MainPort.Name = "comboBox_MainPort";
            this.comboBox_MainPort.Size = new System.Drawing.Size(95, 29);
            this.comboBox_MainPort.TabIndex = 94;
            this.comboBox_MainPort.DropDown += new System.EventHandler(this.comboBox_MainPort_DropDown);
            // 
            // comboBox_Type
            // 
            this.comboBox_Type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Type.Enabled = false;
            this.comboBox_Type.FormattingEnabled = true;
            this.comboBox_Type.Location = new System.Drawing.Point(197, 72);
            this.comboBox_Type.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Type.Name = "comboBox_Type";
            this.comboBox_Type.Size = new System.Drawing.Size(95, 29);
            this.comboBox_Type.TabIndex = 93;
            this.comboBox_Type.DropDown += new System.EventHandler(this.comboBox_Type_DropDown);
            // 
            // comboBox_Brand
            // 
            this.comboBox_Brand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Brand.Enabled = false;
            this.comboBox_Brand.FormattingEnabled = true;
            this.comboBox_Brand.Location = new System.Drawing.Point(197, 31);
            this.comboBox_Brand.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_Brand.Name = "comboBox_Brand";
            this.comboBox_Brand.Size = new System.Drawing.Size(95, 29);
            this.comboBox_Brand.TabIndex = 92;
            this.comboBox_Brand.DropDown += new System.EventHandler(this.comboBox_Brand_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(107, 75);
            this.label4.Margin = new System.Windows.Forms.Padding(10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 21);
            this.label4.TabIndex = 91;
            this.label4.Text = "Type";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(107, 34);
            this.label3.Margin = new System.Windows.Forms.Padding(10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 21);
            this.label3.TabIndex = 89;
            this.label3.Text = "Brand";
            // 
            // Label_E84Port
            // 
            this.Label_E84Port.AutoSize = true;
            this.Label_E84Port.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_E84Port.Location = new System.Drawing.Point(542, 33);
            this.Label_E84Port.Margin = new System.Windows.Forms.Padding(10);
            this.Label_E84Port.Name = "Label_E84Port";
            this.Label_E84Port.Size = new System.Drawing.Size(75, 21);
            this.Label_E84Port.TabIndex = 87;
            this.Label_E84Port.Text = "E84 Port";
            // 
            // label_RFIDPort
            // 
            this.label_RFIDPort.AutoSize = true;
            this.label_RFIDPort.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_RFIDPort.Location = new System.Drawing.Point(322, 31);
            this.label_RFIDPort.Margin = new System.Windows.Forms.Padding(10);
            this.label_RFIDPort.Name = "label_RFIDPort";
            this.label_RFIDPort.Size = new System.Drawing.Size(83, 21);
            this.label_RFIDPort.TabIndex = 83;
            this.label_RFIDPort.Text = "RFID Port";
            // 
            // Label_MainPort
            // 
            this.Label_MainPort.AutoSize = true;
            this.Label_MainPort.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_MainPort.Location = new System.Drawing.Point(322, 73);
            this.Label_MainPort.Margin = new System.Windows.Forms.Padding(10);
            this.Label_MainPort.Name = "Label_MainPort";
            this.Label_MainPort.Size = new System.Drawing.Size(85, 21);
            this.Label_MainPort.TabIndex = 80;
            this.Label_MainPort.Text = "Main Port";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(19, 205);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 21);
            this.label1.TabIndex = 79;
            this.label1.Text = "Door";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label7.Location = new System.Drawing.Point(19, 123);
            this.Label7.Margin = new System.Windows.Forms.Padding(10);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(78, 21);
            this.Label7.TabIndex = 78;
            this.Label7.Text = "CarrierID";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label6.Location = new System.Drawing.Point(19, 82);
            this.Label6.Margin = new System.Windows.Forms.Padding(10);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(55, 21);
            this.Label6.TabIndex = 74;
            this.Label6.Text = "Mode";
            // 
            // DGV_Wafer
            // 
            this.DGV_Wafer.AllowUserToAddRows = false;
            this.DGV_Wafer.AllowUserToDeleteRows = false;
            this.DGV_Wafer.AllowUserToResizeColumns = false;
            this.DGV_Wafer.AllowUserToResizeRows = false;
            this.DGV_Wafer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_Wafer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_Wafer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_Slot,
            this.Column_State});
            this.DGV_Wafer.Dock = System.Windows.Forms.DockStyle.Top;
            this.DGV_Wafer.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV_Wafer.Location = new System.Drawing.Point(0, 0);
            this.DGV_Wafer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DGV_Wafer.Name = "DGV_Wafer";
            this.DGV_Wafer.RowTemplate.Height = 24;
            this.DGV_Wafer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_Wafer.Size = new System.Drawing.Size(311, 630);
            this.DGV_Wafer.TabIndex = 4;
            // 
            // Column_Slot
            // 
            this.Column_Slot.HeaderText = "Slot";
            this.Column_Slot.Name = "Column_Slot";
            this.Column_Slot.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_State
            // 
            this.Column_State.HeaderText = "State";
            this.Column_State.Name = "Column_State";
            this.Column_State.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // button_ReloadSetting
            // 
            this.button_ReloadSetting.Location = new System.Drawing.Point(23, 326);
            this.button_ReloadSetting.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_ReloadSetting.Name = "button_ReloadSetting";
            this.button_ReloadSetting.Size = new System.Drawing.Size(180, 36);
            this.button_ReloadSetting.TabIndex = 98;
            this.button_ReloadSetting.Text = "Reload Setting";
            this.button_ReloadSetting.UseVisualStyleBackColor = true;
            this.button_ReloadSetting.Visible = false;
            this.button_ReloadSetting.Click += new System.EventHandler(this.button_ReloadSetting_Click);
            // 
            // groupBox_Setting
            // 
            this.groupBox_Setting.Controls.Add(this.label_LP);
            this.groupBox_Setting.Controls.Add(this.label3);
            this.groupBox_Setting.Controls.Add(this.label4);
            this.groupBox_Setting.Controls.Add(this.comboBox_Brand);
            this.groupBox_Setting.Controls.Add(this.comboBox_E84Port);
            this.groupBox_Setting.Controls.Add(this.comboBox_Type);
            this.groupBox_Setting.Controls.Add(this.comboBox_RFIDPort);
            this.groupBox_Setting.Controls.Add(this.comboBox_MainPort);
            this.groupBox_Setting.Controls.Add(this.Label_MainPort);
            this.groupBox_Setting.Controls.Add(this.Label_E84Port);
            this.groupBox_Setting.Controls.Add(this.label_RFIDPort);
            this.groupBox_Setting.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox_Setting.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox_Setting.Location = new System.Drawing.Point(0, 0);
            this.groupBox_Setting.Name = "groupBox_Setting";
            this.groupBox_Setting.Size = new System.Drawing.Size(793, 109);
            this.groupBox_Setting.TabIndex = 99;
            this.groupBox_Setting.TabStop = false;
            this.groupBox_Setting.Text = "Setting";
            // 
            // label_LP
            // 
            this.label_LP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_LP.Dock = System.Windows.Forms.DockStyle.Left;
            this.label_LP.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
            this.label_LP.ForeColor = System.Drawing.Color.Blue;
            this.label_LP.Location = new System.Drawing.Point(3, 25);
            this.label_LP.Name = "label_LP";
            this.label_LP.Size = new System.Drawing.Size(90, 81);
            this.label_LP.TabIndex = 97;
            this.label_LP.Text = "P?";
            this.label_LP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_Protrusion);
            this.groupBox1.Controls.Add(this.label_2);
            this.groupBox1.Controls.Add(this.label_Mode);
            this.groupBox1.Controls.Add(this.button_ReloadSetting);
            this.groupBox1.Controls.Add(this.label_Clamp);
            this.groupBox1.Controls.Add(this.label_Door);
            this.groupBox1.Controls.Add(this.button_SaveTest);
            this.groupBox1.Controls.Add(this.label_CarrierID);
            this.groupBox1.Controls.Add(this.label_Error);
            this.groupBox1.Controls.Add(this.Label6);
            this.groupBox1.Controls.Add(this.ClampLabelTitle);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label_Busy);
            this.groupBox1.Controls.Add(this.label_Lock);
            this.groupBox1.Controls.Add(this.Label7);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(0, 109);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 565);
            this.groupBox1.TabIndex = 101;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // label_Protrusion
            // 
            this.label_Protrusion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Protrusion.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Protrusion.Location = new System.Drawing.Point(148, 246);
            this.label_Protrusion.Margin = new System.Windows.Forms.Padding(10);
            this.label_Protrusion.Name = "label_Protrusion";
            this.label_Protrusion.Size = new System.Drawing.Size(100, 21);
            this.label_Protrusion.TabIndex = 100;
            this.label_Protrusion.Text = "Unknown";
            this.label_Protrusion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_2
            // 
            this.label_2.AutoSize = true;
            this.label_2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_2.Location = new System.Drawing.Point(19, 244);
            this.label_2.Margin = new System.Windows.Forms.Padding(10);
            this.label_2.Name = "label_2";
            this.label_2.Size = new System.Drawing.Size(90, 21);
            this.label_2.TabIndex = 99;
            this.label_2.Text = "Protrusion";
            this.label_2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_Mode
            // 
            this.label_Mode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Mode.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Mode.Location = new System.Drawing.Point(148, 82);
            this.label_Mode.Margin = new System.Windows.Forms.Padding(10);
            this.label_Mode.Name = "label_Mode";
            this.label_Mode.Size = new System.Drawing.Size(100, 21);
            this.label_Mode.TabIndex = 82;
            this.label_Mode.Text = "Unknown";
            this.label_Mode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Clamp
            // 
            this.label_Clamp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Clamp.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Clamp.Location = new System.Drawing.Point(148, 164);
            this.label_Clamp.Margin = new System.Windows.Forms.Padding(10);
            this.label_Clamp.Name = "label_Clamp";
            this.label_Clamp.Size = new System.Drawing.Size(100, 21);
            this.label_Clamp.TabIndex = 81;
            this.label_Clamp.Text = "Unknown";
            this.label_Clamp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Door
            // 
            this.label_Door.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Door.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Door.Location = new System.Drawing.Point(148, 205);
            this.label_Door.Margin = new System.Windows.Forms.Padding(10);
            this.label_Door.Name = "label_Door";
            this.label_Door.Size = new System.Drawing.Size(100, 21);
            this.label_Door.TabIndex = 84;
            this.label_Door.Text = "Unknown";
            this.label_Door.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CarrierID
            // 
            this.label_CarrierID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_CarrierID.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_CarrierID.Location = new System.Drawing.Point(148, 123);
            this.label_CarrierID.Margin = new System.Windows.Forms.Padding(10);
            this.label_CarrierID.Name = "label_CarrierID";
            this.label_CarrierID.Size = new System.Drawing.Size(100, 21);
            this.label_CarrierID.TabIndex = 83;
            this.label_CarrierID.Text = "Unknown";
            this.label_CarrierID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Error
            // 
            this.label_Error.BackColor = System.Drawing.Color.Red;
            this.label_Error.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Error.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Error.Location = new System.Drawing.Point(148, 33);
            this.label_Error.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Error.Name = "label_Error";
            this.label_Error.Size = new System.Drawing.Size(100, 21);
            this.label_Error.TabIndex = 80;
            this.label_Error.Text = "ERROR";
            this.label_Error.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Error.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listBox_LP_Command);
            this.groupBox2.Controls.Add(this.button_LP_Command);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox2.Location = new System.Drawing.Point(390, 109);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 565);
            this.groupBox2.TabIndex = 102;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Load Port Control";
            // 
            // listBox_LP_Command
            // 
            this.listBox_LP_Command.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_LP_Command.FormattingEnabled = true;
            this.listBox_LP_Command.ItemHeight = 21;
            this.listBox_LP_Command.Location = new System.Drawing.Point(3, 25);
            this.listBox_LP_Command.Name = "listBox_LP_Command";
            this.listBox_LP_Command.Size = new System.Drawing.Size(194, 498);
            this.listBox_LP_Command.TabIndex = 0;
            // 
            // button_LP_Command
            // 
            this.button_LP_Command.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button_LP_Command.Location = new System.Drawing.Point(3, 523);
            this.button_LP_Command.Name = "button_LP_Command";
            this.button_LP_Command.Size = new System.Drawing.Size(194, 39);
            this.button_LP_Command.TabIndex = 1;
            this.button_LP_Command.Text = "LP Command";
            this.button_LP_Command.UseVisualStyleBackColor = true;
            this.button_LP_Command.Click += new System.EventHandler(this.button_LP_Command_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label_E84_TP);
            this.groupBox3.Controls.Add(this.numericUpDown_TPsec);
            this.groupBox3.Controls.Add(this.comboBox_E84_TP);
            this.groupBox3.Controls.Add(this.listBox_E84_Command);
            this.groupBox3.Controls.Add(this.button_E84_Command);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox3.Location = new System.Drawing.Point(590, 109);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 565);
            this.groupBox3.TabIndex = 103;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "E84 Control";
            // 
            // label_E84_TP
            // 
            this.label_E84_TP.AutoSize = true;
            this.label_E84_TP.Location = new System.Drawing.Point(6, 298);
            this.label_E84_TP.Name = "label_E84_TP";
            this.label_E84_TP.Size = new System.Drawing.Size(170, 42);
            this.label_E84_TP.TabIndex = 6;
            this.label_E84_TP.Text = "Timeout(1~255 sec):\r\n不開放Socket指令\r\n";
            this.label_E84_TP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown_TPsec
            // 
            this.numericUpDown_TPsec.Location = new System.Drawing.Point(109, 350);
            this.numericUpDown_TPsec.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_TPsec.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_TPsec.Name = "numericUpDown_TPsec";
            this.numericUpDown_TPsec.Size = new System.Drawing.Size(67, 29);
            this.numericUpDown_TPsec.TabIndex = 5;
            this.numericUpDown_TPsec.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_TPsec.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // comboBox_E84_TP
            // 
            this.comboBox_E84_TP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_E84_TP.FormattingEnabled = true;
            this.comboBox_E84_TP.Location = new System.Drawing.Point(10, 349);
            this.comboBox_E84_TP.Name = "comboBox_E84_TP";
            this.comboBox_E84_TP.Size = new System.Drawing.Size(93, 29);
            this.comboBox_E84_TP.TabIndex = 4;
            // 
            // listBox_E84_Command
            // 
            this.listBox_E84_Command.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox_E84_Command.FormattingEnabled = true;
            this.listBox_E84_Command.ItemHeight = 21;
            this.listBox_E84_Command.Location = new System.Drawing.Point(3, 25);
            this.listBox_E84_Command.Name = "listBox_E84_Command";
            this.listBox_E84_Command.Size = new System.Drawing.Size(194, 256);
            this.listBox_E84_Command.TabIndex = 3;
            this.listBox_E84_Command.SelectedIndexChanged += new System.EventHandler(this.listBox_E84_Command_SelectedIndexChanged);
            // 
            // button_E84_Command
            // 
            this.button_E84_Command.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button_E84_Command.Location = new System.Drawing.Point(3, 523);
            this.button_E84_Command.Name = "button_E84_Command";
            this.button_E84_Command.Size = new System.Drawing.Size(194, 39);
            this.button_E84_Command.TabIndex = 2;
            this.button_E84_Command.Text = "E84 Command";
            this.button_E84_Command.UseVisualStyleBackColor = true;
            this.button_E84_Command.Click += new System.EventHandler(this.button_E84_Command_Click);
            // 
            // backgroundWorker_E84
            // 
            this.backgroundWorker_E84.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_E84_DoWork);
            this.backgroundWorker_E84.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_E84_RunWorkerCompleted);
            // 
            // backgroundWorker_LP_Send
            // 
            this.backgroundWorker_LP_Send.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_LP_Send_DoWork);
            // 
            // backgroundWorker_LP_Receive
            // 
            this.backgroundWorker_LP_Receive.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_LP_Receive_DoWork);
            // 
            // NoWaferTitleLabel
            // 
            this.NoWaferTitleLabel.AutoSize = true;
            this.NoWaferTitleLabel.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.NoWaferTitleLabel.Location = new System.Drawing.Point(5, 635);
            this.NoWaferTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NoWaferTitleLabel.Name = "NoWaferTitleLabel";
            this.NoWaferTitleLabel.Size = new System.Drawing.Size(62, 17);
            this.NoWaferTitleLabel.TabIndex = 99;
            this.NoWaferTitleLabel.Text = "NoWafer";
            // 
            // UnknownLabel
            // 
            this.UnknownLabel.BackColor = System.Drawing.Color.Gainsboro;
            this.UnknownLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.UnknownLabel.Location = new System.Drawing.Point(243, 654);
            this.UnknownLabel.Name = "UnknownLabel";
            this.UnknownLabel.Size = new System.Drawing.Size(60, 12);
            this.UnknownLabel.TabIndex = 101;
            // 
            // WaferMappedLabel
            // 
            this.WaferMappedLabel.BackColor = System.Drawing.Color.RoyalBlue;
            this.WaferMappedLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WaferMappedLabel.Location = new System.Drawing.Point(89, 654);
            this.WaferMappedLabel.Name = "WaferMappedLabel";
            this.WaferMappedLabel.Size = new System.Drawing.Size(60, 12);
            this.WaferMappedLabel.TabIndex = 109;
            // 
            // labelWaferMappedTitle
            // 
            this.labelWaferMappedTitle.AutoSize = true;
            this.labelWaferMappedTitle.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelWaferMappedTitle.Location = new System.Drawing.Point(71, 635);
            this.labelWaferMappedTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelWaferMappedTitle.Name = "labelWaferMappedTitle";
            this.labelWaferMappedTitle.Size = new System.Drawing.Size(95, 17);
            this.labelWaferMappedTitle.TabIndex = 102;
            this.labelWaferMappedTitle.Text = "WaferMapped";
            // 
            // WaferErrorLabel
            // 
            this.WaferErrorLabel.BackColor = System.Drawing.Color.LightCoral;
            this.WaferErrorLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WaferErrorLabel.Location = new System.Drawing.Point(169, 654);
            this.WaferErrorLabel.Name = "WaferErrorLabel";
            this.WaferErrorLabel.Size = new System.Drawing.Size(60, 12);
            this.WaferErrorLabel.TabIndex = 110;
            // 
            // UnknownTitleLabel
            // 
            this.UnknownTitleLabel.AutoSize = true;
            this.UnknownTitleLabel.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.UnknownTitleLabel.Location = new System.Drawing.Point(239, 635);
            this.UnknownTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UnknownTitleLabel.Name = "UnknownTitleLabel";
            this.UnknownTitleLabel.Size = new System.Drawing.Size(67, 17);
            this.UnknownTitleLabel.TabIndex = 105;
            this.UnknownTitleLabel.Text = "Unknown";
            // 
            // WaferErrorTitleLabel
            // 
            this.WaferErrorTitleLabel.AutoSize = true;
            this.WaferErrorTitleLabel.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.WaferErrorTitleLabel.Location = new System.Drawing.Point(166, 635);
            this.WaferErrorTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WaferErrorTitleLabel.Name = "WaferErrorTitleLabel";
            this.WaferErrorTitleLabel.Size = new System.Drawing.Size(74, 17);
            this.WaferErrorTitleLabel.TabIndex = 106;
            this.WaferErrorTitleLabel.Text = "WaferError";
            // 
            // NowaferLabel
            // 
            this.NowaferLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.NowaferLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NowaferLabel.Location = new System.Drawing.Point(10, 654);
            this.NowaferLabel.Name = "NowaferLabel";
            this.NowaferLabel.Size = new System.Drawing.Size(60, 12);
            this.NowaferLabel.TabIndex = 108;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.UnknownLabel);
            this.panel1.Controls.Add(this.WaferMappedLabel);
            this.panel1.Controls.Add(this.NoWaferTitleLabel);
            this.panel1.Controls.Add(this.WaferErrorLabel);
            this.panel1.Controls.Add(this.DGV_Wafer);
            this.panel1.Controls.Add(this.NowaferLabel);
            this.panel1.Controls.Add(this.UnknownTitleLabel);
            this.panel1.Controls.Add(this.WaferErrorTitleLabel);
            this.panel1.Controls.Add(this.labelWaferMappedTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(793, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(311, 674);
            this.panel1.TabIndex = 111;
            // 
            // LoadPort
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 674);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_Setting);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoadPort";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LoadPort";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoadPort_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.LoadPort_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Wafer)).EndInit();
            this.groupBox_Setting.ResumeLayout(false);
            this.groupBox_Setting.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TPsec)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label ClampLabelTitle;
        private System.Windows.Forms.Label label_Lock;
        private System.Windows.Forms.Label label_Busy;
        internal System.Windows.Forms.Label Label_E84Port;
        internal System.Windows.Forms.Label label_RFIDPort;
        internal System.Windows.Forms.Label Label_MainPort;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label Label6;
        private System.Windows.Forms.DataGridView DGV_Wafer;
        internal System.Windows.Forms.Label label4;
        internal System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_SaveTest;
        private System.Windows.Forms.ComboBox comboBox_E84Port;
        private System.Windows.Forms.ComboBox comboBox_RFIDPort;
        private System.Windows.Forms.ComboBox comboBox_MainPort;
        private System.Windows.Forms.ComboBox comboBox_Type;
        private System.Windows.Forms.ComboBox comboBox_Brand;
        private System.Windows.Forms.Button button_ReloadSetting;
        private System.Windows.Forms.GroupBox groupBox_Setting;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Slot;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_State;
        private System.Windows.Forms.Label label_Error;
        private System.Windows.Forms.Button button_LP_Command;
        private System.Windows.Forms.ListBox listBox_LP_Command;
        private System.Windows.Forms.Button button_E84_Command;
        private System.Windows.Forms.ListBox listBox_E84_Command;
        private System.ComponentModel.BackgroundWorker backgroundWorker_E84;
        private System.ComponentModel.BackgroundWorker backgroundWorker_LP_Send;
        private System.ComponentModel.BackgroundWorker backgroundWorker_LP_Receive;
        internal System.Windows.Forms.Label label_Mode;
        private System.Windows.Forms.Label label_Clamp;
        private System.Windows.Forms.Label label_Door;
        internal System.Windows.Forms.Label label_CarrierID;
        private System.Windows.Forms.ComboBox comboBox_E84_TP;
        private System.Windows.Forms.Label label_E84_TP;
        private System.Windows.Forms.NumericUpDown numericUpDown_TPsec;
        private System.Windows.Forms.Label label_LP;
        private System.Windows.Forms.Label NoWaferTitleLabel;
        private System.Windows.Forms.Label UnknownLabel;
        private System.Windows.Forms.Label WaferMappedLabel;
        private System.Windows.Forms.Label labelWaferMappedTitle;
        private System.Windows.Forms.Label WaferErrorLabel;
        private System.Windows.Forms.Label UnknownTitleLabel;
        private System.Windows.Forms.Label WaferErrorTitleLabel;
        private System.Windows.Forms.Label NowaferLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label_Protrusion;
        private System.Windows.Forms.Label label_2;
    }
}