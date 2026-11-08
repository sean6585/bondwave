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
    public partial class ForkView : UserControl
    {
        public ForkView()
        {
            InitializeComponent();
        }

    
        public void Init(Fork _f)
        {
          
            groupBox1.Text = _f.RobotArm.ToString();
            RefreshUI(_f);
        }

        /*  
        public RobotType RobotType = RobotType.Unknown;
        public TargetType TargetType = TargetType.Unknown;
        public ForkWaferPresent WaferPresent = ForkWaferPresent.Unknown;
        public VacuumStatus VacuumStatus = VacuumStatus.Unknown;      
        */

        public void RefreshUI(Fork _f)
        {
            label_Target.Text = _f.TargetType.ToString();
            label_VacGrip.Text = _f.VacuumStatus.ToString();
            label_WaferPresent.Text = _f.WaferPresent.ToString();
            label_Type.Text = _f.RobotType.ToString();
        }
    }
}
