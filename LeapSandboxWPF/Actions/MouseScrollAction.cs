using System;
using System.Collections.Generic;
using Leap;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS.Actions
{
    class MouseScrollAction : PositionTrackingAction
    {
        public int Lines { get; set; }
        public bool IsAccelerated { get; set; }
        public bool IsInverted { get; set; }

        public MouseScrollAction(BaseTrigger trigger)
            : base(trigger)
        {
        }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes 
        {
            get { return new[] { PositionTrackingAxis.X, PositionTrackingAxis.Y, PositionTrackingAxis.Z }; }
        }
        protected override void ApplyPositionUpdate(Vector change)
        {
            var linesToScroll = (IsInverted ? -Lines : Lines);
            if (IsAccelerated)
                linesToScroll *= Convert.ToInt32(Math.Floor(change.Magnitude / MinDistance));
            InputSimulator.Mouse.VerticalScroll(linesToScroll);
        }
    }

    internal class MouseMoveAction : PositionTrackingAction
    {
        private static readonly int _Sensitivity = 10;
        public MouseMoveAction(BaseTrigger trigger)
            : base(trigger)
        {
        }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes
        {
            get { return new[] { PositionTrackingAxis.Screen, PositionTrackingAxis.Table }; }
        }
        protected override void ApplyPositionUpdate(Vector change)
        {
            var x = Convert.ToInt32(change.x) * _Sensitivity;
            var y = Convert.ToInt32( Axis == PositionTrackingAxis.Screen ? change.y : change.z ) * _Sensitivity;
            InputSimulator.Mouse.MoveMouseBy(x, y);
        }
    }
}
