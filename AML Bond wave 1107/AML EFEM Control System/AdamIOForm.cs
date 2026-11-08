using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AML
{
    public partial class AdamIOForm : Form
    {
        public AdamIOForm()
        {
            InitializeComponent();
        }

        public bool ExitFunction = false;
        private Thread RefreshUI;

        private Adam6000s IO;
        public void Init(Adam6000s _IO)
        {
            IO = _IO;

            groupBox1.Text = "";// _IO.AdamIP;
            InitDGV();
            //timer200ms.Enabled = true;
            ExitFunction = false;
            RefreshUI = new Thread(RefreshUIControl);
            RefreshUI.IsBackground = true;
            RefreshUI.Start();
        }       
        
        public void InitDGV()
        {
            /*
                DOCount = 6;
                DICount = 12;
                AICount = 0;
                AOCount = 0;
            */
            //if (IO.AdamType == Adam6000s.AdammSeries.Adam6050)
            //{
            DGV_DI_0_9.RowCount = IO.DI.Length;
            DGV_DO.RowCount = IO.DO.Length;
            


            DGV_DI_0_9.RowHeadersVisible = true;
            DGV_DI_0_9.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            for (int i = 0; i < DGV_DI_0_9.RowCount; i++)
            {
                DGV_DI_0_9.Rows[i].Cells["Column_DI_No_0"].Value = (i).ToString();
                DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Value = "Unknown";
            }
            DGV_DO.RowHeadersVisible = true;
            DGV_DO.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            for (int i = 0; i < DGV_DO.RowCount; i++)
            {
                DGV_DO.Rows[i].Cells["Column_DO_No_0"].Value = (i).ToString();
                DGV_DO.Rows[i].Cells["Column_DO_State_0"].Value = "Unknown";
                comboBox_DO.Items.Add(i);
            }



            comboBox_DO.SelectedIndex = 0;
        }

        public void RefreshDGV()
        {
            if (IO.IsConnect)
            {
                Label_Online.Text = IO.AdamIP +Environment.NewLine + "Online";
                Label_Online.BackColor = Color.Lime;
            }
            else
            {
                Label_Online.Text = IO.AdamIP + Environment.NewLine + "Offline";
                Label_Online.BackColor = Color.Salmon;
            }


            for (int i = 0; i < DGV_DI_0_9.RowCount; i++)
            {
              
                if (IO.IsConnect)
                {
                    DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Value = IO.DI[i].ToString();
                    if (IO.DI[i])
                        DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Style.BackColor = Color.Yellow;
                    else
                        DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Style.BackColor = Color.White;
                }
                else
                {
                    DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Value = "Unknown";
                    DGV_DI_0_9.Rows[i].Cells["Column_DI_State_0"].Style.BackColor = Color.Salmon;
                }
            }

            //for (int i = 0; i < DGV_DI_10_19.RowCount; i++)
            //{
             
            //    if (IO.IsConnect)
            //    {
            //        DGV_DI_10_19.Rows[i].Cells["Column_DI_State_1"].Value = IO.DI[i + 10].ToString();
            //        if (IO.DI[i + 10])
            //            DGV_DI_10_19.Rows[i].Cells["Column_DI_State_1"].Style.BackColor = Color.Yellow;
            //        else
            //            DGV_DI_10_19.Rows[i].Cells["Column_DI_State_1"].Style.BackColor = Color.White;
            //    }
            //    else
            //    {
            //        DGV_DI_10_19.Rows[i].Cells["Column_DI_State_1"].Value = "Unknown";
            //        DGV_DI_10_19.Rows[i].Cells["Column_DI_State_1"].Style.BackColor = Color.Salmon;
            //    }

            //}

            for (int i = 0; i < DGV_DO.RowCount; i++)
            {         
                if (IO.IsConnect)
                {
                    DGV_DO.Rows[i].Cells["Column_DO_State_0"].Value = IO.DO[i].ToString();
                    if (IO.DO[i])
                        DGV_DO.Rows[i].Cells["Column_DO_State_0"].Style.BackColor = Color.Yellow;
                    else
                        DGV_DO.Rows[i].Cells["Column_DO_State_0"].Style.BackColor = Color.White;
                }
                else
                {
                    DGV_DO.Rows[i].Cells["Column_DO_State_0"].Value = "Unknown";
                    DGV_DO.Rows[i].Cells["Column_DO_State_0"].Style.BackColor = Color.Salmon;
                }
            }
        }

        private void AdamIOForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void timer200ms_Tick(object sender, EventArgs e)
        {
            if (Visible)
                RefreshDGV();
        }
        private void RefreshUIControl(object obj)
        {
            while (true)
            {
                if (ExitFunction)
                    return;
                Thread.Sleep(200);
                if (Visible)
                    RefreshDGV();
            }
        }

        private void button_DO_True_Click(object sender, EventArgs e)
        {
            IO.WriteDO(comboBox_DO.SelectedIndex, true);
        }

        private void button_DO_False_Click(object sender, EventArgs e)
        {
            IO.WriteDO(comboBox_DO.SelectedIndex, false);
        }
    }
}
