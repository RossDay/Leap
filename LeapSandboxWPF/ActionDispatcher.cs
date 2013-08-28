using System;
using System.Collections.Generic;
using System.Linq;
using Vyrolan.VMCS.Actions;
using Vyrolan.VMCS.Gestures;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS
{
    internal class ActionDispatcher : IGestureDispatcher
    {
        private readonly object _Lock = new object();
        private readonly Configuration _Configuration;

        private IDictionary<string, BaseTrigger> _Triggers { get { return _Configuration.Triggers; } } 
        private IDictionary<string, BaseAction> _Actions { get { return _Configuration.Actions; } } 
        private IDictionary<string, ConfigurationMode> _Modes { get { return _Configuration.Modes; } } 

        private readonly LinkedList<ConfigurationMode> _ActiveModes = new LinkedList<ConfigurationMode>();
        private Dictionary<BaseTrigger, BaseAction> _ActiveMap;

        public ActionDispatcher(Configuration config)
        {
            _Configuration = config;
            _ActiveMap = new Dictionary<BaseTrigger, BaseAction>();
            UpdateActiveMap(null, true);
            _Configuration.TriggerChanged += OnTriggerChanged;
        }

        #region UpdateActiveMap
        private bool UpdateActiveMap(string modeName, bool isActivate)
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
                    if (actionName.StartsWith("AM:") || actionName.StartsWith("DM:"))
                        action = new ModeAction(actionName); // TODO - make a factory and intern these
                    else if (!_Actions.TryGetValue(actionName, out action))
                        continue; // must be bad config
                    map.Add(triggerPair.Value, action);
                    break;
                }

            // check that new map won't instantly deactivate/reactivate
            if (
                map.Count(p => p.Key.IsTriggered
                               && p.Value is ModeAction
                               && ((ModeAction)p.Value).IsMatch(modeName, !isActivate)
                         ) > 0
            )
                return false;

            // check all triggered Triggers in current map to see if action is gone/different in new map
            foreach (var mapPair in _ActiveMap.Where(p => p.Key.IsTriggered))
                if (!map.TryGetValue(mapPair.Key, out action))
                    // mapping is gone...action should end
                    EndAction(action);
                else if (!action.Equals(mapPair.Value))
                {
                    // different action...end old, begin new
                    EndAction(action);
                    BeginAction(mapPair.Value);
                }

            // begin any new triggered action not present in current
            foreach (var mapPair in map)
                if (mapPair.Key.IsTriggered && !_ActiveMap.ContainsKey(mapPair.Key))
                    BeginAction(mapPair.Value);

            _ActiveMap = map;
            return true;
        } 
        #endregion

        #region Activate/Deactivate Mode
        private void ActivateMode(string modeName)
        {
            lock (_Lock)
            {
                ConfigurationMode mode;
                if (!_Modes.TryGetValue(modeName, out mode))
                    return; // must  be bad config

                if (_ActiveModes.First.Value.Equals(mode))
                    return;

                var temp = _ActiveModes.Find(mode);
                if (temp != null)
                {
                    var holder = temp.Previous;
                    _ActiveModes.Remove(temp);
                    temp = holder;
                }

                _ActiveModes.AddFirst(mode);

                if (!UpdateActiveMap(modeName, true))
                {
                    _ActiveModes.RemoveFirst();
                    if (temp != null)
                        _ActiveModes.AddAfter(temp, mode);
                }
            }
        }
        private void DeactivateMode(string modeName)
        {
            lock (_Lock)
            {
                ConfigurationMode mode;
                if (!_Modes.TryGetValue(modeName, out mode))
                    return; // must  be bad config

                var temp = _ActiveModes.Find(mode);
                if (temp == null)
                    return;

                var prev = temp.Previous;
                _ActiveModes.Remove(temp);
                if (!UpdateActiveMap(modeName, false))
                {
                    if (prev == null)
                        _ActiveModes.AddFirst(mode);
                    else
                        _ActiveModes.AddAfter(prev, mode);
                }
            }
        }
        #endregion

        #region BeginAction / EndAction
        private void BeginAction(BaseAction action)
        {
            var modeAction = action as ModeAction;
            if (modeAction != null)
            {
                if (modeAction.IsActivate)
                    ActivateMode(modeAction.ModeName);
                else
                    DeactivateMode(modeAction.ModeName);
            }
            else
                action.Begin();
        }

        private void EndAction(BaseAction action)
        {
            var modeAction = action as ModeAction;
            if (modeAction != null && modeAction.IsActivate)
                DeactivateMode(modeAction.ModeName);
            else
                action.End();
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
                BeginAction(action);
            else
                EndAction(action);
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
