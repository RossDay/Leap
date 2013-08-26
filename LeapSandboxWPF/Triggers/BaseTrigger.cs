using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Vyrolan.VMCS.Triggers
{
    internal class TriggerEventArgs : EventArgs
    {
        public bool IsTriggered { get; set; }
    }

    internal abstract class BaseTrigger
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
            get { return _IsTriggered; }
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
            var xml = new StringBuilder();
            xml.AppendFormat("<Trigger type=\"{0}\" ", GetType().Name);

            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                var param = prop.GetCustomAttributes(typeof(ConfigurationParameterAttribute), false).Cast<ConfigurationParameterAttribute>().FirstOrDefault();
                if (param == null) continue;

                xml.AppendFormat("{0}=\"{1}\" ", param.ParameterName, prop.GetValue(this, null));
            }

            xml.Append(" />");
            return xml.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    internal sealed class ConfigurationParameterAttribute : Attribute
    {
        public string ParameterName { get; private set; }

        public ConfigurationParameterAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
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
