using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseMoveAction : PositionTrackingAction
    {
        private static readonly int _SensitivityX = 60;
        private static readonly int _SensitivityY = 40;
        private double _ScaleFactorX;
        private double _ScaleFactorY;

        public MouseMoveAction(string name)
            : base(name)
        {
            SetSensitivities();
        }

        private void SetSensitivities()
        {
            var screenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var screenHeight = System.Windows.SystemParameters.VirtualScreenHeight;

            double motionWidth, motionHeight;
            if (screenWidth >= screenHeight)
            {
                motionWidth = 300;
                motionHeight = 300.0 / screenWidth * screenHeight;
            }
            else
            {
                motionWidth = 300.0 / screenHeight * screenWidth;
                motionHeight = 300;
            }

            _ScaleFactorX = (_SensitivityX / 100.0) * screenWidth / motionWidth;
            _ScaleFactorY = (_SensitivityY / 100.0) * screenHeight / motionHeight;
        }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes
        {
            get { return new[] { PositionTrackingAxis.Screen, PositionTrackingAxis.Table }; }
        }
        protected override void ApplyPositionUpdate(PersistentHand hand, Vector change, int velocity)
        {
            var x = Convert.ToInt32(GetX(change) * 2 * (velocity > 50 ? _ScaleFactorX : 1));
            var y = Convert.ToInt32(GetY(change) * 2 * (velocity > 50 ? _ScaleFactorY : 1));

            InputSimulator.Mouse.MoveMouseBy(x, -y);
        }
    }
}
