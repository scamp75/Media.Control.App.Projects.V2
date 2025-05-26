using Media.Control.App.RP.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using static MaterialDesignThemes.Wpf.Theme;


namespace Media.Control.App.RP.Controls
{
    /// RecorderButtonControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecorderButtonControl : System.Windows.Controls.UserControl , INotifyPropertyChanged
    {

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        // 부모 폼에서 사용할 이벤트
        public event EventHandler<object> ButtonClicked;
        // 클래스 멤버로 _toggleButtons 선언
        private readonly List<System.Windows.Controls.Primitives.ToggleButton> _toggleButtons;



        public ObservableCollection<string> InputItems
        {
            get { return (ObservableCollection<string>)GetValue(InputItemsProperty); }
            set { SetValue(InputItemsProperty, value); }
        }

        // DependencyProperty 정의
        public static readonly DependencyProperty InputItemsProperty =
            DependencyProperty.Register("InputItems", typeof(ObservableCollection<string>)
                , typeof(RecorderButtonControl), new PropertyMetadata(null));



        public static readonly DependencyProperty isDurationProperty =
                 DependencyProperty.Register(
                 nameof(isDuration),
                 typeof(bool),
                 typeof(RecorderButtonControl),
                 new PropertyMetadata(false));


        public bool isDuration
        {
            get => (bool)GetValue(isDurationProperty);
            set => SetValue(isDurationProperty, value);
        }

        public static readonly DependencyProperty isStartTimeProperty =
                 DependencyProperty.Register(
                 nameof(isStartTime),
                 typeof(bool),
                 typeof(RecorderButtonControl),
                 new PropertyMetadata(false));


        public bool isStartTime
        {
            get => (bool)GetValue(isStartTimeProperty);
            set => SetValue(isStartTimeProperty, value);
        }


        public static readonly DependencyProperty isStopTimeProperty =
                DependencyProperty.Register(
                nameof(isStopTime),
                typeof(bool),
                typeof(RecorderButtonControl),
                new PropertyMetadata(false));


        public bool isStopTime
        {
            get => (bool)GetValue(isStopTimeProperty);
            set => SetValue(isStopTimeProperty, value);
        }


        public static readonly DependencyProperty SelectInputProperty =
          DependencyProperty.Register(
          nameof(SelectInput),
          typeof(string),
          typeof(RecorderButtonControl),
          new PropertyMetadata(""));


        public string SelectInput
        {
            get => (string)GetValue(SelectInputProperty);
            set => SetValue(SelectInputProperty, value);
        }


       public static readonly DependencyProperty ControlHeightProperty =
       DependencyProperty.Register(
       nameof(controlHeight),
       typeof(double),
       typeof(RecorderButtonControl),
       new PropertyMetadata(0.0));


        public double controlHeight
        {
            get => (double)GetValue(ControlHeightProperty);
            set => SetValue(ControlHeightProperty, value);
        }


        public double ControlHeight
        {
            get => controlHeight;
            set
            {
                controlHeight = value;
                OnPropertyChanged(nameof(ControlHeight));
            }
        }

        public RecorderButtonControl()
        {
            InitializeComponent();

            // 모든 ToggleButton을 리스트에 추가
            _toggleButtons = new List<System.Windows.Controls.Primitives.ToggleButton>
                {
                   ButRecord,
                   ButStop,
                   ButPrepared
                };

            isDuration = false;
            isStartTime = false;
            isStopTime = false;

            ControlHeight = 36;
            butOption.IsChecked = false;
        }


        public void SetButtonEnable(string state)
        {

            ButRecord.IsEnabled = false;
            ButStop.IsEnabled = false;
            ButPrepared.IsEnabled = false;

            switch (state)
            {
                case "Record":
                    ButStop.IsEnabled = true;
                    break;
                case "Pause":
                    ButPrepared.IsEnabled = true;
                    ButPrepared.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    break;
                case "Prepared":
                    ButRecord.IsEnabled = true;
                    ButStop.IsEnabled = true;
                    break;
            }

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ToggleButton;

            if (clickedButton == null) return;

            ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달


            // 선택된 버튼은 체크 유지, 다른 버튼 해제 방지
            foreach (var button in _toggleButtons)
            {
                if (button != clickedButton && button.IsChecked == true)
                {
                    button.IsChecked = false;
                    button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(206, 206, 206));
                    //button.Foreground = new SolidColorBrush(Color;
                }
            }


            clickedButton.IsChecked = true; // 항상 클릭된 버튼을 활성화
            if(clickedButton.Name == "ButRecord")
                clickedButton.Foreground = new SolidColorBrush(Colors.Red);
            else if (clickedButton.Name == "ButPrepared")
                clickedButton.Foreground = new SolidColorBrush(Colors.GreenYellow);
            else 
                clickedButton.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 249, 225));

            //clickedButton.Foreground = new SolidColorBrush(Colors.WhiteSmoke);



        }

        private void ChkStop_checked(object sender, RoutedEventArgs e)
        {
            if(ChkStop.IsChecked == true)
            {
                StopTimeAt.SelectedDateTime = DateTime.Now.AddMinutes(1);
            }
            else
            {
                StopTimeAt.SelectedDateTime = null;
            }
        }

        private void ChkStart_checked(object sender, RoutedEventArgs e)
        {
            if(ChkStart.IsChecked == true)
            {
                StartTimeAt.SelectedDateTime = DateTime.Now.AddMinutes(1);
            }
            else
            {
                StartTimeAt.SelectedDateTime = null;
            }
        }

        private void InputList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e); // 부모 폼에 이벤트 전달
            
        }

        private void butOption_Click(object sender, RoutedEventArgs e)
        {
            if(butOption.IsChecked == true)
                ControlHeight = 100;
            else
                ControlHeight = 36;
        }


        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
