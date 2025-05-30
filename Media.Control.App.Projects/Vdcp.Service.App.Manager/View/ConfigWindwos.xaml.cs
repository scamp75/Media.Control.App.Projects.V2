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
using Vdcp.Service.App.Manager.ViewModel;

namespace Vdcp.Service.App.Manager.View
{
    /// <summary>
    /// ConfigWindwos.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigWindwos : Window
    {
        private ConfigWindwosViewModel _configWindwosViewModel;

        public ConfigWindwos()
        {
            InitializeComponent();

            _configWindwosViewModel = new ViewModel.ConfigWindwosViewModel(this);
            this.DataContext = _configWindwosViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _configWindwosViewModel.ConfigLoad();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloaseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _configWindwosViewModel.ConfigSave();
        }
    }
}
