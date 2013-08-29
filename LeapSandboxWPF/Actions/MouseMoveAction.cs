using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseMoveAction : PositionTrackingAction
    {
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

            _ScaleFactorX = (Configuration.Instance.MouseSensitivityX / 100.0) * screenWidth / motionWidth;
            _ScaleFactorY = (Configuration.Instance.MouseSensitivityY / 100.0) * screenHeight / motionHeight;
        }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes
        {
            get { return new[] { PositionTrackingAxis.Screen, PositionTrackingAxis.Table }; }
        }
        protected override void ApplyPositionUpdate(PersistentHand hand, Vector change, int velocity)
        {
            double scaleX;
            double scaleY;
            if (velocity > 150)
            {
                scaleX = _ScaleFactorX;
                scaleY = _ScaleFactorY;
            }
            else if (velocity > 100)
            {
                scaleX = _ScaleFactorX * 2.0 / 3.0;
                scaleY = _ScaleFactorY * 2.0 / 3.0;
            }
            else if (velocity > 50)
            {
                scaleX = _ScaleFactorX * 1.0 / 3.0;
                scaleY = _ScaleFactorY * 1.0 / 3.0;
            }
            else 
            {
                scaleX = 1;
                scaleY = 1;
            }
            var x = Convert.ToInt32(GetX(change) * 2 * scaleX);
            var y = Convert.ToInt32(GetY(change) * 2 * scaleY);

            InputSimulator.Mouse.MoveMouseBy(x, -y);
        }
    }
}
