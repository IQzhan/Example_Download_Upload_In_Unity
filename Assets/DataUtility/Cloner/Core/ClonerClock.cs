using System.Diagnostics;

namespace E.Data
{
    public static class ClonerClock
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        static ClonerClock()
        {
            stopwatch.Start();
        }

        public static double Seconds
        {
            get
            {
                return (double)stopwatch.ElapsedMilliseconds / 1000;
            }
        }

        public static long Milliseconds
        {
            get
            {
                return stopwatch.ElapsedMilliseconds;
            }
        }
    }
}
