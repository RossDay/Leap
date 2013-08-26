using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vyrolan.VMCS.Actions;
using Vyrolan.VMCS.Gestures;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS
{
    internal class ActionDispatcher : IGestureDispatcher
    {
        private ICollection<BaseTrigger> _Triggers = new List<BaseTrigger>();
        private ICollection<BaseAction> _Actions = new List<BaseAction>();
        private ICollection<ConfigurationMode> _Modes = new List<ConfigurationMode>();

        public void AddTrigger(BaseTrigger trigger)
        {
            _Triggers.Add(trigger);
        }

        public bool RemoveTrigger(BaseTrigger trigger)
        {
            return _Triggers.Remove(trigger);
        }

        public void Dispatch(VyroGesture gesture)
        {
            foreach (GestureTrigger trigger in _Triggers.Where(t => t is GestureTrigger))
                if (trigger.Check(gesture))
                    trigger.Activate();
        }
    }

    internal class ConfigurationMode
    {

    }
}
