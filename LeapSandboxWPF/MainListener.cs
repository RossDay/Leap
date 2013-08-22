using System;
using System.Collections.Generic;
using Leap;

namespace Vyrolan.VMCS
{
    internal class MainListener : Listener, IFrameUpdater
    {
        private readonly Action<string> _LogAction;
        private readonly LinkedList<IFrameUpdate> _FrameUpdateItems = new LinkedList<IFrameUpdate>();

        public MainListener(Action<string> logAction)
        {
            _LogAction = logAction;
            _LogAction("Main Listener Constructed");
        }

        #region Listener Implementation
        public override void OnInit(Controller controller)
        {
            _LogAction("Initialized");
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
        }

        public override void OnConnect(Controller controller)
        {
            _LogAction("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            //controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            //controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
            _LogAction("Gesures Enabled");
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            _LogAction("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            _LogAction("Exited");
        } 
        #endregion

        public void RegisterForFrameUpdates(IFrameUpdate item)
        {
            lock (_FrameUpdateItems)
                _FrameUpdateItems.AddLast(item);
        }
        public void UnregisterForFrameUpdates(IFrameUpdate item)
        {
            lock (_FrameUpdateItems)
                _FrameUpdateItems.Remove(item);
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            var frame = controller.Frame();

            try
            {
                // Update all registered items
                lock (_FrameUpdateItems)
                {
                    var item = _FrameUpdateItems.First;
                    while (item != null)
                    {
                        var next = item.Next;
                        if (!item.Value.Update(frame))
                            _FrameUpdateItems.Remove(item);
                        item = next;
                    }
                }
            }
            catch (Exception e)
            {
                _LogAction("Exception: " + e.GetType().FullName + "\n" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
