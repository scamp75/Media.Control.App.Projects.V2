using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Media.Control.App.LogView.Model
{
    public class MediaApiConnecter
    {
        private string ApiUrlMedia = "http://localhost:5050/api/";
        public HttpClient client { get; set; }
        private HubConnection connection = null;

        public delegate void HubConnectHandleAge(string message);
        public event HubConnectHandleAge DoHubEventSend;

        private string Hub { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public MediaApiConnecter(string hub)
        {
            Hub = hub;
        }

        public HttpClient Client()
        {
            if (IpAddress != string.Empty)
                ApiUrlMedia = $"http://{IpAddress}:5050/api/";

            client = new HttpClient { BaseAddress = new Uri(ApiUrlMedia) };
            return client;
        }

        public HubConnection Connection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"http://{IpAddress}:5050/{Hub}")  // SignalR Hub 경로
                .Build();


            //ReceiveMedia
            //ReceiveLog
            // SignalR 메세지 수신 핸들러
            connection.On<string>(Hub, (message) =>
            {
                DoHubEventSend(message);
            });

            return connection;
        }

        public async void StartHub()
        {
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    await connection.StartAsync();  // SignalR 연결 시작
                }
                else
                {
                    MessageBox.Show("Connection is already started or in the process of starting.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        public async void CloseHub()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

}
