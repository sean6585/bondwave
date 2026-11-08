using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AML
{
    public partial class Stage : UserControl
    {
        public Stage()
        {
            InitializeComponent();
        }

        public void Init(StationInfo s)
        {
            groupBox1.Text = s.StationName;
            Label_Slot.Text = s.MaxSlot.ToString();
            label_SlotPitch.Text = s.SlotPitch + " (mm)";
            label_Type.Text = s.TargetType.ToString();
            label_PutAddress.Text = s.LowerPutAddress.ToString();
        }
    }
}
