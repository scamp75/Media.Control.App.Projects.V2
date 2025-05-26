using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public static class StereoAudioLevel
    {
        public static (double left, double right) CalculateStereoAudioLevel(byte[] audioData)
        {
            if (audioData == null || audioData.Length < 2)
                return (0, 0);

            double sumSquaresLeft = 0;
            double sumSquaresRight = 0;
            int sampleCount = audioData.Length / 2;

            // 8비트 PCM 무음 기준: 128
            // RMS 계산을 위해 먼저 DC 오프셋을 제거한 값을 제곱해서 평균을 구한 뒤, 제곱근 취함
            for (int i = 0; i < audioData.Length; i += 2)
            {
                double leftVal = audioData[i] - 128;
                double rightVal = audioData[i + 1] - 128;
                sumSquaresLeft += leftVal * leftVal;
                sumSquaresRight += rightVal * rightVal;
            }

            double rmsLeft = Math.Sqrt(sumSquaresLeft / sampleCount);
            double rmsRight = Math.Sqrt(sumSquaresRight / sampleCount);

            // RMS 값이 128을 넘지는 않으므로, 적절한 스케일링 상수를 적용할 수 있습니다.
            // 예를 들어, RMS 값이 0~128 범위라면 이를 백분율로 변환:
            double percentageLeft = (rmsLeft / 128.0) * 100;
            double percentageRight = (rmsRight / 128.0) * 100;
            //percentageRight *= 0.8;


            return (percentageLeft, percentageRight);
        }


    }
}
