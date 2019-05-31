using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Chronos.Tracking
{
    public class ActivityTracker
    {
        public KeyboardTracker Keyboard { get; protected set; }
        public MouseTracker Mouse { get; protected set; }
        public WindowTracker Window { get; protected set; }

        public MouseClick MouseClick { get; protected set; }
        public Vector MousePosition { get; protected set; }

        private static ActivityTracker _instance;
        public static ActivityTracker I
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityTracker();
                }
                return _instance;
            }
        }

        protected ActivityTracker()
        {
            Keyboard = KeyboardTracker.I;
            Mouse = MouseTracker.I;
            Window = WindowTracker.I;
        }
    }
}
