using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal class HandStateChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
        public long CurrentTime { get; set; }
    }

    internal abstract class HandState<T> : IFrameUpdate
    {
        public PersistentHand Hand { get; private set; }

        protected HandState(PersistentHand hand)
        {
            Hand = hand;
        }

        public virtual bool Update(Frame frame)
        {
            _CurrentTime = frame.Timestamp;
            return true;
        }

        private long _CurrentTime;
        private T _CurrentValue;
        public T CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (!CurrentValue.Equals(value))
                {
                    var old = _CurrentValue;
                    _CurrentValue = value;
                    OnValueChanged(old, _CurrentValue);
                }
            }
        }

        public virtual void InitValue(T value)
        {
            _CurrentValue = value;
        }

        public event EventHandler<HandStateChangedEventArgs<T>> ValueChanged;
        protected void OnValueChanged(T oldValue, T newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new HandStateChangedEventArgs<T> { OldValue = oldValue, NewValue = newValue, CurrentTime = _CurrentTime });
        }
    }
}
