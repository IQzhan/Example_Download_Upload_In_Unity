namespace E.Data
{
    public abstract class ProcessAsyncOperation : ConnectionAsyncOperation
    {
        /// <summary>
        /// Size of data bytes
        /// </summary>
        public long Size { get; protected set; } = -1;

        /// <summary>
        /// processed data bytes
        /// </summary>
        public long ProcessedBytes { get; protected set; } = 0;

        /// <summary>
        /// progress of this task [0, 1]
        /// </summary>
        public override double Progress { get { return (Size > -1) ? ((double)ProcessedBytes / Size) : 0; } }

        /// <summary>
        /// speed byte/second
        /// </summary>
        public double Speed
        { get { CalculateSpeed(ref lastProcessedBytesTime, ref lastProcessedBytes, ProcessedBytes, ref speed); return speed; } }

        /// <summary>
        /// remaining time seconds
        /// </summary>
        public double RemainingTime
        { get { return CalculateRemainingTime(ProcessedBytes, Speed); } }

        private long lastProcessedBytesTime;

        private long lastProcessedBytes;

        private double speed;

        private const long oneSecond = 1000;

        private void CalculateSpeed(ref long lastTime, ref long lastSize, long currentSize, ref double speed)
        {
            long deltaTime = DataProcessorClock.Milliseconds - lastTime;
            if (deltaTime >= oneSecond)
            {
                long deltaSize = currentSize - lastSize;
                speed = (double)deltaSize / deltaTime * oneSecond;
                lastTime = DataProcessorClock.Milliseconds;
                lastSize = currentSize;
            }
        }

        private double CalculateRemainingTime(long currentSize, double speed)
        {
            long deltaSize = Size - currentSize;
            if (deltaSize == 0) { return 0; }
            else { if (speed == 0) { return 0; } else { return deltaSize / speed; } }
        }
    }
}