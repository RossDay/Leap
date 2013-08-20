using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace Vyrolan.VMCS.Gestures
{
    internal enum VyroGestureState
    {
        Invalid,
        ContinuousComplete,
        DiscreteComplete,
        Progressing,
        IterationComplete,
    }

    internal abstract class VyroGesture
    {
        protected long LastUpdateTime { get; set; }

        protected abstract VyroGestureState UpdateGesture(Frame frame);

        public VyroGestureState Update(Frame frame)
        {
            var state = UpdateGesture(frame);
            LastUpdateTime = frame.Timestamp;
            return state;
        }
    }
}
