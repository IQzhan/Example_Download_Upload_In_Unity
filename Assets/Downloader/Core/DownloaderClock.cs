using System.Diagnostics;

namespace E.Net
{
    public static class DownloaderClock
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        static DownloaderClock()
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
