using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsInput;
using WindowsInput.Native;

namespace LeapSandboxWPF
{
    internal abstract class BaseAction
    {
        protected InputSimulator InputSimulator = new InputSimulator();

        public BaseAction(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggered;
        }

        void OnTriggered(object sender, TriggerEventArgs e)
        {
            if (e.IsTriggered)
                Begin();
            else
                End();
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
