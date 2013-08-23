using System;
using System.Collections;
using System.Collections.Generic;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class KeyPressAction : DiscreteAction
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

        protected override void Fire()
        {
            if (KeySet)
                InputSimulator.Keyboard.ModifiedKeyStroke(_Modifiers, Key);
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

    internal class KeyMacroAction : DiscreteAction
    {
        private ICollection<VirtualKeyCode> _Keys = new List<VirtualKeyCode>();

        public void AddKey(VirtualKeyCode key)
        {
            _Keys.Add(key);
        }

        public void AddKeys(IEnumerable<VirtualKeyCode> keys)
        {
            foreach (var k in keys)
                AddKey(k);
        }

        public void AddKeys(params VirtualKeyCode[] keys)
        {
            AddKeys((IEnumerable<VirtualKeyCode>)keys);
        }

        public void ClearKeys()
        {
            _Keys.Clear();
        }

        protected override void Fire()
        {
            var activeModifiers = new List<VirtualKeyCode>();

            ControlSystem.StaticLog("Key Macro Firing:");
            foreach (var key in _Keys)
            {
                ControlSystem.StaticLog(" * " + key);
                if (IsModifier(key))
                    activeModifiers.Add(key);
                else
                {
                    InputSimulator.Keyboard.ModifiedKeyStroke(activeModifiers, key);
                    activeModifiers.Clear();
                    InputSimulator.Keyboard.Sleep(5);
                }
            }
        }
    }
 }
