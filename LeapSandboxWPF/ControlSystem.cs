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
        private readonly ActionDispatcher _ActionDispatcher;

        public ControlSystem(Label log)
        {
            _Log = log;
            _LogAction = SafeWriteLine;
            StaticLog = _LogAction;
            _LogAction("Control System Constructed");

            _HandManager = new HandManager();
            _ActionDispatcher = new ActionDispatcher();
            _GestureRecognizer = new GestureRecognizer(_ActionDispatcher);

            _Controller = new Controller();
            _Listener = new MainListener(_LogAction);
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

            var sa = new KeyPressAction("PressShiftA") { Key = "VK_A" };
            sa.Modifiers = "SHIFT,LCONTROL";
            var rt = new GestureTriggerCircle("RightHandCwCircle") { Hand = _HandManager.RightHand, IsClockwise = true, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true };
            _ActionDispatcher.AddAction(sa);
            _ActionDispatcher.AddTrigger(rt);

            var sa2 = new KeyPressAction("PressA") { Key = "VK_A" };
            var rt2 = new GestureTriggerCircle("RightHandCcwCircle") { Hand = _HandManager.RightHand, IsClockwise = false, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true };
            _ActionDispatcher.AddAction(sa2);
            _ActionDispatcher.AddTrigger(rt2);

            var kma = new KeyMacroAction("Alt+Tab");
            kma.Keys = "LMENU,TAB";
            var lt = new GestureTriggerCircle("LeftHandCwCircle") { Hand = _HandManager.LeftHand, IsClockwise = true, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true };
            _ActionDispatcher.AddAction(kma);
            _ActionDispatcher.AddTrigger(lt);

            var kma2 = new KeyMacroAction("CtrlR+CtrlR");
            kma2.Keys = "LCONTROL,VK_R,LCONTROL,VK_R";
            var lt2 = new GestureTriggerCircle("LeftHandCcwCircle") { Hand = _HandManager.LeftHand, IsClockwise = false, MinRadius = 0, MaxRadius = 1000, RequiresStabilized = true };
            _ActionDispatcher.AddAction(kma2);
            _ActionDispatcher.AddTrigger(lt2);

            var mc = new MouseClickAction("LeftClick") { Button = "LBUTTON" };

            //_LogAction(rt.ToXml());
            //_LogAction(rt2.ToXml());
            //_LogAction(lt.ToXml());
            //_LogAction(lt2.ToXml());
            //_LogAction(sa.ToXml());
            //_LogAction(sa2.ToXml());
            //_LogAction(kma.ToXml());
            //_LogAction(kma2.ToXml());
            //_LogAction(mc.ToXml());

            var x = new System.Xml.XmlDocument();
            x.LoadXml(@"
<Configuration>
  <Triggers>
    <Trigger type=""GestureTriggerCircle"" isClockwise=""True"" minRadius=""0"" maxRadius=""1000"" name=""RightHandCwCircle"" reqStable=""True""  />
    <Trigger type=""GestureTriggerCircle"" isClockwise=""False"" minRadius=""0"" maxRadius=""1000"" name=""RightHandCcwCircle"" reqStable=""True""  />
    <Trigger type=""GestureTriggerCircle"" isClockwise=""True"" minRadius=""0"" maxRadius=""1000"" name=""LeftHandCwCircle"" reqStable=""True""  />
    <Trigger type=""GestureTriggerCircle"" isClockwise=""False"" minRadius=""0"" maxRadius=""1000"" name=""LeftHandCcwCircle"" reqStable=""True""  />
  </Triggers>
  <Actions>
    <Action type=""KeyPressAction"" key=""VK_A"" mods=""SHIFT,LCONTROL"" name=""PressShiftA""  />
    <Action type=""KeyPressAction"" key=""VK_A"" mods="""" name=""PressA""  />
    <Action type=""KeyMacroAction"" keys=""LMENU,TAB"" name=""Alt+Tab""  />
    <Action type=""KeyMacroAction"" keys=""LCONTROL,VK_R,LCONTROL,VK_R"" name=""CtrlR+CtrlR""  />
    <Action type=""MouseClickAction"" isDbl=""False"" button=""LBUTTON"" name=""LeftClick""  />
  </Actions>
</Configuration>
");
            foreach (System.Xml.XmlNode node in x.SelectNodes("/Configuration/Actions/Action"))
            {
                var o = (BaseAction)ConfigurationSerializer.FromXml(node);
                _LogAction(o.ToXml());
            }
            foreach (System.Xml.XmlNode node in x.SelectNodes("/Configuration/Triggers/Trigger"))
            {
                var o = (BaseTrigger)ConfigurationSerializer.FromXml(node);
                _LogAction(o.ToXml());
            }
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
                    System.IO.File.AppendAllText("C:\\leap.log", line + "\n");
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
