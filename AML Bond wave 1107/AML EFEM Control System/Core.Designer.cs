namespace AML
{
    partial class Core
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_LP = new System.Windows.Forms.Panel();
            this.button_Exit = new System.Windows.Forms.Button();
            this.panel_Socket = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label_Port = new System.Windows.Forms.Label();
            this.label_IP = new System.Windows.Forms.Label();
            this.ConnectiionLabel = new System.Windows.Forms.Label();
            this.button_Remote = new System.Windows.Forms.Button();
            this.button_Local = new System.Windows.Forms.Button();
            this.button_Robot1 = new System.Windows.Forms.Button();
            this.button_Aligner1 = new System.Windows.Forms.Button();
            this.panel_Stage = new System.Windows.Forms.Panel();
            this.panel_StageSub = new System.Windows.Forms.Panel();
            this.groupBox_Stage = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.panel_IO = new System.Windows.Forms.Panel();
            this.panel_Robot_Aligner_IO = new System.Windows.Forms.Panel();
            this.button_Aligner2 = new System.Windows.Forms.Button();
            this.button_Robot2 = new System.Windows.Forms.Button();
            this.panel_LPStatus = new System.Windows.Forms.Panel();
            this.panel_RobotAligner_Status = new System.Windows.Forms.Panel();
            this.panel_LP.SuspendLayout();
            this.panel_Socket.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel_Stage.SuspendLayout();
            this.groupBox_Stage.SuspendLayout();
            this.panel_Robot_Aligner_IO.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_LP
            // 
            this.panel_LP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_LP.Controls.Add(this.button_Exit);
            this.panel_LP.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_LP.Location = new System.Drawing.Point(0, 340);
            this.panel_LP.Name = "panel_LP";
            this.panel_LP.Size = new System.Drawing.Size(877, 129);
            this.panel_LP.TabIndex = 0;
            // 
            // button_Exit
            // 
            this.button_Exit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_Exit.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Exit.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
            this.button_Exit.Location = new System.Drawing.Point(757, 0);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(118, 127);
            this.button_Exit.TabIndex = 12;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = false;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // panel_Socket
            // 
            this.panel_Socket.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Socket.Controls.Add(this.panel2);
            this.panel_Socket.Controls.Add(this.button_Remote);
            this.panel_Socket.Controls.Add(this.button_Local);
            this.panel_Socket.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Socket.Location = new System.Drawing.Point(0, 0);
            this.panel_Socket.Name = "panel_Socket";
            this.panel_Socket.Size = new System.Drawing.Size(877, 80);
            this.panel_Socket.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label_Port);
            this.panel2.Controls.Add(this.label_IP);
            this.panel2.Controls.Add(this.ConnectiionLabel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(239, 78);
            this.panel2.TabIndex = 10;
            // 
            // label_Port
            // 
            this.label_Port.Location = new System.Drawing.Point(129, 45);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(109, 21);
            this.label_Port.TabIndex = 9;
            this.label_Port.Text = "Port:";
            this.label_Port.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_IP
            // 
            this.label_IP.Location = new System.Drawing.Point(1, 45);
            this.label_IP.Name = "label_IP";
            this.label_IP.Size = new System.Drawing.Size(148, 21);
            this.label_IP.TabIndex = 8;
            this.label_IP.Text = "IP:";
            this.label_IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ConnectiionLabel
            // 
            this.ConnectiionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConnectiionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ConnectiionLabel.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Bold);
            this.ConnectiionLabel.Location = new System.Drawing.Point(0, 0);
            this.ConnectiionLabel.Name = "ConnectiionLabel";
            this.ConnectiionLabel.Size = new System.Drawing.Size(237, 38);
            this.ConnectiionLabel.TabIndex = 7;
            this.ConnectiionLabel.Text = "Listen";
            this.ConnectiionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_Remote
            // 
            this.button_Remote.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Remote.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
            this.button_Remote.Location = new System.Drawing.Point(639, 0);
            this.button_Remote.Name = "button_Remote";
            this.button_Remote.Size = new System.Drawing.Size(118, 78);
            this.button_Remote.TabIndex = 11;
            this.button_Remote.Text = "Remote";
            this.button_Remote.UseVisualStyleBackColor = true;
            this.button_Remote.Click += new System.EventHandler(this.button_Remote_Click);
            // 
            // button_Local
            // 
            this.button_Local.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Local.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold);
            this.button_Local.Location = new System.Drawing.Point(757, 0);
            this.button_Local.Name = "button_Local";
            this.button_Local.Size = new System.Drawing.Size(118, 78);
            this.button_Local.TabIndex = 10;
            this.button_Local.Text = "Local";
            this.button_Local.UseVisualStyleBackColor = true;
            this.button_Local.Click += new System.EventHandler(this.button_Local_Click);
            // 
            // button_Robot1
            // 
            this.button_Robot1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Robot1.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Robot1.Location = new System.Drawing.Point(0, 0);
            this.button_Robot1.Name = "button_Robot1";
            this.button_Robot1.Size = new System.Drawing.Size(110, 94);
            this.button_Robot1.TabIndex = 2;
            this.button_Robot1.Text = "Robot1";
            this.button_Robot1.UseVisualStyleBackColor = true;
            this.button_Robot1.Click += new System.EventHandler(this.button_Robot_Click);
            // 
            // button_Aligner1
            // 
            this.button_Aligner1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Aligner1.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Aligner1.Location = new System.Drawing.Point(110, 0);
            this.button_Aligner1.Name = "button_Aligner1";
            this.button_Aligner1.Size = new System.Drawing.Size(110, 94);
            this.button_Aligner1.TabIndex = 4;
            this.button_Aligner1.Text = "Aligner1";
            this.button_Aligner1.UseVisualStyleBackColor = true;
            this.button_Aligner1.Click += new System.EventHandler(this.button_WaferAligner_Click);
            // 
            // panel_Stage
            // 
            this.panel_Stage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Stage.Controls.Add(this.panel_StageSub);
            this.panel_Stage.Controls.Add(this.groupBox_Stage);
            this.panel_Stage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Stage.Location = new System.Drawing.Point(0, 204);
            this.panel_Stage.Name = "panel_Stage";
            this.panel_Stage.Size = new System.Drawing.Size(877, 136);
            this.panel_Stage.TabIndex = 5;
            // 
            // panel_StageSub
            // 
            this.panel_StageSub.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_StageSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_StageSub.Location = new System.Drawing.Point(119, 0);
            this.panel_StageSub.Name = "panel_StageSub";
            this.panel_StageSub.Size = new System.Drawing.Size(756, 134);
            this.panel_StageSub.TabIndex = 90;
            // 
            // groupBox_Stage
            // 
            this.groupBox_Stage.Controls.Add(this.label4);
            this.groupBox_Stage.Controls.Add(this.label3);
            this.groupBox_Stage.Controls.Add(this.label2);
            this.groupBox_Stage.Controls.Add(this.Label6);
            this.groupBox_Stage.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox_Stage.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox_Stage.Location = new System.Drawing.Point(0, 0);
            this.groupBox_Stage.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_Stage.Name = "groupBox_Stage";
            this.groupBox_Stage.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_Stage.Size = new System.Drawing.Size(119, 134);
            this.groupBox_Stage.TabIndex = 89;
            this.groupBox_Stage.TabStop = false;
            this.groupBox_Stage.Text = "Stage";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(5, 80);
            this.label4.Margin = new System.Windows.Forms.Padding(17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 21);
            this.label4.TabIndex = 92;
            this.label4.Text = "PutAddress";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(5, 106);
            this.label3.Margin = new System.Windows.Forms.Padding(17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 21);
            this.label3.TabIndex = 90;
            this.label3.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(5, 51);
            this.label2.Margin = new System.Windows.Forms.Padding(17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 21);
            this.label2.TabIndex = 88;
            this.label2.Text = "Slot Pitch";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label6.Location = new System.Drawing.Point(5, 25);
            this.Label6.Margin = new System.Windows.Forms.Padding(17);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(77, 21);
            this.Label6.TabIndex = 86;
            this.Label6.Text = "Max Slot";
            // 
            // panel_IO
            // 
            this.panel_IO.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel_IO.Location = new System.Drawing.Point(465, 0);
            this.panel_IO.Name = "panel_IO";
            this.panel_IO.Size = new System.Drawing.Size(410, 94);
            this.panel_IO.TabIndex = 6;
            // 
            // panel_Robot_Aligner_IO
            // 
            this.panel_Robot_Aligner_IO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Robot_Aligner_IO.Controls.Add(this.button_Aligner2);
            this.panel_Robot_Aligner_IO.Controls.Add(this.button_Robot2);
            this.panel_Robot_Aligner_IO.Controls.Add(this.button_Aligner1);
            this.panel_Robot_Aligner_IO.Controls.Add(this.panel_IO);
            this.panel_Robot_Aligner_IO.Controls.Add(this.button_Robot1);
            this.panel_Robot_Aligner_IO.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Robot_Aligner_IO.Location = new System.Drawing.Point(0, 80);
            this.panel_Robot_Aligner_IO.Name = "panel_Robot_Aligner_IO";
            this.panel_Robot_Aligner_IO.Size = new System.Drawing.Size(877, 96);
            this.panel_Robot_Aligner_IO.TabIndex = 0;
            // 
            // button_Aligner2
            // 
            this.button_Aligner2.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Aligner2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Aligner2.Location = new System.Drawing.Point(330, 0);
            this.button_Aligner2.Name = "button_Aligner2";
            this.button_Aligner2.Size = new System.Drawing.Size(110, 94);
            this.button_Aligner2.TabIndex = 8;
            this.button_Aligner2.Text = "Aligner2";
            this.button_Aligner2.UseVisualStyleBackColor = true;
            this.button_Aligner2.Click += new System.EventHandler(this.button_Aligner2_Click);
            // 
            // button_Robot2
            // 
            this.button_Robot2.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Robot2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Robot2.Location = new System.Drawing.Point(220, 0);
            this.button_Robot2.Name = "button_Robot2";
            this.button_Robot2.Size = new System.Drawing.Size(110, 94);
            this.button_Robot2.TabIndex = 7;
            this.button_Robot2.Text = "Robot2";
            this.button_Robot2.UseVisualStyleBackColor = true;
            this.button_Robot2.Click += new System.EventHandler(this.button_Robot2_Click);
            // 
            // panel_LPStatus
            // 
            this.panel_LPStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_LPStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_LPStatus.Location = new System.Drawing.Point(0, 469);
            this.panel_LPStatus.Name = "panel_LPStatus";
            this.panel_LPStatus.Size = new System.Drawing.Size(877, 27);
            this.panel_LPStatus.TabIndex = 6;
            // 
            // panel_RobotAligner_Status
            // 
            this.panel_RobotAligner_Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_RobotAligner_Status.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_RobotAligner_Status.Location = new System.Drawing.Point(0, 176);
            this.panel_RobotAligner_Status.Name = "panel_RobotAligner_Status";
            this.panel_RobotAligner_Status.Size = new System.Drawing.Size(877, 27);
            this.panel_RobotAligner_Status.TabIndex = 7;
            // 
            // Core
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel_RobotAligner_Status);
            this.Controls.Add(this.panel_Stage);
            this.Controls.Add(this.panel_Robot_Aligner_IO);
            this.Controls.Add(this.panel_Socket);
            this.Controls.Add(this.panel_LP);
            this.Controls.Add(this.panel_LPStatus);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Core";
            this.Size = new System.Drawing.Size(877, 496);
            this.panel_LP.ResumeLayout(false);
            this.panel_Socket.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel_Stage.ResumeLayout(false);
            this.groupBox_Stage.ResumeLayout(false);
            this.groupBox_Stage.PerformLayout();
            this.panel_Robot_Aligner_IO.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_LP;
        private System.Windows.Forms.Panel panel_Socket;
        private System.Windows.Forms.Button button_Robot1;
        private System.Windows.Forms.Button button_Aligner1;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.Button button_Remote;
        private System.Windows.Forms.Label label_IP;
        private System.Windows.Forms.Button button_Local;
        private System.Windows.Forms.Label ConnectiionLabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel_Stage;
        private System.Windows.Forms.Panel panel_IO;
        private System.Windows.Forms.GroupBox groupBox_Stage;
        internal System.Windows.Forms.Label label4;
        internal System.Windows.Forms.Label label3;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Label Label6;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Panel panel_Robot_Aligner_IO;
        private System.Windows.Forms.Panel panel_LPStatus;
        private System.Windows.Forms.Panel panel_StageSub;
        private System.Windows.Forms.Panel panel_RobotAligner_Status;
        private System.Windows.Forms.Button button_Aligner2;
        private System.Windows.Forms.Button button_Robot2;
    }
}
