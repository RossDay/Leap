using System;
using System.Linq;
using System.Windows.Controls;
using Leap;
using Vyrolan.VMCS.Actions;
using Vyrolan.VMCS.Gestures;
using Vyrolan.VMCS.Triggers;
using WindowsInput.Native;

namespace Vyrolan.VMCS
{
    internal class ControlSystem : IFrameUpdate, IDisposable
    {
        private readonly Label _Log;
        private readonly Action<string> _LogAction;
        public static Action<string> StaticLog;
        private readonly Object _Lock = new Object();

        private readonly Controller _Controller;
        private readonly MainListener _Listener;
        private readonly HandManager _HandManager;
        private readonly GestureRecognizer _GestureRecognizer;
        private readonly GestureDispatcher _GestureDispatcher;

        public ControlSystem(Label log)
        {
            _Log = log;
            _LogAction = SafeWriteLine;
            StaticLog = _LogAction;
            _LogAction("Control System Constructed");

            _Controller = new Controller();
            _Listener = new MainListener(_LogAction);
            _HandManager = new HandManager();
            _GestureDispatcher = new GestureDispatcher();
            _GestureRecognizer = new GestureRecognizer(_GestureDispatcher);

            _Listener.RegisterForFrameUpdates(_HandManager);
            _Listener.RegisterForFrameUpdates(_GestureRecognizer);
            _Listener.RegisterForFrameUpdates(this);

            _Controller.AddListener(_Listener);

            /*
            var mma = new MouseMoveAction { Axis = PositionTrackingAxis.Screen, MinDistance = 0, Tracker = _HandManager.RightHand.FingerTracker };
            var rt = new RangeTrigger(_HandManager.RightHand.FingerCountState) { RequiresStabilized = true, MinValue = 1, MaxValue = 1, Resistance = 0, Stickiness = 1, Name = "1 Finger" };
            mma.RegisterTrigger(rt);

            var sa = new MouseClickAction { Button = VirtualKeyCode.RBUTTON, IsDoubleClick = false };
            var rt = new GestureTriggerCircle { Hand = _HandManager.RightHand, IsClockwise = true, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true, Name = "circle!" };
            sa.RegisterTrigger(rt);
            _GestureDispatcher.RegisterTrigger(rt);

            var mma2 = new ScrollAction { Axis = PositionTrackingAxis.Y, Tracker = _HandManager.LeftHand.HandTracker, IsAccelerated = true, Lines = 1, IsContinuous = true, MinDistance = 25 };
            var lt = new RangeTrigger(_HandManager.LeftHand.FingerCountState) { RequiresStabilized = true, MinValue = 1, MaxValue = 1, Resistance = 0, Stickiness = 1, Name = "LH1F" };
            mma2.RegisterTrigger(lt);

            var mma2 = new KeyHoldAction {Key = VirtualKeyCode.LSHIFT};
            var lt3 = new RangeTrigger(_HandManager.LeftHand.RollState) { RequiresStabilized = true, MinValue = 45, MaxValue = 105, Resistance = 0, Stickiness = 5, Name = "LH1F" };
            mma2.RegisterTrigger(lt3);
            */

            var sa = new KeyPressAction {Key = VirtualKeyCode.VK_A};
            sa.AddModifier(VirtualKeyCode.SHIFT);
            var rt = new GestureTriggerCircle { Hand = _HandManager.RightHand, IsClockwise = true, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true, Name = "circle!" };
            sa.RegisterTrigger(rt);
            _GestureDispatcher.RegisterTrigger(rt);

            var sa2 = new KeyPressAction { Key = VirtualKeyCode.VK_A };
            var rt2 = new GestureTriggerCircle { Hand = _HandManager.RightHand, IsClockwise = false, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true, Name = "circle!" };
            sa2.RegisterTrigger(rt2);
            _GestureDispatcher.RegisterTrigger(rt2);

            var kma = new KeyMacroAction();
            kma.AddKeys(VirtualKeyCode.LMENU, VirtualKeyCode.TAB);
            var lt = new GestureTriggerCircle { Hand = _HandManager.LeftHand, IsClockwise = true, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true, Name = "circle!" };
            kma.RegisterTrigger(lt);
            _GestureDispatcher.RegisterTrigger(lt);

            var kma2 = new KeyMacroAction();
            kma2.AddKeys(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_R, VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_R);
            var lt2 = new GestureTriggerCircle { Hand = _HandManager.LeftHand, IsClockwise = false, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true, Name = "circle!" };
            kma2.RegisterTrigger(lt2);
            _GestureDispatcher.RegisterTrigger(lt2);
        }

        private long _LastLogTime;
        public bool Update(Leap.Frame frame)
        {
            var s = _HandManager.Dump();
            if (!String.IsNullOrWhiteSpace(s))
            {
                if (frame.Timestamp > _LastLogTime + 0)
                {
                    _LastLogTime = frame.Timestamp;
                    _LogAction(s);
                }
            }
            return true;
        }

        #region SafeWriteLine
        private void SafeWriteLine(String line)
        {
            try
            {
                lock (_Lock)
                {
                    _Log.Dispatcher.Invoke(new Action(delegate
                    {
                        var newContent = _Log.Content + line + "\n";
                        var lines = newContent.Split('\n');
                        newContent = String.Join("\n", lines.Skip(lines.Length - 35));
                        _Log.Content = newContent;
                    }));
                }
            }
            catch (Exception e)
            {
                _Log.Dispatcher.Invoke(new Action(delegate
                {
                    _Log.Content = "Exception: " + e.GetType().FullName + "\n" + e.Message + "\n" + e.StackTrace;
                }));
            }
        } 
        #endregion

        #region IDisposable
        private bool _IsDisposed; // to detect redundant calls
        public bool IsDisposed { get { return _IsDisposed; } }

        protected virtual void Dispose(bool disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    if (_Listener != null)
                    {
                        if (_Controller != null)
                            _Controller.RemoveListener(_Listener);
                        _Listener.Dispose();
                    }
                    if (_Controller != null) _Controller.Dispose();
                }
                // shared cleanup logic goes here
                _IsDisposed = true;
            }
        }

        ~ControlSystem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
