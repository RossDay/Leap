using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
    class PersistentHand
    {
        public int Id { get; private set; }
        public Hand DetectedHand { get; private set; }
        public long DetectedTime { get { return DetectedHand.Frame.Timestamp; } }
        public Hand FinalHand { get; private set; }
        public bool IsFinalized { get { return !DetectedHand.IsValid || FinalHand.IsValid; } }

        public Hand StabilizedHand { get; private set; }
	    private readonly SmoothedBooleanState<Hand> _Stabilized = new SmoothedBooleanState<Hand>(h => h.PalmVelocity.Magnitude < 5, 250000, long.MaxValue);
        public bool IsStabilized { get { return _Stabilized.CurrentState; } }
	    //private long _StabilizingTime { get { return _Stabilized.LastChangeTime; } }

        public Hand CurrentHand { get; private set; }
        public long Duration { get { return CurrentHand.Frame.Timestamp - (StabilizedHand.IsValid ? StabilizedHand : DetectedHand).Frame.Timestamp; } }

        public static PersistentHand Create(Hand hand)
        {
            return new PersistentHand
            {
                Id = hand.Id,
                DetectedHand = hand,
                StabilizedHand = Hand.Invalid,
                CurrentHand = hand,
                FinalHand = Hand.Invalid,
            };
        }
        public static PersistentHand CreateFinalized()
        {
            return Create(Hand.Invalid);
        }

        public void Update(Frame frame)
        {
            var hand = frame.Hand(Id);
            if (!hand.IsValid)
            {
                // Our hand is not in this frame,
                // but we won't give up for 100ms.
                if (frame.Timestamp - CurrentHand.Frame.Timestamp < 100000)
                    return;

                // Our hand is truly gone...
                FinalHand = CurrentHand;
                CurrentHand = Hand.Invalid;
            }

            // We still exist in this frame, update current
            //var previous = CurrentHand;
            CurrentHand = hand;

            if (!IsStabilized)
            {
				// Check for stabilization complete
                if (_Stabilized.Update(CurrentHand, frame))
                    StabilizedHand = hand;
                else
                    return;
            }
        }
    }

	internal interface ISmoothedState<in T, out U>
	{
		U Update(T obj, Frame frame);
	}


	internal abstract class SmoothedNumericalState<T, U> : ISmoothedState<T, U>
    {
	    private U _CurrentValue;
	    private readonly Func<T, U> _ValueGetter;
	    private long _SmoothTime;

		protected SmoothedNumericalState(Func<T, U> valueGetter, long smoothTime)
	    {
		    _ValueGetter = valueGetter;
	    }

		protected abstract U SmoothValue(U currentValue, U newValue, double frameSmoothedImpact);

	    public U Update(T obj, Frame frame)
	    {
		    var newValue = _ValueGetter(obj);

		    var frameTimeDistance = 1000000f/frame.CurrentFramesPerSecond;
		    var frameSmoothedImpact = frameTimeDistance/_SmoothTime;

			//_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
		    _CurrentValue = SmoothValue(_CurrentValue, newValue, frameSmoothedImpact);

		    return _CurrentValue;
	    }
    }

	internal class SmoothedIntegerState<T> : SmoothedNumericalState<T, int>
	{
		public SmoothedIntegerState(Func<T, int> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

		protected override int SmoothValue(int currentValue, int newValue, double frameSmoothedImpact)
		{
			return Convert.ToInt32(currentValue * (1.0 * frameSmoothedImpact) + newValue * frameSmoothedImpact);
		}
	}

	internal class SmoothedDecimalState<T> : SmoothedNumericalState<T, double>
	{
		public SmoothedDecimalState(Func<T, double> valueGetter, long smoothTime) : base(valueGetter, smoothTime) { }

		protected override double SmoothValue(double currentValue, double newValue, double frameSmoothedImpact)
		{
			return currentValue * (1.0 * frameSmoothedImpact) + newValue * frameSmoothedImpact;
		}
	}

	internal class SmoothedBooleanState<T> : ISmoothedState<T, bool>
	{
        private readonly Predicate<T> _EnterPredicate;
        private readonly Predicate<T> _ExitPredicate;
        private readonly long _EnterTimeThreshold;
		private readonly long _ExitTimeThreshold;
		private long _CurrentChangeTime;

		public long LastChangeTime { get; private set; }
		public bool CurrentState { get; private set; }

        public SmoothedBooleanState(Predicate<T> enterPredicate, long enterTime, long exitTime)
        {
            _EnterPredicate = enterPredicate;
            _ExitPredicate = (t => !_EnterPredicate(t));
            _EnterTimeThreshold = enterTime;
            _ExitTimeThreshold = exitTime;
        }
        public SmoothedBooleanState(Predicate<T> enterPredicate, long enterTime)
            : this(enterPredicate, enterTime, enterTime)
        {
        }
        public SmoothedBooleanState(Predicate<T> enterPredicate, Predicate<T> exitPredicate, long enterTime, long exitTime)
            : this(enterPredicate, enterTime, exitTime)
        {
            _ExitPredicate = exitPredicate;
        }

		public bool Update(T obj, Frame frame)
		{
            var current = (CurrentState ? _ExitPredicate(obj) : _EnterPredicate(obj));
			var time = frame.Timestamp;

			// same as current state, cancel any active change
			if (CurrentState == current)
				_CurrentChangeTime = 0;
			// first time seeing change, record start time
			else if (_CurrentChangeTime == 0)
				_CurrentChangeTime = time;
			// mid change, see if it's been long enough
			else if (time - _CurrentChangeTime > (CurrentState ? _ExitTimeThreshold : _EnterTimeThreshold))
			{
				// change complete
				_CurrentChangeTime = 0;
				LastChangeTime = time;
				CurrentState = current;
			}

            return CurrentState;
		}
	}
}
