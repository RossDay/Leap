using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal abstract class NumericalHandState<T> : HandState<T>
    {
        private readonly Func<Hand, T> _ValueGetter;
        public long SmoothTime { get; set; }
        public bool IsAccelerated { get; set; }
        private double SmoothedValue { get; set; }

        protected NumericalHandState(PersistentHand hand, Func<Hand, T> valueGetter, long smoothTime)
            : base(hand)
        {
            _ValueGetter = valueGetter;
            SmoothTime = smoothTime;
            IsAccelerated = false;
        }

        protected abstract double SmoothValue(double currentValue, T newValue, double frameSmoothedImpact);
        protected abstract T ConvertSmoothToCurrent(double smoothed);

        public override bool Update(Frame frame)
        {
            var newValue = _ValueGetter(Hand.CurrentHand);

            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var accelerationFactor = (IsAccelerated ? (1.0 + Math.Floor((Hand.Velocity > 50 ? Hand.Velocity - 50 : 0) / 50.0)) : 1.0);
            var frameSmoothedImpact = frameTimeDistance * accelerationFactor / SmoothTime;

            //_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
            SmoothedValue = SmoothValue(SmoothedValue, newValue, frameSmoothedImpact);
            CurrentValue = ConvertSmoothToCurrent(SmoothedValue);

            return true;
        }
    }

    internal class IntegerHandState : NumericalHandState<int>
    {
        public IntegerHandState(PersistentHand hand, Func<Hand, int> valueGetter, long smoothTime) : base(hand, valueGetter, smoothTime) { }

        protected override double SmoothValue(double currentValue, int newValue, double frameSmoothedImpact)
        {
            return currentValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
        }

        protected override int ConvertSmoothToCurrent(double smoothed)
        {
            return Convert.ToInt32(smoothed);
        }
    }

    internal class DecimalHandState : NumericalHandState<double>
    {
        public DecimalHandState(PersistentHand hand, Func<Hand, double> valueGetter, long smoothTime) : base(hand, valueGetter, smoothTime) { }

        protected override double SmoothValue(double currentValue, double newValue, double frameSmoothedImpact)
        {
            return currentValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
        }

        protected override double ConvertSmoothToCurrent(double smoothed)
        {
            return smoothed;
        }
    }
}
