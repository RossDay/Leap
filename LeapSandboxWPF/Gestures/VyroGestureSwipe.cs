using System;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal class VyroGestureSwipe : VyroLeapGesture
    {
        private SwipeGesture Gesture { get; set; }
        protected override Gesture LeapGesture { get { return Gesture; } }

        public Vector Direction { get { return Gesture.Direction; } }
        public long Duration { get { return Gesture.Duration; } }
        private readonly SmoothedIntegerState _Velocity = new SmoothedIntegerState(50000);
        public long Velocity { get { return _Velocity.CurrentValue; } }

        protected static VyroGesture CreateFromLeapGesture(SwipeGesture gesture)
        {
            var s = new VyroGestureSwipe { Gesture = gesture };
            s._Velocity.Initialize(Convert.ToInt64(s.Gesture.Speed));
            return s;
        }

        protected override VyroGestureState UpdateGestureImpl(Frame frame)
        {
            Gesture = new SwipeGesture(frame.Gesture(Gesture.Id));

            switch (Gesture.State)
            {
                case Leap.Gesture.GestureState.STATEINVALID:
                    return VyroGestureState.Invalid;
                case Leap.Gesture.GestureState.STATESTOP:
                    return VyroGestureState.DiscreteComplete;
                case Leap.Gesture.GestureState.STATEUPDATE:
                    _Velocity.Update(Convert.ToInt64(Gesture.Speed), frame);
                    return VyroGestureState.Progressing;
                default:
                    return VyroGestureState.Progressing;
            }
        }
    }
}
