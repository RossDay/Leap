using System.Collections.Generic;
using System.Linq;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal abstract class VyroLeapGesture : VyroGesture
    {
        protected abstract Gesture LeapGesture { get; }
        public int LeapGestureId { get { return (LeapGesture.IsValid ? LeapGesture.Id : 0); } }

        public override IEnumerable<int> HandIds
        {
            get { return LeapGesture.Hands.Select(h => h.Id); }
        }

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

        protected abstract VyroGestureState UpdateGestureImpl(Frame frame);

        protected override VyroGestureState UpdateGesture(Frame frame)
        {
            var newGesture = frame.Gesture(LeapGesture.Id);
            
            if (newGesture.IsValid && newGesture.Type == LeapGesture.Type)
                return UpdateGestureImpl(frame);

            return ((frame.Timestamp - LastUpdateTime) > 25000 ? VyroGestureState.ContinuousComplete : VyroGestureState.Progressing);
        }
    }
}
