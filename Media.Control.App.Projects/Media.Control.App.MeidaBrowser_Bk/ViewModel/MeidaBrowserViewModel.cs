using Media.Control.App.MeidaBrowser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using System.Data.Common;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Media.Control.App.MeidaBrowser.Controls;
using System.Windows.Interop;
using System.IO;

namespace Media.Control.App.MeidaBrowser.ViewModel
{
    enum EnmMediaStat { Prepared, Done, Recoding, Error }
    public class MeidaBrowserViewModel : INotifyPropertyChanged 
    {
        public ObservableCollection<MediaDataInfo> MediaDataList {  get; set; }
        public ObservableCollection<string> ChannelLists { get; set; }

        private double titleWidth { get; set; } = 800;

        public double TitleWidth { get => titleWidth; set { titleWidth = value; OnPropertyChanged(nameof(TitleWidth)); } }

        private double nameWidth { get; set; } = 300;

        public double NameWidht { get => nameWidth; set { nameWidth = value; OnPropertyChanged(nameof(NameWidht)); }  }

        private int gridHeight { get; set; } = 500;

        public int GridHeight { get => gridHeight; set { gridHeight = value; OnPropertyChanged(nameof(GridHeight)); } }

        public bool _IsCheckedCreateDate { get; set; } = true;
        public bool IsCheckedCreateDate { get => _IsCheckedCreateDate; 
            set { _IsCheckedCreateDate = value; OnPropertyChanged(nameof(IsCheckedCreateDate)); } }

        private DateTime _SelectedDate { get; set; } = DateTime.Now.Date;

        public DateTime SelectedDate
        { 
            get => _SelectedDate; 
            set 
            {
                _SelectedDate = value;
                OnPropertyChanged(nameof(SelectedDate)); 
            } 
        }

        private string selecteChannel { get; set; } = "All Channel";
        public string SelecteChannel { get=> selecteChannel; set { selecteChannel = value; OnPropertyChanged(nameof(SelecteChannel)); } }

        private string searchName { get; set; } 

        public string SearchName { get => searchName; set { searchName = value; OnPropertyChanged(nameof(SearchName)); } }

        public ICommand Command_Close { get; }
        public ICommand Command_Min { get; }
        public ICommand Command_Max { get; }

        public ICommand Command_Search { get; }

        public ICommand Command_Save { get; }

        private double Left { get; set; } = 0;
        private double Top { get; set; } = 0;

        private MediaApiConnecter ApiConnecter { get; set; }
        private  MainWindow mainWindow = null;
        private MediaListControlViewModel mediaListViewModel { get; set; }

        public void UpdateUI(Action uiAction)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(uiAction);
        }

    

        public MeidaBrowserViewModel(MainWindow window) 
        {
            try
            {
                mainWindow = window;

                ApiConnecter = new MediaApiConnecter("mediahub");
                ApiConnecter.Connection();
                ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend1; ;

                Command_Close = new RelayCommand(CommandClose);
                Command_Max = new RelayCommand(CommandMax);
                Command_Min = new RelayCommand(CommandMin);
                Command_Search = new RelayCommand(CommandSearch);
                Command_Save = new RelayCommand(CommandSave);
                mainWindow.SizeChanged += MainWindow_SizeChanged;

                //MediaDataList = new ObservableCollection<MediaDataInfo>();
                SelectedDate = DateTime.Now;
                mediaListViewModel = mainWindow.MediaListControl.mediaListViewModel;
                mediaListViewModel.MeidaListItems = new ObservableCollection<MediaDataInfo>();

                // mediaListViewModel.isImage = false;
                //mediaListViewModel.isPicture = true;

                ChannelLists = new ObservableCollection<string>();
                ChannelLists.Add("All");
            }
            catch (Exception ex) 
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            
        }

