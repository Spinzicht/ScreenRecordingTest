using Ludio.VM;
using System.Windows;

namespace Ludio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        StreamViewModel vm = new StreamViewModel();

        public Overlay()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //vm.Loaded();
        }

        private void Scale_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            vm.CaptureWindow.ScaleEnabled = true;
        }

        private void Scale_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            vm.CaptureWindow.ScaleEnabled = false;
        }

        private void Logo_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            vm.Logo.MoveEnabled = true;
            vm.CaptureWindow.MoveEnabled = true;
        }

        private void Logo_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            vm.CaptureWindow.MoveEnabled = false;
            vm.Logo.MoveEnabled = false;
        }

        private void Resize_Click(object sender, RoutedEventArgs e)
        {
            vm.CaptureWindow.Maximized = !vm.CaptureWindow.Maximized;
        }
    }
}
