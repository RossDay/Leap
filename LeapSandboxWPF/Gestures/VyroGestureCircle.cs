using System;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal class VyroGestureCircle : VyroLeapGesture
    {
        private CircleGesture Gesture { get; set; }
        protected override Gesture LeapGesture { get { return Gesture; } }

        public int Progress { get; private set; }
        private readonly SmoothedIntegerState _Radius = new SmoothedIntegerState(50000);
        public long Radius { get { return _Radius.CurrentValue; } }
        public bool IsClockwise { get; private set; }

        public static VyroGesture CreateFromLeapGesture(CircleGesture gesture)
        {
            var c = new VyroGestureCircle { Gesture = gesture };
            c._Radius.Initialize(Convert.ToInt64(c.Gesture.Radius));
            c.IsClockwise = (c.Gesture.Pointable.Direction.AngleTo(c.Gesture.Normal) <= Math.PI / 4);
            ControlSystem.StaticLog(string.Format("{0} <-- {1}", c.IsClockwise, c.Gesture.Pointable.Direction.AngleTo(c.Gesture.Normal)));
            return c;
        }

        protected override VyroGestureState UpdateGestureImpl(Frame frame)
        {
            Gesture = new CircleGesture(frame.Gesture(Gesture.Id));

            switch (Gesture.State)
            {
                case Leap.Gesture.GestureState.STATEINVALID:
                    return VyroGestureState.Invalid;
                case Leap.Gesture.GestureState.STATESTOP:
                    return VyroGestureState.ContinuousComplete;
                case Leap.Gesture.GestureState.STATEUPDATE:
                    _Radius.Update(Convert.ToInt64(Gesture.Radius), frame);
                    if (Gesture.Progress > (Progress + 1.0))
                    {
                        Progress = Convert.ToInt32(Math.Floor(Gesture.Progress));
                        return VyroGestureState.IterationComplete;
                    }
                    return VyroGestureState.Progressing;
                default:
                    return VyroGestureState.Progressing;
            }
        }
    }
}
