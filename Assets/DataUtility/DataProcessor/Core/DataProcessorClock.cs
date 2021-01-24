using System.Diagnostics;

namespace E.Data
{
    public static class DataProcessorClock
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        static DataProcessorClock()
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
