using Vyrolan.VMCS.Gestures;

namespace Vyrolan.VMCS.Triggers
{
    internal abstract class GestureTrigger : DiscreteTrigger
    {
        protected GestureTrigger(string name) : base(name) { }
        
        protected abstract bool CheckGesture(VyroGesture gesture);

        public bool Check(VyroGesture gesture)
        {
            return (CheckHand(gesture.HandIds) && CheckGesture(gesture));
        }

        public void Activate()
        {
            IsTriggered = true;
        }
    }

    internal class GestureTriggerCircle : GestureTrigger
    {
        [ConfigurationParameter("isClockwise")]
        public bool IsClockwise { get; set; }
        [ConfigurationParameter("minRadius")]
        public int MinRadius { get; set; }
        [ConfigurationParameter("maxRadius")]
        public int MaxRadius { get; set; }

        public GestureTriggerCircle(string name) : base(name) { }

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

        public GestureTriggerSwipe(string name) : base(name) { }

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
