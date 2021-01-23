namespace E.Data
{
    public class AsyncOperation
    {
        protected AsyncOperation() { }

        public System.Action onClose;

        /// <summary>
        /// is the task finished?
        /// </summary>
        public bool IsClosed { get; protected set; } = false;

        public virtual void Close()
        {
            IsClosed = true;
        }
    }
}
