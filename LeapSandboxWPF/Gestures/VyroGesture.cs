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

        protected virtual VyroGestureState UpdateGesture(Frame frame)
        {
            return VyroGestureState.Invalid;
        }
        public VyroGestureState Update(Frame frame)
        {
            var state = UpdateGesture(frame);
            LastUpdateTime = frame.Timestamp;
            return state;
        }
    }
}
