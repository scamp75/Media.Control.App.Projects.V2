using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Windows;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Vdcp.Service.App.Manager.Model
{
    public class MediaApiConnecter
    {
        private string ApiUrlMedia = "http://localhost:5050/api/";
        public HttpClient client { get; set; }
        private HubConnection connection = null;

        public delegate void HubConnectHandleAge(string type, string message);
        public event HubConnectHandleAge DoHubEventSend;

        private string Hub { get; set; }


        public string IpAddress { get; set; } = string.Empty;


        public MediaApiConnecter(string hub)
        {
            Hub = hub;
            
        }

        public HttpClient Client()
        {
            if(IpAddress != string.Empty)
            ApiUrlMedia = $"http://{IpAddress}:5050/api/";
            client = new HttpClient { BaseAddress = new Uri(ApiUrlMedia) };
            return client;
        }

        public bool Connection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"http://{IpAddress}:5050/{Hub}")  // SignalR Hub 경로
                .Build();

            // SignalR 메세지 수신 핸들러
            connection.On<string, string>(Hub, (type, message) =>
            {
                DoHubEventSend(type, message);
            });

            return true;
            //return connection.State == HubConnectionState.Connected ? true : false;
        }

        public void StartHub()
        {
            connection.StartAsync(); // Await the asynchronous operation
        }

        public async void CloseHub()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }

    }

}
