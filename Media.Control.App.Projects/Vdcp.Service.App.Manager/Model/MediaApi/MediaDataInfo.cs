using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{

    public class UpdateMediaData()
    {
        public string Id { get; set; }
        public string State { get; set; }

    }

    public class UpDateInPoint
    {
        public string Id { get; set; }

        public int InPoint  { get; set; }
        public string InTimecode  { get; set; }
        public int Frame  { get; set; }
        public string Duration { get; set; }
    }


    public class UpDateOutPoint
    {
        public string Id { get; set; }

        public int OutPoint { get; set; }
        public string OutTimecode { get; set; }
        public int Frame { get; set; }
        public string Duration { get; set; }
    }


    public class MediaDataInfo : INotifyPropertyChanged
    {
        public string MediaId { get; set; }


        public string _Image { get; set; } = @$"{AppDomain.CurrentDomain.BaseDirectory}\Noimage.png";
        public string Image
        {
            get => _Image;
            set
            {   
                _Image = value;
                OnPropertyChanged(nameof(Image));
                
            }
        }

        public string Name { get; set; }
        public string _Duration { get; set; }
        public string Duration
        {
            get => _Duration;
            set
            {
                _Duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }


        public int _Frame { get; set; }

        public int Frame
        {
            get => _Frame;
            set
            {
                _Frame = value;
                OnPropertyChanged(nameof(Frame));
            }
        }
        public DateTime CreatDate { get; set; }
        public int _InPoint { get; set; }

        public int InPoint
        {
            get => _InPoint;
            set
            {
                _InPoint = value;
                OnPropertyChanged(nameof(InPoint));
            }
        }
        public string _InTimeCode { get; set; }

        public string InTimeCode
        {
            get => _InTimeCode;
            set
            {
                _InTimeCode = value;
                OnPropertyChanged(nameof(InTimeCode));
            }
        }
        public int _OutPoint { get; set; }
        public int OutPoint
        {
            get => _OutPoint;
            set
            {
                _OutPoint = value;
                OnPropertyChanged(nameof(OutPoint));
            }
        }
        public string _OutTimeCode { get; set; }
        public string OutTimeCode
        {
            get => _OutTimeCode;
            set
            {
                _OutTimeCode = value;
                OnPropertyChanged(nameof(OutTimeCode));
            }
        }

        public string Creator { get; set; }
        public string Type { get; set; }
        public string Proxy { get; set; }
        public string Path { get; set; }
        public int Fps { get; set; }
        public string Des { get; set; }

        private string _state;
        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }


        public MediaDataInfo ShallowCopy()
        {
            return (MediaDataInfo)this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
