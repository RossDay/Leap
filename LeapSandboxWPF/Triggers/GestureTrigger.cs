using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vyrolan.VMCS.Gestures;

namespace Vyrolan.VMCS.Triggers
{
    internal class GestureTrigger : BaseTrigger
    {
        public VyroGesture Gesture { get; set; }
    }

    internal class GestureTriggerCircle : GestureTrigger
    {
        private new VyroGestureCircle Gesture
        {
            get { return base.Gesture as VyroGestureCircle; }
        }
    }
}
