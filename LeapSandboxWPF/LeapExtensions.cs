using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
	public static class LeapExtensions
	{
		public static int RollDegress(this Hand hand)
		{
			return Convert.ToInt32(hand.PalmNormal.Roll*180.0f/(float) Math.PI);
		}
		public static int PitchDegress(this Hand hand)
		{
            return Convert.ToInt32(hand.Direction.Pitch * 180.0f / (float)Math.PI);
		}
		public static int YawDegress(this Hand hand)
		{
			return Convert.ToInt32(hand.Direction.Yaw * 180.0f / (float)Math.PI);
		}
	}
}
