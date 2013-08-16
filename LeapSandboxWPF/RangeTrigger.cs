using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeapSandboxWPF
{
    class RangeTrigger : BaseTrigger
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Resistance { get; set; }
        public int Stickiness { get; set; }

        public RangeTrigger(HandState<int> state)
        {
            state.ValueChanged += OnStateValueChanged;
        }

        private void OnStateValueChanged(object sender, HandStateChangedEventArgs<int> e)
        {
            var newValue = e.NewValue;
            if (IsTriggered)
            {
                if (newValue < (MinValue - Stickiness) || newValue > (MaxValue + Stickiness))
                    IsTriggered = false;
            }
            else
            {
                if (newValue >= (MinValue + Resistance) && newValue <= (MaxValue - Resistance))
                    IsTriggered = true;
            }
        }
    }
}
