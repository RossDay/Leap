﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
    class GrabAndScroll
    {
        private const uint WM_VSCROLL = 277;
        private const int SB_THUMBTRACK = 276;
        private const int SB_ENDSCROLL = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public uint left;
            public uint top;
            public uint right;
            public uint bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCapred;
            public RECT rcCaret;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(
                 IntPtr hWnd,         // handle to destination window
                 uint Msg,         // message
                 IntPtr wParam,   // first message parameter
                 IntPtr lParam   // second message parameter
        );

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        public static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);


	    private IntPtr GetActiveWindow()
	    {
		    var gInfo = new GUITHREADINFO();
		    gInfo.cbSize = (uint) Marshal.SizeOf(gInfo);
		    GetGUIThreadInfo(0, out gInfo);
		    return gInfo.hwndFocus;
	    }

	    private void scrollActiveWindow()
	    {
		    var focusedWin = GetActiveWindow();
            SendMessage(focusedWin, WM_VSCROLL, (IntPtr)1, IntPtr.Zero);
            //textBox1.Text = textBox1.Text + "\r\n" + focusedWin.ToString();
        }


        private bool _IsGrabbed;
	    private IntPtr _ActiveWindow;
        private int _Progress;
	    private int _ActiveHand;


        public void OnFrame(Frame frame)
        {
	        Hand hand;
	        if (_ActiveHand == 0 && !frame.Hands.IsEmpty)
	        {
		        hand = frame.Hands.Leftmost;
		        _ActiveHand = hand.Id;
	        }

            foreach (var g in frame.Gestures().Where(g => g.Type == Gesture.GestureType.TYPECIRCLE))
            {
                CircleGesture circle = new CircleGesture(g);

				//if 

                var isClockwise = (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4);

                if (circle.Progress < _Progress || circle.Progress > _Progress + 1)
                {
                    scrollActiveWindow();
                    _Progress = Convert.ToInt32(Math.Floor(circle.Progress));
                }
            }
        }

    }
}
