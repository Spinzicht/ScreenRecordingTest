using Ludio.Base;
using System.Windows;

namespace Ludio.VM
{
    public class WindowControls : ViewModel
    {
        private Window _window;

        public RelayCommand MaximizeCommand { get; private set; }
        public RelayCommand MinimizeCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public bool IsWindowMaximized { get { return _window?.WindowState == WindowState.Maximized; } }

        public WindowControls()
        {
            MaximizeCommand = new RelayCommand(Maximize);
            MinimizeCommand = new RelayCommand(Minimize);
            CloseCommand = new RelayCommand(Close);
        }

        public void Init(Window window)
        {
            _window = window;
            Notify(nameof(IsWindowMaximized));
        }

        private void Minimize(object o)
        {
            _window.WindowState = WindowState.Minimized;
            Notify(nameof(IsWindowMaximized));
        }

        private void Maximize(object obj)
        {
            _window.WindowState = _window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            Notify(nameof(IsWindowMaximized));
        }

        private void Close(object obj)
        {
            _window.Close();
        }
    }
}
