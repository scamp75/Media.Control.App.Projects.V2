

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vdcp.Service.App.Manager.Model
{


    public enum LogType
    {
        Info,
        Error,
        Warning
    }

    public class LoggerApiConnecter
    {
        private MediaApiConnecter ApiConnecter { get; set; }
        public LoggerApiConnecter(string url) 
        {
            ApiConnecter = new MediaApiConnecter("loghub");
            ApiConnecter.IpAddress =url;
            ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend;

        }

        public bool Connection()
        {
             return ApiConnecter.Connection();
        }

        public async void Log(string type, string channel, string title, string messag )
        {
            try
            {
                var logData = new LogData()
                {
                    Type = type,
                    Title = title,
                    Message = messag,
                    Channel = channel,
                    CreateDate = DateTime.Now,
                    Time = DateTime.Now.ToString("HH:mm:ss:fff")
                };

                var json = JsonConvert.SerializeObject(logData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiConnecter.Client().PostAsync("Log", content);
            }
            catch (Exception ex) { }
        }

        public async void ConnectHub()
        {
            try
            {
                ApiConnecter.StartHub();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private async void CloseHub(object? obj)
        {
            ApiConnecter.CloseHub();

        }


        private void ApiConnecter_DoHubEventSend(string type, string message)
        {
            
        }
    }
}
