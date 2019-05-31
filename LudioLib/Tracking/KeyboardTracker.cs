using Core.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chronos.Tracking
{
    public class KeyboardTracker
    {
        private static KeyboardTracker _instance;

        public static KeyboardTracker I
        {
            get
            {
                if (_instance == null) _instance = new KeyboardTracker();
                return _instance;
            }
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private KeyboardTracker()
        {
            _proc = HookCallback;
            Start();
        }

        ~KeyboardTracker()
        {
            Stop();
        }

        public void Start()
        {
            _hookID = SetHook(_proc);
        }

        public void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    if (_onKeyPressed != null) { _onKeyPressed.Raise(this, new KeyEventArgs(vkCode)); }
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    if (_onKeyReleased != null) { _onKeyReleased.Raise(this, new KeyEventArgs(vkCode)); }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected readonly WeakEventSource<KeyEventArgs> _onKeyPressed = new WeakEventSource<KeyEventArgs>();
        public event EventHandler<KeyEventArgs> OnKeyPressed
        {
            add { _onKeyPressed.Subscribe(value); }
            remove { _onKeyPressed.Unsubscribe(value); }
        }

        protected readonly WeakEventSource<KeyEventArgs> _onKeyReleased = new WeakEventSource<KeyEventArgs>();
        public event EventHandler<KeyEventArgs> OnKeyReleased
        {
            add { _onKeyReleased.Subscribe(value); }
            remove { _onKeyReleased.Unsubscribe(value); }
        }
    }

    public class KeyEventArgs : EventArgs
    {
        public int Key { get; private set; }

        public KeyEventArgs(int key)
        {
            Key = key;
        }
    }
}
