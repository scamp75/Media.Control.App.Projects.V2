using Ampp.Control.lib.Model;
using Media.Control.App.RP.Command;
using Media.Control.App.RP.Controls;
using Media.Control.App.RP.Model;
using Media.Control.App.RP.Model.Config;
using MpvNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Media.Control.App.RP.ViewModel
{


    public class MediaListControlViewModel : INotifyPropertyChanged
    {
        private MediaListControl MediaListControl = null;
        private  Mutex _mutex;

        private string _ChannelType;

        public string ChannelType
        {
            get => _ChannelType;
            set
            {
                if (_ChannelType != value)
                {
                    _ChannelType = value;
                    OnPropertyChanged(nameof(ChannelType));
                    MediaListControl.ButPreview.Visibility = value == "Recodrder" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private ListStyle _meidaListStyle;
        public ListStyle MeidaListStyle
        {
            get => _meidaListStyle;
            set
            {
                if (_meidaListStyle != value)
                {
                    _meidaListStyle = value;
                    OnPropertyChanged(nameof(MeidaListStyle));
                }
            }
        }

        private ObservableCollection<MediaDataInfo> _meidaListItems;
        public ObservableCollection<MediaDataInfo> MeidaListItems
        {
            get => _meidaListItems;
            set
            {
                if (_meidaListItems != value)
                {
                    _meidaListItems = value;
                    OnPropertyChanged(nameof(MeidaListItems));
                }
            }
        }


        private MediaDataInfo _MediaSelectItem { get; set; }

        public MediaDataInfo MediaSelectItem
        {
            get => _MediaSelectItem;
            set
            {
                if (_MediaSelectItem != value)
                {
                    _MediaSelectItem = value;
                    OnPropertyChanged(nameof(MediaSelectItem));
                }
            }

        }

        public void SetInTimeCode(string name, string timecode, int inpoint)
        {
            var item = MeidaListItems.Where(x => x.MediaId == name).FirstOrDefault();
            if (item != null)
            {
                item.InPoint = inpoint;
                item.InTimeCode = timecode;
            }
        }

        public void SetOutTimeCode(string name, string timecode, int outpoint)
        {
            var item = MeidaListItems.Where(x => x.MediaId == name).FirstOrDefault();
            if (item != null)
            {
                item.OutPoint = outpoint;
                item.OutTimeCode = timecode;
            }
        }

        public void SetDuration(string name, string timecode, int frame)
        {
            var item = MeidaListItems.Where(x => x.MediaId == name).FirstOrDefault();
            if (item != null)
            {
                item.Frame = frame;
                item.Duration = timecode;
            }
        }

        private string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public ICommand Command_PorxyPreview { get; }

        public MediaListControlViewModel(MediaListControl mediaList)
        {
            if (mediaList != null)
            {
                MeidaListItems = new ObservableCollection<MediaDataInfo>();
                MediaListControl = mediaList;
            }

            Command_PorxyPreview = new RelayCommand(PreviewProxy);
        }

        private void PreviewProxy()
        {


//#if DEBUG
//            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Mpv.Player.App\bin\Debug\net8.0-windows\Mpv.Player.App.exe";
//#else
//            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Mpv.Player.App\bin\Release\net8.0-windows\Mpv.Player.App.exe";
//#endif

            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Mpv.Player.App\bin\Debug\net8.0-windows\Mpv.Player.App.exe";
            if (MediaSelectItem != null)
            {

                //bool createdNew =false;
                //_mutex = new Mutex(true, "pv.Player.App.exe", out createdNew);

                //if (!createdNew)
                //{
                //    Process[] processes = Process.GetProcessesByName("pv.Player.App");
                //    foreach (Process process in processes)
                //    {
                //        process.Kill(); // 강제 종료
                //    }

                   

                //    // 이미 실행 중인 인스턴스 존재
                //    //System.Windows.MessageBox.Show("프로그램이 이미 실행 중입니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                //    return;
                //}

                string argList = MediaSelectItem.MediaId + " " + MediaSelectItem.Path +" "+ MediaSelectItem.Name;

                if (!string.IsNullOrEmpty(argList))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = programName,
                        Arguments = argList,
                        Verb = "runas",  // 관리자 권한 실행
                        UseShellExecute = false
                    };

                    try
                    {
                        Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

        }

        public MediaDataInfo GetFirstItem()
        {
            return MeidaListItems?.Where(c => c.State != "Error").FirstOrDefault();
        }

        public MediaDataInfo GetPlayItem()
        {
            return MeidaListItems?.Where(x => x.State == "Play").FirstOrDefault();
        }

        public MediaDataInfo GetCueItem()
        {
            return MeidaListItems?.Where(x => x.State == "Cue").FirstOrDefault();
        }


        public string GetDuration(string name)
        {
            string duration = "00:00:00:00";

            var item = MeidaListItems.Where(x => x.Name == name).FirstOrDefault();

            if (item != null)
            {
                duration = item.Duration;
            }

            return duration;
        }

        public bool isPlayFinish()
        {
            var Item = MeidaListItems.Where(c => c.State != "Error").OrderBy(c => c.Index).Last();

            if (Item != null)
            {
                return Item.State == "Play" ? true : false;
            }
            else
            {
                return false;
            }

            //var item = MeidaListItems.Where(x => x.Index == MeidaListItems.Count).FirstOrDefault();
            //return item.State == "Play" ? true : false;
        }

        public MediaDataInfo LastMedia()
        {
            return MeidaListItems.Where(c => c.State != "Error").OrderBy(c => c.Index).Last();
        }

        public bool isNextItem()
        {
            var item = MeidaListItems.Where(x => x.State == "Cue").ToList().Count();

            return item == 0 ? false : true;
        }

        public MediaDataInfo GetNextItem()
        {
            //MediaDataInfo media = null;

            MediaDataInfo media  = MeidaListItems.Where(x => x.State == "Wait").OrderBy(c => c.Index)?.FirstOrDefault();

            if(media == null)
                media = MeidaListItems.Where(x => x.State != "Error").OrderBy(c => c.Index).ToList()[0];

            return media;
        }

        public void SetMediaState(int index, string state )
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var media = MeidaListItems.Where(x => x.Index == index).FirstOrDefault();

                if(media != null)
                {
                    media.State = state;
                }

            });
        }

        public void SetMediInitState()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach(var item in MeidaListItems)
                {
                    item.State = "Wait";
                }
            });
        }


        public void SetLoopState()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in MeidaListItems)
                {
                    if(item.State == "Done")
                        item.State = "Wait";
                }
            });
        }

        
             

        public void SetMedisState(string id, string state)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var media = MeidaListItems.Where(x => x.MediaId == id).FirstOrDefault();
                media.State = state;

                if (MediaListControl.MediaView_List.ItemsSource != null)
                    CollectionViewSource.GetDefaultView(MediaListControl.MediaView_List.ItemsSource)?.Refresh();

                if (MediaListControl.MediaView_Image.ItemsSource != null)
                    CollectionViewSource.GetDefaultView(MediaListControl.MediaView_Image.ItemsSource)?.Refresh();

            });
        }


        public void CleanMedia(string listname ="")
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MeidaListItems.Clear();
            });
        }

        public void RemoveMediaItem()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MeidaListItems.Remove(MediaSelectItem);
            });
        }


        public void Addmediaitem(MediaDataInfo item)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                item.Index = MeidaListItems.Count + 1;
                MeidaListItems.Add(item);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
