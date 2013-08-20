using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vyrolan.VMCS.Triggers;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    class MouseClickAction : BaseAction
    {
        public bool IsDoubleClick { get; set; }

        private VirtualKeyCode _Button;
        public VirtualKeyCode Button
        {
            get { return _Button; }
            set
            {
                if (IsMouseButton(value) && value != VirtualKeyCode.MBUTTON)
                    _Button = value;
                else 
                    throw new ArgumentOutOfRangeException("value", "Must be the left or right mouse button.");
            }
        }

        public MouseClickAction(BaseTrigger trigger) : base(trigger)
        {
        }

        protected override void Begin()
        {
            switch (Button)
            {
                case VirtualKeyCode.LBUTTON:
                    if (IsDoubleClick)
                        InputSimulator.Mouse.LeftButtonDoubleClick();
                    else
                        InputSimulator.Mouse.LeftButtonClick();
                    break;
                case VirtualKeyCode.RBUTTON:
                    if (IsDoubleClick)
                        InputSimulator.Mouse.RightButtonDoubleClick();
                    else
                        InputSimulator.Mouse.RightButtonClick();
                    break;
            }
        }

        protected override void End()
        {
            
        }
    }

    class MouseDragAction : BaseAction
    {
        private VirtualKeyCode _Button;
        public VirtualKeyCode Button
        {
            get { return _Button; }
            set
            {
                if (IsMouseButton(value) && value != VirtualKeyCode.MBUTTON)
                    _Button = value;
                else
                    throw new ArgumentOutOfRangeException("value", "Must be the left or right mouse button.");
            }
        }

        public MouseDragAction(BaseTrigger trigger)
            : base(trigger)
        {
        }

        protected override void Begin()
        {
            switch (Button)
            {
                case VirtualKeyCode.LBUTTON:
                    InputSimulator.Mouse.LeftButtonDown();
                    break;
                case VirtualKeyCode.RBUTTON:
                    InputSimulator.Mouse.RightButtonDown();
                    break;
            }
        }

        protected override void End()
        {
            switch (Button)
            {
                case VirtualKeyCode.LBUTTON:
                    InputSimulator.Mouse.LeftButtonUp();
                    break;
                case VirtualKeyCode.RBUTTON:
                    InputSimulator.Mouse.RightButtonUp();
                    break;
            }
        }
    }

}
