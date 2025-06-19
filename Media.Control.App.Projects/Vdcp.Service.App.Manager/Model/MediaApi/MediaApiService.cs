using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.ViewModel;

namespace Vdcp.Service.App.Manager.Model
{
    public  class MediaApiService
    {
        public MediaApiService() { }
        public MediaApiConnecter ApiConnecter { get; set; } = null;
        public List<MediaDataInfo> MediaDataInfos { get; set; }
        public List<string> ClipList { get; set; } = new List<string>();
        public event Action<string, string> DoHubEventSend;


        public void ConnectApi(string IpAddress)
        {
            ApiConnecter = new MediaApiConnecter("media");
            ApiConnecter.IpAddress = IpAddress;
            ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend;

            ApiConnecter.Connection();
            ApiConnecter.StartHub();
        }

        private async void ApiConnecter_DoHubEventSend(string type, string message)
        {
            DoHubEventSend?.Invoke(type, message);
        }

        public string GetClipPath(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);

            return mediaData == null ? string.Empty : mediaData.Path;
        }


        public long GetFrame(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? 0 : mediaData.Frame;
        }

        public string GetSom(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? "00:00:00;00" : mediaData.InTimeCode;
        }

        public string GetEom(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? "00:00:00;00" : mediaData.OutTimeCode;
        }

        public int GetFps(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);

            return mediaData == null ? 0 : mediaData.Fps;
        }

        public string GetDuration(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);

            return mediaData == null ? "00:00:00;00" : mediaData.Duration;
        }

        public async Task<List<MediaDataInfo>> GetMediaData()
        {
            try
            {
                string query = $"MediaInfo";

                var response = await ApiConnecter?.Client().GetAsync(query);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var medias = JsonConvert.DeserializeObject<ObservableCollection<MediaDataInfo>>(json);

                    MediaDataInfos = medias.ToList();
                        
                    return medias.ToList();
                }
            }
            catch
            {
                // Log or handle the exception as needed
            }

            // Ensure a return value in all code paths
            return new List<MediaDataInfo>();
        }

        public async Task<bool> MediaSave(MediaDataInfo media)
        {
            bool result = false;

            try
            {
                var json = JsonConvert.SerializeObject(media);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiConnecter.Client().PostAsync("MediaInfo", content);

                if (response.IsSuccessStatusCode)
                    return true;

            }
            catch (Exception ex)
            {

            }

            return result;
        }

      
        public async Task<bool> UpDateMedia(string id, string state)
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/{id}?state={state}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter?.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
        }
        public async Task<bool> UpDateInPoint(string id, int inpoint, string inTimecode, int frame, string duration)
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/Inpoint/{id}?inPoint={inpoint}&intimecode={inTimecode}&frame={frame}&duration={duration}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter?.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        public async Task<bool> UpDateDuration(string id, int frame, string duration)
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/duration/{id}?frame={frame}&duration={duration}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter?.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        public async Task<bool> UpDateOutPoint(string id, int outpoint, string outTimecode, int frame, string duration)
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/outpoint/{id}?outPoint={outpoint}&outtimecode={outTimecode}&frame={frame}&duration={duration}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter?.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
        }



        private void DeleteMedia(string type, string message)
        {
            if (type == "MediaInfo")
            {
                //GetMediaData();
            }
        }


    }
}
