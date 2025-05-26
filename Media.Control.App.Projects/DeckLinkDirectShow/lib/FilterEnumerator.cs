using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DirectShowLib;

namespace DeckLinkDirectShowLib
{
    public class FilterEnumerator
    {

        public List<string> FilterList = new List<string>();
        public List<string> aFilterList = new List<string>();
        /// <summary>
        /// VideoInputDevice 카테고리에서 등록된 필터 중 이름에 "DeckLink"가 포함된 필터를 찾아
        /// 해당 DevicePath에서 CLSID 정보를 추출하여 출력합니다.
        /// </summary>
        public void FindDeckLinkFilter()
        {
            // DirectShow의 VideoInputDevice 카테고리(비디오 캡처 장치)를 열거합니다.
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (DsDevice device in devices)
            {
                if (device.Name.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug.WriteLine("Found DeckLink device:");
                    Debug.WriteLine($"Name: {device.Name}");
                    Debug.WriteLine($"DevicePath: {device.DevicePath}");

                    // DevicePath 문자열 내에 포함된 GUID 패턴을 추출합니다.
                    string clsid = ExtractCLSID(device.DevicePath);
                    if (!string.IsNullOrEmpty(clsid))
                    {
                        Debug.WriteLine($"CLSID: {clsid}");
                        FilterList.Add($"{device.Name}:{clsid}");
                    }
                    else
                    {
                        Debug.WriteLine("Could not extract CLSID from DevicePath.");
                    }
                }
            }

            DsDevice[] devices1 = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);

            foreach (DsDevice device in devices1)
            {
                if (device.Name.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug.WriteLine("Found DeckLink device:");
                    Debug.WriteLine($"Name: {device.Name}");
                    Debug.WriteLine($"DevicePath: {device.DevicePath}");

                    // DevicePath 문자열 내에 포함된 GUID 패턴을 추출합니다.
                    string clsid = ExtractCLSID(device.DevicePath);
                    if (!string.IsNullOrEmpty(clsid))
                    {
                        Debug.WriteLine($"CLSID: {clsid}");
                        aFilterList.Add($"{device.Name}:{clsid}");
                    }
                    else
                    {
                        Debug.WriteLine("Could not extract CLSID from DevicePath.");
                    }
                }
            }

        }


        /// <summary>
        /// 입력된 문자열에서 GUID 패턴(예, {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX})을 정규식을 이용해 추출합니다.
        /// </summary>
        /// <param name="devicePath">DevicePath 문자열 (예: "@device:sw:{GUID}" 형식)</param>
        /// <returns>추출된 CLSID 문자열 또는 없으면 null</returns>
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
