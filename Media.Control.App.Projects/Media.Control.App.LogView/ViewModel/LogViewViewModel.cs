using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Media.Control.App.LogView.Model;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Media.Control.App.LogView.lib;
using System.Reflection.Metadata;

namespace Media.Control.App.LogView.ViewModel
{  
    public class LogViewViewModel : INotifyPropertyChanged
    {
        
        private readonly MainWindow _mainWindow;

        public ObservableCollection<string> Notifications { get; set; }
        public ObservableCollection<string> ChannelList { get; set; }

        public ObservableCollection<LogData> LogDataList { get; set; }

        private CancellationTokenSource cancellationTokenSource;

        private bool autoRefresh { get; set; } = true;  

        public bool AutoRefresh { get => autoRefresh; set { autoRefresh = value; OnPropertyChanged(); }
    }
        private bool onlyError { get; set; } = false;

        public bool OnlyError { get => onlyError; set { onlyError = value; OnPropertyChanged();} }

        private string channel { get;set; }

        public string Channel { get => channel; set { channel = value; OnPropertyChanged(); } }

        private string selecteCreateDate { get; set; }
        public string SelecteCreateDate 
        { 
            get => SelecteCreateDate; 
            set { selecteCreateDate = value; 
                OnPropertyChanged(nameof(SelecteCreateDate));}
        }

        private DateTime _SelectedDate { get; set; } = DateTime.Now.Date;

        public DateTime SelectedDate
        { 
            get => _SelectedDate; 
            set
            { 
                _SelectedDate = value; 
                OnPropertyChanged(nameof(SelectedDate));
            } 
        }

        private int gridHeight { get; set; } = 400;

        public int GridHeight { get => gridHeight; set { gridHeight = value; OnPropertyChanged(); } }

        public string selecteChannel { get; set; } = "All Channel";
        public string SelecteChannel
        {
            get => selecteChannel;
            set
            {
                if (selecteChannel != value)
                {
                    selecteChannel = value;
                    OnPropertyChanged(nameof(selecteChannel));
                }
            }
        }

        public ICommand Command_Search { get; }
        public ICommand Command_Click { get; }
        public ICommand Command_Min { get; }    
        public ICommand Command_Max { get; }
        public ICommand Command_Close { get; }

        private MediaApiConnecter ApiConnecter { get; set; }


        private double Left { get; set; } = 0;
        private double Top { get; set; } = 0;


        public void UpdateUI(Action uiAction)
        {
            Application.Current.Dispatcher.Invoke(uiAction);
        }

        public LogViewViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            ApiConnecter = new MediaApiConnecter("loghub");
            ApiConnecter.Connection();
            
            ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend;

            Command_Search = new RelayCommand(CommandSearch);
            Command_Click = new RelayCommand(CommandClik);

            Command_Max = new RelayCommand(CommandMax);
            Command_Min = new RelayCommand(CommandMix);
            Command_Close = new RelayCommand(CommandClose);

            LogDataList = new ObservableCollection<LogData>();

        }

        private void ApiConnecter_DoHubEventSend(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var logData = JsonConvert.DeserializeObject<LogData>(message);
             
                if (logData != null)
                {
                    if (OnlyError && logData.Type == "Error")
                        LogDataList.Insert(0, logData);
                    else if (!OnlyError)
                        LogDataList.Insert(0, logData);
                }
            });
        }

        public async void ConnectHub()
        {
            try
            {
                ApiConnecter.StartHub(); // SignalR 연결 시작
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        public async void CommandClose(object? obj)
        {
            ApiConnecter.CloseHub();
            Thread.Sleep(300);
            _mainWindow.Close();    
        }

        private void CommandMix(object? obj)
        {
            _mainWindow.WindowState = WindowState.Minimized;
        }

        private void CommandMax(object? obj)
        {
            if (_mainWindow.WindowState == WindowState.Maximized)
                _mainWindow.WindowState = WindowState.Normal;
            else
                _mainWindow.WindowState = WindowState.Maximized;
        }

        private async  void CommandClik(object? obj)
        {
            var logData = new LogData()
            {
                Type = "Error",
                Title = "title message___9098",
                Message = "Webhook unregistered successfully.",
                Channel = "Channel3",
                CreateDate = DateTime.Now,
                Time = DateTime.Now.ToString("HH:mm:ss:fff")
            };
            
            var json = JsonConvert.SerializeObject(logData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ApiConnecter.Client().PostAsync("Log", content);

            if (response.IsSuccessStatusCode)
            {
                //MessageBox.Show("Log created successfully!");
            }
            else
            {
                //MessageBox.Show("Failed to create log.");
            }
        }

        private async void CommandSearch(object? obj)
        {

            try
            {
                LogDataList.Clear();

                string query = $"Log?channel={SelecteChannel}&createDate={SelectedDate.ToShortDateString()}";

                var response = await ApiConnecter.Client().GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var logs = JsonConvert.DeserializeObject<ObservableCollection<LogData>>(json);
                    foreach (var log in logs)
                    {
                        LogDataList.Insert(0, log);
                    }
                }
            }
            catch { }
        }
             
        public void Close()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
