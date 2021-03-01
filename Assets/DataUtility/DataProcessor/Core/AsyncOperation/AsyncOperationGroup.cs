namespace E.Data
{
    public abstract class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        /// <summary>
        /// total number of tasks
        /// </summary>
        public abstract int TotalTasks { get; }

        /// <summary>
        /// successful tasks
        /// </summary>
        public abstract int SuccessfulTasks { get; }

        /// <summary>
        /// faild tasks
        /// </summary>
        public abstract int FaildTasks { get; }

        /// <summary>
        /// SuccessfulTasks + FaildTasks
        /// </summary>
        public int CompletedTasks { get { return SuccessfulTasks + FaildTasks; } }
    }
}