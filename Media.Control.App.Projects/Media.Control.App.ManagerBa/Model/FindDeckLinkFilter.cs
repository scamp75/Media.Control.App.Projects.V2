using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DirectShowLib;
using Media.Control.App.ManagerBa.Model.Config;

namespace Media.Control.App.ManagerBa.Model
{
    public class FindDeckLinkFilter
    {

        public List<OverlayFilter> GetVideoDeckLinkFilter()
        {
            List <OverlayFilter> deckLinkFilters = new List<OverlayFilter>();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (DsDevice device in devices)
            {
                if (device.Name.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // DevicePath 문자열 내에 포함된 GUID 패턴을 추출합니다.
                    string clsid = ExtractCLSID(device.DevicePath);

                    if (!string.IsNullOrEmpty(clsid))
                    {

                        OverlayFilter deckLinkFilter = new OverlayFilter
                        {
                            VideoFilter = device.Name,
                            VideoClsid = clsid
                        };
                        deckLinkFilters.Add(deckLinkFilter);
                    }
                    else
                    {
                        Debug.WriteLine("Could not extract CLSID from DevicePath.");
                    }
                }
            }

            return deckLinkFilters;
        }


        public List<OverlayFilter> GetAudioDeckLinkFilter()
        {
            List<OverlayFilter> deckLinkFilters = new List<OverlayFilter>();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);

            foreach (DsDevice device in devices)
            {
                if (device.Name.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // DevicePath 문자열 내에 포함된 GUID 패턴을 추출합니다.
                    string clsid = ExtractCLSID(device.DevicePath);

                    if (!string.IsNullOrEmpty(clsid))
                    {

                        OverlayFilter deckLinkFilter = new OverlayFilter
                        {
                            AudioFilter = device.Name,
                            AudioClsid = clsid
                        };
                        deckLinkFilters.Add(deckLinkFilter);
                    }
                    else
                    {
                        Debug.WriteLine("Could not extract CLSID from DevicePath.");
                    }
                }
            }

            return deckLinkFilters;
        }

        private string ExtractCLSID(string devicePath)
        {
            // GUID 패턴을 찾는 정규식 정의
            Regex regex = new Regex(@"\{[0-9A-Fa-f\-]+\}");
            // devicePath 내의 모든 GUID 패턴을 검색
            MatchCollection matches = regex.Matches(devicePath);

            // 두 개 이상의 GUID가 존재할 경우 두 번째 GUID 반환
            if (matches.Count >= 2)
            {
                return matches[1].Value;
            }
            return null;
        }

    }

}
