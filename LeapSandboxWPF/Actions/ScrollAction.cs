using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    internal class ScrollAction : PositionTrackingAction
    {
        [ConfigurationParameter("lines")]
        public int Lines { get; set; }
        [ConfigurationParameter("isAccel")]
        public bool IsAccelerated { get; set; }
        [ConfigurationParameter("isInverted")]
        public bool IsInverted { get; set; }

        public ScrollAction(string name) : base(name) { }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes
        {
            get { return new[] { PositionTrackingAxis.X, PositionTrackingAxis.Y, PositionTrackingAxis.Z }; }
        }
        protected override void ApplyPositionUpdate(PersistentHand hand, Vector change, int velocity)
        {
            var linesToScroll = Lines;
            if (IsAccelerated)
                linesToScroll *= Convert.ToInt32(Math.Floor(Math.Abs(change.Magnitude) / MinDistance));

            var isUp = ((IsInverted ? -1 : 1) * Math.Sign(GetX(change)) == 1);

            for (var i = 0; i < linesToScroll; i++)
                Native.ScrollActiveWindow(isUp);
        }
    }

    internal class DiscreteScrollAction : DiscreteAction
    {
        [ConfigurationParameter("lines")]
        public int Lines { get; set; }
        [ConfigurationParameter("isUp")]
        public bool IsUp { get; set; }

        public DiscreteScrollAction(string name) : base(name) { }

        protected override void Fire()
        {
            for (var i = 0; i < Lines; i++)
                Native.ScrollActiveWindow(IsUp);
        }
    }
}
