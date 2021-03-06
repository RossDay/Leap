﻿using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseScrollAction : PositionTrackingAction
    {
        [ConfigurationParameter("lines")]
        public int Lines { get; set; }
        [ConfigurationParameter("isAccel")]
        public bool IsAccelerated { get; set; }
        [ConfigurationParameter("isInverted")]
        public bool IsInverted { get; set; }

        public MouseScrollAction(string name) : base(name) { }

        protected override IEnumerable<PositionTrackingAxis> ValidAxes 
        {
            get { return new[] { PositionTrackingAxis.X, PositionTrackingAxis.Y, PositionTrackingAxis.Z }; }
        }
        protected override void ApplyPositionUpdate(PersistentHand hand, Vector change, int velocity)
        {
            var linesToScroll = (IsInverted ? -Lines : Lines) * Math.Sign(GetX(change));
            if (IsAccelerated)
                linesToScroll *= Convert.ToInt32(Math.Floor(change.Magnitude / MinDistance));
            InputSimulator.Mouse.VerticalScroll(linesToScroll);
        }
    }

    internal class DiscreteMouseScrollAction : DiscreteAction
    {
        public DiscreteMouseScrollAction(string name) : base(name) { }

        [ConfigurationParameter("lines")]
        public int Lines { get; set; }
        [ConfigurationParameter("isUp")]
        public bool IsUp { get; set; }

        protected override void Fire()
        {
            InputSimulator.Mouse.VerticalScroll(Lines * (IsUp ? 1 : -1));
        }
    }
}
