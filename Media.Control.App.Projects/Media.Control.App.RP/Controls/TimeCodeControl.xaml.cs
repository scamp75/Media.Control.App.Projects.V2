using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;
using System.ComponentModel;

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// TimeCodeControl.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class TimeCodeControl : System.Windows.Controls.UserControl
    {
        private string _inputCache = ""; // 입력 캐시

        public static readonly DependencyProperty HHProperty =
         DependencyProperty.Register(
             nameof(HH),
             typeof(string),
             typeof(TimeCodeControl),
             new PropertyMetadata("00"));

        public string HH
        {
            get => (string)GetValue(HHProperty);
            set => SetValue(HHProperty, value);
        }

        public static readonly DependencyProperty MMProperty =
         DependencyProperty.Register(
             nameof(MM),
             typeof(string),
             typeof(TimeCodeControl),
             new PropertyMetadata("00"));

        public string MM
        {
            get => (string)GetValue(MMProperty);
            set => SetValue(MMProperty, value);
        }

        public static readonly DependencyProperty SSProperty =
         DependencyProperty.Register(
             nameof(SS),
             typeof(string),
             typeof(TimeCodeControl),
             new PropertyMetadata("00"));

        public string SS
        {
            get => (string)GetValue(SSProperty);
            set => SetValue(SSProperty, value);
        }

        public static readonly DependencyProperty FFProperty =
         DependencyProperty.Register(
             nameof(FF),
             typeof(string),
             typeof(TimeCodeControl),
             new PropertyMetadata("00"));

        public string FF
        {
            get => (string)GetValue(FFProperty);
            set => SetValue(FFProperty, value);
        }




        public static readonly DependencyProperty TimeCodeProperty =
            DependencyProperty.Register(
          nameof(TimeCode),
          typeof(string),
          typeof(TimeCodeControl),
          new PropertyMetadata("TimeCode"));


        public string TimeCode
        {
            get => $"{HH}:{MM}:{SS}:{FF}";
            set
            {
                SetValue(TimeCodeProperty, value);
                // TimeCode 속성이 변경되면 컨트롤에 반영
                if (value.Length == 11)
                {   HH = value.Substring(0, 2);
                    MM =  value.Substring(3, 2);
                    SS = value.Substring(6, 2);  
                    FF = value.Substring(9, 2);
                }
            }
        }


        

        // UserControl 클래스 내부에 추가
        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register("Font", typeof(System.Windows.Media.FontFamily), typeof(TimeCodeControl), 
                new PropertyMetadata(new System.Windows.Media.FontFamily("Segoe UI"), OnFontChanged));

        public static readonly DependencyProperty FontColorProperty = DependencyProperty.Register(
            "FontColor", typeof(System.Windows.Media.Color), typeof(TimeCodeControl), new PropertyMetadata(Colors.Black));


        // 속성 정의
        public System.Windows.Media.FontFamily Font
        {
            get => (System.Windows.Media.FontFamily)GetValue(FontProperty);
            set => SetValue(FontProperty, value);
        }

        public System.Windows.Media.Color FontColor
        {
            get { return (System.Windows.Media.Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }


        // 속성 변경 시 호출되는 메서드
        private static void OnFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeCodeControl control)
            {
                control.UpdateFontFamily((System.Windows.Media.FontFamily)e.NewValue);
            }
        }

        private static void OnFontColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeCodeControl control)
            {
                control.UpdateFontColor((System.Windows.Media.Brush)e.NewValue);
            }
        }

        // Helper 메서드
        private void UpdateFontFamily(System.Windows.Media.FontFamily newFont)
        {
            HoursTextBox.FontFamily = newFont;
            MinutesTextBox.FontFamily = newFont;
            SecondsTextBox.FontFamily = newFont;
            FramesTextBox.FontFamily = newFont;
        }

        private void UpdateFontColor(System.Windows.Media.Brush newColor)
        {
            HoursTextBox.Foreground = newColor;
            MinutesTextBox.Foreground = newColor;
            SecondsTextBox.Foreground = newColor;
            FramesTextBox.Foreground = newColor;
        }
        public TimeCodeControl()
        {
            InitializeComponent();

            this.DataContext = this;
          
            
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 숫자만 입력 가능
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                DisableSelectionChanged(textBox);
                textBox.SelectAll(); // 포커스 시 전체 선택
                EnableSelectionChanged(textBox);
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // 포커스를 잃으면 유효성 검사
            FinalizeInput((TextBox)sender);
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                if (e.Key == Key.Delete)
                {
                    _inputCache = ""; // 캐시 초기화
                    textBox.Text = "00"; // 삭제 시 초기화
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    ValidateAndFixValues(); // 완료 처리
                }
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                HandleInput(textBox);
            }
        }

        private void HandleInput(TextBox textBox)
        {
            // 입력 처리 로직
            string currentInput = textBox.Text;
            if (!Regex.IsMatch(currentInput, "^[0-9]{1,2}$"))
            {
                textBox.Text = "00"; // 유효하지 않은 입력일 경우 초기화
                _inputCache = "";
                return;
            }

            // 캐시와 현재 입력을 결합
            if (_inputCache.Length == 0)
            {
                _inputCache = currentInput;
                DisableSelectionChanged(textBox);
                textBox.Text = currentInput.PadRight(2, '0'); // 첫 입력 시 30, 40처럼 표시
                EnableSelectionChanged(textBox);
            }
            else
            {
                // 두 번째 입력으로 캐시 완성
                _inputCache += currentInput;
                DisableSelectionChanged(textBox);
                textBox.Text = _inputCache.Substring(0, 2); // 최종 2자리 반영
                EnableSelectionChanged(textBox);
                _inputCache = ""; // 캐시 초기화
            }

            textBox.SelectionStart = textBox.Text.Length; // 커서 이동
        }

        private void FinalizeInput(System.Windows.Controls.TextBox textBox)
        {
            // 입력 값 유효성 검사 및 초기화
            if (textBox == HoursTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 23).ToString("D2");
                HH = textBox.Text;
            }
            else if (textBox == MinutesTextBox || textBox == SecondsTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 59).ToString("D2");
                if (textBox == MinutesTextBox)
                {
                    MM = textBox.Text;
                }
                else
                {
                    SS = textBox.Text;
                }

            }
            else if (textBox == FramesTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 29).ToString("D2"); // 프레임은 예제에서 30fps로 가정
                FF = textBox.Text;
            }

            _inputCache = ""; // 캐시 초기화
        }

        private int ValidateNumber(string text, int min, int max)
        {
            // 숫자가 유효한 범위인지 확인
            if (int.TryParse(text, out int value) && value >= min && value <= max)
            {
                return value;
            }
            return min; // 유효하지 않을 경우 최소값으로 초기화
        }

        private void ValidateAndFixValues()
        {
            // 모든 텍스트박스 유효성 검사
            FinalizeInput(HoursTextBox);
            FinalizeInput(MinutesTextBox);
            FinalizeInput(SecondsTextBox);
            FinalizeInput(FramesTextBox);
        }

        private void DisableSelectionChanged(TextBox textBox)
        {
            textBox.SelectionChanged -= OnSelectionChanged;
        }

        private void EnableSelectionChanged(TextBox textBox)
        {
            textBox.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // SelectionChanged 이벤트가 필요하다면 여기에 처리 로직 추가
        }

    }
}
