using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Leap;
using WindowsInput;

namespace LeapSandboxWPF
{
    class GrabAndScroll
    {
        #region Externs: GetActiveWindow, SendMessage
        private const uint WM_VSCROLL = 277;
        private const int SB_LINEUP = 0;
        private const int SB_LINEDOWN = 1;

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

        private int MakeDWord(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }

        private IntPtr GetActiveWindow()
        {
            var gInfo = new GUITHREADINFO();
            gInfo.cbSize = (uint)Marshal.SizeOf(gInfo);
            GetGUIThreadInfo(0, out gInfo);
            return gInfo.hwndFocus;
        } 
        #endregion

        public GrabAndScroll(Action<string> logAction)
        {
            _LogAction = logAction;
        }

	    private void ScrollActiveWindow(bool IsUp)
	    {
            //var p = GetCursorPosition();
            //focusedWin = WindowFromPoint(p.X, p.Y);
            //SendMessage(focusedWin, WM_MOUSEWHEEL, (IntPtr)MakeDWord((IsUp ? 2 : -2)*WHEEL_DELTA, 0), (IntPtr)MakeDWord(p.X, p.Y));
            //_LogAction(String.Format("Scrolling Window {0} {1} with Mouse at {2}, {3}", focusedWin.ToInt64(), (IsUp ? "Up" : "Down"), p.X, p.Y));

			var focusedWin = GetActiveWindow();
			SendMessage(focusedWin, WM_VSCROLL, (IntPtr)(IsUp ? SB_LINEUP : SB_LINEDOWN), IntPtr.Zero);

			//_LogAction(String.Format("Mouse Wheel Scrolling {0}", (IsUp ? "Up" : "Down")));
			//_InputSim.Mouse.VerticalScroll(IsUp ? 1 : -1);
	    }

	    private readonly InputSimulator _InputSim = new InputSimulator();
        private readonly Action<string> _LogAction;
        private bool _IsGrabbed;
        private float _StartingY;
        private int _Progress;
	    private int _ActiveHand;

        public void OnFrame(Frame frame)
        {
            if (frame.Hands.IsEmpty)
                return;

            Hand hand = null;
            if (_ActiveHand != 0)
            {
                hand = frame.Hand(_ActiveHand);
                if (!hand.IsValid)
                {
                    _ActiveHand = 0;
                    _IsGrabbed = false;
                }
            }
	        if (_ActiveHand == 0)
	        {
		        hand = frame.Hands.Leftmost;
		        _ActiveHand = hand.Id;
	        }

            // We have the same Hand as in the past and we're still grabbed.
            if (_IsGrabbed)
            {
                if (hand.Fingers.Count >= 3)
                {
                    _IsGrabbed = false;
                    _LogAction(String.Format("Hand {0} Released.", _ActiveHand));
                }
                else
                {
                    var y = hand.StabilizedPalmPosition.y;
                    _LogAction(String.Format("Hand {0} now at {1:0.0} was grabbed at {2:0.0}.", _ActiveHand, y, _StartingY));
                    if (y < _StartingY - 10)
                        for (var i = 0; i < Math.Floor((_StartingY - y) / 15); i++)
                            ScrollActiveWindow(false);
                    else if (y > _StartingY + 10)
                        for (var i = 0; i < Math.Floor((y - _StartingY) / 15); i++)
                            ScrollActiveWindow(true);
                }
            }
            else if (hand.Fingers.Count < 2)
            {
                _IsGrabbed = true;
                _StartingY = hand.StabilizedPalmPosition.y;
                _LogAction(String.Format("Hand {0} Grabbed at {1:0.0}.", _ActiveHand, _StartingY));
            }

            foreach (var g in frame.Gestures().Where(g => g.Type == Gesture.GestureType.TYPECIRCLE))
            {
                CircleGesture circle = new CircleGesture(g);

                var isClockwise = (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4);

                if (circle.Progress < _Progress || circle.Progress > _Progress + 1)
                {
                    ScrollActiveWindow(!isClockwise);
                    _Progress = Convert.ToInt32(Math.Floor(circle.Progress));
                }
            }
        }

    }
}
