using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Media.Control.App.MeidaBrowser.ViewModel;
using System.Windows.Threading;
using System.Windows.Controls;
using Mpv.Player.App;
using System.Windows.Forms.Integration;
using MaterialDesignThemes.Wpf.Converters;
using MahApps.Metro.Controls;
using Media.Control.App.MeidaBrowser.Model;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using Image = System.Windows.Controls.Image;
using wf = System.Windows.Forms; // Add this line at the top with other using directives


using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Text.Json;


namespace Media.Control.App.MeidaBrowser.Controls
{
    /// <summary>
    /// ListControl.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class MediaListControl : System.Windows.Controls.UserControl
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public MediaListControlViewModel mediaListViewModel = null;      
        public event EventHandler<object> ButtonClicked;
        public event EventHandler<MediaDataInfo> MenuItemClicked;

        private System.Windows.Point _dragStartPoint;
        private readonly List<System.Windows.Controls.Primitives.ToggleButton> _toggleButtons;

        private string MediaPath { get;set; }
        private MpvControl mpvPlayer = null;

        public double Listheight
        {
            get { return MediaView_List.Height; }
            set 
            { 
                MediaView_List.Height = value;
                MediaView_Image.Height = value;
            }
        }


        public MediaListControl()
        {
            InitializeComponent();

            _toggleButtons = new List<System.Windows.Controls.Primitives.ToggleButton>
            {
                butList,
                butImage
            };

            mediaListViewModel = new MediaListControlViewModel(this);
            this.DataContext = mediaListViewModel;

            mediaListViewModel.MeidaListStyle = ListStyle.List;
            this.butList.IsChecked = true;

            mpvPlayer= new MpvControl();

        }

        
        private void InitializeMpvPlayer(IntPtr hwnd, string path)
        {
            try
            {
                if (mpvPlayer != null) { mpvPlayer?.Close(); }
                
                mpvPlayer?.Init(hwnd);
                mpvPlayer?.Load(path);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error initializing MpvPlayer: {ex.Message}");
            }
        }


        private void CheckAudioLevels()
        {
            try
            {
                // 'audio-levels' 속성 가져오기
                //string audioLevels = mpvPlayer.API.GetPropertyString("audio-levels");
                //Console.WriteLine($"Audio Levels: {audioLevels}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving audio levels: {ex.Message}");
            }
        }


        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ToggleButton;

            if (clickedButton == null) return;

            ButtonClicked?.Invoke(this, sender); // 부모 폼에 이벤트 전달

            if(clickedButton.Name == "butList")
                mediaListViewModel.MeidaListStyle = ListStyle.List;
            else
                mediaListViewModel.MeidaListStyle = ListStyle.Image;

            // 선택된 버튼은 체크 유지, 다른 버튼 해제 방지
            foreach (var button in _toggleButtons)
            {
                if (button != clickedButton && button.IsChecked == true)
                {
                    button.IsChecked = false;
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 55, 52));
                    button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }
            }

            clickedButton.IsChecked = true; // 항상 클릭된 버튼을 활성화
            clickedButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 31));
        }


        private FrameworkElement EnterSelectCard;
        private void SetMpvMediaSetting(FrameworkElement SelectCard)
        {

            if( SelectCard == null) return;
            // 1. DataContext에서 바인딩된 데이터 가져오기
            var dataContext = SelectCard.DataContext as MediaDataInfo; // 실제 데이터 클래스에 맞게 변경
            if (dataContext == null) return;


            //if (MediaPath != dataContext.Path)
            {
                var PicPreview = VisualTreeHelperExtensions.
                    FindVisualChild<FrameworkElement>
                    (SelectCard, "PicPreview") as Border;
                if (PicPreview != null) PicPreview.Visibility = Visibility.Visible;


                // 2. Card 내부의 ImgPreview 숨기기
                var imgPreview = VisualTreeHelperExtensions.FindVisualChild<FrameworkElement>
                    (SelectCard, "ImgPreview") as Border;
                if (imgPreview != null) imgPreview.Visibility = Visibility.Hidden;

                                
                // 3. mp4Handel에 영상을 출력
                var mp4Handle = VisualTreeHelperExtensions.FindVisualChild<WindowsFormsHost>(SelectCard, "WindowsFormsHostControl");
                if (mp4Handle?.Child is System.Windows.Forms.Panel panel)
                {
                    InitializeMpvPlayer(panel.Handle, dataContext.Proxy); // Path: MP4 파일 경로
                    MediaPath = dataContext.Proxy;
                    //
                }

            }
           
        }

        private void SetMpvMediaRemove(FrameworkElement SelectCard)
        {
            if (mpvPlayer != null && SelectCard != null)
            {
                var dataContext = SelectCard.DataContext as dynamic; // 실제 데이터 클래스에 맞게 변경
                if (dataContext == null) return;

                var PicPreview = VisualTreeHelperExtensions.FindVisualChild<FrameworkElement>(SelectCard, "PicPreview") as Border;
                if (PicPreview != null) PicPreview.Visibility = Visibility.Hidden;

                var imgPreview = VisualTreeHelperExtensions.FindVisualChild<FrameworkElement>(SelectCard, "ImgPreview") as Border;
                if (imgPreview != null) imgPreview.Visibility = Visibility.Visible;

                mpvPlayer?.Stop();
            }

        }
     
        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("<--- Card Enter");
            EnterSelectCard = (sender as FrameworkElement);

        }


        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("Card Leave --->");
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMpvMediaSetting(EnterSelectCard);
            //System.Windows.Forms.MessageBox.Show("Image의 PreviewMouseLeftButtonDown 이벤트가 호출되었습니다.");
        }

        private void mp4Handel_MouseEnter(object sender, EventArgs e)
        {

        }

        private void mp4Handel_MouseLeave(object sender, EventArgs e)
        {
            SetMpvMediaRemove(EnterSelectCard);
        }

        private void mp4Handel_MouseDown(object sender, wf.MouseEventArgs e)
        {
           // mpvPlayer?.FF();
        }

        private void mp4Handel_MouseUp(object sender, wf.MouseEventArgs e)
        {
            //mpvPlayer?.Pause();
        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListView listView)
            {
                _dragStartPoint = e.GetPosition(listView);
            }

        }

        private void ListView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (sender is System.Windows.Controls.ListView listView)
                    {
                        // 이벤트의 원본에서 ListViewItem을 찾습니다.
                        System.Windows.Controls.ListViewItem listViewItem = FindAncestor<System.Windows.Controls.ListViewItem>((DependencyObject)e.OriginalSource);
                        if (listViewItem != null)
                        {
                            // ListViewItem에 바인딩된 MediaItem 객체 추출
                            Dispatcher.Invoke(() =>
                            {
                                MediaDataInfo mediaItem = listView.ItemContainerGenerator.ItemFromContainer(listViewItem) as MediaDataInfo;
                                if (mediaItem != null)
                                {
                                    try
                                    {
                                        string jsonString = JsonConvert.SerializeObject(mediaItem, Formatting.Indented);
                                        DragDrop.DoDragDrop(listViewItem, jsonString, System.Windows.DragDropEffects.Copy);
                                    }
                                    catch { }
                                }
                            });
                        }
                    }
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T desired)
                    return desired;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {

            MediaDataInfo mediaDataInfo = mediaListViewModel.SelectItem.ShallowCopy();

            mediaListViewModel.DeleteMediaList();
            MenuItemClicked?.Invoke(this, mediaDataInfo);
           

        }

        private void ButPreview_Click(object sender, RoutedEventArgs e)
        {
            mediaListViewModel.PreviewProxy();
        }
    }
}
