using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mpv.Player.App.Controls
{
    /// <summary>
    /// TimeCodeControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TimeCodeControl : UserControl
    {
        private string _inputCache = ""; // 입력 캐시

        private string _hours { get; set; } = "00";
        private string _minutes { get; set; } = "00";
        private string _seconds { get; set; } = "00";
        private string _frames { get; set; } = "00";

        public string TimeCode
        {
            get => $"{_hours}:{_minutes}:{_seconds}:{_frames}";
            set
            {
                // TimeCode 속성이 변경되면 컨트롤에 반영
                if (value.Length == 11)
                {
                    HoursTextBox.Text = value.Substring(0, 2);
                    MinutesTextBox.Text = value.Substring(3, 2);
                    SecondsTextBox.Text = value.Substring(6, 2);
                    FramesTextBox.Text = value.Substring(9, 2);

                    _hours = value.Substring(0, 2);
                    _minutes =  value.Substring(3, 2);
                    _seconds = value.Substring(6, 2);  
                    _frames = value.Substring(9, 2);
                }
            }
        }

        // UserControl 클래스 내부에 추가
        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register("Font", typeof(FontFamily), typeof(TimeCodeControl), new PropertyMetadata(new FontFamily("Segoe UI"), OnFontChanged));

        public static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor", typeof(Brush), typeof(TimeCodeControl), new PropertyMetadata(Brushes.Black, OnFontColorChanged));


        // 속성 정의
        public FontFamily Font
        {
            get => (FontFamily)GetValue(FontProperty);
            set => SetValue(FontProperty, value);
        }

        public Brush FontColor
        {
            get => (Brush)GetValue(FontColorProperty);
            set => SetValue(FontColorProperty, value);
        }


        // 속성 변경 시 호출되는 메서드
        private static void OnFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeCodeControl control)
            {
                control.UpdateFontFamily((FontFamily)e.NewValue);
            }
        }

        private static void OnFontColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeCodeControl control)
            {
                control.UpdateFontColor((Brush)e.NewValue);
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
            InitializeDefaultValues();
        }

        private void InitializeDefaultValues()
        {
            // 컨트롤 초기화 시 기본 값을 설정
            HoursTextBox.Text = "00";
            MinutesTextBox.Text = "00";
            SecondsTextBox.Text = "00";
            FramesTextBox.Text = "00";
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 숫자만 입력 가능
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
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

        private void FinalizeInput(TextBox textBox)
        {
            // 입력 값 유효성 검사 및 초기화
            if (textBox == HoursTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 23).ToString("D2");
                _hours = textBox.Text;
            }
            else if (textBox == MinutesTextBox || textBox == SecondsTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 59).ToString("D2");
                if (textBox == MinutesTextBox)
                {
                    _minutes = textBox.Text;
                }
                else
                {
                    _seconds = textBox.Text;
                }

            }
            else if (textBox == FramesTextBox)
            {
                textBox.Text = ValidateNumber(textBox.Text, 0, 29).ToString("D2"); // 프레임은 예제에서 30fps로 가정
                _frames = textBox.Text;
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
