using System;
using System.Threading;
using Leap;

namespace Vyrolan.VMCS
{
    internal class PositionTrackerEventArgs : EventArgs
    {
        public Vector NewPosition { get; set; }
    }

    internal class PositionTracker : IFrameUpdate
    {
        private Func<Vector> PositionGetter { get; set; }
        private readonly object _EnabledLock = new object();
        private int EnabledCount { get; set; }
        public bool IsEnabled { get { return (EnabledCount > 0); } }

        public PositionTracker(Func<Vector> positionGetter)
        {
            PositionGetter = positionGetter;
            CurrentPosition = PositionGetter();
        }

        public bool Update(Frame frame)
        {
            if (IsEnabled)
                CurrentPosition = PositionGetter();
            return true;
        }

        public void Enable()
        {
            lock (_EnabledLock)
            {
                if (!IsEnabled)
                    CurrentPosition = PositionGetter();
                ++EnabledCount;
            }
        }

        public void Disable()
        {
            lock (_EnabledLock)
                --EnabledCount;
        }

        private Vector _CurrentPosition;
        public Vector CurrentPosition 
        {
            get { return _CurrentPosition; }
            private set 
            {
                _CurrentPosition = value;
                OnPositionUpdated();
            }
        }

        public event EventHandler<PositionTrackerEventArgs> PositionUpdated;
        protected void OnPositionUpdated()
        {
            if (PositionUpdated != null && IsEnabled)
                PositionUpdated(this, new PositionTrackerEventArgs {NewPosition = CurrentPosition});
        }
    }
}
