﻿using System;
using WindowsInput.Native;

namespace Vyrolan.VMCS.Actions
{
    internal class MouseClickAction : DiscreteAction
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

        public MouseClickAction(string name) : base(name) { }

        protected override void Fire()
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
    }

    internal class MouseDragAction : BaseAction
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

        public MouseDragAction(string name) : base(name) { }

        protected override void BeginImpl()
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

        protected override void EndImpl()
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
