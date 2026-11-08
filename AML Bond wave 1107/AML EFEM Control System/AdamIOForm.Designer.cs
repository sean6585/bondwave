namespace AML
{
    partial class AdamIOForm
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
            this.DGV_DI_0_9 = new System.Windows.Forms.DataGridView();
            this.Column_DI_No_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_DI_State_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_DO = new System.Windows.Forms.ComboBox();
            this.button_DO_False = new System.Windows.Forms.Button();
            this.button_DO_True = new System.Windows.Forms.Button();
            this.Label_Online = new System.Windows.Forms.Label();
            this.DGV_DO = new System.Windows.Forms.DataGridView();
            this.Column_DO_No_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_DO_State_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_AI = new System.Windows.Forms.DataGridView();
            this.Column_AI_No_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_AI_State_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_AO = new System.Windows.Forms.DataGridView();
            this.Column_AO_No_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_AO_State_0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_DI_0_9)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_DO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_AI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_AO)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV_DI_0_9
            // 
            this.DGV_DI_0_9.AllowUserToAddRows = false;
            this.DGV_DI_0_9.AllowUserToDeleteRows = false;
            this.DGV_DI_0_9.AllowUserToResizeColumns = false;
            this.DGV_DI_0_9.AllowUserToResizeRows = false;
            this.DGV_DI_0_9.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_DI_0_9.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_DI_0_9.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_DI_No_0,
            this.Column_DI_State_0});
            this.DGV_DI_0_9.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV_DI_0_9.Location = new System.Drawing.Point(0, 106);
            this.DGV_DI_0_9.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DGV_DI_0_9.Name = "DGV_DI_0_9";
            this.DGV_DI_0_9.RowTemplate.Height = 24;
            this.DGV_DI_0_9.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_DI_0_9.Size = new System.Drawing.Size(288, 322);
            this.DGV_DI_0_9.TabIndex = 5;
            // 
            // Column_DI_No_0
            // 
            this.Column_DI_No_0.HeaderText = "DI";
            this.Column_DI_No_0.Name = "Column_DI_No_0";
            this.Column_DI_No_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_DI_State_0
            // 
            this.Column_DI_State_0.HeaderText = "State";
            this.Column_DI_State_0.Name = "Column_DI_State_0";
            this.Column_DI_State_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBox_DO);
            this.groupBox1.Controls.Add(this.button_DO_False);
            this.groupBox1.Controls.Add(this.button_DO_True);
            this.groupBox1.Controls.Add(this.Label_Online);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(882, 106);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(602, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 20);
            this.label2.TabIndex = 94;
            this.label2.Text = "AO:";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(646, 23);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(82, 28);
            this.comboBox1.TabIndex = 93;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(318, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 20);
            this.label1.TabIndex = 92;
            this.label1.Text = "DO:";
            // 
            // comboBox_DO
            // 
            this.comboBox_DO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_DO.FormattingEnabled = true;
            this.comboBox_DO.Location = new System.Drawing.Point(362, 23);
            this.comboBox_DO.Name = "comboBox_DO";
            this.comboBox_DO.Size = new System.Drawing.Size(82, 28);
            this.comboBox_DO.TabIndex = 91;
            // 
            // button_DO_False
            // 
            this.button_DO_False.Location = new System.Drawing.Point(450, 57);
            this.button_DO_False.Name = "button_DO_False";
            this.button_DO_False.Size = new System.Drawing.Size(122, 39);
            this.button_DO_False.TabIndex = 90;
            this.button_DO_False.Text = "False";
            this.button_DO_False.UseVisualStyleBackColor = true;
            this.button_DO_False.Click += new System.EventHandler(this.button_DO_False_Click);
            // 
            // button_DO_True
            // 
            this.button_DO_True.Location = new System.Drawing.Point(322, 57);
            this.button_DO_True.Name = "button_DO_True";
            this.button_DO_True.Size = new System.Drawing.Size(122, 39);
            this.button_DO_True.TabIndex = 89;
            this.button_DO_True.Text = "True";
            this.button_DO_True.UseVisualStyleBackColor = true;
            this.button_DO_True.Click += new System.EventHandler(this.button_DO_True_Click);
            // 
            // Label_Online
            // 
            this.Label_Online.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Label_Online.Dock = System.Windows.Forms.DockStyle.Left;
            this.Label_Online.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Bold);
            this.Label_Online.Location = new System.Drawing.Point(3, 25);
            this.Label_Online.Margin = new System.Windows.Forms.Padding(17);
            this.Label_Online.Name = "Label_Online";
            this.Label_Online.Size = new System.Drawing.Size(285, 78);
            this.Label_Online.TabIndex = 88;
            this.Label_Online.Text = "Offline";
            this.Label_Online.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DGV_DO
            // 
            this.DGV_DO.AllowUserToAddRows = false;
            this.DGV_DO.AllowUserToDeleteRows = false;
            this.DGV_DO.AllowUserToResizeColumns = false;
            this.DGV_DO.AllowUserToResizeRows = false;
            this.DGV_DO.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_DO.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_DO.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_DO_No_0,
            this.Column_DO_State_0});
            this.DGV_DO.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV_DO.Location = new System.Drawing.Point(296, 106);
            this.DGV_DO.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DGV_DO.Name = "DGV_DO";
            this.DGV_DO.RowTemplate.Height = 24;
            this.DGV_DO.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_DO.Size = new System.Drawing.Size(288, 203);
            this.DGV_DO.TabIndex = 8;
            // 
            // Column_DO_No_0
            // 
            this.Column_DO_No_0.HeaderText = "DO";
            this.Column_DO_No_0.Name = "Column_DO_No_0";
            this.Column_DO_No_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_DO_State_0
            // 
            this.Column_DO_State_0.HeaderText = "State";
            this.Column_DO_State_0.Name = "Column_DO_State_0";
            this.Column_DO_State_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_AI
            // 
            this.DGV_AI.AllowUserToAddRows = false;
            this.DGV_AI.AllowUserToDeleteRows = false;
            this.DGV_AI.AllowUserToResizeColumns = false;
            this.DGV_AI.AllowUserToResizeRows = false;
            this.DGV_AI.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_AI.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_AI.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_AI_No_0,
            this.Column_AI_State_0});
            this.DGV_AI.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV_AI.Location = new System.Drawing.Point(592, 106);
            this.DGV_AI.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DGV_AI.Name = "DGV_AI";
            this.DGV_AI.RowTemplate.Height = 24;
            this.DGV_AI.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_AI.Size = new System.Drawing.Size(288, 322);
            this.DGV_AI.TabIndex = 9;
            // 
            // Column_AI_No_0
            // 
            this.Column_AI_No_0.HeaderText = "AI";
            this.Column_AI_No_0.Name = "Column_AI_No_0";
            this.Column_AI_No_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_AI_State_0
            // 
            this.Column_AI_State_0.HeaderText = "State";
            this.Column_AI_State_0.Name = "Column_AI_State_0";
            this.Column_AI_State_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_AO
            // 
            this.DGV_AO.AllowUserToAddRows = false;
            this.DGV_AO.AllowUserToDeleteRows = false;
            this.DGV_AO.AllowUserToResizeColumns = false;
            this.DGV_AO.AllowUserToResizeRows = false;
            this.DGV_AO.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV_AO.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_AO.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_AO_No_0,
            this.Column_AO_State_0});
            this.DGV_AO.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV_AO.Location = new System.Drawing.Point(296, 320);
            this.DGV_AO.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DGV_AO.Name = "DGV_AO";
            this.DGV_AO.RowTemplate.Height = 24;
            this.DGV_AO.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_AO.Size = new System.Drawing.Size(288, 108);
            this.DGV_AO.TabIndex = 10;
            // 
            // Column_AO_No_0
            // 
            this.Column_AO_No_0.HeaderText = "AO";
            this.Column_AO_No_0.Name = "Column_AO_No_0";
            this.Column_AO_No_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_AO_State_0
            // 
            this.Column_AO_State_0.HeaderText = "State";
            this.Column_AO_State_0.Name = "Column_AO_State_0";
            this.Column_AO_State_0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // AdamIOForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 430);
            this.Controls.Add(this.DGV_AO);
            this.Controls.Add(this.DGV_AI);
            this.Controls.Add(this.DGV_DO);
            this.Controls.Add(this.DGV_DI_0_9);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdamIOForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdamIOForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdamIOForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_DI_0_9)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_DO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_AI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_AO)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DGV_DI_0_9;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView DGV_DO;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_DI_No_0;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_DI_State_0;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_DO_No_0;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_DO_State_0;
        internal System.Windows.Forms.Label Label_Online;
        private System.Windows.Forms.Button button_DO_False;
        private System.Windows.Forms.Button button_DO_True;
        private System.Windows.Forms.ComboBox comboBox_DO;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView DGV_AI;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_AI_No_0;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_AI_State_0;
        private System.Windows.Forms.DataGridView DGV_AO;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_AO_No_0;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_AO_State_0;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}