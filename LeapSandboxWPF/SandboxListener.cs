/******************************************************************************\
* Copyright (C) 2012-2013 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using System;
using System.Linq;

using Leap;
using WindowsInput;
using WindowsInput.Native;

namespace LeapSandboxWPF
{
	class SandboxListener : Listener
	{
		private readonly InputSimulator _InputSim = new InputSimulator();
        private readonly GrabAndScroll grabAndScroll;
		private readonly Object thisLock = new Object();
		private readonly System.Windows.Controls.Label Log;
		private int FrameCount;

		public SandboxListener(System.Windows.Controls.Label log)
		{
			Log = log;
			Log.Content += "Let's go!\n";
            grabAndScroll = new GrabAndScroll(SafeWriteLine);
		}

		private void SafeWriteLine(String line)
		{
			try
			{
				lock (thisLock)
				{
					Log.Dispatcher.Invoke(new Action(delegate
						{
							var newContent = Log.Content + line + "\n";
							var lines = newContent.Split('\n');
							newContent = String.Join("\n", lines.Skip(lines.Length - 35));
							Log.Content = newContent;
						}));
					//Console.WriteLine(line);
				}
			}
			catch (Exception e)
			{
				Log.Content = "Exception: " + e.GetType().FullName + "\n" + e.Message;
			}
		}

		public override void OnInit(Controller controller)
		{
			SafeWriteLine("Initialized");
			controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
		}

		public override void OnConnect(Controller controller)
		{
			SafeWriteLine("Connected");
			controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
			controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
			SafeWriteLine("Gesures Enabled");
		}

		public override void OnDisconnect(Controller controller)
		{
			//Note: not dispatched when running in a debugger.
			SafeWriteLine("Disconnected");
		}

		public override void OnExit(Controller controller)
		{
			SafeWriteLine("Exited");
		}

		public override void OnFrame(Controller controller)
		{
			// Get the most recent frame and report some basic information
			Frame frame = controller.Frame();

            grabAndScroll.OnFrame(frame);

			++FrameCount;
			if (FrameCount < 100 && frame.Gestures().Count == 0)
				return;
			FrameCount = 0;

			//SafeWriteLine("Frame id: " + frame.Id + ", timestamp: " + frame.Timestamp + ", hands: " + frame.Hands.Count + ", fingers: " + frame.Fingers.Count + ", tools: " + frame.Tools.Count + ", gestures: " + frame.Gestures().Count);

			if (!frame.Hands.Empty)
			{
				// Get the first hand
				Hand hand = frame.Hands[0];

				// Check if the hand has any fingers
				FingerList fingers = hand.Fingers;
				if (!fingers.Empty)
				{
					// Calculate the hand's average finger tip position
					Vector avgPos = Vector.Zero;
					foreach (Finger finger in fingers)
					{
						avgPos += finger.TipPosition;
					}
					avgPos /= fingers.Count;
					//SafeWriteLine("Hand has " + fingers.Count + " fingers, average finger tip position: " + avgPos);
				}

				/*
				try
				{
					if (fingers.Count == 1)
					{
						SafeWriteLine("InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_1);");
						_InputSim.Keyboard.KeyPress(VirtualKeyCode.VK_1);
					}
					else if (fingers.Count == 2)
					{
						SafeWriteLine("InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_2);");
						_InputSim.Keyboard.KeyPress(VirtualKeyCode.VK_2);
					}
					else if (fingers.Count == 3)
					{
						SafeWriteLine("InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_3);");
						_InputSim.Keyboard.KeyPress(VirtualKeyCode.VK_3);
					}
					else if (fingers.Count == 4)
					{
						SafeWriteLine("InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_4);");
						_InputSim.Keyboard.KeyPress(VirtualKeyCode.VK_4);
					}
					else if (fingers.Count == 5)
					{
						SafeWriteLine("InputSimulator.SimulateKeyPress(VirtualKeyCode.RETURN);");
						_InputSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
					}
				}
				catch (Exception e)
				{
					SafeWriteLine("EXCEPT: " + e.GetType().Name + "\n" + e.Message);
				}
				*/

				// Get the hand's sphere radius and palm position
				//SafeWriteLine("Hand sphere radius: " + hand.SphereRadius.ToString("n2") + " mm, palm position: " + hand.PalmPosition);

				// Get the hand's normal vector and direction
				Vector normal = hand.PalmNormal;
				Vector direction = hand.Direction;

				// Calculate the hand's pitch, roll, and yaw angles
				//SafeWriteLine("Hand pitch: " + direction.Pitch * 180.0f / (float)Math.PI + " degrees, " + "roll: " + normal.Roll * 180.0f / (float)Math.PI + " degrees, " + "yaw: " + direction.Yaw * 180.0f / (float)Math.PI + " degrees");
			}

			// Get gestures
			GestureList gestures = frame.Gestures();
			for (int i = 0; i < gestures.Count; i++)
			{
				Gesture gesture = gestures[i];

				switch (gesture.Type)
				{
					case Gesture.GestureType.TYPECIRCLE:
						CircleGesture circle = new CircleGesture(gesture);

						// Calculate clock direction using the angle between circle normal and pointable
						String clockwiseness;
						if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4)
						{
							//Clockwise if angle is less than 90 degrees
							clockwiseness = "clockwise";
						}
						else
						{
							clockwiseness = "counterclockwise";
						}

						float sweptAngle = 0;

						// Calculate angle swept since last frame
						if (circle.State != Gesture.GestureState.STATESTART)
						{
							CircleGesture previousUpdate = new CircleGesture(controller.Frame(1).Gesture(circle.Id));
							sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
						}

						//SafeWriteLine("Circle id: " + circle.Id + ", " + circle.State + ", progress: " + circle.Progress + ", radius: " + circle.Radius + ", angle: " + sweptAngle + ", " + clockwiseness); 
						break;
					case Gesture.GestureType.TYPESWIPE:
						SwipeGesture swipe = new SwipeGesture(gesture);
						//SafeWriteLine("Swipe id: " + swipe.Id + ", " + swipe.State + ", position: " + swipe.Position + ", direction: " + swipe.Direction + ", speed: " + swipe.Speed);
						if (swipe.State == Gesture.GestureState.STATESTART)
						{
							VirtualKeyCode[] nums = {VirtualKeyCode.VK_1, VirtualKeyCode.VK_2, VirtualKeyCode.VK_3, VirtualKeyCode.VK_4};
							if (!frame.Hands.Empty && frame.Hands[0].Fingers.Count >= 0 && frame.Hands[0].Fingers.Count < 5)
								_InputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, nums[frame.Hands[0].Fingers.Count - 1]);
						}
						break;
					case Gesture.GestureType.TYPEKEYTAP:
						KeyTapGesture keytap = new KeyTapGesture(gesture);
						//SafeWriteLine("Tap id: " + keytap.Id + ", " + keytap.State + ", position: " + keytap.Position + ", direction: " + keytap.Direction);
						break;
					case Gesture.GestureType.TYPESCREENTAP:
						ScreenTapGesture screentap = new ScreenTapGesture(gesture);
						//SafeWriteLine("Tap id: " + screentap.Id + ", " + screentap.State + ", position: " + screentap.Position + ", direction: " + screentap.Direction);
						break;
					default:
						SafeWriteLine("Unknown gesture type.");
						break;
				}
			}

			if (!frame.Hands.Empty || !frame.Gestures().Empty)
			{
				//SafeWriteLine("");
			}
		}
	}
}
