using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
    internal class GestureRecognizer : IFrameUpdate
    {
        private readonly LinkedList<VyroGesture> _CurrentGestures = new LinkedList<VyroGesture>();

        public bool Update(Frame frame)
        {
            var item = _CurrentGestures.First;
            while (item != null)
            {
                var next = item.Next;
                if (!item.Value.Update(frame))
                    _CurrentGestures.Remove(item);
                item = next;
            }

            // List of Ids for current Leap Gestures
            var currentIds = _CurrentGestures.Where(g => g is VyroLeapGesture).Select(g => ((VyroLeapGesture)g).LeapGesture.Id).ToList();

            // Look for new Leap Gestures
            foreach (var g in frame.Gestures().Where(ge => !currentIds.Contains(ge.Id)))
                _CurrentGestures.AddLast(VyroLeapGesture.CreateFromLeapGesture(g));

            return true;
        }
    }

    internal class VyroGesture : IFrameUpdate
    {
        private long _LastUpdateTime;

        public virtual bool Update(Frame frame)
        {
            _LastUpdateTime = frame.Timestamp;
            return false;
        }

    }

    internal abstract class VyroLeapGesture : VyroGesture
    {
        public abstract Gesture LeapGesture { get; }


        public static VyroGesture CreateFromLeapGesture(Gesture gesture)
        {
            switch (gesture.Type)
            {
                case Gesture.GestureType.TYPECIRCLE:
                    return VyroGestureCircle.CreateFromLeapGesture(new CircleGesture(gesture));
                case Gesture.GestureType.TYPESWIPE:
                    return VyroGestureSwipe.CreateFromLeapGesture(new SwipeGesture(gesture));
            }
            return null;
        }
    }

    internal class VyroGestureCircle : VyroLeapGesture
    {
        private CircleGesture _Gesture;
        public override Gesture LeapGesture { get { return _Gesture; } }

        public static VyroGesture CreateFromLeapGesture(CircleGesture gesture)
        {
            return new VyroGestureCircle {_Gesture = gesture};
        }

    }


    internal class VyroGestureSwipe : VyroLeapGesture
    {
        private SwipeGesture _Gesture;
        public override Gesture LeapGesture { get { return _Gesture; } }

        public static VyroGesture CreateFromLeapGesture(SwipeGesture gesture)
        {
            return new VyroGestureSwipe {_Gesture = gesture};
        }
    }








    internal class GestureTrigger : BaseTrigger
    {
        public VyroGesture Gesture { get; set; }
    }

    internal class GestureCircleTrigger : GestureTrigger
    {
        private new VyroGestureCircle Gesture
        {
            get { return base.Gesture as VyroGestureCircle; }
        }
    }
}
