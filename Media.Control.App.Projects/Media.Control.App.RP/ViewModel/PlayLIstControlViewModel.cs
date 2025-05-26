using Media.Control.App.RP.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Media.Control.App.RP.ViewModel
{
    public class PlayLIstControlViewModel : INotifyPropertyChanged
    {

        private PlayListControl _PlayListControl;

        private string _PlayListName { get; set; }
        public string PlayListName
        {
            get => _PlayListName;
            set
            {
                if (_PlayListName != value)
                {
                    _PlayListName = value;
                    OnPropertyChanged(nameof(PlayListName));
                }
            }
        }

        public PlayLIstControlViewModel(PlayListControl playListControl)
        {
            _PlayListControl = playListControl;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
