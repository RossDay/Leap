using System;
using System.Linq;
using Leap;

namespace Vyrolan.VMCS
{
    class GrabAndScroll
    {
        public GrabAndScroll(Action<string> logAction)
        {
            _LogAction = logAction;
        }

        //private readonly InputSimulator _InputSim = new InputSimulator();
        private readonly Action<string> _LogAction;
        private int _Progress;

        private readonly PersistentHand _ActiveHand = new PersistentHand();
        private bool _IsGrabbed;

        public void OnFrame(Frame frame)
        {
            if (!_ActiveHand.Update(frame))
            {
                _IsGrabbed = false;
                if (!frame.Hands.Empty)
                    _ActiveHand.Initialize(frame.Hands.Leftmost);
            }

            if (!_ActiveHand.IsFinalized)
                _LogAction(_ActiveHand.Dump());

            // Do nothing if the hand is not yet stabilized
            if (!_ActiveHand.IsStabilized)
                return;

            // We have the same Hand as in the past and we're still grabbed.
            if (_IsGrabbed)
            {
                if (_ActiveHand.CurrentHand.Fingers.Count >= 3)
                {
                    _IsGrabbed = false;
                }
                else
                {
                    var startY = _ActiveHand.StabilizedHand.StabilizedPalmPosition.y;
                    var y = _ActiveHand.CurrentHand.StabilizedPalmPosition.y;
                    //_LogAction(String.Format("Hand {0} now at {1:0.0} was grabbed at {2:0.0}.", _ActiveHand.Id, y, startY));
                    if (y < startY - 15)
                        for (var i = 0; i < Math.Floor((startY - y) / 20); i++)
                            Native.ScrollActiveWindow(false);
                    else if (y > startY + 15)
                        for (var i = 0; i < Math.Floor((y - startY) / 20); i++)
                            Native.ScrollActiveWindow(true);
                }
            }
            else if (_ActiveHand.CurrentHand.Fingers.Count < 2)
            {
                _IsGrabbed = true;
            }

            foreach (var g in frame.Gestures().Where(g => g.Type == Gesture.GestureType.TYPECIRCLE))
            {
                var circle = new CircleGesture(g);

                var isClockwise = (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4);

                if (circle.Progress < _Progress || circle.Progress > _Progress + 1)
                {
                    Native.ScrollActiveWindow(!isClockwise);
                    _Progress = Convert.ToInt32(Math.Floor(circle.Progress));
                }
            }
        }

    }
}
