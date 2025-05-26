using Media.Control.App.MeidaBrowser.Controls;
using Media.Control.App.MeidaBrowser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Mpv.Player.App.Command;

using System.Numerics;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows;
using Accessibility;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;

namespace Media.Control.App.MeidaBrowser.ViewModel
{
    public enum ListStyle
    {
        List,
        Image
    }

    public class MediaListControlViewModel : INotifyPropertyChanged
    {

        private Mutex _mutex = null;
        private MediaListControl MediaListControl = null;
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


        private MediaDataInfo _SelectItem;

        public MediaDataInfo SelectItem
        {
            get => _SelectItem;
            set
            {
                if (_SelectItem != value)
                {
                    _SelectItem = value;
                    OnPropertyChanged(nameof(SelectItem));
                }
            }
        }


        public void SetInTimceCode(UpDateInPoint upDateIn)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var media = MeidaListItems.Where(x => x.MediaId == upDateIn.Id).FirstOrDefault();
                if (media != null)
                {
                    media.InTimeCode = upDateIn.InTimecode;
                    media.InPoint = upDateIn.InPoint;
                    media.Duration = upDateIn.Duration;
                    media.Frame = upDateIn.Frame;
                }
            });
        }

        public void SetOutTimceCode(UpDateOutPoint upDateOut)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var media = MeidaListItems.Where(x => x.MediaId == upDateOut.Id).FirstOrDefault();
                if (media != null)
                {
                    media.OutTimeCode = upDateOut.OutTimecode;
                    media.OutPoint = upDateOut.OutPoint;
                    media.Duration = upDateOut.Duration;
                    media.Frame = upDateOut.Frame;
                }
            });
        }


        public void SetMediaStste(string id , string state)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var media = MeidaListItems.Where(x => x.MediaId == id).FirstOrDefault();
                if (media != null)
                {
                    media.State = state;

                    media.Image = media.Image == null ? @$"{AppDomain.CurrentDomain.BaseDirectory}\Noimage.png" : media.Image;
                }

            });
        }

        //private bool _isPicture { get; set; }   

        //public bool isPicture
        //{
        //    get => _isPicture;
        //    set
        //    {
        //          _isPicture = value;
        //        OnPropertyChanged(nameof(isPicture));   
        //    }
        //}

        private bool _isImage { get; set; } 

        public bool isImage
        {
            get => _isImage;
            set
            {
                _isImage = value;
                OnPropertyChanged(nameof(isImage));
            }
        }

        private ICommand Command_PorxyPreview { get; }

        

        public MediaListControlViewModel(MediaListControl mediaList)
        {

            if (mediaList != null)
            {
                MeidaListItems = new ObservableCollection<MediaDataInfo>();
                MediaListControl = mediaList;
            }

            Command_PorxyPreview = new RelayCommand(PreviewProxy);


        }

        public void PreviewProxy()
        {


#if DEBUG
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Mpv.Player.App\bin\Debug\net8.0-windows\Mpv.Player.App.exe";
#else
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Mpv.Player.App\bin\Release\net8.0-windows\Mpv.Player.App.exe";
#endif
            if (SelectItem != null)
            {
                bool createdNew = false;
                _mutex = new Mutex(true, "pv.Player.App.exe", out createdNew);

                if (!createdNew)
                {
                    // 이미 실행 중인 인스턴스 존재
                    //System.Windows.MessageBox.Show("프로그램이 이미 실행 중입니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string argList = SelectItem.MediaId + " " + SelectItem.Path + " " + SelectItem.Name;

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


        public void SetMediaList(ObservableCollection<MediaDataInfo> media)
        {
            MeidaListItems.Clear();

            if (media != null)
            {
                MeidaListItems = media;
            }
        }

        public void DeleteMediaList()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (SelectItem != null)
                {
                    MeidaListItems.Remove(SelectItem);

                    
                }
            });
        }


        private void API_FileLoaded(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
