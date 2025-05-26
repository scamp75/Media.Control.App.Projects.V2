using Microsoft.OpenApi.MicrosoftExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.Api.Manager.ViewModel
{
    public class MainWindowsViewModel : INotifyPropertyChanged
    {

        private readonly MainWindow mainWindow;


        private string apiMessage { get; set; } = "Media Service Api DisConnected ...";

        public string ApiMessage
        {
            get { return apiMessage; }
            set { apiMessage = value; OnPropertyChanged(nameof(ApiMessage)); }
        }

        private bool isIndeterminate { get; set; } = false;

        public bool IsIndeterminate
        {
            get => isIndeterminate;
            set { isIndeterminate = value; OnPropertyChanged(nameof(IsIndeterminate)); }
        }

        public MainWindowsViewModel(MainWindow window)
        {
            this.mainWindow = window;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