        private void ApiConnecter_DoHubEventSend1(string type, string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                if(type == "InTimecode")
                {
                    var mediaData = JsonConvert.DeserializeObject<UpDateInPoint>(message);
                    mediaListViewModel.SetInTimceCode(mediaData);

                }
                else if(type == "OutTimecode")
                {
                    var mediaData = JsonConvert.DeserializeObject<UpDateOutPoint>(message);
                    mediaListViewModel.SetOutTimceCode(mediaData);
                }
                else if (type == "UpDate")
                {
                    var mediaData = JsonConvert.DeserializeObject<UpdateMediaData>(message);
                    mediaListViewModel.SetMediaStste(mediaData.Id, mediaData.State);
                }
                else if(type=="Insert")
                {
                    var mediaData = JsonConvert.DeserializeObject<MediaDataInfo>(message);
                    mediaListViewModel.MeidaListItems.Insert(0, mediaData);
                }
                
            });
        }

        

        public async void ConnectHub()
        {
            try
            {
                ApiConnecter.StartHub();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Connection failed: " + ex.Message);
            }
        }
       
        private async void CommandSave(object? obj)
        {
            var mediaData = new MediaDataInfo
            {
                CreatDate = DateTime.Now,
                Des = "",
                Creator = "Channel2",
                Duration = "00:23:23:00",
                Frame = 9876,
                Image = "D:\\Noimage.png",
                InPoint = 0,
                InTimeCode = "00:00:00:00",
                OutPoint = 2345,
                OutTimeCode = "00:23:23:00",
                MediaId = "83sswSS2324343dd42342342",
                Name = "테스트 미디어00098",
                Path = "c:\\temp\\media.mxf",
                Fps = 30,
                Proxy = "c:\\temp\\media.mp4",

                Type = "HD",
                State = "Done"

            };

          //  mainWindow.MediaListControl.mediaListViewModel.MeidaListItems.Insert(0, mediaData);

            var json = JsonConvert.SerializeObject(mediaData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ApiConnecter.Client().PostAsync("MeidaInfo", content);

            if (response.IsSuccessStatusCode)
            {
                System.Windows.MessageBox.Show("Log created successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to create log.");
            }

        }

        public async void DeleteMedia(MediaDataInfo item)
        {
            if (item == null) return;

            var response = await ApiConnecter.client.DeleteAsync($"MediaInfo/{item.MediaId}");

            if (response.IsSuccessStatusCode)
            {
                //System.Windows.MessageBox.Show("Log created successfully!");
            }
            else
            {
                //System.Windows.MessageBox.Show("Failed to create log.");
            }


            try
            {

                FileInfo fileInfo = new FileInfo(item.Path);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            catch(Exception ex)
            {

            }

            try
            {
                FileInfo fileInfo = new FileInfo(item.Proxy);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            catch(Exception ex)
            {

            }

            try
            {
                FileInfo fileInfo = new FileInfo(item.Image);

                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            catch(Exception ex)
            {

            }
        }

        private async void CommandSearch(object? obj)
        {
            try
            {
                string channel = "null";
                string name = "null";
                if (SelecteChannel != "All")  channel = SelecteChannel;
                if(!string.IsNullOrEmpty(SearchName) ) name = SearchName;

                string query = $"MediaInfo?createDate={SelectedDate.ToShortDateString()}&iscreateDate={IsCheckedCreateDate}&channel={channel}&title={name}";

                var response = await ApiConnecter.Client().GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var medias = JsonConvert.DeserializeObject<ObservableCollection<MediaDataInfo>>(json);

                    mainWindow.MediaListControl.mediaListViewModel.SetMediaList(medias);

                    //mainWindow.MediaListControl.mediaListViewModel.MeidaListItems.Clear();
                    //foreach (var media in medias)
                    //{
                    //    mainWindow.MediaListControl.mediaListViewModel.MeidaListItems.Insert(0, media);
                    //}
                }
            }
            catch { }
        }

        private void MainWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if(e.WidthChanged)
            {
                TitleWidth = e.NewSize.Width - 305;
                NameWidht = 300 + e.NewSize.Width - 840;
            }

            if (e.HeightChanged)
            {
                GridHeight = (int)e.NewSize.Height - 10;
            }

        }

        private void CommandMin(object? obj)
        {
            mainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CommandMax(object? obj)
        {
            if (mainWindow.WindowState == System.Windows.WindowState.Normal)
                mainWindow.WindowState = System.Windows.WindowState.Maximized;
            else mainWindow.WindowState = System.Windows.WindowState.Normal;
        }

        public async void CommandClose(object? obj)
        {

            ApiConnecter.CloseHub();
            mainWindow?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
