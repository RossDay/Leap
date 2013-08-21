using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS.Actions
{
    internal enum PositionTrackingAxis
    {
        X, Y, Z, Screen, Table
    }

    internal abstract class PositionTrackingAction : BaseAction
    {
        public int MinDistance { get; set; }
        public bool IsContinuous { get; set; }

        protected abstract IEnumerable<PositionTrackingAxis> ValidAxes { get; }
        private PositionTrackingAxis _Axis;
        public PositionTrackingAxis Axis
        {
            get { return _Axis; }
            set
            {
                if (!ValidAxes.Contains(value))
                    throw new ArgumentOutOfRangeException("Axis");
                _Axis = value;
            }
        }

        protected Vector CurrentPosition { get; set; }
        protected bool IsEnabled { get; private set; }
        private PositionTracker _Tracker;
        protected PositionTracker Tracker 
        {
            get { return _Tracker; }
            set
            {
                if (_Tracker != null)
                    _Tracker.PositionUpdated -= OnPositionUpdated;
                _Tracker = value;
                _Tracker.PositionUpdated += OnPositionUpdated;
            }
        }

        public PositionTrackingAction(BaseTrigger trigger)
            : base(trigger)
        {
        }

        protected override void Begin()
        {
            Tracker.Enable();
            CurrentPosition = Tracker.CurrentPosition;
            IsEnabled = true;
        }

        protected override void End()
        {
            IsEnabled = false;
            Tracker.Disable();
        }

        private Vector NormalizeVectorToAxis(Vector vector)
        {
            switch (Axis)
            {
                case PositionTrackingAxis.X:
                    return new Vector(vector.x, 0, 0);
                case PositionTrackingAxis.Y:
                    return new Vector(0, vector.y, 0);
                case PositionTrackingAxis.Z:
                    return new Vector(0, 0, vector.z);
                case PositionTrackingAxis.Screen:
                    return new Vector(vector.x, vector.y, 0);
                default:
                    return new Vector(vector.x, 0, vector.z);
            }
        }

        protected abstract void ApplyPositionUpdate(Vector change);
        private void OnPositionUpdated(object sender, PositionTrackerEventArgs e)
        {
            var temp = NormalizeVectorToAxis(e.NewPosition);
            var change = temp - CurrentPosition;
            if (change.Magnitude >= MinDistance)
            {
                ApplyPositionUpdate(change);
                if (!IsContinuous)
                    CurrentPosition = temp;
            }
        }
    }
}
