﻿using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    class ScrollAction : PositionTrackingAction
    {
        public int Lines { get; set; }
        public bool IsAccelerated { get; set; }
        public bool IsInverted { get; set; }

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

    class DiscreteScrollAction : DiscreteAction
    {
        public int Lines { get; set; }
        public bool IsUp { get; set; }

        protected override void Fire()
        {
            for (var i = 0; i < Lines; i++)
                Native.ScrollActiveWindow(IsUp);
        }
    }
}
