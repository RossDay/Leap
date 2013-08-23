using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal class BooleanHandState : HandState<bool>
    {
        private readonly Predicate<Hand> _EnterPredicate;
        private readonly Predicate<Hand> _ExitPredicate;
        private long _CurrentChangeTime;

        public long EnterTimeThreshold { get; set; }
        public long ExitTimeThreshold { get; set; }

        public long LastChangeTime { get; private set; }

        public BooleanHandState(PersistentHand hand, Predicate<Hand> enterPredicate, long enterTime, long exitTime)
            : base(hand)
        {
            _EnterPredicate = enterPredicate;
            _ExitPredicate = (t => !_EnterPredicate(t));
            EnterTimeThreshold = enterTime;
            ExitTimeThreshold = exitTime;
        }
        public BooleanHandState(PersistentHand hand, Predicate<Hand> enterPredicate, long enterTime)
            : this(hand, enterPredicate, enterTime, enterTime)
        {
        }
        public BooleanHandState(PersistentHand hand, Predicate<Hand> enterPredicate, Predicate<Hand> exitPredicate, long enterTime, long exitTime)
            : this(hand, enterPredicate, enterTime, exitTime)
        {
            _ExitPredicate = exitPredicate;
        }

        public override bool Update(Frame frame)
        {
            base.Update(frame);

            var current = (CurrentValue ? _ExitPredicate(Hand.CurrentHand) : _EnterPredicate(Hand.CurrentHand));
            var time = frame.Timestamp;

            // same as current state, cancel any active change
            if (CurrentValue == current)
                _CurrentChangeTime = 0;
            // first time seeing change, record start time
            else if (_CurrentChangeTime == 0)
                _CurrentChangeTime = time;
            // mid change, see if it's been long enough
            else if (time - _CurrentChangeTime > (CurrentValue ? ExitTimeThreshold : EnterTimeThreshold))
            {
                // change complete
                _CurrentChangeTime = 0;
                LastChangeTime = time;
                CurrentValue = current;
            }

            return CurrentValue;
        }
    }
}
