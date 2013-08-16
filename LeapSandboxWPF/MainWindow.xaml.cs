using System.Windows;
using Leap;

namespace LeapSandboxWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SandboxListener listener;
		private Controller controller;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Log.Content = "Hello!\n";
			Log.Content += "Yea!\n";

            try
            {
                // Create a sample listener and controller
                listener = new SandboxListener(Log);
                Log.Content += "Okay!\n";
                controller = new Controller();
                Log.Content += "Huzzah!\n";

                // Have the sample listener receive events from the controller
                controller.AddListener(listener);
                Log.Content += "Bingo!\n";
            }
            catch (System.Exception ex)
            {
                Log.Content += ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace + "\n";
            }
		}

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Remove the sample listener when done
            controller.RemoveListener(listener);
            listener.Dispose();
            controller.Dispose();
        }
	}
}
