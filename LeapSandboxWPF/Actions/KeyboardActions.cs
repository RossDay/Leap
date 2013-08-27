using System;
using System.Collections.Generic;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class KeyPressAction : DiscreteAction
    {
        private readonly ICollection<VirtualKeyCode> _Modifiers = new List<VirtualKeyCode>(4);
        private VirtualKeyCode _Key;
        private bool KeySet { get; set; }

        public KeyPressAction(string name) : base(name) { }

            [ConfigurationParameter("key")]
            public string Key
            {
                get { return _Key.ToString(); }
                set
                {
                    try
                    {
                        _Key = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), value);
                        KeySet = true;
                    }
                    catch
                    {
                        ; // alert user about bad config
                        KeySet = false;
                    }
                }
            }

        [ConfigurationParameter("mods")]
        public string Modifiers
        {
            get { return String.Join(",", _Modifiers); }
            set
            {
                _Modifiers.Clear();
                VirtualKeyCode vk;
                foreach (var key in value.Split(','))
                {
                    try
                    {
                        vk = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), key);
                    }
                    catch
                    {
                        continue; // alert user about bad config
                    }
                    if (!IsModifier(vk))
                        continue; // alert user about bad config
                    _Modifiers.Add(vk);
                }
            }
        }

        protected override void Fire()
        {
            if (KeySet)
                InputSimulator.Keyboard.ModifiedKeyStroke(_Modifiers, _Key);
        }
    }

    internal class KeyHoldAction : BaseAction
    {
        private VirtualKeyCode _Key;
        private bool KeySet { get; set; }

        [ConfigurationParameter("key")]
        public string Key
        {
            get { return _Key.ToString(); }
            set
            {
                try
                {
                    _Key = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), value);
                    KeySet = true;
                }
                catch
                {
                    ; // alert user about bad config
                    KeySet = false;
                }
            }
        }

        public KeyHoldAction(string name) : base(name) { }

        protected override void BeginImpl()
        {
            if (KeySet)
                InputSimulator.Keyboard.KeyDown(_Key);
        }

        protected override void EndImpl()
        {
            if (KeySet)
                InputSimulator.Keyboard.KeyUp(_Key);
        }
    }

    internal class KeyMacroAction : DiscreteAction
    {
        private ICollection<VirtualKeyCode> _Keys = new List<VirtualKeyCode>();

        public KeyMacroAction(string name) : base(name) { }

        [ConfigurationParameter("keys")]
        public string Keys
        {
            get { return String.Join(",", _Keys); }
            set
            {
                _Keys.Clear();
                VirtualKeyCode vk;
                foreach (var key in value.Split(','))
                {
                    try
                    {
                        vk = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), key);
                    }
                    catch
                    {
                        continue; // alert user about bad config
                    }
                    _Keys.Add(vk);
                }
            }
        }

        protected override void Fire()
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
                    InputSimulator.Keyboard.Sleep(5);
                }
            }
        }
    }
 }
