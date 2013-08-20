using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS.Actions
{
    internal class PositionTrackingAction : BaseAction
    {
        private PositionTracker Tracker { get; set; }
        private Vector StartingPosition { get; set; }
        private bool IsEnabled { get; set; }

        public PositionTrackingAction(BaseTrigger trigger, PositionTracker tracker)
            : base(trigger)
        {
            Tracker = tracker;
            Tracker.PositionUpdated += OnPositionUpdated;
        }

        protected override void Begin()
        {
            Tracker.Enable();
            StartingPosition = Tracker.CurrentPosition;
            IsEnabled = true;
        }

        protected override void End()
        {
            IsEnabled = false;
            Tracker.Disable();
        }

        void OnPositionUpdated(object sender, PositionTrackerEventArgs e)
        {
            var position = e.NewPosition;
        }
    }
}
