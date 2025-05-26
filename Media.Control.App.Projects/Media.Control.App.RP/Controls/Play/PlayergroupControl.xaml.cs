using Media.Control.App.RP.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// PlayergroupControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayergroupControl : System.Windows.Controls.UserControl,INotifyPropertyChanged
    {
        public event EventHandler<object> ButtonClicked;
        private readonly List<System.Windows.Controls.Button> _toggleButtons;

        public ObservableCollection<string> PlayLists
        {
            get { return (ObservableCollection<string>)GetValue(PlayLIstsProperty); }
            set { SetValue(PlayLIstsProperty, value); }
        }

        public static readonly DependencyProperty PlayLIstsProperty =
           DependencyProperty.Register("PlayLists", typeof(ObservableCollection<string>)
               , typeof(PlayergroupControl), new PropertyMetadata(null));


       public static readonly DependencyProperty SelectPlayListProperty =
            DependencyProperty.Register(
            nameof(SelectPlayList),
            typeof(string),
            typeof(PlayergroupControl),
            new PropertyMetadata(""));
        public string SelectPlayList
        {
            get => (string)GetValue(SelectPlayListProperty);
            set => SetValue(SelectPlayListProperty, value);
        }


        public static readonly DependencyProperty TotalTimeCodeProperty =
            DependencyProperty.Register("totalTimeCode",
            typeof(string),
            typeof(PlayergroupControl), 
            new PropertyMetadata(""));

        public string totalTimeCode
        {
            get=> (string)GetValue(TotalTimeCodeProperty);
            set=> SetValue(TotalTimeCodeProperty, value);
        }

        public string TotalTimeCode
        {
            get => totalTimeCode;
            set
            {
                totalTimeCode = value;
                OnPropertyChanged(nameof(TotalTimeCode));
            }
        }

        public static readonly DependencyProperty TotalFrameProperty =
             DependencyProperty.Register("TotalFrame",
            typeof(int),
            typeof(PlayergroupControl),
            new PropertyMetadata(0));

        public int TotalFrame
        {
            get => (int)GetValue(TotalFrameProperty);
            set => SetValue(TotalFrameProperty, value);
        }


        public string _ListPlayContent;
        public string ListPlayContent
        {
            get => _ListPlayContent;
            set
            {
                _ListPlayContent = value;
                OnPropertyChanged(nameof(ListPlayContent));
            }
        }

        public bool _isLopp;

        public bool isLopp
        {
            get => _isLopp;
            set
            {
                _isLopp = value;
                OnPropertyChanged(nameof(isLopp));
            }
        }


        private string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public PlayergroupControl()
        {
            InitializeComponent();
            _toggleButtons = new List<System.Windows.Controls.Button>
            {
                butClean,
                butDelete,
                butNext,
                butListPlay,
                butReCue
            };

            foreach (var button in _toggleButtons)
            {
               button.IsEnabled = false;
            }

            PlayLists = new ObservableCollection<string>{};

            string path = appPath + @"\PlayList";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                File.Create(path + @"\PlayList.json");
            }
            
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles())
            {
                PlayLists.Add(System.IO.Path.GetFileNameWithoutExtension(file.Name));
            }

           // txtTimecode.Text = "00:00:00;00";
            TotalTimeCode = "00:00:00;00";

            ListPlayContent = "Play";
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Button;

            if (clickedButton == null) return;

            ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달

        }

        public void SetButtonEnabled(bool isEnabled)
        {
            foreach (var button in _toggleButtons)
            {
                button.IsEnabled = isEnabled;
            }
        }

        public void SetButtonEnabled(string buttonName, bool isEnabled)
        {
            foreach (var button in _toggleButtons)
            {
                if (button.Name == buttonName)
                    button.IsEnabled = isEnabled;
            }
        }


        public void SetDoneButtonEnabled()
        {

            foreach (var button in _toggleButtons)
            {
                if (button.Name == "butReCue")
                    button.IsEnabled = true;
                else if (button.Name == "butListPlay")
                    button.IsEnabled = false;
                else if (button.Name == "butNext")
                    button.IsEnabled = true;
                else if (button.Name == "butDelete")
                    button.IsEnabled = true;
                else if (button.Name == "butClean")
                    button.IsEnabled = true;
            }
        }

        public void SetButtonEnabled(string state)
        {
            foreach (var button in _toggleButtons)
            {
               button.IsEnabled = false;
            }

            if(state == "Play")
            {
                this.butListPlay.IsEnabled = true;
                this.butDelete.IsEnabled = true;
            }
            else if(state == "Stop")
            {
                this.butNext.IsEnabled = true;
                this.butReCue.IsEnabled = true;
                this.butClean.IsEnabled = true; 
            }
            else if (state == "Cue")
            {
                this.butNext.IsEnabled = true;
                this.butListPlay.IsEnabled = true;
                this.butDelete.IsEnabled = true;
            }
        }


        private void PlayListSave_Click(object sender, RoutedEventArgs e)
        {
            PlayListControl playListControl = new PlayListControl();

            
            // UserControl이 속한 Window를 가져옵니다.
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                playListControl.Owner = parentWindow;
            }
            else
            {
                // Owner를 찾지 못한 경우 CenterScreen으로 강제 지정할 수 있습니다.
                playListControl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            bool? result = playListControl.ShowDialog();

            if (result == true)
            {
                if (!PlayLists.Contains(playListControl.ControlViewModel.PlayListName))
                {
                    string path = appPath + @"\PlayList";
                    File.Create(path + $@"\{playListControl.ControlViewModel.PlayListName}.json");

                    PlayLists.Add(playListControl.ControlViewModel.PlayListName);
                    this.SelectPlayList = playListControl.ControlViewModel.PlayListName;
                }

                //var clickedButton = sender as System.Windows.Controls.Button;
                //if (clickedButton == null) return;
                
                //ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달

            }
        }

        private void PlayListDelet_Click(object sender, RoutedEventArgs e)
        {

            if(File.Exists(appPath + $@"\PlayList\{SelectPlayList}.json"))
            {
                File.Delete(appPath + $@"\PlayList\{SelectPlayList}.json");
            }

            PlayLists.Remove(SelectPlayList);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
