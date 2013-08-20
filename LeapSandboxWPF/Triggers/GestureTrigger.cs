using System.Linq;
using Vyrolan.VMCS.Gestures;

namespace Vyrolan.VMCS.Triggers
{
    internal abstract class GestureTrigger : BaseTrigger
    {
        public PersistentHand Hand { get; set; }
        public bool RequiresStabilized { get; set; }

        protected abstract bool CheckGesture(VyroGesture gesture);

        private bool CheckHand(VyroGesture gesture)
        {
            // No or Finalized Hand means it can be any Hand
            if (Hand == null || Hand.IsFinalized)
                return true;

            // If stabilized required and we're not, not a match
            if (RequiresStabilized && !Hand.IsStabilized)
                return false;

            // If the gesture has our hand in it, it's a match
            return gesture.HandIds.Contains(Hand.Id);
        }

        public bool Check(VyroGesture gesture)
        {
            return (CheckHand(gesture) && CheckGesture(gesture));
        }
    }

    internal class GestureTriggerCircle : GestureTrigger
    {
        public bool IsClockwise { get; set; }
        public int MinRadius { get; set; }
        public int MaxRadius { get; set; }

        protected override bool CheckGesture(VyroGesture gesture)
        {
            var circle = gesture as VyroGestureCircle;
            if (circle == null) return false;
            return (circle.IsClockwise == IsClockwise && circle.Radius >= MinRadius && circle.Radius <= MaxRadius);
        }
    }

    internal class GestureTriggerSwipe : GestureTrigger
    {
        public int MinAngle { get; set; }
        public int MaxAngle { get; set; }
        public long MinDistance { get; set; }
        public long MaxDistance { get; set; }
        public int MinVelocity { get; set; }
        public int MaxVelocity { get; set; }

        protected override bool CheckGesture(VyroGesture gesture)
        {
            var swipe = gesture as VyroGestureSwipe;
            if (swipe == null) return false;

            var angle = swipe.Direction.RollDegrees();
            if (angle < 0)
                angle += 360;

            return (
                       swipe.Velocity >= MinVelocity && swipe.Velocity <= MaxVelocity
                       &&
                       swipe.Distance >= MinDistance && swipe.Distance <= MaxDistance
                       &&
                       angle >= MinAngle && angle <= MaxAngle
                   );
        }
    }
}
