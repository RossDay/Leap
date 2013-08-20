using System.Linq;
using Vyrolan.VMCS.Triggers;
using WindowsInput;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal abstract class BaseAction
    {
        protected static InputSimulator InputSimulator = new InputSimulator();
        public bool IsFiring { get; protected set; }

        protected BaseAction(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggered;
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

        private void foo()
        {
            //InputSimulator.Mouse.
        }

        private void OnTriggered(object sender, TriggerEventArgs e)
        {
            if (e.IsTriggered)
            {
                IsFiring = true;
                Begin();
            }
            else
            {
                End();
                IsFiring = false;
            }
        }

        protected abstract void Begin();
        protected abstract void End();
    }
}
