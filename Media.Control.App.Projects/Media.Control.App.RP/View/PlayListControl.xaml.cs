using Media.Control.App.RP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Media.Control.App.RP.View
{
    /// <summary>
    /// PlayListControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayListControl : Window
    {

        public PlayLIstControlViewModel ControlViewModel = null;   
        public PlayListControl()
        {
            InitializeComponent();

            ControlViewModel = new PlayLIstControlViewModel(this);
            this.DataContext = ControlViewModel;
        }

        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void butClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
