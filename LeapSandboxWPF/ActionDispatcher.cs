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
        private Dictionary<BaseTrigger, BaseAction> _ActiveMap;

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

        #region UpdateActiveMap
        private void UpdateActiveMap()
        {
            string actionName;
            BaseAction action;

            // construct the new map
            var map = new Dictionary<BaseTrigger, BaseAction>();
            foreach (var triggerPair in _Triggers)
                foreach (var mode in _ActiveModes)
                {
                    actionName = mode.GetActionForTrigger(triggerPair.Key);
                    if (String.IsNullOrEmpty(actionName))
                        continue; // trigger not in mode
                    if (!_Actions.TryGetValue(actionName, out action))
                        continue; // must be bad config
                    map.Add(triggerPair.Value, action);
                    break;
                }

            // check all triggered Triggers in current map to see if action is gone/different in new map
            foreach (var mapPair in _ActiveMap.Where(p => p.Key.IsTriggered))
                if (!map.TryGetValue(mapPair.Key, out action))
                    // mapping is gone...action should end
                    action.End();
                else if (!action.Equals(mapPair.Value))
                {
                    // different action...end old, begin new
                    action.End();
                    mapPair.Value.Begin();
                }

            // begin any new triggered action not present in current
            foreach (var mapPair in map)
                if (!_ActiveMap.ContainsKey(mapPair.Key))
                    // mapping is gone...action should end
                    mapPair.Value.Begin();

            _ActiveMap = map;
        } 
        #endregion

        #region Activate/Deactivate Mode
        public void ActivateMode(string modeName)
        {
            lock (_Lock)
            {
                ConfigurationMode mode;
                if (!_Modes.TryGetValue(modeName, out mode))
                    return; // must  be bad config
                _ActiveModes.Remove(mode);
                _ActiveModes.AddFirst(mode);
                UpdateActiveMap();
            }
        }
        public void DeactivateMode(string modeName)
        {
            lock (_Lock)
            {
                ConfigurationMode mode;
                if (!_Modes.TryGetValue(modeName, out mode))
                    return; // must  be bad config
                _ActiveModes.Remove(mode);
                UpdateActiveMap();
            }
        }
        #endregion

        #region OnTriggerChanged
        private void OnTriggerChanged(object sender, TriggerEventArgs e)
        {
            BaseAction action;
            lock (_Lock)
                if (!_ActiveMap.TryGetValue((BaseTrigger)sender, out action))
                    return;
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
