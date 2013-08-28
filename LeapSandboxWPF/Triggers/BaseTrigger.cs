using System;
using System.Collections.Generic;
using System.Linq;

namespace Vyrolan.VMCS.Triggers
{
    internal class TriggerEventArgs : EventArgs
    {
        public bool IsTriggered { get; set; }
    }

    internal abstract class BaseTrigger : IEquatable<BaseTrigger>
    {
        [ConfigurationParameter("name")]
        public string Name { get; private set; }
        public PersistentHand Hand { get; set; }

        [ConfigurationParameter("reqStable")]
        public bool RequiresStabilized { get; set; }

        protected BaseTrigger(string name)
        {
            Name = name;
        }

        protected bool _IsTriggered;
        public virtual bool IsTriggered
        {
            get { return _IsTriggered && !(this is DiscreteTrigger); }
            protected set
            {
                if (CheckHand(null) && (!_IsTriggered.Equals(value) || value))
                {
                    _IsTriggered = value;
                    OnTriggered();
                }
            }
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

        public string ToXml()
        {
            return ConfigurationSerializer.ToXml(this);
        }

        #region Equals / GetHashCode
        public override bool Equals(object obj)
        {
            var trigger = obj as BaseTrigger;
            if (trigger == null) return false;
            return Equals(trigger);
        }

        public bool Equals(BaseTrigger other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        #endregion
    }

    internal abstract class DiscreteTrigger : BaseTrigger
    {
        protected DiscreteTrigger(string name) : base(name) { }

        public override bool IsTriggered
        {
            get { return base.IsTriggered; }
            protected set
            {
                base.IsTriggered = value;
                _IsTriggered = false;
            }
        }
    }
}
