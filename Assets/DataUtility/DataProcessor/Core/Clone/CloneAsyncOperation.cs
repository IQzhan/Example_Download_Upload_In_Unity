namespace E.Data
{
    public class CloneAsyncOperation : AsyncOperation
    {
        public struct User
        {
            public string username;
            public string password;
        }

        public User sourceUser;

        public User targetUser;

        protected CloneAsyncOperation() { }

        public int Timeout = 5 * 1000;

        public int MaxRetryTime = 12;

        public bool ForceTestConnection
        { 
            get { return forceTestConnection; } 
            set 
            { 
                if (!IsWorking) { forceTestConnection = value; } 
                else throw new System.MemberAccessException("can't access ForceTestConnection while task is working."); 
            } 
        }

        public bool LoadData
        {
            get { return loadData; }
            set
            {
                if (!IsWorking) { loadData = value; }
                else throw new System.MemberAccessException("can't access LoadAfterDownloaded while task is working.");
            }
        }

        public byte[] Data { get; protected set; }

        /// <summary>
        /// Close connection
        /// </summary>
        public override void Close()
        {
            IsConnecting = false;
            base.Close();
        }

        /// <summary>
        /// connecting to source uri?
        /// </summary>
        public bool IsConnecting { get; protected set; } = false;

        /// <summary>
        /// Size of data bytes
        /// </summary>
        public long Size { get; protected set; }

        /// <summary>
        /// processed data bytes
        /// </summary>
        public long ProcessedBytes { get; protected set; } = 0;

        /// <summary>
        /// progress of this task [0, 1]
        /// </summary>
        public override double Progress { get { return (double)ProcessedBytes / Size; } }

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

        private bool forceTestConnection = false;

        private bool loadData = true;

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
