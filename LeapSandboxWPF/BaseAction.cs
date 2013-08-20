using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsInput;
using WindowsInput.Native;

namespace Vyrolan.VMCS
{
    internal abstract class BaseAction
    {
        protected static InputSimulator InputSimulator = new InputSimulator();
        public bool IsFiring { get; protected set; }

        public BaseAction(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggered;
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


    internal class KeyPressAction : BaseAction
    {
        public VirtualKeyCode Key { get; set; }

        public KeyPressAction(BaseTrigger trigger, VirtualKeyCode key) : base(trigger)
        {
            Key = key;
        }

        protected override void Begin()
        {
            InputSimulator.Keyboard.KeyPress(Key);
        }

        protected override void End()
        {
        }
    }

    internal class KeyHoldAction : BaseAction
    {
        public VirtualKeyCode Key { get; set; }

        public KeyHoldAction(BaseTrigger trigger, VirtualKeyCode key)
            : base(trigger)
        {
            Key = key;
        }

        protected override void Begin()
        {
            InputSimulator.Keyboard.KeyDown(Key);
        }

        protected override void End()
        {
            InputSimulator.Keyboard.KeyUp(Key);
        }
    }

}
