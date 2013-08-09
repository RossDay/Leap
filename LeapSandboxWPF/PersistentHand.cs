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
        public bool IsStabilized { get { return StabilizedHand.IsValid; } }
        private int _StabilizingTime;
        private const float _StableVelocityThreshold = 5;
        private const int _StableTimeTreshold = 250000;

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
            return PersistentHand.Create(Hand.Invalid);
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
                // Check for moving too fast while stabilizing
                if (hand.PalmVelocity.Magnitude > _StableVelocityThreshold)
                    _StabilizingTime = 0;
                // Check for stabilization complete
                else if (frame.Timestamp - _StabilizingTime > _StableTimeTreshold)
                    StabilizedHand = hand;
                return;
            }
        }
    }
}
