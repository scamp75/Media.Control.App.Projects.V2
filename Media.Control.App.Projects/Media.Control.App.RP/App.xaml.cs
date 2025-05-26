using System.Configuration;
using System.Data;
using System.Windows;

namespace Media.Control.App.RP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App :  System.Windows.Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 명령줄 인수 확인
            string[] args = e.Args;

            // MainWindow에 인수 전달
            //MainWindow mainWindow = new MainWindow(args);
           // mainWindow.Show();
        }
    }

}
