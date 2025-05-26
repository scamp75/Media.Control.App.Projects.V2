using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Media
{
    public static class MediaThumbnail
    {
        //public static string ffmpegPath { get; set; } // FFmpeg 실행 파일 경로
        //public static string videoPath { get; set; }  // 입력 동영상 파일 경로
        //public static string outputImagePath { get; set; } // 생성될 썸네일 경로

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ffmpegPath"></param>
        /// <param name="videoPath"></param>
        /// <param name="outputImagePath"></param>
        /// <param name="captureTime"></param>
        /// <exception cref="Exception"></exception>
        public static void GenerateThumbnail(string ffmpegPath, string videoPath, string outputImagePath, TimeSpan captureTime)
        {
            // FFmpeg 명령어 설정
            string arguments = $"-i \"{videoPath}\" -ss {captureTime.TotalSeconds} -vframes 1 -q:v 2 \"{outputImagePath}\"";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("FFmpeg error: " + error);
                    throw new Exception("Failed to generate thumbnail.");
                }
            }
        }
    }
}
