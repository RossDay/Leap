using System.Collections.Generic;
using System.Linq;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal class GestureRecognizer : IFrameUpdate
    {
        private readonly LinkedList<VyroGesture> _CurrentGestures = new LinkedList<VyroGesture>();
        private readonly GestureDispatcher _Dispatcher;

        public bool Update(Frame frame)
        {
            // List of Ids for current Leap Gestures
            var currentIds = _CurrentGestures.Where(g => g is VyroLeapGesture).Select(g => ((VyroLeapGesture)g).LeapGestureId).ToList();
            // Find any new Leap Gestures
            var newLeapGestures = frame.Gestures().Where(ge => !currentIds.Contains(ge.Id));

            // Update current gestures dispatching activated ones and removing the invalid/complete ones
            var item = _CurrentGestures.First;
            while (item != null)
            {
                var next = item.Next;
                var state = item.Value.Update(frame);
                if (state == VyroGestureState.DiscreteComplete || state == VyroGestureState.IterationComplete)
                    ; // TODO: Dispatch
                if (state == VyroGestureState.Invalid || state == VyroGestureState.DiscreteComplete || state == VyroGestureState.ContinuousComplete)
                    _CurrentGestures.Remove(item);
                item = next;
            }

            // Add new Leap Gestures
            foreach (var g in newLeapGestures)
                _CurrentGestures.AddLast(VyroLeapGesture.CreateFromLeapGesture(g));

            return true;
        }
    }
}
