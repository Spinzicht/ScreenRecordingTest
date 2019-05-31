using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chronos.Display
{
    public class Window
    {
        public IntPtr ID { get; set; }

        public Window(IntPtr id)
        {
            ID = id;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow( );

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr id, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr id, out Rectangle lpRect);


        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public string GetTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(ID, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public Rectangle GetBounds()
        {
            Rectangle r = new Rectangle();
            GetWindowRect(ID, out r);
            //needs to be tested, used to work with system.drawing dll
            return r;
        }
    }
}
