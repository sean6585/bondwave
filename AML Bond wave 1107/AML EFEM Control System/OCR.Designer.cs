namespace AML
{
    partial class OCR
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_ReadOCR = new System.Windows.Forms.Button();
            this.button_ReConnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label_OCR = new System.Windows.Forms.Label();
            this.label_Connected = new System.Windows.Forms.Label();
            this.label_Port = new System.Windows.Forms.Label();
            this.label_IP = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_ReadOCR);
            this.groupBox1.Controls.Add(this.button_ReConnect);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label_OCR);
            this.groupBox1.Controls.Add(this.label_Connected);
            this.groupBox1.Controls.Add(this.label_Port);
            this.groupBox1.Controls.Add(this.label_IP);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(398, 123);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // button_ReadOCR
            // 
            this.button_ReadOCR.Location = new System.Drawing.Point(227, 80);
            this.button_ReadOCR.Name = "button_ReadOCR";
            this.button_ReadOCR.Size = new System.Drawing.Size(144, 30);
            this.button_ReadOCR.TabIndex = 91;
            this.button_ReadOCR.Text = "Read OCR";
            this.button_ReadOCR.UseVisualStyleBackColor = true;
            this.button_ReadOCR.Click += new System.EventHandler(this.button_ReadOCR_Click);
            // 
            // button_ReConnect
            // 
            this.button_ReConnect.Location = new System.Drawing.Point(10, 78);
            this.button_ReConnect.Name = "button_ReConnect";
            this.button_ReConnect.Size = new System.Drawing.Size(144, 30);
            this.button_ReConnect.TabIndex = 90;
            this.button_ReConnect.Text = "Re-Connect";
            this.button_ReConnect.UseVisualStyleBackColor = true;
            this.button_ReConnect.Click += new System.EventHandler(this.button_ReConnect_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(174, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 21);
            this.label2.TabIndex = 88;
            this.label2.Text = "OCR:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_OCR
            // 
            this.label_OCR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_OCR.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_OCR.Location = new System.Drawing.Point(227, 50);
            this.label_OCR.Margin = new System.Windows.Forms.Padding(10);
            this.label_OCR.Name = "label_OCR";
            this.label_OCR.Size = new System.Drawing.Size(144, 21);
            this.label_OCR.TabIndex = 87;
            this.label_OCR.Text = "Unknown";
            this.label_OCR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Connected
            // 
            this.label_Connected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Connected.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Connected.Location = new System.Drawing.Point(10, 50);
            this.label_Connected.Margin = new System.Windows.Forms.Padding(10);
            this.label_Connected.Name = "label_Connected";
            this.label_Connected.Size = new System.Drawing.Size(144, 21);
            this.label_Connected.TabIndex = 86;
            this.label_Connected.Text = "Unknown";
            this.label_Connected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Port
            // 
            this.label_Port.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Port.Location = new System.Drawing.Point(174, 25);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(109, 21);
            this.label_Port.TabIndex = 11;
            this.label_Port.Text = "Port:";
            this.label_Port.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_IP
            // 
            this.label_IP.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_IP.Location = new System.Drawing.Point(6, 25);
            this.label_IP.Name = "label_IP";
            this.label_IP.Size = new System.Drawing.Size(148, 21);
            this.label_IP.TabIndex = 10;
            this.label_IP.Text = "IP:";
            this.label_IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OCR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "OCR";
            this.Size = new System.Drawing.Size(398, 123);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.Label label_IP;
        internal System.Windows.Forms.Label label_Connected;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Label label_OCR;
        private System.Windows.Forms.Button button_ReadOCR;
        private System.Windows.Forms.Button button_ReConnect;
    }
}
