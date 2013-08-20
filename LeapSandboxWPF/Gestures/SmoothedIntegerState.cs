using System;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal class SmoothedIntegerState
    {
        public long SmoothTime { get; set; }
        public long CurrentValue { get; private set; }
        private double SmoothedValue { get; set; }

        public SmoothedIntegerState(long smoothTime)
        {
            SmoothTime = smoothTime;
        }

        public void Initialize(long initValue)
        {
            CurrentValue = initValue;
        }
        public long Update(long newValue, Frame frame)
        {
            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var frameSmoothedImpact = frameTimeDistance / SmoothTime;

            //_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
            SmoothedValue = CurrentValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
            CurrentValue = Convert.ToInt64(SmoothedValue);

            return CurrentValue;
        }
    }
}
