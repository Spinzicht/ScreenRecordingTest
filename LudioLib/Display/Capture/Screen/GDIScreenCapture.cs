using Chronos.Tracking;
using Core;
using Core.Util;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Chronos.Display
{
    public class GDIScreenCapture : DisplayCapture
    {
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            public const int CAPTUREBLT = 0x00CC0020 | 0x40000000; //CAPTUREBLT
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        public override Bitmap Display { get => base.Display;
            set
            {
                base.Display = value;
                UpdateHandlers();
            }
        }

        Window window;

        public bool Windowed { get; set; } = false;

        public GDIScreenCapture()
        {
            window = WindowTracker.I.Window;
            WindowTracker.I.OnWindowChanged += WindowChanged;
        }

        private void WindowChanged(object sender, WindowEventArgs e)
        {
            window = e.Window;
            window?.GetTitle()?.Print();
            if (window.ID == IntPtr.Zero)
                return;

            Resume();
        }

        protected readonly WeakEventSource<EventArgs> _onScreenCaptured = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> ScreenCaptured
        {
            add { _onScreenCaptured.Subscribe(value); }
            remove { _onScreenCaptured.Unsubscribe(value); }
        }

        public void UpdateHandlers()
        {
            _onScreenCaptured.Raise(this, new EventArgs());
        }

        public override async Task Run()
        {
            await base.Run();
            await Task.Run(() => {
                try
                {
                    // get the size
                    Rectangle windowRect = window.GetBounds();

                    int width = windowRect.Width;
                    int height = windowRect.Height;

                    if (height < 300 || width < 300)
                    {
                        Pause();
                        return;
                    }

                    IntPtr id = Windowed ? window.ID : IntPtr.Zero;
                    // get te hDC of the target window
                    IntPtr hdcSrc = Window.GetDC(id);
                    int nWidth = width / Zoom;
                    int nHeight = height / Zoom;

                    int middleX = (int)mousePos.X - nWidth / 2 ;
                    int startX = !Windowed ? Math.Max(Math.Min(middleX, width - nWidth - 1 + windowRect.Left), windowRect.Left):
                                             Math.Max(Math.Min(middleX, width - nWidth - 1), 0);

                    int middleY = (int)mousePos.Y - nHeight / 2 ;
                    int startY = !Windowed ? Math.Max(Math.Min(middleY, height - nHeight - 1 + windowRect.Top), windowRect.Top) :
                                             Math.Max(Math.Min(middleY, height - nHeight - 1 ), 0);

                    // create a device context we can copy to
                    IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                    // create a bitmap we can copy it to,
                    // using GetDeviceCaps to get the width/height
                    IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, nWidth, nHeight);
                    // select the bitmap object
                    IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                    // bitblt over
                    GDI32.BitBlt(hdcDest, 0, 0, nWidth, nHeight, hdcSrc, startX, startY, GDI32.SRCCOPY);
                    // restore selection
                    GDI32.SelectObject(hdcDest, hOld);
                    // clean up 
                    GDI32.DeleteDC(hdcDest);
                    Window.ReleaseDC(id, hdcSrc);
                    // get a .NET image object for it

                    var img = Image.FromHbitmap(hBitmap);

                    if (img != null)
                    {
                        Display = img;
                    }

                    // free up the Bitmap object
                    GDI32.DeleteObject(hBitmap);
                }
                catch(Exception e)
                {
                    "Stop".Print();
                    window.ID = Window.GetForegroundWindow();
                    if (window.ID == IntPtr.Zero)
                        Pause();
                }
            });
        }
    }
}
