using Media.Control.App.ManagerBa.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.ManagerBa.ViewModel
{
    public partial class ConfigWindowViewModel 
    {

        private EnuEnaginType selectEnaginType {  get; set; }  

        public EnuEnaginType SelectEnaginType
        {
            set => selectEnaginType = value;
            get { return selectEnaginType; OnPropertyChanged(nameof(SelectEnaginType)); }
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
            set => apikey = value;
            get { return apikey; OnPropertyChanged(nameof(ApiKey)); }
        }

        private string workNode { get; set; }    
        public string WorkNode
        {
            set => workNode = value;
            get { return workNode; OnPropertyChanged(nameof(WorkNode)); }
        }

        private string fabric { get; set; }
        public string Fabric
        {
            set => fabric = value;
            get { return fabric; OnPropertyChanged(nameof(Fabric)); }
        }

        private string producerName { get; set; }
        public string ProducerName
        {
            set => ProducerName = value;
            get { return ProducerName; OnPropertyChanged(nameof(ProducerName)); }
        }

        private int playerGangPort { get; set; }

        public int PlayerGangPort
        {
            set => playerGangPort = value;
            get { return playerGangPort; OnPropertyChanged(nameof(PlayerGangPort)); }
        }

        private int recorderGangPort { get; set; }

        public int RecorderGangPort
        { 
            set => recorderGangPort = value;
            get { return recorderGangPort; OnPropertyChanged(nameof(RecorderGangPort)); }
        }

        private string defaultRcName { get; set; }  

        public string DefaultRcName
        { 
            set => defaultRcName = value;
            get { return defaultRcName; OnPropertyChanged(nameof(DefaultRcName)); }
        }

        private string defaultReFolder { get; set; }

        public string DefaultReFolder
        {
            set => defaultReFolder = value;
            get
            {
                return defaultReFolder; OnPropertyChanged(nameof(DefaultReFolder));
            }
        }

        private int recordOffset { get; set; }
        public int RecordOffset
        { 
            set => recordOffset = value;
            get { return recordOffset; OnPropertyChanged(nameof(RecordOffset)); }   
        }

        private string mediaUrl { get; set; }
        public string MediaUrl
        {
            set => mediaUrl = value;
            get { return mediaUrl; OnPropertyChanged(nameof(MediaUrl)); }
        }

        private string mediaApiKey { get; set; }
        public string MediaApiKey
        {
            set => mediaApiKey = value;
            get { return mediaApiKey; OnPropertyChanged(nameof(mediaApiKey)); }
        }

        private string mediaWorkNode { get; set; }
        public string MediaWorkNode
        {
            set => mediaWorkNode = value;
            get { return mediaWorkNode; OnPropertyChanged(nameof(MediaWorkNode)); }
        }


    }
}
