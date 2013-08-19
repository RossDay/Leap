using System.Windows;
using Leap;

namespace LeapSandboxWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Listener listener;
        private Controller controller;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a sample listener and controller
                //listener = new SandboxListener(Log);
                listener = new MainListener(Log);
                controller = new Controller();

                // Have the sample listener receive events from the controller
                controller.AddListener(listener);
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
