using Vyrolan.VMCS.Gestures;

namespace Vyrolan.VMCS.Triggers
{
    internal abstract class GestureTrigger : BaseTrigger
    {
        public abstract bool CheckGesture(VyroGesture gesture);
    }

    internal class GestureTriggerCircle : GestureTrigger
    {
        public bool IsClockwise { get; set; }
        public int MinRadius { get; set; }
        public int MaxRadius { get; set; }

        public override bool CheckGesture(VyroGesture gesture)
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
        public int MinVelocity { get; set; }
        public int MaxVelocity { get; set; }

        public override bool CheckGesture(VyroGesture gesture)
        {
            var swipe = gesture as VyroGestureSwipe;
            if (swipe == null) return false;

            var angle = swipe.Direction.RollDegrees();
            if (angle < 0)
                angle += 360;

            return (
                       swipe.Velocity >= MinVelocity && swipe.Velocity <= MaxVelocity
                       &&
                       angle >= MinAngle && angle <= MaxAngle
                   );
        }
    }
}
