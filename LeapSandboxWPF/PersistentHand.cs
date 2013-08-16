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
	    public float CurrentFPS { get; set; }

	    public Hand StabilizedHand { get; private set; }
	    private readonly BooleanHandState _Stabilized = new BooleanHandState(h => h.PalmVelocity.Magnitude < 50, 25000, long.MaxValue);
        public bool IsStabilized { get { return _Stabilized.CurrentValue; } }
	    //private long _StabilizingTime { get { return _Stabilized.LastChangeTime; } }

        private IntegerHandState _Velocity = new IntegerHandState(h => Convert.ToInt32(h.PalmVelocity.Magnitude), 100000);
        private IntegerHandState _X = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.x), 200000);
        private IntegerHandState _Y = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.y), 200000);
        private IntegerHandState _Z = new IntegerHandState(h => Convert.ToInt32(h.PalmPosition.z), 200000);
        private IntegerHandState _Pitch = new IntegerHandState(h => h.PitchDegrees(), 200000);
        private IntegerHandState _Roll = new IntegerHandState(h => h.RollDegrees(), 200000);
        private IntegerHandState _Yaw = new IntegerHandState(h => h.YawDegrees(), 200000);
        private IntegerHandState _FingerCount = new IntegerHandState(h => h.Fingers.Count, 200000);

	    private ICollection<BaseTrigger> _Triggers;

	    public PersistentHand()
	    {
			Id = 0;
			DetectedHand = Hand.Invalid;
			StabilizedHand = Hand.Invalid;
			CurrentHand = Hand.Invalid;
			FinalHand = Hand.Invalid;

		    _Triggers = new List<BaseTrigger>();
			_Triggers.Add(new RangeTrigger(_Roll) { Name = "Roll+", MinValue = 45, MaxValue = 105, Resistance = 5, Stickiness = 10 });
			_Triggers.Add(new RangeTrigger(_Roll) { Name = "Roll-", MinValue = -105, MaxValue = -45, Resistance = 5, Stickiness = 10 });
			_Triggers.Add(new RangeTrigger(_Pitch) { Name = "Pitch+", MinValue = 20, MaxValue = 45, Resistance = 0, Stickiness = 5 });
			_Triggers.Add(new RangeTrigger(_Pitch) { Name = "Pitch-", MinValue = -45, MaxValue = -15, Resistance = 0, Stickiness = 5 });
		}

	    public void Initialize(Hand hand)
	    {
		    Id = hand.Id;
		    DetectedHand = hand;
		    StabilizedHand = Hand.Invalid;
		    CurrentHand = hand;
		    FinalHand = Hand.Invalid;
	    }

	    public bool Update(Frame frame)
	    {
		    CurrentFPS = frame.CurrentFramesPerSecond;
			if (IsFinalized)
				return false;

            var hand = frame.Hand(Id);
            if (!hand.IsValid)
            {
                // Our hand is not in this frame,
                // but we won't give up for 100ms.
                if (frame.Timestamp - CurrentHand.Frame.Timestamp < 100000)
                    return true;

                // Our hand is truly gone...
                FinalHand = CurrentHand;
                DetectedHand = Hand.Invalid;
                StabilizedHand = Hand.Invalid;
                CurrentHand = Hand.Invalid;
                _Stabilized.CurrentValue = false;
				return false;
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
                    return true;
            }

            _Velocity.Update(hand, frame);
            _X.Update(hand, frame);
            _Y.Update(hand, frame);
            _Z.Update(hand, frame);
            if (_Velocity.CurrentValue < 125)
            {
                _Pitch.Update(hand, frame);
                _Roll.Update(hand, frame);
                _Yaw.Update(hand, frame);
            }
            _FingerCount.Update(hand, frame);

			return true;
        }


	    public string Dump()
	    {
		    var desc = new StringBuilder();

		    desc.AppendFormat("Hand: {0}, FPS = {1}", Id, CurrentFPS);
		    if (IsStabilized)
			    desc.AppendFormat(", Stabilized, {0} Fingers", _FingerCount.CurrentValue);
		    else
                desc.Append(", Not Stabilized");
            desc.AppendFormat(", Velocity = {0}", _Velocity.CurrentValue);
            desc.AppendLine();

		    desc.AppendFormat("Position: X = {0}, Y = {1}, Z = {2}", _X.CurrentValue, _Y.CurrentValue, _Z.CurrentValue);
			desc.AppendLine();

			//desc.AppendFormat("Raw Orient.: Roll = {0}, Pitch = {1}, Yaw = {2}", CurrentHand.RollDegrees(), CurrentHand.PitchDegrees(), CurrentHand.YawDegrees());
			//desc.AppendLine();
			desc.AppendFormat("Orientation: Roll = {0}, Pitch = {1}, Yaw = {2}", _Roll.CurrentValue, _Pitch.CurrentValue, _Yaw.CurrentValue);
			desc.AppendLine();

		    if (_Triggers.Count > 0)
		    {
			    desc.Append("Activated Triggers: ");
			    foreach (var t in _Triggers)
				    if (t.IsTriggered)
					    desc.Append(t.Name).Append(", ");
			    desc.Length -= 2;
		    }

		    return desc.ToString();
	    }
    }
}
