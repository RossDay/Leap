using System;
using System.Collections.Generic;
using System.Linq;

namespace Vyrolan.VMCS.Triggers
{
    internal class TriggerEventArgs : EventArgs
    {
        public bool IsTriggered { get; set; }
    }

    internal abstract class BaseTrigger
    {
        public string Name { get; set; }
        public PersistentHand Hand { get; set; }
        public bool RequiresStabilized { get; set; }

        private bool _IsTriggered;
        public bool IsTriggered
        {
            get { return _IsTriggered; }
            set
            {
                if (CheckHand(null) && (!_IsTriggered.Equals(value) || value))
                {
                    _IsTriggered = value;
                    OnTriggered();
                }
            }
        }

        public void FireManually()
        {
            IsTriggered = true;
        }

        public event EventHandler<TriggerEventArgs> Triggered;
        protected void OnTriggered()
        {
            if (Triggered != null)
                Triggered(this, new TriggerEventArgs { IsTriggered = IsTriggered });
        }

        protected bool CheckHand(IEnumerable<int> candidateHandIds)
        {
            // No or Finalized Hand means it can be any Hand
            if (Hand == null || Hand.IsFinalized)
                return true;

            // If stabilized required and we're not, not a match
            if (RequiresStabilized && !Hand.IsStabilized)
                return false;

            // Check the trigger's list of involved hands if there is one
            return (candidateHandIds == null || candidateHandIds.Contains(Hand.Id));
        }
    }
}
