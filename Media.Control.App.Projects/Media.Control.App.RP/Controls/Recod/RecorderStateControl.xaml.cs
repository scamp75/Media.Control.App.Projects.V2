using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Media.Control.App.RP.Controls
{
    /// <summary>
    /// RecordStateControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecordStateControl : System.Windows.Controls.UserControl
    {
        public RecordStateControl()
        {
            InitializeComponent();
            State = "PUSE";
        }

        public static readonly DependencyProperty StateProperty =
          DependencyProperty.Register(
              nameof(State),
              typeof(string),
              typeof(RecordStateControl),
              new PropertyMetadata("State"));

        public string State
        {
            get => (string)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty TimecodeProperty =
         DependencyProperty.Register(
             nameof(Timecode),
             typeof(string),
             typeof(RecordStateControl),
             new PropertyMetadata("Timecode"));

        public string Timecode
        {
            get => (string)GetValue(TimecodeProperty);
            set => SetValue(TimecodeProperty, value);
        }


        public static readonly DependencyProperty MediaFormateProperty =
            DependencyProperty.Register(
           nameof(MediaFormate),
           typeof(string),
           typeof(RecordStateControl),
           new PropertyMetadata("MediaFormate"));

        public string MediaFormate
        {
            get => (string)GetValue(MediaFormateProperty);
            set => SetValue(MediaFormateProperty, value);
        }

        public static readonly DependencyProperty DiskSizeProperty =
            DependencyProperty.Register(
            nameof(DiskSize),
            typeof(string),
            typeof(RecordStateControl),
            new PropertyMetadata(""));

        public string DiskSize
        {
            get => (string)GetValue(DiskSizeProperty);
            set => SetValue(DiskSizeProperty, value);
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(
            nameof(FileName),
            typeof(string),
            typeof(RecordStateControl),
            new PropertyMetadata("FileName"));

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }


    }
}
