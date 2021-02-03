namespace E.Data
{
    public abstract class AsyncOperation
    {
        protected AsyncOperation() { }

        public struct Account
        {
            public string username;
            public string password;
        }

        public Account sourceAccount;

        public Account targetAccount;

        public int Timeout = 5 * 1000;

        /// <summary>
        /// callback on closed
        /// </summary>
        public System.Action onClose;
        
        /// <summary>
        /// is the task working?
        /// </summary>
        public bool IsWorking { get; protected set; } = false;

        /// <summary>
        /// is the task finished?
        /// </summary>
        public bool IsClosed { get; private set; } = false;

        /// <summary>
        /// is the task caused error?
        /// </summary>
        public bool IsError { get; protected set; } = false;

        /// <summary>
        /// connecting to source uri?
        /// </summary>
        public bool IsConnecting { get { return IsWorking && !IsProcessing; } }

        /// <summary>
        /// task is processing
        /// </summary>
        public bool IsProcessing { get { return IsWorking && Progress > 0 && Progress < 1; } }

        /// <summary>
        /// is the task process data complete?
        /// </summary>
        public bool IsProcessingComplete { get { return Progress == 1; } }

        /// <summary>
        /// progress of this task [0, 1]
        /// </summary>
        public abstract double Progress { get; }

        public virtual void Close()
        {
            IsWorking = false;
            IsClosed = true;
        }
    }
}
