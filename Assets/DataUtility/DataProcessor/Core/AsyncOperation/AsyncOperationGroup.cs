namespace E.Data
{
    public abstract class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        /// <summary>
        /// total number of tasks
        /// </summary>
        public int TotalTasks { get; protected set; }

        /// <summary>
        /// successful tasks
        /// </summary>
        public int SuccessfulTasks { get; protected set; }

        /// <summary>
        /// faild tasks
        /// </summary>
        public int FaildTasks { get; protected set; }

        /// <summary>
        /// SuccessfulTasks + FaildTasks
        /// </summary>
        public int CompletedTasks { get { return SuccessfulTasks + FaildTasks; } }
    }
}