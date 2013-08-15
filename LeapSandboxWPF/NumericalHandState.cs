using System;
using Leap;

namespace LeapSandboxWPF
{
    internal abstract class NumericalHandState<T> : HandState<T>
    {
        private readonly Func<Hand, T> _ValueGetter;
        public long SmoothTime { get; set; }

        protected NumericalHandState(Func<Hand, T> valueGetter, long smoothTime)
        {
            _ValueGetter = valueGetter;
            SmoothTime = smoothTime;
        }

        protected abstract T SmoothValue(T currentValue, T newValue, double frameSmoothedImpact);

        public override T Update(Hand hand, Frame frame)
        {
            var newValue = _ValueGetter(hand);

            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var frameSmoothedImpact = frameTimeDistance / SmoothTime;

            //_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
            CurrentValue = SmoothValue(CurrentValue, newValue, frameSmoothedImpact);

            return CurrentValue;
        }
    }

    internal class IntegerHandState : NumericalHandState<int>
    {
        public IntegerHandState(Func<Hand, int> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

        protected override int SmoothValue(int currentValue, int newValue, double frameSmoothedImpact)
        {
            return Convert.ToInt32(currentValue * (1.0 * frameSmoothedImpact) + newValue * frameSmoothedImpact);
        }
    }

    internal class DecimalHandState : NumericalHandState<double>
    {
        public DecimalHandState(Func<Hand, double> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

        protected override double SmoothValue(double currentValue, double newValue, double frameSmoothedImpact)
        {
            return currentValue * (1.0 * frameSmoothedImpact) + newValue * frameSmoothedImpact;
        }
    }
}
