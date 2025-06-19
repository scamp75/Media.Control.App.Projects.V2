using System;
using System.Diagnostics;
using System.Threading;

namespace Vdcp.Service.App.Manager.Model
{
    public class FrameSmoother
    {
        public bool isFrameSmoother { get; set; } = true;

        public int CurrentFrame { get; private set; }
        public int TargetFrame { get; private set; }
        public int FPS { get; private set; }

        private DateTime _startTime;
        private int _initialFrame;

        public event EventHandler<int>? FrameUpdated;

        private System.Threading.Timer? _timer;

        private int _naturalFrame = 0;
        private bool _externalMode = false;
        private int _externalValue = 0;
        private int _externalTickCount = 0;
        private int _direction = 1;

        public bool IsRunning { get; private set; } = false;
        public double Interval { get; set; } = 30;

        public void Initialize(int startFrame, int fps)
        {
            if (fps <= 0)
                throw new ArgumentException("FPS는 0보다 커야 합니다.", nameof(fps));

            FPS = fps;
            _naturalFrame = startFrame;
            CurrentFrame = startFrame;
            TargetFrame = startFrame;
            _externalMode = false;
            _externalValue = 0;
            _externalTickCount = 0;
            _direction = 1;

            int interval = (int)(1000.0 / FPS);

            _timer?.Dispose(); // 기존 타이머 해제

            _timer = new System.Threading.Timer(Timer_Tick, null, Timeout.Infinite, interval);
        }

        public void Start()
        {

            if (_timer != null && !IsRunning)
            {
                _startTime = DateTime.UtcNow;
                _initialFrame = CurrentFrame;

                int interval = (int)(1000.0 / FPS);
                _timer.Change(0, interval);
                IsRunning = true;
            }

            //if (_timer != null && !IsRunning)
            //{
            //    int interval = (int)(1000.0 / FPS);
            //    _timer.Change(0, interval); // 즉시 시작
            //    IsRunning = true;
            //}
        }

        public void Start(int value)
        {
            CurrentFrame = value;
            Start();
        }

        public   void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
        }

        public void UpdateExternalFrame(int newFrame, int Tick = 1, bool isIncreasing = true)
        {
            if (Tick == 0) Tick = 1;

            TargetFrame = newFrame;
            _externalMode = true;
            _externalValue = newFrame;
            _externalTickCount = 0;
            _direction = isIncreasing ? Tick : -Tick;
        }

        public void ResetFrame(int newStartFrame)
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

        private void Timer_Tick(object? state)
        {
            int newValue;
            bool hasChanged = false;

            if (_externalMode)
            {
                // 외부 보정 모드에서는 기존 방식 유지
                _externalTickCount++;
                if (_direction > 0)
                {
                    newValue = _externalValue + _externalTickCount;
                }
                else
                {
                    newValue = _externalValue - _externalTickCount;
                    if (newValue < 0) newValue = 0;
                }
            }
            else
            {
                // 시간 기반 프레임 계산
                var elapsed = DateTime.UtcNow - _startTime;
                double framesPassed = elapsed.TotalSeconds * FPS;

                newValue = _initialFrame + (int)framesPassed;
            }

            if (newValue != CurrentFrame)
            {
                CurrentFrame = newValue;
                hasChanged = true;
            }

            if (isFrameSmoother && hasChanged)
            {
                FrameUpdated?.Invoke(null, CurrentFrame);
            }
        }


        //private void Timer_Tick(object? state)
        //{
        //    bool hasChanged = false;
        //    int newValue;

        //    if (_externalMode)
        //    {
        //        _externalTickCount++;

        //        if (_direction > 0)
        //        {
        //            newValue = _externalValue + _externalTickCount;
        //        }
        //        else
        //        {
        //            newValue = _externalValue - _externalTickCount;
        //            if (newValue < 0) newValue = 0;
        //        }
        //    }
        //    else
        //    {
        //        newValue = _naturalFrame + 1;
        //        _naturalFrame = newValue;
        //    }

        //    if (newValue != CurrentFrame)
        //    {
        //        CurrentFrame = newValue;
        //        hasChanged = true;
        //    }

        //    if (isFrameSmoother && hasChanged)
        //    {
        //        FrameUpdated?.Invoke(null, CurrentFrame);
        //    }
        //}
    }
}
