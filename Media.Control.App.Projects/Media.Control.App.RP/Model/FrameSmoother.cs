using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Media.Control.App.RP.Model
{
    public static class FrameSmoother
    {
        public static bool isFrameSmoother { get; set; } = true;   

        /// <summary>
        /// 현재 보정되어 표시되는 프레임 값
        /// </summary>
        public static int CurrentFrame { get; private set; }

        /// <summary>
        /// 외부에서 입력받은 목표 프레임 값 (참고용)
        /// </summary>
        public static int TargetFrame { get; private set; }

        /// <summary>
        /// 초당 프레임 수(FPS)
        /// </summary>
        public static int FPS { get; private set; }

        /// <summary>
        /// 프레임 보정 시 업데이트된 프레임 값을 알리기 위한 이벤트
        /// </summary>
        public static event EventHandler<int> FrameUpdated;

        private static DispatcherTimer _timer;

        // 자연 진행 및 외부 업데이트 관련 내부 변수
        private static int _naturalFrame = 0;      // 외부 업데이트 모드가 아닐 때의 진행 값
        private static bool _externalMode = false;   // 외부 업데이트 모드 여부
        private static int _externalValue = 0;       // 외부 업데이트로 받은 값
        private static int _externalTickCount = 0;   // 외부 업데이트 모드 시작 이후 틱 카운트
        private static int _direction = 1;           // 진행 방향: +1 (증가) 또는 -1 (감소)

        /// <summary>
        /// 현재 타이머가 실행 중인지 여부
        /// </summary>
        public static bool IsRunning => _timer != null && _timer.IsEnabled;
        public static bool IsStart = false;

        public static double Interval { get; set; } = 30;
        /// <summary>
        /// 초기 프레임과 FPS를 설정하여 FrameSmoother를 초기화합니다.
        /// </summary>
        /// <param name="startFrame">초기 프레임 값</param>
        /// <param name="fps">초당 프레임 수</param>
        public static void Initialize(int startFrame, int fps)
        {
            if (fps <= 0)
                throw new ArgumentException("FPS는 0보다 커야 합니다.", nameof(fps));

            _naturalFrame = startFrame;
            CurrentFrame = startFrame;
            TargetFrame = startFrame;
            _externalMode = false;
            _externalValue = 0;
            _externalTickCount = 0;
            _direction = 1;
            FPS = fps ==0 ? 30 : fps;

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += Timer_Tick;
            }

            _timer.Interval = TimeSpan.FromMilliseconds(1000 / fps);
        }

        /// <summary>
        /// 프레임 보정 타이머를 시작합니다.
        /// </summary>
        public static void Start()
        {
            if (_timer != null && !_timer.IsEnabled)
            {
                _timer.Start();
                IsStart = true;
            }
                
        }

        public static void Start(int value)
        {
            if (_timer != null && !_timer.IsEnabled)
            {
                _timer.Start();
                IsStart = true;
                CurrentFrame = value;
            }

        }

        /// <summary>
        /// 프레임 보정 타이머를 중지합니다.
        /// </summary>
        public static void Stop()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
                IsStart = false;
            }
                
        }

        /// <summary>
        /// 외부 입력으로 새로운 프레임 값을 업데이트합니다.
        /// 만약 새 값이 현재 진행 값보다 작으면, 타이머 틱마다 감소하는 모드로 전환합니다.
        /// 새 값이 현재보다 크면 증가하는 모드로 전환합니다.
        /// </summary>
        /// <param name="newFrame">새로운 프레임 값</param>
        public static void UpdateExternalFrame(int newFrame, int Tick = 1, bool isIncreasing = true)
        {
            TargetFrame = newFrame;

            if (Tick == 0) Tick = 1;
            
            if (isIncreasing == false)
            {
                _direction = -Tick;
            }
            else if (isIncreasing == true)
            {
                _direction = Tick;
            }
            
            _externalMode = true;
            _externalValue = newFrame;
            _externalTickCount = 0;
        }

        /// <summary>
        /// 보정된 프레임 값을 초기화(리셋)합니다.
        /// 외부 업데이트 모드를 해제하고 자연 진행 값을 새 값으로 설정합니다.
        /// </summary>
        /// <param name="newStartFrame">초기화할 프레임 값</param>
        public static void ResetFrame(int newStartFrame)
        {
            _externalMode = false;
            _externalValue = 0;
            _externalTickCount = 0;
            _direction = 1;
            _naturalFrame = newStartFrame;
            CurrentFrame = newStartFrame;
            TargetFrame = newStartFrame;
            FrameUpdated?.Invoke(null, CurrentFrame);
        }

        /// <summary>
        /// 타이머 Tick 이벤트에서 실행됩니다.
        /// 자연 진행 모드에서는 1씩 증가하고, 외부 업데이트 모드에서는 지정된 진행 방향(_direction)에 따라
        /// 외부 값(_externalValue)에서 시작하여 틱마다 증가 또는 감소한 값을 출력합니다.
        /// 단, 감소하는 경우 0 미만으로 내려가지 않도록 합니다.
        /// </summary>
        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (_externalMode)
            {
                _externalTickCount++;
                if (_direction > 0)
                {
                    // 증가하는 경우: 외부 업데이트 값 + 틱수
                    CurrentFrame = _externalValue + _externalTickCount;
                }
                else if (_direction < 0)
                {
                    // 감소하는 경우: 외부 업데이트 값 - 틱수, 단 0 미만이 되지 않도록 함
                    int value = _externalValue - _externalTickCount;
                    
                    if (value < 0)
                    {
                        value = 0;
                    }

                    CurrentFrame = value;
                }
            }
            else
            {
                _naturalFrame++;
                CurrentFrame = _naturalFrame;
            }

            if(isFrameSmoother) FrameUpdated?.Invoke(null, CurrentFrame);
        }
    }
}
