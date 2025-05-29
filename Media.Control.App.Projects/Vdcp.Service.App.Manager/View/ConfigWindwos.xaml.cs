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

namespace Vdcp.Service.App.Manager.View
{
    /// <summary>
    /// ConfigWindwos.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigWindwos : Window
    {
        public ConfigWindwos()
        {
            InitializeComponent();

            this.DataContext = new ViewModel.ConfigWindwosViewModel(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
