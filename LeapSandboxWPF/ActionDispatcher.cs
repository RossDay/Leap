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
        private Dictionary<string, BaseTrigger> _Triggers = new Dictionary<string, BaseTrigger>();
        private Dictionary<string, BaseAction> _Actions = new Dictionary<string, BaseAction>();
        private ICollection<ConfigurationMode> _Modes = new List<ConfigurationMode>();

        public void AddTrigger(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggerChanged;
            _Triggers.Add(trigger.Name, trigger);
        }
        public bool RemoveTrigger(BaseTrigger trigger)
        {
            trigger.Triggered -= OnTriggerChanged;
            return _Triggers.Remove(trigger.Name);
        }

        public void AddAction(BaseAction action)
        {
            _Actions.Add(action.Name, action);
        }
        public bool RemoveAction(BaseAction action)
        {
            return _Actions.Remove(action.Name);
        }

        private void OnTriggerChanged(object sender, TriggerEventArgs e)
        {
        }

        public void Dispatch(VyroGesture gesture)
        {
            foreach (GestureTrigger trigger in _Triggers.Values.Where(t => t is GestureTrigger))
                if (trigger.Check(gesture))
                    trigger.Activate();
        }
    }

    internal class ConfigurationMode
    {

    }
}
