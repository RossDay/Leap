using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal abstract class NumericalHandState<T> : HandState<T>
    {
        private readonly Func<Hand, T> _ValueGetter;
        public long SmoothTime { get; set; }
        private double SmoothedValue { get; set; }

        protected NumericalHandState(Func<Hand, T> valueGetter, long smoothTime)
        {
            _ValueGetter = valueGetter;
            SmoothTime = smoothTime;
        }

        protected abstract double SmoothValue(double currentValue, T newValue, double frameSmoothedImpact);
        protected abstract T ConvertSmoothToCurrent(double smoothed);

        public override T Update(Hand hand, Frame frame)
        {
            var newValue = _ValueGetter(hand);

            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var frameSmoothedImpact = frameTimeDistance / SmoothTime;

            //_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
            SmoothedValue = SmoothValue(SmoothedValue, newValue, frameSmoothedImpact);
            CurrentValue = ConvertSmoothToCurrent(SmoothedValue);

            return CurrentValue;
        }
    }

    internal class IntegerHandState : NumericalHandState<int>
    {
        public IntegerHandState(Func<Hand, int> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

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
        public DecimalHandState(Func<Hand, double> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

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
