using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Leap;

namespace Vyrolan.VMCS
{
    internal class MainListener : Listener, IFrameUpdater
    {
        private readonly Label _Log;
        private readonly Action<string> _LogAction;
        private readonly LinkedList<IFrameUpdate> _FrameUpdateItems = new LinkedList<IFrameUpdate>();
        private readonly PersistentHand[] _PotentialHands = new[] { new PersistentHand(), new PersistentHand() };
        private PersistentHand _LeftHand = new PersistentHand();
        private PersistentHand _RightHand = new PersistentHand();

        public MainListener(Label log)
        {
            _Log = log;
            _LogAction = SafeWriteLine;
            _LogAction("Constructed");

            RegisterForFrameUpdates(_LeftHand);
            RegisterForFrameUpdates(_RightHand);
            RegisterForFrameUpdates(_PotentialHands[0]);
            RegisterForFrameUpdates(_PotentialHands[1]);
        }

        #region Listener Implementation / SafeWriteLine
        private readonly Object _Lock = new Object();
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
                            _Log.Content = "Exception: " + e.GetType().FullName + "\n" + e.Message;
                        }));
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            //controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            //controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
            SafeWriteLine("Gesures Enabled");
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        } 
        #endregion

        public void RegisterForFrameUpdates(IFrameUpdate item)
        {
            _FrameUpdateItems.AddLast(item);
        }
        public void UnregisterForFrameUpdates(IFrameUpdate item)
        {
            _FrameUpdateItems.Remove(item);
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            var frame = controller.Frame();

            // Update all registered items
            var item = _FrameUpdateItems.First;
            while (item != null)
            {
                var next = item.Next;
                if (!item.Value.Update(frame))
                    _FrameUpdateItems.Remove(item);
                item = next;
            }

            foreach (var h in frame.Hands)
            {
                // Skip all the currently-tracked hands
                if (h.Id == _LeftHand.Id 
                    || h.Id == _RightHand.Id 
                    || h.Id == _PotentialHands[0].Id
                    || h.Id == _PotentialHands[1].Id
                )
                    continue;

                // New potential hand
                foreach (var ph in _PotentialHands)
                    if (ph.IsFinalized)
                    {
                        ph.Initialize(h);
                        break;
                    }
                // else we already have two potentials?  too many hands!
            }

            // Check for a new left hand
            if (_LeftHand.IsFinalized)
                for (var i = 0; i < 2; i++)
                    if (_PotentialHands[i].IsStabilized && _PotentialHands[i].StabilizedHand.PalmPosition.x < 0)
                    {
                        var ph = _LeftHand;
                        _LeftHand = _PotentialHands[i];
                        _PotentialHands[i] = ph;
                        break;
                    }

            // Check for a new left hand
            if (_RightHand.IsFinalized)
                for (var i = 0; i < 2; i++)
                    if (_PotentialHands[i].IsStabilized && _PotentialHands[i].StabilizedHand.PalmPosition.x > 0)
                    {
                        var ph = _RightHand;
                        _RightHand = _PotentialHands[i];
                        _PotentialHands[i] = ph;
                        break;
                    }

            var s = "";
            if (_LeftHand.IsStabilized)
                s += "\nLeft Hand:\n------------------------------------------------------------------------\n" + _LeftHand.Dump();
            if (_RightHand.IsStabilized)
                s += "\nRight Hand:\n------------------------------------------------------------------------\n" + _RightHand.Dump();
            if (!_PotentialHands[0].IsFinalized)
                s += "\nPotential Hand 0:\n------------------------------------------------------------------------\n" + _PotentialHands[0].Dump();
            if (!_PotentialHands[1].IsFinalized)
                s += "\nPotential Hand 1:\n------------------------------------------------------------------------\n" + _PotentialHands[1].Dump();
            if (!String.IsNullOrEmpty(s))
                _LogAction(s);

        }


    }
}
