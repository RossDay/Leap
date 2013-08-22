using System;
using System.Text;
using Leap;

namespace Vyrolan.VMCS
{
    internal class PersistentHand : IFrameUpdate
    {
        #region Data Members and Properties
        public int Id { get; private set; }
        public Hand DetectedHand { get; private set; }
        //public long DetectedTime { get { return DetectedHand.Frame.Timestamp; } }
        public Hand FinalHand { get; private set; }
        public bool IsFinalized { get { return !DetectedHand.IsValid || FinalHand.IsValid; } }
        public Hand CurrentHand { get; private set; }
        public long CurrentHandTime { get; private set; }
        public long Duration { get { return CurrentHand.Frame.Timestamp - (StabilizedHand.IsValid ? StabilizedHand : DetectedHand).Frame.Timestamp; } }
        public float CurrentFPS { get; set; }

        public Hand StabilizedHand { get; private set; }
        private readonly BooleanHandState _Stabilized;
        public bool IsStabilized { get { return _Stabilized.CurrentValue; } }

        private readonly IntegerHandState _Velocity;
        private readonly IntegerHandState _X;
        private readonly IntegerHandState _Y;
        private readonly IntegerHandState _Z;
        private readonly IntegerHandState _Pitch;
        private readonly IntegerHandState _Roll;
        private readonly IntegerHandState _Yaw;
        private readonly IntegerHandState _FingerCount;

        public IntegerHandState VelocityState { get { return _Velocity; } }
        public IntegerHandState XState { get { return _X; } }
        public IntegerHandState YState { get { return _Y; } }
        public IntegerHandState ZState { get { return _Z; } }
        public IntegerHandState PitchState { get { return _Pitch; } }
        public IntegerHandState RollState { get { return _Roll; } }
        public IntegerHandState YawState { get { return _Yaw; } }
        public IntegerHandState FingerCountState { get { return _FingerCount; } }

        public PositionTracker HandTracker { get; private set; }
        public PositionTracker FingerTracker { get; private set; }

        public int Velocity { get { return _Velocity.CurrentValue; } }
        public int X { get { return _X.CurrentValue; } }
        public int Y { get { return _Y.CurrentValue; } }
        public int Z { get { return _Z.CurrentValue; } }
        public Vector Position { get { return new Vector(X, Y, Z); } }
        public int OffsetX { get { return (!IsStabilized ? 0 : _X.CurrentValue - (int)StabilizedHand.PalmPosition.x); } }
        public int OffsetY { get { return (!IsStabilized ? 0 : _Y.CurrentValue - (int)StabilizedHand.PalmPosition.y); } }
        public int OffsetZ { get { return (!IsStabilized ? 0 : _Z.CurrentValue - (int)StabilizedHand.PalmPosition.z); } }
        public Vector OffsetPosition { get { return new Vector(OffsetX, OffsetY, OffsetZ); } }
        public int Pitch { get { return _Pitch.CurrentValue; } }
        public int Roll { get { return _Roll.CurrentValue; } }
        public int Yaw { get { return _Yaw.CurrentValue; } }
        public int FingerCount { get { return _FingerCount.CurrentValue; } } 
        #endregion

        #region Constructor / Initialize
        public PersistentHand()
        {
            Id = 0;
            DetectedHand = Hand.Invalid;
            StabilizedHand = Hand.Invalid;
            CurrentHand = Hand.Invalid;
            FinalHand = Hand.Invalid;

            _Stabilized = new BooleanHandState(this, h => h.PalmVelocity.Magnitude < 50, 25000, long.MaxValue);
            _Velocity = new IntegerHandState(this, h => Convert.ToInt32(h.PalmVelocity.Magnitude), 50000);
            _X = new IntegerHandState(this, h => Convert.ToInt32(h.PalmPosition.x), 125000) { StableTime = 250000, StableDelta = 25, StableVelocity = 60 };
            _Y = new IntegerHandState(this, h => Convert.ToInt32(h.PalmPosition.y), 125000) { StableTime = 250000, StableDelta = 25, StableVelocity = 60 };
            _Z = new IntegerHandState(this, h => Convert.ToInt32(h.PalmPosition.z), 125000) { StableTime = 250000, StableDelta = 25, StableVelocity = 60 };
            _Pitch = new IntegerHandState(this, h => h.PitchDegrees(), 250000) { StableTime = 500000, StableDelta = 5, StableVelocity = int.MaxValue };
            _Roll = new IntegerHandState(this, h => h.RollDegrees(), 250000) { StableTime = 500000, StableDelta = 5, StableVelocity = int.MaxValue };
            _Yaw = new IntegerHandState(this, h => h.YawDegrees(), 250000) { StableTime = 500000, StableDelta = 5, StableVelocity = int.MaxValue };
            _FingerCount = new IntegerHandState(this, h => h.Fingers.Count, 0) { StableDelta = 5, StableTime = 50000, StableVelocity = int.MaxValue };

            HandTracker = new PositionTracker(this, h => h.Position, h => h.Velocity);
            FingerTracker = new PositionTracker(this
                                                , h => h.CurrentHand.Fingers.Leftmost.StabilizedTipPosition
                                                , h => Convert.ToInt32(h.CurrentHand.Fingers.Leftmost.TipVelocity.Magnitude)
                                               );
        }

