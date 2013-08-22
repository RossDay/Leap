using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal class PositionTrackerEventArgs : EventArgs
    {
        public Vector NewPosition { get; set; }
    }

    internal class PositionTracker : IFrameUpdate
    {
        public PersistentHand Hand { get; private set; }
        private Func<PersistentHand, Vector> PositionGetter { get; set; }
        private readonly object _EnabledLock = new object();
        private int EnabledCount { get; set; }
        public bool IsEnabled { get { return (EnabledCount > 0); } }

        public PositionTracker(PersistentHand hand, Func<PersistentHand, Vector> positionGetter)
        {
            Hand = hand;
            PositionGetter = positionGetter;
            _CurrentPosition = PositionGetter(Hand);
        }

        public bool Update(Frame frame)
        {
            if (IsEnabled)
                CurrentPosition = PositionGetter(Hand);
            return true;
        }

        public void Enable()
        {
            lock (_EnabledLock)
            {
                if (!IsEnabled)
                    _CurrentPosition = PositionGetter(Hand);
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

        public void InitPosition(Vector position)
        {
            _CurrentPosition = position;
        }

        public event EventHandler<PositionTrackerEventArgs> PositionUpdated;
        protected void OnPositionUpdated()
        {
            if (PositionUpdated != null && IsEnabled)
                PositionUpdated(this, new PositionTrackerEventArgs {NewPosition = CurrentPosition});
        }
    }
}
