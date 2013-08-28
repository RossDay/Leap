using System;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal abstract class BaseAction : IEquatable<BaseAction>
    {
        protected static InputSimulator InputSimulator = new InputSimulator();
        private readonly object _Lock = new object();
        private int _FiringCount;
        public bool IsFiring { get { return _FiringCount > 0; } }
        [ConfigurationParameter("name")]
        public string Name { get; private set; }

        public BaseAction(string name)
        {
            Name = name;
        }

        private static readonly VirtualKeyCode[] _Modifiers =
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

        private static readonly VirtualKeyCode[] _MouseButtons = { VirtualKeyCode.LBUTTON, VirtualKeyCode.MBUTTON, VirtualKeyCode.RBUTTON };
        protected static bool IsMouseButton(VirtualKeyCode key)
        {
            return _MouseButtons.Contains(key);
        }
        public void Begin()
        {
            lock (_Lock)
            {
                ++_FiringCount;
                if (_FiringCount == 1)
                    BeginImpl();
            }
        }

        public void End()
        {
            lock (_Lock)
            {
                if (_FiringCount < 1)
                    return; // End without Begin
                --_FiringCount;
                if (!IsFiring)
                    EndImpl();
            }
        }

        protected abstract void BeginImpl();
        protected abstract void EndImpl();

        public string ToXml()
        {
            return ConfigurationSerializer.ToXml(this);
        }

        #region Equals / GetHashCode
        public override bool Equals(object obj)
        {
            var action = obj as BaseAction;
            if (action == null) return false;
            return Equals(action);
        }

        public bool Equals(BaseAction other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        #endregion
    }

    internal abstract class DiscreteAction : BaseAction
    {
        protected DiscreteAction(string name) : base(name) { }

        protected abstract void Fire();

        protected override void BeginImpl()
        {
            Fire();
            End();
        }

        protected override void EndImpl()
        {
        }
    }
}
