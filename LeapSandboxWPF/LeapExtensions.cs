using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
	public static class LeapExtensions
	{
		public static float RollDegress(this Hand hand)
		{
			return hand.PalmNormal.Roll*180.0f/(float) Math.PI;
		}
		public static float PitchDegress(this Hand hand)
		{
			return hand.Direction.Pitch * 180.0f / (float)Math.PI;
		}
		public static float YawDegress(this Hand hand)
		{
			return hand.Direction.Yaw * 180.0f / (float)Math.PI;
		}
	}
}
