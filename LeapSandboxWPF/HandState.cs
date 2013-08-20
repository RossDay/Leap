using System;
using Leap;

namespace Vyrolan.VMCS
{
    internal class HandStateChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }

    internal abstract class HandState<T>
    {
        public abstract T Update(Hand hand, Frame frame);

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

        public event EventHandler<HandStateChangedEventArgs<T>> ValueChanged;
        protected void OnValueChanged(T oldValue, T newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new HandStateChangedEventArgs<T> { OldValue = oldValue, NewValue = newValue });
        }
    }
}
