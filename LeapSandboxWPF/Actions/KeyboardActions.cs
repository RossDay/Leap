using Vyrolan.VMCS.Triggers;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class KeyPressAction : BaseAction
    {
        public VirtualKeyCode Key { get; set; }

        public KeyPressAction(BaseTrigger trigger, VirtualKeyCode key)
            : base(trigger)
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
