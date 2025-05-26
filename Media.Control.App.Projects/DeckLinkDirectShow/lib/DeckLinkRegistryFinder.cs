using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace DeckLinkDirectShowLib
{
    public class DeckLinkRegistryFinder
    {
        public Dictionary<string, string> DeckLinkCLSIDLists = null;

        public void SearchDeckLinkCLSID()
        {

            DeckLinkCLSIDLists = new Dictionary<string, string>();
            // HKEY_CLASSES_ROOT\CLSID 경로를 연다.
            using (RegistryKey clsidRoot = Registry.ClassesRoot.OpenSubKey("CLSID"))
            {
                if (clsidRoot == null)
                {
                    Console.WriteLine("CLSID 레지스트리 키를 열 수 없습니다.");
                    return;
                }

                foreach (string subKeyName in clsidRoot.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subKey = clsidRoot.OpenSubKey(subKeyName))
                        {
                            if (subKey == null)
                                continue;

                            // 기본 값(Description)에서 DeckLink 문자열이 있는지 확인
                            string description = subKey.GetValue(null) as string;
                            if (!string.IsNullOrEmpty(description) && description.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Console.WriteLine($"Found DeckLink CLSID: {subKeyName} - {description}");
                            }
                            else
                            {
                                // InprocServer32 키 내부의 값(실제 DLL 경로)에 DeckLink 문자열이 포함되어 있는지 확인
                                using (RegistryKey inprocKey = subKey.OpenSubKey("InprocServer32"))
                                {
                                    if (inprocKey != null)
                                    {
                                        string serverPath = inprocKey.GetValue(null) as string;
                                        if (!string.IsNullOrEmpty(serverPath) && serverPath.IndexOf("DeckLink", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {

                                            DeckLinkCLSIDLists.Add(subKeyName, serverPath);
                                            Console.WriteLine($"Found DeckLink CLSID: {subKeyName} - Server: {serverPath}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 각 키 접근 시 발생할 수 있는 예외를 출력 (권한 문제 등)
                        Console.WriteLine($"Error accessing key {subKeyName}: {ex.Message}");
                    }
                }
            }
        }

        //public static void Main(string[] args)
        //{
        //    Console.WriteLine("DeckLink 관련 CLSID를 검색합니다...");
        //    SearchDeckLinkCLSID();
        //    Console.WriteLine("검색 완료. 아무 키나 누르면 종료합니다.");
        //    Console.ReadKey();
        //}
    }
}
