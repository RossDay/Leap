using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace Vyrolan.VMCS
{
    internal class GestureDispatcher
    {
        void foo()
        {
            //VyroGestureCircle c;
            //VyroGestureSwipe s;
        }
    }

    internal class GestureRecognizer : IFrameUpdate
    {
        private readonly LinkedList<VyroGesture> _CurrentGestures = new LinkedList<VyroGesture>();
        private readonly GestureDispatcher _Dispatcher;

        public bool Update(Frame frame)
        {
            // List of Ids for current Leap Gestures
            var currentIds = _CurrentGestures.Where(g => g is VyroLeapGesture).Select(g => ((VyroLeapGesture)g).LeapGestureId).ToList();
            // Find any new Leap Gestures
            var newLeapGestures = frame.Gestures().Where(ge => !currentIds.Contains(ge.Id));

            // Update current gestures dispatching activated ones and removing the invalid/complete ones
            var item = _CurrentGestures.First;
            while (item != null)
            {
                var next = item.Next;
                var state = item.Value.Update(frame);
                if (state == VyroGestureState.DiscreteComplete || state == VyroGestureState.IterationComplete)
                    ; // TODO: Dispatch
                if (state == VyroGestureState.Invalid || state == VyroGestureState.DiscreteComplete || state == VyroGestureState.ContinuousComplete)
                    _CurrentGestures.Remove(item);
                item = next;
            }

            // Add new Leap Gestures
            foreach (var g in newLeapGestures)
                _CurrentGestures.AddLast(VyroLeapGesture.CreateFromLeapGesture(g));

            return true;
        }
    }

    internal enum VyroGestureState
    {
        Invalid,
        ContinuousComplete,
        DiscreteComplete,
        Progressing,
        IterationComplete,
    }

    internal abstract class VyroGesture
    {
        protected long LastUpdateTime { get; set; }

        protected virtual VyroGestureState UpdateGesture(Frame frame)
        {
            return VyroGestureState.Invalid;
        }
        public VyroGestureState Update(Frame frame)
        {
            var state = UpdateGesture(frame);
            LastUpdateTime = frame.Timestamp;
            return state;
        }
    }

    internal abstract class VyroLeapGesture : VyroGesture
    {
        protected abstract Gesture LeapGesture { get; }
        public int LeapGestureId { get { return (LeapGesture.IsValid ? LeapGesture.Id : 0); } }

        public static VyroGesture CreateFromLeapGesture(Gesture gesture)
        {
            switch (gesture.Type)
            {
                case Gesture.GestureType.TYPECIRCLE:
                    return CreateFromLeapGesture(new CircleGesture(gesture));
                case Gesture.GestureType.TYPESWIPE:
                    return CreateFromLeapGesture(new SwipeGesture(gesture));
            }
            return null;
        }

        protected abstract VyroGestureState UpdateGestureImpl(Frame frame);

        protected override VyroGestureState UpdateGesture(Frame frame)
        {
            var newGesture = frame.Gesture(LeapGesture.Id);

            if (newGesture.IsValid && newGesture.Type == LeapGesture.Type)
                return UpdateGestureImpl(frame);

            return ((frame.Timestamp - LastUpdateTime) > 25000 ? VyroGestureState.ContinuousComplete : VyroGestureState.Progressing);
        }
    }

    internal class VyroGestureCircle : VyroLeapGesture
    {
        private CircleGesture Gesture { get; set; }
        protected override Gesture LeapGesture { get { return Gesture; } }

        public int Progress { get; private set; }
        private readonly SmoothedIntegerState _Radius = new SmoothedIntegerState(50000);
        public long Radius { get { return _Radius.CurrentValue; } }
        public bool IsClockwise { get; private set; }

        protected static VyroGesture CreateFromLeapGesture(CircleGesture gesture)
        {
            var c = new VyroGestureCircle {Gesture = gesture};
            c._Radius.Initialize(Convert.ToInt64(c.Gesture.Radius));
            c.IsClockwise = (c.Gesture.Pointable.Direction.AngleTo(c.Gesture.Normal) <= Math.PI / 4);
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
            var s = new VyroGestureSwipe {Gesture = gesture};
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



    internal class SmoothedIntegerState
    {
        public long SmoothTime { get; set; }
        public long CurrentValue { get; private set; }
        private double SmoothedValue { get; set; }

        public SmoothedIntegerState(long smoothTime)
        {
            SmoothTime = smoothTime;
        }

        public void Initialize(long initValue)
        {
            CurrentValue = initValue;
        }
        public long Update(long newValue, Frame frame)
        {
            var frameTimeDistance = 1000000f / frame.CurrentFramesPerSecond;
            var frameSmoothedImpact = frameTimeDistance / SmoothTime;

            //_CurrentValue = _CurrentValue*(1.0 - frameSmoothedImpact) + newValue*frameSmoothedImpact;
            SmoothedValue = CurrentValue * (1.0 - frameSmoothedImpact) + newValue * frameSmoothedImpact;
            CurrentValue = Convert.ToInt64(SmoothedValue);

            return CurrentValue;
        }
    }






    internal class GestureTrigger : BaseTrigger
    {
        public VyroGesture Gesture { get; set; }
    }

    internal class GestureTriggerCircle : GestureTrigger
    {
        private new VyroGestureCircle Gesture
        {
            get { return base.Gesture as VyroGestureCircle; }
        }
    }
}
