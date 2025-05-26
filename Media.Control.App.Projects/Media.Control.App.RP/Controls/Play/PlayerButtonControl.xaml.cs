
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Media.Control.App.RP.Command;
using static MaterialDesignThemes.Wpf.Theme;

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// ButtonControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayerButtonControl : System.Windows.Controls.UserControl //, INotifyPropertyChanged
    {
        public event EventHandler<object> ButtonClicked;

        private readonly List<System.Windows.Controls.Primitives.ToggleButton> _toggleButtons;
        private readonly List<System.Windows.Controls.Button> _buttonButtons;


        public PlayerButtonControl()
        {
            InitializeComponent();
            // 모든 ToggleButton을 리스트에 추가
            _toggleButtons = new List<System.Windows.Controls.Primitives.ToggleButton>
            {
               ButEnd,
               ButFoward,
               ButFrsit,
               ButPlay,
               ButReWind,
               ButEnd,
               ButEject,

               ButBack10Frame,
               ButBack1Frame,
               ButForward10Frame,
               ButForward1Frame,
            };


            _buttonButtons = new List<System.Windows.Controls.Button>
            {
              

            };


            IsAtion(false);
         
        }


        public static readonly DependencyProperty IconPlayStopProperty =
          DependencyProperty.Register("IconPlayStop",
          typeof(string),
          typeof(PlayergroupControl),
          new PropertyMetadata(""));

        public string IconPlayStop
        {
            get => (string)GetValue(IconPlayStopProperty);
            set => SetValue(IconPlayStopProperty, value);
        }

        #region Commands

        public static readonly DependencyProperty FrsitCommandProperty =
        DependencyProperty.Register(nameof(CommandFrsit), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandFrsit
        {
            get => (ICommand)GetValue(FrsitCommandProperty);
            set => SetValue(FrsitCommandProperty, value);
        }


        public static readonly DependencyProperty RwCommandProperty =
        DependencyProperty.Register(nameof(CommandRw), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandRw
        {
            get => (ICommand)GetValue(RwCommandProperty);
            set => SetValue(RwCommandProperty, value);
        }


        public static readonly DependencyProperty B10FrameCommandProperty =
        DependencyProperty.Register(nameof(CommandB10Frame), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandB10Frame
        {
            get => (ICommand)GetValue(B10FrameCommandProperty);
            set => SetValue(B10FrameCommandProperty, value);
        }

        

        public static readonly DependencyProperty B1FrameCommandProperty =
       DependencyProperty.Register(nameof(CommandB1Frame), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandB1Frame
        {
            get => (ICommand)GetValue(B1FrameCommandProperty);
            set => SetValue(B1FrameCommandProperty, value);
        }

        public static readonly DependencyProperty PlayStopCommandProperty =
       DependencyProperty.Register(nameof(CommandPlayStop), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandPlayStop
        {
            get => (ICommand)GetValue(PlayStopCommandProperty);
            set 
            {
                
                SetValue(PlayStopCommandProperty, value); 
            }
        }

        public static readonly DependencyProperty F1FrameCommandProperty =
       DependencyProperty.Register(nameof(CommandF1Frame), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandF1Frame
        {
            get => (ICommand)GetValue(F1FrameCommandProperty);
            set => SetValue(F1FrameCommandProperty, value);
        }

        public static readonly DependencyProperty F10FrameCommandProperty =
        DependencyProperty.Register(nameof(CommandF10Frame), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandF10Frame
        {
            get => (ICommand)GetValue(F10FrameCommandProperty);
            set => SetValue(F10FrameCommandProperty, value);
        }

        public static readonly DependencyProperty FFCommandProperty =
        DependencyProperty.Register(nameof(CommandFF), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandFF
        {
            get => (ICommand)GetValue(FFCommandProperty);
            set => SetValue(FFCommandProperty, value);
        }

        public static readonly DependencyProperty EndCommandProperty =
        DependencyProperty.Register(nameof(CommandEnd), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandEnd
        {
            get => (ICommand)GetValue(EndCommandProperty);
            set => SetValue(EndCommandProperty, value);
        }

        public static readonly DependencyProperty SaveCommandProperty =
       DependencyProperty.Register(nameof(CommandSave), typeof(ICommand), typeof(PlayerButtonControl), new PropertyMetadata(null));

        public ICommand CommandSave
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        #endregion

        //private bool _playChecked { get; set; } = false;
        //public bool PlayChecked
        //{
        //    get => _playChecked;
        //    set
        //    {
        //        if (_playChecked != value)
        //        {
        //            _playChecked = value;
        //            OnPropertyChanged(nameof(PlayChecked));
        //        }
        //    }
        //}

        public void IsAtion(bool b)
        {
            this.IsEnabled = b;

            //foreach (var t in _toggleButtons)
            //{
            //    t.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            //}


        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ToggleButton;

            if (clickedButton == null) return;
                      
            ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달

            // 선택된 버튼은 체크 유지, 다른 버튼 해제 방지
            foreach (var button in _toggleButtons)
            {
                button.IsChecked = false;
                //if (button != clickedButton && button.IsChecked == true)
                //{
                //    button.IsChecked = false;
                //}
            }

            if (clickedButton.Name == "ButFrsit"
                || clickedButton.Name == "ButReWind"
                || clickedButton.Name == "ButFoward"
                || clickedButton.Name == "ButEnd"
                || clickedButton.Name == "ButEnd")
            clickedButton.IsChecked = true;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ToggleButton;

            if (clickedButton == null) return;

            // 선택된 버튼은 체크 유지, 다른 버튼 해제 방지
            foreach (var button in _toggleButtons)
            {
                if (button != clickedButton && button.IsChecked == true)
                {
                    button.IsChecked = false;
                    //button.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 55, 52));
                }
            }

            ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달

        }

        private void ButFrsit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void Buttton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }


        // INotifyPropertyChanged 구현
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }

}
