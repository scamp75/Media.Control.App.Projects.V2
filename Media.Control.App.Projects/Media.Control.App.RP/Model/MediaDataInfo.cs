using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class MediaDataInfo : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string MediaId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public int Frame { get; set; }
        public DateTime CreatDate { get; set; }
        public int InPoint { get; set; }
        public string InTimeCode { get; set; }
        public int OutPoint { get; set; }
        public string OutTimeCode { get; set; }
        public string Creator { get; set; }
        public string Type { get; set; }
        public string Proxy { get; set; }
        public string Path { get; set; }
        public int Fps { get; set; }
        public string Des { get; set; }
        //public string State { get; set; } = "Wait";

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

        public MediaDataInfo()
        {
        }

        [JsonConstructor]
        public MediaDataInfo(int index =0 , string mediaId = "", string image = "", string name = "", string duration = "",
                         int frame = 0, DateTime? creatDate = null, int inPoint = 0, string inTimeCode = "",
                         int outPoint = 0, string outTimeCode = "", string creator = "",
                         string type = "", string proxy = "", string path = "", int fps = 0, string des = "", string state = "")
        {
            Index = index;
            MediaId = mediaId;
            Image = image;
            Name = name;
            Duration = duration;
            Frame = frame;
            CreatDate = creatDate ?? DateTime.Now; 
            InPoint = inPoint;
            InTimeCode = inTimeCode;
            OutPoint = outPoint;
            OutTimeCode = outTimeCode;
            Creator = creator;
            Type = type;
            Proxy = proxy;
            Path = path;
            Fps = fps;
            Des = des;
            State = "Wait";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class MediaDataInfo2
    {
        public string MediaId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public int Frame { get; set; }
        public DateTime CreatDate { get; set; }
        public int InPoint { get; set; }
        public string InTimeCode { get; set; }
        public int OutPoint { get; set; }
        public string OutTimeCode { get; set; }
        public string Creator { get; set; }
        public string Type { get; set; }
        public string Proxy { get; set; }
        public string Path { get; set; }
        public int Fps { get; set; }

        public string Des { get; set; }
        public string State { get; set; }
    }
    
}