        public void Initialize(Hand hand)
        {
            Id = hand.Id;
            DetectedHand = hand;
            StabilizedHand = Hand.Invalid;
            CurrentHand = hand;
            FinalHand = Hand.Invalid;
        } 
        #endregion

        #region Update
        public bool Update(Frame frame)
        {
            if (IsFinalized)
                return true;

            CurrentFPS = frame.CurrentFramesPerSecond;

            var hand = frame.Hand(Id);
            if (!hand.IsValid)
            {
                // Our hand is not in this frame,
                // but we won't give up for 25ms.
                if (frame.Timestamp - CurrentHandTime < 25000)
                    return true;

                // Our hand is truly gone...
                FinalHand = CurrentHand;
                DetectedHand = Hand.Invalid;
                StabilizedHand = Hand.Invalid;
                CurrentHand = Hand.Invalid;
                _Stabilized.CurrentValue = false;
                Id = 0;
                return true;
            }

            // We still exist in this frame, update current
            CurrentHand = hand;
            CurrentHandTime = frame.Timestamp;

            if (!IsStabilized)
            {
                // Check for stabilization complete
                if (_Stabilized.Update(frame))
                    StabilizedHand = hand;
            }

            _Velocity.Update(frame);
            _X.Update(frame);
            _Y.Update(frame);
            _Z.Update(frame);
            if (_Velocity.CurrentValue < 300)
            {
                _Pitch.Update(frame);
                _Roll.Update(frame);
                _Yaw.Update(frame);
            }
            _FingerCount.Update(frame);
            HandTracker.Update(frame);
            FingerTracker.Update(frame);

            return true;
        } 
        #endregion

        #region PromotePotentialHand
        public void PromotePotentialHand(PersistentHand potential)
        {
            _Velocity.InitValue(potential.Velocity);
            _X.InitValue(potential.X);
            _Y.InitValue(potential.Y);
            _Z.InitValue(potential.Z);
            _Pitch.InitValue(potential.Pitch);
            _Roll.InitValue(potential.Roll);
            _Yaw.InitValue(potential.Yaw);
            _FingerCount.InitValue(potential.FingerCount);
            HandTracker.InitPosition(potential.HandTracker.CurrentPosition);
            FingerTracker.InitPosition(potential.FingerTracker.CurrentPosition);

            Id = potential.Id;
            DetectedHand = potential.DetectedHand;
            FinalHand = potential.FinalHand;
            CurrentHand = potential.CurrentHand;
            CurrentFPS = potential.CurrentFPS;
            StabilizedHand = potential.StabilizedHand;

            potential.FinalHand = potential.CurrentHand;
            potential.DetectedHand = Hand.Invalid;
            potential.StabilizedHand = Hand.Invalid;
            potential.CurrentHand = Hand.Invalid;
            potential._Stabilized.CurrentValue = false;
            potential.Id = 0;
        } 
        #endregion

        #region Dump
        public string Dump()
        {
            var desc = new StringBuilder();

            desc.AppendFormat("Hand: {0}, FPS = {1}", Id, CurrentFPS);
            if (IsStabilized)
                desc.Append(", Stabilized");
            else
                desc.Append(", Not Stabilized");
            desc.AppendFormat(", {0} Fingers, Velocity = {1}", _FingerCount.CurrentValue, _Velocity.CurrentValue);
            desc.AppendLine();

            //desc.AppendFormat("Raw Pos.: X = {0:0}, Y = {1:0}, Z = {2:0}", CurrentHand.PalmPosition.x, CurrentHand.PalmPosition.y, CurrentHand.PalmPosition.z);
            //desc.AppendLine();
            desc.AppendFormat("Position: X = {0}, Y = {1}, Z = {2}", _X.CurrentValue, _Y.CurrentValue, _Z.CurrentValue);
            desc.AppendLine();

            //desc.AppendFormat("Raw Orient.: Roll = {0}, Pitch = {1}, Yaw = {2}", CurrentHand.RollDegrees(), CurrentHand.PitchDegrees(), CurrentHand.YawDegrees());
            //desc.AppendLine();
            desc.AppendFormat("Orientation: Roll = {0}, Pitch = {1}, Yaw = {2}", _Roll.CurrentValue, _Pitch.CurrentValue, _Yaw.CurrentValue);
            desc.AppendLine();

            return desc.ToString();
        } 
        #endregion
    }
}
