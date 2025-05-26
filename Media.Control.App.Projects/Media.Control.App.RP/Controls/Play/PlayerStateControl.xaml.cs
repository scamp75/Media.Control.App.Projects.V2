using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;
using Media.Control.App.RP.Command;
using Media.Control.App.RP.Model;
using System.Diagnostics;
using System.Windows.Threading;

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// PlayerStateControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayerStateControl : System.Windows.Controls.UserControl
    {
        #region commands


        public static readonly DependencyProperty JogShuttleValueProperty =
          DependencyProperty.Register(
          nameof(JogShuttleValue),
          typeof(string),
          typeof(PlayerStateControl),
          new PropertyMetadata("x1"));

        public string JogShuttleValue
        {
            get => (string)GetValue(JogShuttleValueProperty);
            set => SetValue(JogShuttleValueProperty, value);
        }

        public static readonly DependencyProperty ButJogShuttleNameProperty =
          DependencyProperty.Register(
          nameof(ButJogShuttleName),
          typeof(string),
          typeof(PlayerStateControl),
          new PropertyMetadata(""));

        public string ButJogShuttleName
        {
            get => (string)GetValue(ButJogShuttleNameProperty);
            set => SetValue(ButJogShuttleNameProperty, value);
        }


        public static readonly DependencyProperty PlayerStateProperty =
          DependencyProperty.Register(
          nameof(PlayerState),
          typeof(string),
          typeof(PlayerStateControl),
          new PropertyMetadata("PlayerState"));

        public string PlayerState
        {
            get => (string)GetValue(PlayerStateProperty);
            set => SetValue(PlayerStateProperty, value);
        }

        public static readonly DependencyProperty CurrentTimecodeProperty =
          DependencyProperty.Register(
          nameof(CurrentTimecode),
          typeof(string),
          typeof(PlayerStateControl),
          new PropertyMetadata("CurrentTimecode"));

        public string CurrentTimecode
        {
            get => (string)GetValue(CurrentTimecodeProperty);
            set => SetValue(CurrentTimecodeProperty, value);
        }

        public static readonly DependencyProperty RemainTimecodeProperty =
          DependencyProperty.Register(
          nameof(RemainTimecode),
          typeof(string),
          typeof(PlayerStateControl),
          new PropertyMetadata("RemainTimecode"));

        public string RemainTimecode
        {
            get => (string)GetValue(RemainTimecodeProperty);
            set => SetValue(RemainTimecodeProperty, value);
        }



        public static readonly DependencyProperty MaxDurationProperty =
        DependencyProperty.Register(
        nameof(MaxDuration),
        typeof(double),
        typeof(PlayerStateControl),
        new PropertyMetadata(0.0));

        public double MaxDuration
        {
            get => (double)GetValue(MaxDurationProperty);
            set => SetValue(MaxDurationProperty, value);
        }

        public static readonly DependencyProperty OutTimeCodeValueProperty =
           DependencyProperty.Register(
           nameof(OutTimeCodeValue),
           typeof(string),
           typeof(PlayerStateControl),
           new PropertyMetadata("OutTimeCode"));

        public string OutTimeCodeValue
        {
            get => (string)GetValue(OutTimeCodeValueProperty);
            set 
            { 
                
                SetValue(OutTimeCodeValueProperty, value);
                this.OutTimeCode.TimeCode = value;
            }
        }

        public static readonly DependencyProperty InTimeCodeValueProperty =
        DependencyProperty.Register(
        nameof(InTimeCodeValue),
        typeof(string),
        typeof(PlayerStateControl),
        new PropertyMetadata("InTimeCodeValue"));

        public string InTimeCodeValue
        {
            get => (string)GetValue(InTimeCodeValueProperty);
            set 
            {
                SetValue(InTimeCodeValueProperty, value);
                this.InTimeCode.TimeCode = value;
            }
        }


        public static readonly DependencyProperty InPointProperty =
        DependencyProperty.Register(
        nameof(InPoint),
        typeof(double),
        typeof(PlayerStateControl),
        new PropertyMetadata(0.0));

        public double InPoint
        {
            get => (double)GetValue(InPointProperty);
            set
            {
                SetValue(InPointProperty, value);

            }
        }

        public static readonly DependencyProperty OutPointProperty =
          DependencyProperty.Register(
          nameof(OutPoint),
          typeof(double),
          typeof(PlayerStateControl),
          new PropertyMetadata(0.0));

        public double OutPoint
        {
            get => (double)GetValue(OutPointProperty);
            set
            {
                SetValue(OutPointProperty, value);

            }
        }



        public static readonly DependencyProperty SliderValueProperty =
        DependencyProperty.Register(
        nameof(SliderValue),
        typeof(double),
        typeof(PlayerStateControl),
        new PropertyMetadata(0.0));

        public double SliderValue
        {
            get => (double)GetValue(SliderValueProperty);
            set => SetValue(SliderValueProperty, value);
        }

        public static readonly DependencyProperty CommandInGoProperty =
        DependencyProperty.Register(nameof(CommandInGo), typeof(ICommand),
            typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandInGo
        {
            get => (ICommand)GetValue(CommandInGoProperty);
            set => SetValue(CommandInGoProperty, value);
        }


        public static readonly DependencyProperty CommandInProperty =
        DependencyProperty.Register(nameof(CommandIn), typeof(ICommand),
            typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandIn
        {
            get => (ICommand)GetValue(CommandInProperty);
            set => SetValue(CommandInProperty, value);
        }


        public static readonly DependencyProperty CommandInDeleteProperty =
        DependencyProperty.Register(nameof(CommandInDelete), typeof(ICommand),
            typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandInDelete
        {
            get => (ICommand)GetValue(CommandInDeleteProperty);
            set => SetValue(CommandInDeleteProperty, value);
        }

        public static readonly DependencyProperty CommandOutGoProperty =
        DependencyProperty.Register(nameof(CommandOutGo), typeof(ICommand),
            typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandOutGo
        {
            get => (ICommand)GetValue(CommandOutGoProperty);
            set => SetValue(CommandOutGoProperty, value);
        }


        public static readonly DependencyProperty CommandOutProperty =
        DependencyProperty.Register(nameof(CommandOut), typeof(ICommand),
            typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandOut
        {
            get => (ICommand)GetValue(CommandOutProperty);
            set => SetValue(CommandOutProperty, value);
        }

        public static readonly DependencyProperty CommandOutDeleteProperty =
        DependencyProperty.Register(nameof(CommandOutDelete), typeof(ICommand),
         typeof(PlayerStateControl), new PropertyMetadata(null));

        public ICommand CommandOutDelete
        {
            get => (ICommand)GetValue(CommandOutDeleteProperty);
            set => SetValue(CommandOutDeleteProperty, value);
        }

        public static readonly DependencyProperty SliderMinimumProperty =
        DependencyProperty.Register(
        nameof(SliderMinimum),
        typeof(double),
        typeof(PlayerStateControl),
        new PropertyMetadata(0.0));

        public double SliderMinimum
        {
            get => (double)GetValue(SliderMinimumProperty);
            set => SetValue(SliderMinimumProperty, value);
        }

        public static readonly DependencyProperty SliderMaximumProperty =
        DependencyProperty.Register(
        nameof(SliderMaximum),
        typeof(double),
        typeof(PlayerStateControl),
        new PropertyMetadata(0.0));

        public double SliderMaximum
        {
            get => (double)GetValue(SliderMaximumProperty);
            set => SetValue(SliderMaximumProperty, value);
        }

        public static readonly DependencyProperty CountTickProperty =
       DependencyProperty.Register(
       nameof(CountTick),
       typeof(double),
       typeof(PlayerStateControl),
       new PropertyMetadata(0.0));

        public double CountTick
        {
            get => (double)GetValue(CountTickProperty);
            set => SetValue(CountTickProperty, value);
        }

        //CommandJogShuttle
        #endregion

        public event EventHandler<object> ButtonClicked;
        public event EventHandler<object> TriangleButtonClicked;
        public event EventHandler<object> ProgressMouseclicked;
        public event EventHandler<object> SilderValueChanged;

        private readonly List<System.Windows.Controls.Button> _Buttons;
        private List<Marker> markerList = new List<Marker>();
        private int rightTriangleCount = 0;
        private const int MaxRightTriangles = 2;


        public PlayerStateControl()
        {
            InitializeComponent();
            PlayerState = "Pause";
            CurrentTimecode = "00:00:00;00";
            RemainTimecode = "00:00:00;00";

            _Buttons = new List<System.Windows.Controls.Button>
            {
                butGotoIn,
                butGotoOut,
                butInPoint,
                butOutPoint,
                butInDelete,
                butOutDelete

            };
            InitSetting();
            IsAtion(false);
        }

        public void InitSetting()
        {
            InPoint = 0;
            InTimeCode.TimeCode = "00:00:00:00";
            OutPoint = 0;
            OutTimeCode.TimeCode = "00:00:00:00";
            SliderValue = 0;
            CurrentTimecode = "00:00:00;00";
            RemainTimecode = "00:00:00;00";
            MaxDuration = 0;

            AllDeleteMarkers();
            InitJogShuttle();
        }

        public void InitJogShuttle()
        {

            ButJogShuttleName = "Shuttle";
            JogShuttleSlider.Minimum = -10;
            JogShuttleSlider.Maximum = 10;
            JogShuttleSlider.Value = 0;
            CountTick = 2;
        }

        public void IsAtion(bool b)
        {
            this.IsEnabled = b;
        }

        public int GetMarkerCount()
        {
            return markerList.Count;
        }

        public int GetDuration()
        {
            var value = markerList.Last().Duration;

            return (int)value;
        }

        public void UpdateMarkerValue(int index, double value)
        {
            markerList.Where(m => m.Index == index)
                .ToList()
                .ForEach(m =>
                {
                    m.Value = value;
                    Canvas.SetLeft(m.TriangleButton, value);
                    m.MarkerLine.X1 = value;
                    m.MarkerLine.X2 = value;
                });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Button;

            if (clickedButton == null) return;

            ButtonClicked?.Invoke(this, clickedButton.Name); // 부모 폼에 이벤트 전달


        }

        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(DarkProgressBar);
            double value = (pos.X / DarkProgressBar.ActualWidth) * (DarkProgressBar.Maximum - DarkProgressBar.Minimum) + DarkProgressBar.Minimum;

            ProgressMouseclicked(this, value);
        }

     

        // 붉은 실선 그리기
        private void DrawMarkerLine(double value, MarkerShape shape, double duration =0, TriangleDirection direction = TriangleDirection.Left)
        {
            double valueXPos = (value - DarkProgressBar.Minimum) / (DarkProgressBar.Maximum - DarkProgressBar.Minimum) * DarkProgressBar.ActualWidth;

            // 마커 버튼 설정
            var triangleButton = new System.Windows.Controls.Button
            {
                Width = 11,
                Height = 11,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                ToolTip = $"위치: {value:F1}",
                Background = System.Windows.Media.Brushes.Transparent
            };

            triangleButton.Click += (s, e) =>
            {
                TriangleButtonClicked(this, value);
            };

            if (shape == MarkerShape.Equilateral)
            {
                triangleButton.Template = CreateEquilateralTriangleTemplate();
            }
            else
            {
                triangleButton.Template = CreateRightTriangleTemplate(direction);
            }

            // 삼각형 위치: 항상 위쪽
            Canvas.SetTop(triangleButton, -triangleButton.Height);

            // 기본 위치
            double x = valueXPos ;

            // Fixed line  
            System.Windows.Media.Brush brushes = System.Windows.Media.Brushes.Red;

            // 버튼 배치 (삼각형 중심 기준)
            if (shape == MarkerShape.RightTriangle)
            {
                int xpoint = 0;
                if (direction == TriangleDirection.Left)
                    xpoint = 20; // 오른쪽 꼭짓점

                Canvas.SetLeft(triangleButton, valueXPos - xpoint / 2);
                brushes = System.Windows.Media.Brushes.GreenYellow;
            }
            else
            {
                Canvas.SetLeft(triangleButton, valueXPos - triangleButton.Width / 2);
            }
            
            MarkerCanvas.Children.Add(triangleButton);

            // 실선 생성
            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = DarkProgressBar.ActualHeight,
                Stroke = brushes,
                StrokeThickness = 1
            };
            
            MarkerCanvas.Children.Add(line);

            // 마커 저장
            markerList.Add(new Marker
            {
                Index = markerList.Count,
                Value = value,
                Duration = duration,
                TriangleButton = triangleButton,
                MarkerLine = line,
                Shape = shape.ToString(),
                Direction = direction.ToString()
            });
        }

        private ControlTemplate CreateEquilateralTriangleTemplate()
        {
            var template = new ControlTemplate(typeof(System.Windows.Controls.Button));

            var triangle = new FrameworkElementFactory(typeof(Polygon));
            triangle.SetValue(Polygon.PointsProperty, new PointCollection(new[] {
                new System.Windows.Point(0, 0), new System.Windows.Point(6, 12), new System.Windows.Point(12, 0)
            }));

            triangle.SetValue(Polygon.FillProperty, System.Windows.Media.Brushes.Red);

            template.VisualTree = triangle;
            return template;
        }

        private ControlTemplate CreateRightTriangleTemplate(TriangleDirection direction)
        {
            var template = new ControlTemplate(typeof(System.Windows.Controls.Button));
            var triangle = new FrameworkElementFactory(typeof(Polygon));

            if (direction == TriangleDirection.Left)
            {
                triangle.SetValue(Polygon.PointsProperty, new PointCollection(new[] {
                    new System.Windows.Point(0, 0), new System.Windows.Point(0, 12), new System.Windows.Point(12, 12)
                }));
            }
            else
            {
                triangle.SetValue(Polygon.PointsProperty, new PointCollection(new[] {
                    new System.Windows.Point(12, 0), new System.Windows.Point(12, 12), new System.Windows.Point(0, 12)
                }));
            }

            triangle.SetValue(Polygon.FillProperty, System.Windows.Media.Brushes.GreenYellow);
            template.VisualTree = triangle;
            return template;
        }

        public void AddMaker(MarkerShape shape, double value , double duration = 0, TriangleDirection triangleDirection = TriangleDirection.None )
        {
            if (shape == MarkerShape.Equilateral)
            {
                DrawMarkerLine(value, shape, duration);
            }
            else if (shape == MarkerShape.RightTriangle)
            {
                if (shape == MarkerShape.RightTriangle && rightTriangleCount >= MaxRightTriangles) { return; }
                else
                {
                    //var direction = (rightTriangleCount == 0) ? TriangleDirection.Left : TriangleDirection.Right;
                    DrawMarkerLine(value, MarkerShape.RightTriangle, duration : 0, triangleDirection);
                    rightTriangleCount++;
                }
            }
        }


        public void AllDeleteMarkers()
        {
            foreach (var marker in markerList)
            {
                MarkerCanvas.Children.Remove(marker.MarkerLine);
                MarkerCanvas.Children.Remove(marker.TriangleButton);
            }

            
            markerList.Clear();
            MaxDuration = 0;
            rightTriangleCount = 0;
        }

        public void DeleteMarker(double value)
        {
            var marker = markerList.FirstOrDefault(m => m.Value == value);
            if (marker != null)
            {
                MarkerCanvas.Children.Remove(marker.MarkerLine);
                MarkerCanvas.Children.Remove(marker.TriangleButton);
                markerList.Remove(marker);

                MaxDuration -= value;
                // 삼각형이 직각삼각형인 경우 카운트 감소
                if (rightTriangleCount > 0) --rightTriangleCount;

                markerList.Where(m => m.Index > marker.Index)
                    .ToList()
                    .ForEach(m => m.Index--);
            }
        }


        public void SaveMarkers()
        {

            var jsonMarkers = markerList.Select(m => new
            {
                m.Value,
                m.Shape,
                m.Direction
            });

            var json = JsonSerializer.Serialize(jsonMarkers, new JsonSerializerOptions { WriteIndented = true });

            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            systemPath += @"Marker.json";

            System.IO.File.WriteAllText(systemPath, json);

        }

        private bool IsButtonDown = false;
        private void JogShuttleSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsButtonDown = true;
        }

        private void JogShuttleSlider_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        private void JogShuttleSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ButJogShuttleName == "Shuttle") // shuttle
            {
                JogShuttleSlider.Value = 0;
            }
           
            IsButtonDown = false;
        }

        private void JogShuttleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (IsButtonDown)
            {
                SliderChangeValue changeValue = new SliderChangeValue();
                changeValue.Type = ButJogShuttleName;
                
                if (ButJogShuttleName == "Jog")
                {
                    changeValue.Value = e.NewValue;
                    SilderValueChanged(this, changeValue);

                    Debug.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss:fff")}] Type : {changeValue.Type} Value :{changeValue.Value}");
                }
                else
                {
                    double value = 0.0;

                    switch (e.NewValue)
                    {
                        case -10:
                            value = -10;
                            break;
                        case -8:
                            value = -8;
                            break;
                        case -6:
                            value = -4;
                            break;
                        case -4:
                            value = -2;
                            break;
                        case -2:
                            value = -1;
                            break;
                        case 0:                   // 0 기준
                            value = 0;
                            break;
                        case 2:
                            value = 1;
                            break;
                        case 4:
                            value = 2;
                            break;
                        case 6:
                            value = 4;
                            break;
                        case 8:
                            value = 8;
                            break;
                        case 10:
                            value = 10;
                            break;
                        
                    }

                    changeValue.Value = value;
                    SilderValueChanged(this, changeValue);
                    
                    Debug.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss:fff")}] Type : {changeValue.Type} Value :{changeValue.Value}");
                }

                //DoEvents();
            }
        }

        private void ButJogShuttle_Click(object sender, RoutedEventArgs e)
        {
            if (ButJogShuttleName == "Shuttle")
            {
                ButJogShuttleName = "Jog";
                JogShuttleSlider.Minimum = 0;
                JogShuttleSlider.Maximum = 1;

                JogShuttleSlider.Value = 0;
                CountTick = 0.1;

            }
            else
            {
                ButJogShuttleName = "Shuttle";
                JogShuttleSlider.Minimum = -10;
                JogShuttleSlider.Maximum = 10;

                JogShuttleSlider.Value = 0;
                CountTick = 2;
            }
        }

        //public  void DoEvents()
        //{
        //    DispatcherFrame frame = new DispatcherFrame();
        //    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
        //        new DispatcherOperationCallback(delegate (object f)
        //        {
        //            ((DispatcherFrame)f).Continue = false;
        //            return null;
        //        }), frame);
        //    Dispatcher.PushFrame(frame);
        //}
    }
}
