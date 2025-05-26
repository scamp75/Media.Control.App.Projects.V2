using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Media.Control.App.Model
{
    public class MediaApiConnecter
    {
        private const string ApiUrlMedia = "https://localhost:5050/api/";
        public HttpClient client { get; set; }
        private HubConnection connection = null;

        public delegate void HubConnectHandleAge(string type, string message);
        public event HubConnectHandleAge DoHubEventSend;

        private string Hub { get; set; }   

        public MediaApiConnecter(string hub)
        {
            Hub = hub;
        }

        public HttpClient Client()
        {
            client = new HttpClient { BaseAddress = new Uri(ApiUrlMedia) };
            return client;
        }

        public HubConnection Connection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:5050/{Hub}")  // SignalR Hub 경로
                .Build();


            //ReceiveMedia
            //ReceiveLog
            // SignalR 메세지 수신 핸들러
            connection.On<string, string>(Hub, (type, message) =>
            {
                DoHubEventSend(type, message);
            });

            return connection;
        }

        public void StartHub()
        {
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    connection.StopAsync();
                }

                connection.StartAsync();  // SignalR 연결 시작

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        public async void CloseHub()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

}
