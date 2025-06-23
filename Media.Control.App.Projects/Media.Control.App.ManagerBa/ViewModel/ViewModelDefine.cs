using Media.Control.App.ManagerBa.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Media.Control.App.ManagerBa.Model.VdcpConfig;

namespace Media.Control.App.ManagerBa.ViewModel
{
    public partial class ConfigWindowViewModel 
    {

        private EnuEnaginType selectEnaginType {  get; set; }  

        public EnuEnaginType SelectEnaginType
        {
            set
            {
                selectEnaginType = value;
                OnPropertyChanged(nameof(SelectEnaginType));
            }
            get 
            { 
                return selectEnaginType; 
               
            }
        }

        private string channelType { get; set; }

        public string ChannelType { get => channelType; set { channelType = value; OnPropertyChanged(); } }


        private string platformUrl { get; set; }    

        public string PlatformUrl
        {
            set => platformUrl = value;
            get { return platformUrl; OnPropertyChanged(nameof(PlatformUrl)); } 
        }

        private string apikey { get; set; } 

        public string ApiKey
        {
            set {apikey = value; OnPropertyChanged(nameof(ApiKey)); }
            get { return apikey; }
        }

        private string workNode { get; set; }    
        public string WorkNode
        {
            set { workNode = value; OnPropertyChanged(nameof(WorkNode)); }
            get { return workNode; }
        }

        private string fabric { get; set; }
        public string Fabric
        {
            set { fabric = value; OnPropertyChanged(nameof(Fabric)); }
            get { return fabric; }
        }

        private string producerName { get; set; }
        public string ProducerName
        {
            set { ProducerName = value; OnPropertyChanged(nameof(ProducerName)); }
            get { return ProducerName; }
        }

        private int playerGangPort { get; set; }

        public int PlayerGangPort
        {
            set {playerGangPort = value; OnPropertyChanged(nameof(PlayerGangPort)); }
            get { return playerGangPort;}
        }

        private int recorderGangPort { get; set; }

        public int RecorderGangPort
        {
            set { recorderGangPort = value; OnPropertyChanged(nameof(RecorderGangPort)); }
            get { return recorderGangPort;}
        }

        private string defaultRcName { get; set; }  

        public string DefaultRcName
        {
            set { defaultRcName = value; OnPropertyChanged(nameof(DefaultRcName)); }
            get { return defaultRcName;  }
        }

        private string defaultReFolder { get; set; }

        public string DefaultReFolder
        {
            set { defaultReFolder = value; OnPropertyChanged(nameof(DefaultRcName)); }
            get{   return defaultReFolder;             }   
        }

        private int recordOffset { get; set; }
        public int RecordOffset
        { 
            set { recordOffset = value; OnPropertyChanged(nameof(RecordOffset)); }
            get { return recordOffset;}   
        }

        private string mediaUrl { get; set; }
        public string MediaUrl
        {
            set { mediaUrl = value; OnPropertyChanged(nameof(MediaUrl)); }
            get { return mediaUrl; }
        }

        private string mediaApiKey { get; set; }
        public string MediaApiKey
        {
            set { mediaApiKey = value; OnPropertyChanged(nameof(mediaApiKey)); }
            get { return mediaApiKey; }
        }

        private string mediaWorkNode { get; set; }
        public string MediaWorkNode
        {
            set { mediaWorkNode = value; OnPropertyChanged(nameof(MediaWorkNode)); }
            get { return mediaWorkNode; }
        }

     

        private string _ComPort { set;get; }
        public string ComPort
        {
            set { _ComPort = value; OnPropertyChanged(nameof(ComPort)); }
            get { return _ComPort; }
        }

        private int _SelectPort { set; get; }
        public int SelectPort
        {
            set { _SelectPort = value; OnPropertyChanged(nameof(SelectPort)); }
            get { return _SelectPort; }
        }

        private string _PortIpAddress { get; set; }

        public string PortIpAddress
        {
            set { _PortIpAddress = value; OnPropertyChanged(nameof(PortIpAddress)); }
            get { return _PortIpAddress; }
        }

        private int _IpPort { get; set; }

        public int IpPort
        {
            set { _IpPort = value; OnPropertyChanged(nameof(IpPort)); }
            get { return _IpPort; }
        }

    }
}
