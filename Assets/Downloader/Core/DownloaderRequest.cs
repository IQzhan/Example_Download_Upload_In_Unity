namespace E.Net
{
    public abstract class DownloaderRequest
    {
        protected DownloaderRequest() { }

        public int Timeout = 5 * 1000;

        public int MaxRetryTime = 12;

        public bool LoadAfterDownloaded { get; set; } = true;

        public byte[] Data { get; protected set; }

        public System.Action onClose;

        /// <summary>
        /// Close connection
        /// </summary>
        public void Close()
        {
            IsConnecting = false;
            IsDownloading = false;
            IsLoading = false;
            IsClosed = true;
        }

        /// <summary>
        /// connecting to source uri?
        /// </summary>
        public bool IsConnecting { get; protected set; } = false;

        /// <summary>
        /// task is downloading
        /// </summary>
        public bool IsDownloading { get; protected set; } = false;

        /// <summary>
        /// task is loading
        /// </summary>
        public bool IsLoading { get; protected set; } = false;

        /// <summary>
        /// is the task finished?
        /// </summary>
        public bool IsClosed { get; protected set; } = false;

        /// <summary>
        /// is the task caused error?
        /// </summary>
        public bool IsError { get; protected set; } = false;

        /// <summary>
        /// is download complete?
        /// </summary>
        public bool IsDownloadComplete { get { return DownloadProgress == 1; } }

        /// <summary>
        /// is loading successd?
        /// </summary>
        public bool IsLoadingComplete { get { return LoadingProgress == 1; } }

        /// <summary>
        /// Size of data bytes
        /// </summary>
        public long Size { get; protected set; }

        /// <summary>
        /// Downloaded size of data bytes
        /// </summary>
        public long DownloadedSize { get; protected set; } = 0;

        /// <summary>
        /// Loaded size of data bytes
        /// </summary>
        public long LoadedSize { get; protected set; } = 0;

        /// <summary>
        /// value [0, 1]
        /// </summary>
        public double Progress 
        { get { return LoadAfterDownloaded ? (DownloadProgress + LoadingProgress) / 2 : DownloadProgress; } }

        /// <summary>
        /// Download progress value [0, 1]
        /// </summary>
        public double DownloadProgress { get { return (double)DownloadedSize / Size; } }

        /// <summary>
        /// Loading progress value [0, 1]
        /// </summary>
        public double LoadingProgress { get { return (double)LoadedSize / Size; } }

        /// <summary>
        /// byte/second
        /// </summary>
        public double Speed
        { get { return DownloadSpeed + LoadingSpeed; } }

        /// <summary>
        /// Download speed byte/second
        /// </summary>
        public double DownloadSpeed 
        { get { CalculateSpeed(ref lastDownloadedSizeTime, ref lastDownloadedSize, DownloadedSize, ref downloadSpeed); return downloadSpeed; } }

        /// <summary>
        /// Loading speed byte/second
        /// </summary>
        public double LoadingSpeed
        {  get { CalculateSpeed(ref lastLoadedSizeTime, ref lastLoadedSize, LoadedSize, ref loadingSpeed); return loadingSpeed; } }
        
        /// <summary>
        /// second
        /// </summary>
        public double RemainingTime
        { get { return RemainingDownloadTime + RemainingLoadingTime; } }

        /// <summary>
        /// second
        /// </summary>
        public double RemainingDownloadTime
        { get { return CalculateRemainingTime(DownloadedSize, DownloadSpeed); } }

        /// <summary>
        /// second
        /// </summary>
        public double RemainingLoadingTime
        { get { return CalculateRemainingTime(LoadedSize, LoadingSpeed); } }

        private long lastDownloadedSizeTime;

        private long lastDownloadedSize;

        private double downloadSpeed;

        private long lastLoadedSizeTime;

        private long lastLoadedSize;

        private double loadingSpeed;

        private const long oneSecond = 1000;

        private void CalculateSpeed(ref long lastTime, ref long lastSize, long currentSize, ref double speed)
        {
            long deltaTime = DownloaderClock.Milliseconds - lastTime;
            if (deltaTime >= oneSecond)
            {
                long deltaSize = currentSize - lastSize;
                speed = (double)deltaSize / deltaTime * oneSecond;
                lastTime = DownloaderClock.Milliseconds;
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