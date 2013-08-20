using System.Collections.Generic;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS.Gestures
{
    internal class GestureDispatcher
    {
        public ICollection<GestureTrigger> _Triggers = new List<GestureTrigger>();

        public void RegisterTrigger(GestureTrigger trigger)
        {
            _Triggers.Add(trigger);
        }
        public void UnregisterTrigger(GestureTrigger trigger)
        {
            _Triggers.Remove(trigger);
        }

        public void Dispatch(VyroGesture gesture)
        {
            foreach (var trigger in _Triggers)
                if (trigger.Check(gesture))
                    trigger.FireManually();
        }
    }

}
