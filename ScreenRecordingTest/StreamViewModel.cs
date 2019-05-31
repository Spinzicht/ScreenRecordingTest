using System.Windows;
using F = System.Windows.Forms;
using System.Windows.Threading;
using System.Threading;

using Core.Util;
using Core.Data;
using Core;

using Chronos.Audio;
using Chronos.Display;
using Chronos.Tracking;

using Ludio.UI;

namespace Ludio.VM
{
    class StreamViewModel : Observable
    {
        int windowID = 0;
        int windows = 4;

        DispatcherTimer t;

        private DisplayCapture _display = new GDIScreenCapture();
        public DisplayCapture DisplayCapture
        {
            get => _display;
            set
            {
                _display?.Stop();
                Set(ref _display, value);
                _display?.Start();
            }
        }

        public AudioCapture AudioCapture { get; set; } = new NAudioCapture();

        public Editable CaptureWindow { get; set; } = new Scalable(480, 270, 190, 100, 1900, 1080,
                                                      new Movable(new Thickness(15), new Vector(40, 40),
                                                      new Alignable(VerticalAlignment.Top, HorizontalAlignment.Right)));

        public Editable Logo { get; set; } = new Movable(new Thickness(10), new Vector(60, 60),
                                             new Alignable(VerticalAlignment.Top, HorizontalAlignment.Right));

        private string _name = "Ludio";
        private SynchronizationContext _syncContext;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public StreamViewModel()
        {
            SwitchDisplay();
            WindowTracker.I.OnWindowChanged += WindowChanged;
            WindowTracker.I.Start();
            Name = WindowTracker.I.Window.GetTitle();

            KeyboardTracker.I.OnKeyPressed += KeyPressed;

            _syncContext = SynchronizationContext.Current;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (int)F.Keys.Tab:
                    SwitchDisplay();
                    break;
                case (int)F.Keys.Z:
                    ToggleZoom();
                    break;
            }
        }
        private void ToggleZoom()
        {
            if (User.I.Role != Role.Host) return;

            DisplayCapture.Zoom = DisplayCapture.Zoom == 5 ? 1 : DisplayCapture.Zoom + 2;
        }

        private void SwitchDisplay()
        {
            if (User.I.Role != Role.Host) return;

            windowID = windowID == windows - 1 ? 0 : windowID + 1;

            switch (windowID)
            {
                case 0:
                    DisplayCapture = new DXScreenCapture();
                    break;
                case 1:
                    DisplayCapture = new GDIScreenCapture();
                    ((GDIScreenCapture)DisplayCapture).Windowed = true;
                    break;
                case 2:
                    ((GDIScreenCapture)DisplayCapture).Windowed = false;
                    break;
                case 3:
                    DisplayCapture = new AForgeCameraCapture();
                    break;
            }
        }

        private void WindowChanged(object sender, WindowEventArgs e)
        {
            Name = e.Window.GetTitle();
        }

        internal void Loaded()
        {
            if (User.I.Role == Role.Host)
            {
                "started host".Print();
                DisplayCapture.Start();
                //AudioCapture.Start();
            }
            else
            {
                "started client".Print();
                DisplayCapture = new NetworkDisplayCapture();
                //AudioCapture = new NetworkAudioCapture();
            }
        }
    }
}
