using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseMoveAction : PositionTrackingAction
    {
        private static readonly int _Sensitivity = 3;

        protected override IEnumerable<PositionTrackingAxis> ValidAxes
        {
            get { return new[] { PositionTrackingAxis.Screen, PositionTrackingAxis.Table }; }
        }
        protected override void ApplyPositionUpdate(Vector change)
        {
            var x = Convert.ToInt32(GetX(change)) * _Sensitivity;
            var y = Convert.ToInt32(GetY(change)) * _Sensitivity;
            InputSimulator.Mouse.MoveMouseBy(x, -y);
        }
    }
}
