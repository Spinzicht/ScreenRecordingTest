using Core.Util;
using System;

namespace Core
{
    public static class ExtensionMethods
    {
        public static object Print(this object obj, string msg = "")
        {
            msg = msg == "" ? msg : msg + ": ";
            var o = (obj == null ? "null" : obj.ToString());
            Console.WriteLine(msg + o);
            Log.I.Add(msg + o);
            return obj;
        }
    }
}
