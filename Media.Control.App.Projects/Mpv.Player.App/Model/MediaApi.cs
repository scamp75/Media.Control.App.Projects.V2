using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Net.Http;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;
using System.Reflection.Metadata;

namespace Media.Control.App.Model
{
    public class UpdateMediaData()
    {
        public string Id { get; set; }
        public string State { get; set; }
    }



    public class MediaApi
    {
        public event EventHandler<MediaDataInfo2> MediaDataEvent;
        public event EventHandler<UpdateMediaData> UpdateMediaDataEvent;

        private MediaApiConnecter ApiConnecter { get; set; }

        public MediaApi()
        {
            ApiConnecter = new MediaApiConnecter("mediahub");
            ApiConnecter.Connection();
            ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend;

        }

        private void ApiConnecter_DoHubEventSend(string type, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(type == "UpDate")
                {
                    var UpdateData = JsonConvert.DeserializeObject<UpdateMediaData>(message);

                    UpdateMediaDataEvent?.Invoke(this, UpdateData);
                }
                else if(type == "Insert")
                {
                    var mediaData = JsonConvert.DeserializeObject<MediaDataInfo2>(message);
                    if (mediaData != null)
                    {
                        MediaDataEvent?.Invoke(this, mediaData);
                    }
                }
            });
        }

        public async Task<bool> MediaSave(MediaDataInfo2 media)
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


        public async Task<bool> UpDateInPoint(string id, int inpoint,string inTimecode, int frame, string duration )
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/Inpoint/{id}?inPoint={inpoint}&intimecode={inTimecode}&frame={frame}&duration={duration}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter.Client().PutAsync(url, null);

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
                var response = await ApiConnecter.Client().PutAsync(url, null);

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
                var response = await ApiConnecter.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
        }






        public async Task<bool> UpDateMedia(string id , string state)
        {
            bool result = false;

            try
            {
                var url = $"MediaInfo/{id}?state={state}";

                // PUT 요청: 요청 본문은 별도로 전달할 필요가 없으므로 null 사용
                var response = await ApiConnecter.Client().PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (Exception ex)
            {

            }

            return result;
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
    }
}
