using System;
using System.Runtime.InteropServices;

namespace Vyrolan.VMCS
{
    internal class Native
    {
        private const uint WM_VSCROLL = 277;
        private const int SB_LINEUP = 0;
        private const int SB_LINEDOWN = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public uint left;
            public uint top;
            public uint right;
            public uint bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
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
        private static extern int SendMessage(
                 IntPtr hWnd,         // handle to destination window
                 uint Msg,         // message
                 IntPtr wParam,   // first message parameter
                 IntPtr lParam   // second message parameter
        );

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        private static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);

        /*
        private static int MakeDWord(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }
        */

        private static IntPtr GetActiveWindow()
        {
            var gInfo = new GUITHREADINFO();
            gInfo.cbSize = (uint)Marshal.SizeOf(gInfo);
            GetGUIThreadInfo(0, out gInfo);
            return gInfo.hwndFocus;
        }

        public static void ScrollActiveWindow(bool IsUp)
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
    }
}
