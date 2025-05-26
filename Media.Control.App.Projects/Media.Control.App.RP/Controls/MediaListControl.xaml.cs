using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Media.Control.App.RP.Model;
using Media.Control.App.RP.ViewModel;
using Newtonsoft.Json;
using Mpv.Player.App;
using System.Drawing;
using Microsoft.VisualBasic.Devices;
using Point = System.Windows.Point;
using System.Diagnostics;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Media.Control.App.RP.Model.Logger;
using System.Windows.Forms;


namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// ListControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MediaListControl : System.Windows.Controls.UserControl
    {

        private Point _dragStartPoint;

        private string _ChannelType;
        public string ChannelType 
        {
            get { return _ChannelType; }
            set
            {
                if(value == "Player")
                {
                    ButPreview.Visibility = Visibility.Collapsed;
                    
                }
                else
                {
                    ButPreview.Visibility = Visibility.Visible;
                }

                _ChannelType = value;

            }
        } 
        public MediaListControlViewModel mediaListViewModel = null;      
        public event EventHandler<object> ButtonClicked;

        public event EventHandler<object> MediaListEvent;

        private readonly List<System.Windows.Controls.Primitives.ToggleButton> _toggleButtons;

        public double Listheight
        {
            get { return MediaView_List.Height; }
            set 
            {
                if (!double.IsNaN(value) && value > 0)
                {
                    MediaView_List.Height = value;
                    MediaView_Image.Height = value;
                }
            }
        }

        private string MediaPath { get; set; }
        private MpvControl mpvPlayer = null;

        private bool wasMouseOver = false;
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

            mpvPlayer = new MpvControl();

            CompositionTarget.Rendering += (s, e) =>
            {
                if (IsMouseOver && !wasMouseOver)
                {
                    wasMouseOver = true;
                }
                else if (!IsMouseOver && wasMouseOver)
                {
                    wasMouseOver = false;
                }
            };
        }
     
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
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

      
        private void mp4Handel_MouseLeave(object sender, EventArgs e)
        {
            SetMpvMediaRemove(EnterSelectCard);
        }

        private FrameworkElement EnterSelectCard;
        private void SetMpvMediaSetting(FrameworkElement SelectCard)
        {

            if (SelectCard == null) return;
            // 1. DataContext에서 바인딩된 데이터 가져오기
            var dataContext = SelectCard.DataContext as MediaDataInfo; // 실제 데이터 클래스에 맞게 변경
            if (dataContext == null) return;


            //if (MediaPath != dataContext.Proxy)
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
                    InitializeMpvPlayer(panel.Handle, dataContext.Path); // Path: MP4 파일 경로
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

        private void ImgPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMpvMediaSetting(EnterSelectCard);
        }

        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EnterSelectCard = (sender as FrameworkElement);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Listheight = this.Height - 80;
        }

        private void MediaView_List_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            // 드래그된 데이터가 예상하는 형식(예: MediaItem)이 아니면 아무 동작도 하지 않음

            if (e.Data.GetDataPresent(typeof(MediaDataInfo)))  // MediaItem은 데이터 타입(사용하는 타입으로 변경)
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent(typeof(string)))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }

        private void MediaView_List_Drop(object sender, System.Windows.DragEventArgs e)
        {
            try
            {
                ListView listView = sender as ListView;
                var items = listView.ItemsSource as ObservableCollection<MediaDataInfo>;
                if (items == null)
                    return;

                MediaDataInfo droppedData = null;


                if (e.Effects == System.Windows.DragDropEffects.Move)
                {

                    if (e.Data.GetDataPresent(typeof(MediaDataInfo)))
                    {
                        droppedData = e.Data.GetData(typeof(MediaDataInfo)) as MediaDataInfo;
                    }
                }
                else if (e.Effects == System.Windows.DragDropEffects.Copy)
                {

                    if (e.Data.GetDataPresent(typeof(MediaDataInfo2)))
                    {
                        droppedData = e.Data.GetData(typeof(MediaDataInfo)) as MediaDataInfo;
                    }
                    else if (e.Data.GetDataPresent(typeof(string)))
                    {
                        string dataStr = e.Data.GetData(typeof(string)) as string;
                        var jsonData = JsonConvert.DeserializeObject<MediaDataInfo2>(dataStr);
                        droppedData = new MediaDataInfo(items.Count + 1, jsonData.MediaId, jsonData.Image, jsonData.Name, jsonData.Duration,
                                                        jsonData.Frame, jsonData.CreatDate, jsonData.InPoint, jsonData.InTimeCode,
                                                        jsonData.OutPoint, jsonData.OutTimeCode, jsonData.Creator,
                                                        jsonData.Type, jsonData.Proxy, jsonData.Path, jsonData.Fps, jsonData.Des, "Wait");  // MediaDataInfo 생성자에 string 인자 사용
                    }
                }


                #region
                if (droppedData == null)
                    return;

                // ListView내 드롭 위치 파악
                Point pos = e.GetPosition(listView);
                var hitTestResult = VisualTreeHelper.HitTest(listView, pos);
                ListViewItem targetItem = hitTestResult != null ? FindAncestor<ListViewItem>(hitTestResult.VisualHit) : null;

                // 기본 삽입 위치는 리스트 끝
                int insertIndex = items.Count;
                if (targetItem != null)
                {
                    MediaDataInfo targetData = listView.ItemContainerGenerator.ItemFromContainer(targetItem) as MediaDataInfo;
                    if (targetData != null)
                    {
                        insertIndex = items.IndexOf(targetData);

                        // 내부 드래그인 경우: 원래 위치와 비교하여 방향에 따라 강제 설정
                        if (items.Contains(droppedData))
                        {
                            int oldIndex = items.IndexOf(droppedData);
                            if (oldIndex < insertIndex)
                            {
                                // 아래로 드래그 → 항상 타겟 바로 아래에 위치 (인덱스 + 1)
                                insertIndex = insertIndex + 1;
                            }
                            // oldIndex > insertIndex 인 경우는 위로 드래그하므로, 그대로 타겟의 인덱스에 삽입 (즉, 타겟 위)
                        }
                        else
                        {
                            // 외부 드래그인 경우: 드롭된 위치의 Y 좌표를 기준으로 결정
                            Point posInTarget = e.GetPosition(targetItem);
                            if (posInTarget.Y > targetItem.ActualHeight / 2)
                            {
                                insertIndex++;
                            }
                        }
                    }
                }

                // 내부 드래그인 경우, 먼저 기존 항목 제거 (목록에서 제거하면 인덱스가 바뀌므로 이를 보정)
                if (items.Contains(droppedData))
                {
                    int oldIndex = items.IndexOf(droppedData);
                    if (oldIndex < insertIndex)
                    {
                        items.RemoveAt(oldIndex);
                        insertIndex--; // 제거 후 인덱스가 한 칸 당겨짐
                    }
                    else
                    {
                        items.RemoveAt(oldIndex);
                    }
                }

                items.Insert(insertIndex, droppedData);

                int count = 1;
                // 재정렬 결과 확인
                items.ToList().ForEach
                    (x =>
                    {
                        Debug.WriteLine($"index : {x.Index}  {x.Name}");
                        x.Index = count;

                        ++count;
                    }
                );

                MediaListEvent(true, droppedData);
                #endregion
            
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error during drop: {ex.Message}");
            }

        }

        private void MediaView_List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 마우스 클릭 시작 위치 기록
            _dragStartPoint = e.GetPosition(null);
            isDragging= true;

        }

        private bool isDragging = false;
        private string  SendJsonString = string.Empty;

        private void MediaView_List_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPos;

                // 마우스 왼쪽 버튼이 눌린 상태이고 최소 드래그 거리를 초과했으면 드래그 시작
                if (e.LeftButton == MouseButtonState.Pressed &&
                   (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
                    // 마우스 이벤트의 원본에서 ListViewItem 찾기
                    System.Windows.Controls.ListViewItem listViewItem
                        = FindAncestor<System.Windows.Controls.ListViewItem>((DependencyObject)e.OriginalSource);

                    if (listViewItem == null)
                        return;

                    // ListViewItem에 바인딩된 데이터(예: MediaItem) 가져오기
                    var mediaItem = (MediaDataInfo)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);


                    if (mediaItem == null)
                        return;

                    if (ChannelType == "Player")
                    {
                        var data = new System.Windows.DataObject();
                        data.SetData(typeof(MediaDataInfo), mediaItem);

                        DragDrop.DoDragDrop(listViewItem, data, System.Windows.DragDropEffects.Move);
                    }
                    else
                    {
                        SendJsonString = JsonConvert.SerializeObject(mediaItem, Formatting.Indented);
                        DragDrop.DoDragDrop(listViewItem, SendJsonString, System.Windows.DragDropEffects.Copy);

                    }

                    
  
                    
                }
            }
            catch (Exception ex)
            {
               
            }   
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                    return (T)current;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            mediaListViewModel.RemoveMediaItem();
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            mediaListViewModel.CleanMedia();
        }


        private void MediaView_List_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

      
    }
}
