using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Vdcp.Service.App.lib.Api
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

        public bool DeleteMedia(string item)
        {
            bool result = false;

            var response = client.DeleteAsync($"MediaInfo/{item}");

            if (response.IsCompleted)
                result = true;
            return result;
        }
      
    }

}
