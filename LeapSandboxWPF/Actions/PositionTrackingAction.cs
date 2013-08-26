using System;
using System.Collections.Generic;
using System.Linq;
using Leap;

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

        protected PositionTrackingAction(string name) : base(name) { }

        protected abstract IEnumerable<PositionTrackingAxis> ValidAxes { get; }
        private PositionTrackingAxis _Axis;
        public PositionTrackingAxis Axis
        {
            get { return _Axis; }
            set
            {
                if (!ValidAxes.Contains(value))
                    throw new ArgumentOutOfRangeException();
                _Axis = value;
            }
        }

        protected Vector CurrentPosition { get; set; }
        protected bool IsEnabled { get; private set; }
        private PositionTracker _Tracker;
        public PositionTracker Tracker 
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

        protected override void BeginImpl()
        {
            Tracker.Enable();
            CurrentPosition = NormalizeVectorToAxis(Tracker.CurrentPosition);
            IsEnabled = true;
        }

        protected override void EndImpl()
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

        protected float GetX(Vector vector)
        {
            var temp = NormalizeVectorToAxis(vector);
            if (Axis == PositionTrackingAxis.X || Axis == PositionTrackingAxis.Y || Axis == PositionTrackingAxis.Z)
                return temp.x + temp.y + temp.z;
            return temp.x;
        }
        protected float GetY(Vector vector)
        {
            var temp = NormalizeVectorToAxis(vector);
            if (Axis == PositionTrackingAxis.X || Axis == PositionTrackingAxis.Y || Axis == PositionTrackingAxis.Z)
                return 0;
            return temp.y + temp.z;
        }

        protected abstract void ApplyPositionUpdate(PersistentHand hand, Vector change, int velocity);
        private void OnPositionUpdated(object sender, PositionTrackerEventArgs e)
        {
            if (!IsEnabled) return;

            var temp = NormalizeVectorToAxis(e.NewPosition);
            var change = temp - CurrentPosition;
            if (change.Magnitude >= MinDistance)
            {
                ApplyPositionUpdate(_Tracker.Hand, change, e.Velocity);
                if (!IsContinuous)
                    CurrentPosition = temp;
            }
        }
    }
}
