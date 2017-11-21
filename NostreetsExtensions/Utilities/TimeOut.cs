using System;
using System.Linq;
using System.Threading;

namespace NostreetsExtensions.Utilities
{
    public class TimeOut
    {
        public static T Wait<T>(int miliseconds, Func<T> func)
        {
            Thread.Sleep(miliseconds);
            return func();
        }

        public static void Wait(int miliseconds, Action func)
        {
            Thread.Sleep(miliseconds);
            func();
        }
    }
}
