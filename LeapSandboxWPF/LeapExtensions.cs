using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace Vyrolan.VMCS
{
    public static class LeapExtensions
    {
        public static int RollDegrees(this Hand hand)
        {
            return Convert.ToInt32(hand.PalmNormal.Roll*180.0f/(float) Math.PI);
        }
        public static int PitchDegrees(this Hand hand)
        {
            return Convert.ToInt32(hand.Direction.Pitch * 180.0f / (float)Math.PI);
        }
        public static int YawDegrees(this Hand hand)
        {
            return Convert.ToInt32(hand.Direction.Yaw * 180.0f / (float)Math.PI);
        }
    }
}
