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
    public partial class AlignerView : UserControl
    {
        public AlignerView()
        {
            InitializeComponent();
        }


        public void Init(Aligner _A)
        {

            groupBox1.Text = "Aligner";
            RefreshUI(_A);
        }

        /*  
        public RobotType RobotType = RobotType.Unknown;
        public TargetType TargetType = TargetType.Unknown;
        public ForkWaferPresent WaferPresent = ForkWaferPresent.Unknown;
        public VacuumStatus VacuumStatus = VacuumStatus.Unknown;      
        */

        public void RefreshUI(Aligner _A)
        {
            //label_Target.Text = _f.TargetType.ToString();
            //label_VacGrip.Text = _f.VacuumStatus.ToString();
            //label_WaferPresent.Text = _f.WaferPresent.ToString();
        }
    }
}
