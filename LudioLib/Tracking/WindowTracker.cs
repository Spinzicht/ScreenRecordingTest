using System;
using System.Windows.Input;
using System.Runtime.InteropServices;

using Chronos.Display;
using Core.Util;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Chronos.Tracking
{
    public class WindowTracker
    {
        protected readonly WeakEventSource<WindowEventArgs> _onWindowChanged = new WeakEventSource<WindowEventArgs>();
        public event EventHandler<WindowEventArgs> OnWindowChanged
        {
            add { _onWindowChanged.Subscribe(value); }
            remove { _onWindowChanged.Unsubscribe(value); }
        }

        private static WindowTracker _instance;
        public static WindowTracker I
        {
            get
            {
                if (_instance == null) _instance = new WindowTracker();
                return _instance;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        public Window Window { get; set; }

        private WindowTracker()
        {
            Window = new Window(GetForegroundWindow());
        }

        public void Start()
        {
            KeyboardTracker.I.OnKeyReleased += KeyReleased;
            MouseTracker.I.OnMouseClicked += MouseClicked;
        }

        public void Stop()
        {
            KeyboardTracker.I.OnKeyReleased -= KeyReleased;
            MouseTracker.I.OnMouseClicked -= MouseClicked;
        }

        private void MouseClicked(object sender, MouseClickedArgs e)
        {
            UpdateWindow(200);
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            UpdateWindow(200);
        }

        private async void UpdateWindow(int delay, int count = 0)
        {
            await Task.Delay(delay);
            var id = GetForegroundWindow();
            IntPtr ownId = Process.GetCurrentProcess().MainWindowHandle;
            if (ownId == id) return;
            if (id != Window.ID)
            {
                Window.ID = id;
                _onWindowChanged.Raise(this, new WindowEventArgs(Window));
            }
            else if (count < 5)
            {
                UpdateWindow(delay * 2, count++);
            }
        }
    }

    public class WindowEventArgs : EventArgs
    {
        public Window Window { get; private set; }

        public WindowEventArgs(Window w)
        {
            Window = w;
        }
    }
}
