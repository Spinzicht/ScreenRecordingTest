using Core.Util;
using System;

namespace Core.Util
{
    public class Log : Observable
    {
        private static Log _i;

        public static Log I { get { if (_i == null) _i = new Log(); return _i; } }

        private Log() { Add("Log Started"); }

        public void Add(string s)
        {
            if(s == "" || s == null)
                Text += ".";
            else Text += s + Environment.NewLine;
            while(Text.Split(Environment.NewLine.ToCharArray()[0]).Length > 100)
            {
                Text = Text.Substring(Text.IndexOf(Environment.NewLine) + 1);
            }
            Notify(nameof(Text));
        }

        public string Text { get; private set; } = "";
    }
}
