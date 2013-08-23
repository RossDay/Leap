using System.Collections.Generic;
using System.Linq;
using Vyrolan.VMCS.Triggers;
using WindowsInput;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal abstract class BaseAction
    {
        protected static InputSimulator InputSimulator = new InputSimulator();
        protected readonly object _Lock = new object();
        private ICollection<BaseTrigger> _Triggers = new List<BaseTrigger>();
        public bool IsFiring { get; protected set; }

        public void RegisterTrigger(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggered;
            _Triggers.Add(trigger);
        }
        public void UnregisterTrigger(BaseTrigger trigger)
        {
            if (_Triggers.Remove(trigger))
                trigger.Triggered -= OnTriggered;
        }

        private static VirtualKeyCode[] _Modifiers =
            {
                VirtualKeyCode.SHIFT, VirtualKeyCode.LSHIFT, VirtualKeyCode.RSHIFT,
                VirtualKeyCode.CONTROL, VirtualKeyCode.LCONTROL, VirtualKeyCode.RCONTROL, 
                VirtualKeyCode.MENU, VirtualKeyCode.LMENU, VirtualKeyCode.RMENU, 
                VirtualKeyCode.LWIN, VirtualKeyCode.RWIN
            };
        protected static bool IsModifier(VirtualKeyCode key)
        {
            return _Modifiers.Contains(key);
        }

        private static VirtualKeyCode[] _MouseButtons = { VirtualKeyCode.LBUTTON, VirtualKeyCode.MBUTTON, VirtualKeyCode.RBUTTON };
        protected static bool IsMouseButton(VirtualKeyCode key)
        {
            return _MouseButtons.Contains(key);
        }

        private void OnTriggered(object sender, TriggerEventArgs e)
        {
            lock (_Lock)
                if (e.IsTriggered)
                {
                    if (!IsFiring)
                    {
                        IsFiring = true;
                        Begin();
                    }
                }
                else
                {
                    if (IsFiring)
                    {
                        End();
                        IsFiring = false;
                    }
                }
        }

        protected abstract void Begin();
        protected abstract void End();
    }

    internal abstract class DiscreteAction : BaseAction
    {
        protected abstract void Fire();

        protected override void Begin()
        {
            Fire();
            lock (_Lock)
                IsFiring = false;
        }

        protected override void End()
        {
        }
    }
}
