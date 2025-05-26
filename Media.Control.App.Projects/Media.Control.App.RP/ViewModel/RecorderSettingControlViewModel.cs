using Media.Control.App.RP.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.ViewModel 
{
    public class RecorderSettingControlViewModel : INotifyPropertyChanged
    {
        private RecorderSettingControl RecorderSettingControl = null;

        private string _DefaultPath { get; set; }

        public string DefaultPath
        {
            get => _DefaultPath;
            set { _DefaultPath = value; OnPropertyChanged(nameof(DefaultPath)); }
        }

        private string _DefaultTitle { get; set; }
        public string DefaultTitle
        {
            get => _DefaultTitle;
            set { _DefaultTitle = value; OnPropertyChanged(nameof(DefaultTitle)); }
        }

        
        public RecorderSettingControlViewModel(RecorderSettingControl control)
        {
            RecorderSettingControl =  control;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
