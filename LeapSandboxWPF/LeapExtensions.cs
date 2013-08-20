using System;
using Leap;

namespace Vyrolan.VMCS
{
    public static class LeapExtensions
    {
        public static int RollDegrees(this Hand hand)
        {
            return hand.PalmNormal.RollDegrees();
        }
        public static int PitchDegrees(this Hand hand)
        {
            return hand.Direction.PitchDegrees();
        }
        public static int YawDegrees(this Hand hand)
        {
            return hand.Direction.YawDegrees();
        }

        public static int RollDegrees(this Vector vector)
        {
            return Convert.ToInt32(vector.Roll*180.0f/(float) Math.PI);
        }
        public static int PitchDegrees(this Vector vector)
        {
            return Convert.ToInt32(vector.Pitch * 180.0f / (float)Math.PI);
        }
        public static int YawDegrees(this Vector vector)
        {
            return Convert.ToInt32(vector.Yaw*180.0f/(float) Math.PI);
        }
    }
}
