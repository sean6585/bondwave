namespace AML
{
    partial class Stage
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
            this.Label_Slot = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_PutAddress = new System.Windows.Forms.Label();
            this.label_Type = new System.Windows.Forms.Label();
            this.label_SlotPitch = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label_Slot
            // 
            this.Label_Slot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Label_Slot.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label_Slot.Location = new System.Drawing.Point(15, 25);
            this.Label_Slot.Margin = new System.Windows.Forms.Padding(17);
            this.Label_Slot.Name = "Label_Slot";
            this.Label_Slot.Size = new System.Drawing.Size(99, 21);
            this.Label_Slot.TabIndex = 87;
            this.Label_Slot.Text = "Unknown";
            this.Label_Slot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_PutAddress);
            this.groupBox1.Controls.Add(this.label_Type);
            this.groupBox1.Controls.Add(this.label_SlotPitch);
            this.groupBox1.Controls.Add(this.Label_Slot);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(130, 135);
            this.groupBox1.TabIndex = 88;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Name";
            // 
            // label_PutAddress
            // 
            this.label_PutAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_PutAddress.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_PutAddress.Location = new System.Drawing.Point(15, 80);
            this.label_PutAddress.Margin = new System.Windows.Forms.Padding(17);
            this.label_PutAddress.Name = "label_PutAddress";
            this.label_PutAddress.Size = new System.Drawing.Size(99, 21);
            this.label_PutAddress.TabIndex = 93;
            this.label_PutAddress.Text = "Unknown";
            this.label_PutAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Type
            // 
            this.label_Type.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Type.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Type.Location = new System.Drawing.Point(15, 106);
            this.label_Type.Margin = new System.Windows.Forms.Padding(17);
            this.label_Type.Name = "label_Type";
            this.label_Type.Size = new System.Drawing.Size(99, 21);
            this.label_Type.TabIndex = 91;
            this.label_Type.Text = "Unknown";
            this.label_Type.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_SlotPitch
            // 
            this.label_SlotPitch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_SlotPitch.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_SlotPitch.Location = new System.Drawing.Point(15, 51);
            this.label_SlotPitch.Margin = new System.Windows.Forms.Padding(17);
            this.label_SlotPitch.Name = "label_SlotPitch";
            this.label_SlotPitch.Size = new System.Drawing.Size(99, 21);
            this.label_SlotPitch.TabIndex = 89;
            this.label_SlotPitch.Text = "Unknown";
            this.label_SlotPitch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Stage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Stage";
            this.Size = new System.Drawing.Size(130, 135);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.Label Label_Slot;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.Label label_SlotPitch;
        internal System.Windows.Forms.Label label_Type;
        internal System.Windows.Forms.Label label_PutAddress;
    }
}
