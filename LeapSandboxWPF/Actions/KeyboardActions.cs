using System;
using System.Collections.Generic;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class KeyPressAction : BaseAction
    {
        private readonly ICollection<VirtualKeyCode> _Modifiers = new List<VirtualKeyCode>(4);
        private VirtualKeyCode _Key;
        private bool KeySet { get; set; }

        public VirtualKeyCode Key
        {
            get { return _Key; }
            set
            {
                _Key = value;
                KeySet = true;
            }
        }

        public void AddModifier(VirtualKeyCode modifier)
        {
            if (IsModifier(modifier))
                _Modifiers.Add(modifier);
            else
                throw new ArgumentOutOfRangeException("modifier", "Must be a modifier (SHIFT, CTRL, ALT, WIN).");
        }
        public bool RemoveModifier(VirtualKeyCode modifier)
        {
            return _Modifiers.Remove(modifier);
        }

        protected override void Begin()
        {
            if (KeySet)
                InputSimulator.Keyboard.ModifiedKeyStroke(_Modifiers, Key);
        }

        protected override void End()
        {
        }
    }

    internal class KeyHoldAction : BaseAction
    {
        private VirtualKeyCode _Key;
        private bool KeySet { get; set; }

        public VirtualKeyCode Key
        {
            get { return _Key; }
            set
            {
                _Key = value;
                KeySet = true;
            }
        }

        protected override void Begin()
        {
            if (KeySet)
                InputSimulator.Keyboard.KeyDown(Key);
        }

        protected override void End()
        {
            if (KeySet)
                InputSimulator.Keyboard.KeyUp(Key);
        }
    }

    internal class KeyMacroAction : BaseAction
    {
        public ICollection<VirtualKeyCode> _Keys = new List<VirtualKeyCode>();

        public void AddKey(VirtualKeyCode key)
        {
            _Keys.Add(key);
        }
        public void ClearKeys()
        {
            _Keys.Clear();
        }

        protected override void Begin()
        {
            var activeModifiers = new List<VirtualKeyCode>();

            foreach (var key in _Keys)
            {
                if (IsModifier(key))
                    activeModifiers.Add(key);
                else
                {
                    InputSimulator.Keyboard.ModifiedKeyStroke(activeModifiers, key);
                    activeModifiers.Clear();
                    InputSimulator.Keyboard.Sleep(50);
                }
            }
        }

        protected override void End()
        {
        }
    }


}
