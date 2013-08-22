using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal class IntegerHandState : HandState<int>
    {
        private readonly Func<Hand, int> _ValueGetter;
        private double SmoothedValue { get; set; }
        private double LastValue { get; set; }
        private long LastTime { get; set; }

        public long SmoothTime { get; set; }
        public long StableTime { get; set; }
        public int StableDelta { get; set; }
        public int StableVelocity { get; set; }

        public IntegerHandState(PersistentHand hand, Func<Hand, int> valueGetter, long smoothTime)
            : base(hand)
        {
            _ValueGetter = valueGetter;
            SmoothTime = smoothTime;
            StableTime = 0;
            StableDelta = 0;
            StableVelocity = 0;
        }

        public override bool Update(Frame frame)
        {
            var newValue = _ValueGetter(Hand.CurrentHand);

            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var frameSmoothedImpact = (SmoothTime < frameTimeDistance ? 1.0 : frameTimeDistance / SmoothTime);

            SmoothedValue = SmoothedValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
            if ((SmoothedValue - LastValue) >= StableDelta || Hand.Velocity >= StableVelocity)
            {
                CurrentValue = Convert.ToInt32(SmoothedValue);
                LastValue = CurrentValue;
                LastTime = frame.Timestamp;
            }
            else if ((frame.Timestamp - LastTime) > StableTime)
            {
                LastValue = LastValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
                LastTime += Convert.ToInt64(frameSmoothedImpact * StableTime);
                CurrentValue = Convert.ToInt32(LastValue);
            }

            return true;
        }

        public override void InitValue(int value)
        {
            base.InitValue(value);
            SmoothedValue = Convert.ToDouble(value);
            LastValue = CurrentValue;
            LastTime = 0;
        }
    }
}
