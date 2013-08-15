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
        public Hand CurrentHand { get; private set; }
        public long Duration { get { return CurrentHand.Frame.Timestamp - (StabilizedHand.IsValid ? StabilizedHand : DetectedHand).Frame.Timestamp; } }

        public Hand StabilizedHand { get; private set; }
	    private readonly BooleanHandState _Stabilized = new BooleanHandState(h => h.PalmVelocity.Magnitude < 5, 250000, long.MaxValue);
        public bool IsStabilized { get { return _Stabilized.CurrentValue; } }
	    //private long _StabilizingTime { get { return _Stabilized.LastChangeTime; } }

        private IntegerHandState _X = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.x), 125000);
        private IntegerHandState _Y = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.y), 125000);
        private IntegerHandState _Z = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.z), 125000);
        private IntegerHandState _Pitch = new IntegerHandState(h => h.PitchDegress(), 125000);
        private IntegerHandState _Roll = new IntegerHandState(h => h.RollDegress(), 125000);
        private IntegerHandState _Yaw = new IntegerHandState(h => h.YawDegress(), 125000);
        private IntegerHandState _FingerCount = new IntegerHandState(h => h.Fingers.Count, 125000);

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

            _X.Update(hand, frame);
            _Y.Update(hand, frame);
            _Z.Update(hand, frame);
            _Pitch.Update(hand, frame);
            _Roll.Update(hand, frame);
            _Yaw.Update(hand, frame);
            _FingerCount.Update(hand, frame);
        }
    }
}
