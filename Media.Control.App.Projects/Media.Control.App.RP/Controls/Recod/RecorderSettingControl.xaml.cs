using Media.Control.App.RP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// RecorderSettingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecorderSettingControl : System.Windows.Controls.UserControl
    {

        public RecorderSettingControlViewModel ControlViewModel = null;


        public RecorderSettingControl()
        {
            InitializeComponent();
            ControlViewModel = new RecorderSettingControlViewModel(this);
            DataContext = ControlViewModel;
        }
    }
}
