
namespace Vyrolan.VMCS.Triggers
{
    internal class RangeTrigger : BaseTrigger
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Resistance { get; set; }
        public int Stickiness { get; set; }
        public long ResistanceTime { get; set; }
        public long StickinessTime { get; set; }
        private long LastChangeTime { get; set; }

        public RangeTrigger(HandState<int> state)
        {
            state.ValueChanged += OnStateValueChanged;
            ResistanceTime = 1000000;
            StickinessTime = 1000000;
        }

        private void OnStateValueChanged(object sender, HandStateChangedEventArgs<int> e)
        {
            var newValue = e.NewValue;
            if (IsTriggered)
            {
                // If outside the actual range, switch immediately
                if (newValue < MinValue || newValue > MaxValue)
                {
                    LastChangeTime = 0;
                    IsTriggered = false;
                }
                else if (newValue < (MinValue - Stickiness) || newValue > (MaxValue + Stickiness))
                {
                    // first time outside relaxed range, start time
                    if (LastChangeTime == 0)
                        LastChangeTime = e.CurrentTime;
                    // if outside relaxed range long enough, switch
                    else if (e.CurrentTime - LastChangeTime > ResistanceTime)
                    {
                        LastChangeTime = 0;
                        IsTriggered = false;
                    }
                }
            }
            else
            {
                // If inside the actual range, switch immediately
                if (newValue >= MinValue && newValue <= MaxValue)
                {
                    LastChangeTime = 0;
                    IsTriggered = true;
                }
                else if (newValue >= (MinValue + Resistance) && newValue <= (MaxValue - Resistance))
                {
                    // first time inside relaxed range, start time
                    if (LastChangeTime == 0)
                        LastChangeTime = e.CurrentTime;
                    // if inside relaxed range long enough, switch
                    else if (e.CurrentTime - LastChangeTime > ResistanceTime)
                    {
                        LastChangeTime = 0;
                        IsTriggered = true;
                    }
                }
                // Moved outside relaxed range, reset timer
                else
                {
                    LastChangeTime = 0;
                }
            }
        }
    }
}
