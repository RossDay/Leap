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
        private readonly object _Lock = new object();
        private readonly Dictionary<string, BaseTrigger> _Triggers = new Dictionary<string, BaseTrigger>();
        private readonly Dictionary<string, BaseAction> _Actions = new Dictionary<string, BaseAction>();
        private readonly Dictionary<string, ConfigurationMode> _Modes = new Dictionary<string, ConfigurationMode>();
        private readonly LinkedList<ConfigurationMode> _ActiveModes = new LinkedList<ConfigurationMode>();

        #region Add/Remove Trigger/Action/Mode
        public void AddTrigger(BaseTrigger trigger)
        {
            lock (_Lock)
            {
                if (_Triggers.ContainsKey(trigger.Name))
                    return; // must be bad config
                trigger.Triggered += OnTriggerChanged;
                _Triggers.Add(trigger.Name, trigger);
            }
        }
        public bool RemoveTrigger(BaseTrigger trigger)
        {
            lock (_Lock)
            {
                if (!_Triggers.ContainsKey(trigger.Name))
                    return false;
                trigger.Triggered -= OnTriggerChanged;
                return _Triggers.Remove(trigger.Name);
            }
        }

        public void AddAction(BaseAction action)
        {
            lock (_Lock)
            {
                if (_Actions.ContainsKey(action.Name))
                    return; // must be bad config
                _Actions.Add(action.Name, action);
            }
        }
        public bool RemoveAction(BaseAction action)
        {
            lock (_Lock)
            {
                if (!_Actions.ContainsKey(action.Name))
                    return false;
                return _Actions.Remove(action.Name);
            }
        }

        public void AddMode(ConfigurationMode mode)
        {
            lock (_Lock)
            {
                if (_Modes.ContainsKey(mode.Name))
                    return; // must be bad config
                _Modes.Add(mode.Name, mode);
            }
        }
        public bool RemoveMode(ConfigurationMode mode)
        {
            lock (_Lock)
            {
                if (!_Modes.ContainsKey(mode.Name))
                    return false;
                return _Modes.Remove(mode.Name);
            }
        }
        #endregion

        #region Activate/Deactivate Mode
        private void ForAllTriggeredActionsInMode(ConfigurationMode mode, Action<BaseAction> actionDelegate)
        {
            foreach (var t in _Triggers.Where(t => t.Value.IsTriggered))
            {
                var actionName = mode.GetActionForTrigger(t.Key);
                if (String.IsNullOrEmpty(actionName))
                    continue; // trigger not in mode
                BaseAction action;
                if (!_Actions.TryGetValue(actionName, out action))
                    continue; // must be bad config
                actionDelegate(action);
            }
        }
        public void ActivateMode(string modeName)
        {
            lock (_Lock)
            {
                ConfigurationMode mode;
                if (!_Modes.TryGetValue(modeName, out mode))
                    return; // must  be bad config
                DeactivateMode(modeName);
                _ActiveModes.AddFirst(mode);
                ForAllTriggeredActionsInMode(mode, a => a.Begin());
            }
        }
        public void DeactivateMode(string modeName)
        {
            ConfigurationMode mode;
            if (!_Modes.TryGetValue(modeName, out mode))
                return; // must  be bad config
            _ActiveModes.Remove(mode);
            ForAllTriggeredActionsInMode(mode, a => a.End());
        } 
        #endregion

        #region GetCurrentActionForTrigger / OnTriggerChanged
        private BaseAction GetCurrentActionForTrigger(string triggerName)
        {
            string actionName = null;
            foreach (var activeMode in _ActiveModes)
            {
                actionName = activeMode.GetActionForTrigger(triggerName);
                if (!String.IsNullOrEmpty(actionName))
                    break;
            }

            BaseAction action;
            if (!_Actions.TryGetValue(actionName, out action))
                return null; // must be bad config
            return action;
        }

        private void OnTriggerChanged(object sender, TriggerEventArgs e)
        {
            var action = GetCurrentActionForTrigger(e.TriggerName);
            if (e.IsTriggered)
                action.Begin();
            else
                action.End();
        } 
        #endregion

        #region IGestureDispatcher
        public void Dispatch(VyroGesture gesture)
        {
            foreach (GestureTrigger trigger in _Triggers.Values.Where(t => t is GestureTrigger))
                if (trigger.Check(gesture))
                    trigger.Activate();
        } 
        #endregion
    }
}
