using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapSandboxWPF
{
	public static class LeapExtensions
	{
		/*
				// Get the hand's normal vector and direction
				Vector normal = hand.PalmNormal;
				Vector direction = hand.Direction;


 				SafeWriteLine("Hand pitch: " + direction.Pitch * 180.0f / (float)Math.PI + " degrees, "
	              + "roll: " + normal.Roll * 180.0f / (float)Math.PI + " degrees, "
	              + "yaw: " + direction.Yaw * 180.0f / (float)Math.PI + " degrees");
*/
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
