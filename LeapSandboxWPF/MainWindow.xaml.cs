using System.Windows;

namespace Vyrolan.VMCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ControlSystem _ControlSystem;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _ControlSystem = new ControlSystem(Log);
            }
            catch (System.Exception ex)
            {
                Log.Content += ex.GetType().Name + "\n" + ex.Message + "\n" + ex.StackTrace + "\n";
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _ControlSystem.Dispose();
        }
    }
}
