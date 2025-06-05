using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class MediAipModel
    {
        public List<MediaDataInfo> mediaDataInfos { get; set; } = new List<MediaDataInfo>();

        public MediaApiConnecter MediaApiConnecter { get; set; } = null;
        public MediAipModel(string url) 
        {
            MediaApiConnecter = new MediaApiConnecter("mediahub");
            MediaApiConnecter.IpAddress = url; // Set the IP address from the main window
            MediaApiConnecter.Connection();
            MediaApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend1;
        }

        private async void ApiConnecter_DoHubEventSend1(string type, string message)
        {
            mediaDataInfos = await GetMediaListAsync();
        }


        public async Task<bool> MediaSaveAsync(string item)
        {
            string query = $"MediaInfo/{item}";

            var response = await MediaApiConnecter.Client().GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var medias = JsonConvert.DeserializeObject<MediaDataInfo>(json);

                return medias != null ? true : false;
            }
            else return false;
        }

        public async Task<List<MediaDataInfo>> GetMediaListAsync()
        {
            string query = $"MediaInfo";

            var response = await MediaApiConnecter.Client().GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var medias = JsonConvert.DeserializeObject<List<MediaDataInfo>>(json);

                return medias;
            }
            else return null;

        }

        public async void DeleteMedia(string item)
        {
            if (item == null) return;

            var response = await MediaApiConnecter.client.DeleteAsync($"MediaInfo/{item}");

            if (response.IsSuccessStatusCode)
            {
                //System.Windows.MessageBox.Show("Log created successfully!");
            }
            else
            {
                //System.Windows.MessageBox.Show("Failed to create log.");
            }
        }
    }
}
