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
	    private SmoothedState<Hand> _Stabilized = new SmoothedState<Hand>(h => h.PalmVelocity.Magnitude < 5, 250000, long.MaxValue);
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
	            _Stabilized.Update(CurrentHand, frame.Timestamp);

				// Check for stabilization complete
				if (_Stabilized.CurrentState)
	                StabilizedHand = hand;
            }
        }
    }

	internal class SmoothedState<T>
	{
		private readonly Predicate<T> _Check;
		private readonly long _EnterTimeThreshold;
		private readonly long _ExitTimeThreshold;
		private long _CurrentChangeTime;

		public long LastChangeTime { get; private set; }
		public bool CurrentState { get; private set; }

		public SmoothedState(Predicate<T> check, long enterTime, long exitTime)
		{
			_Check = check;
			_EnterTimeThreshold = enterTime;
			_ExitTimeThreshold = exitTime;
		}

		public void Update(T owner, long time)
		{
			var current = _Check(owner);

			// same as current state, cancel any active change
			if (CurrentState == current)
			{
				_CurrentChangeTime = 0;
				return;
			}

			// opposite of current state
			
			// first time seeing change, record start time
			if (_CurrentChangeTime == 0)
			{
				_CurrentChangeTime = time;
				return;
			}

			// mid change, see if it's been long enough
			if (time - _CurrentChangeTime > (CurrentState ? _ExitTimeThreshold : _EnterTimeThreshold))
			{
				// change complete
				_CurrentChangeTime = 0;
				LastChangeTime = time;
				CurrentState = current;
			}
		}
	}
}
