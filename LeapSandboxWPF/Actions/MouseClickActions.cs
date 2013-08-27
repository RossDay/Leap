using System;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseClickAction : DiscreteAction
    {
        private VirtualKeyCode _Button;
        [ConfigurationParameter("isDbl")]
        public bool IsDoubleClick { get; set; }

        public MouseClickAction(string name) : base(name) { }

        [ConfigurationParameter("button")]
        public string Button
        {
            get { return _Button.ToString(); }
            set
            {
                try
                {
                    _Button = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), value);
                }
                catch
                {
                    ; // alert user about bad config
                }
            }
        }

        protected override void Fire()
        {
            switch (_Button)
            {
                case VirtualKeyCode.RBUTTON:
                    if (IsDoubleClick)
                        InputSimulator.Mouse.RightButtonDoubleClick();
                    else
                        InputSimulator.Mouse.RightButtonClick();
                    break;
                default: //case VirtualKeyCode.LBUTTON:
                    if (IsDoubleClick)
                        InputSimulator.Mouse.LeftButtonDoubleClick();
                    else
                        InputSimulator.Mouse.LeftButtonClick();
                    break;
            }
        }
    }

    internal class MouseDragAction : BaseAction
    {
        private VirtualKeyCode _Button;

        public MouseDragAction(string name) : base(name) { }

        [ConfigurationParameter("button")]
        public string Button
        {
            get { return _Button.ToString(); }
            set
            {
                try
                {
                    _Button = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), value);
                }
                catch
                {
                    ; // alert user about bad config
                }
            }
        }

        protected override void BeginImpl()
        {
            switch (_Button)
            {
                case VirtualKeyCode.RBUTTON:
                    InputSimulator.Mouse.RightButtonDown();
                    break;
                default: //case VirtualKeyCode.LBUTTON:
                    InputSimulator.Mouse.LeftButtonDown();
                    break;
            }
        }

        protected override void EndImpl()
        {
            switch (_Button)
            {
                case VirtualKeyCode.RBUTTON:
                    InputSimulator.Mouse.RightButtonUp();
                    break;
                default: //case VirtualKeyCode.LBUTTON:
                    InputSimulator.Mouse.LeftButtonUp();
                    break;
            }
        }
    }
}
