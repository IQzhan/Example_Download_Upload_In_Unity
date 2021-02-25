namespace E.Data
{
    public class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        public int TotalTasks { get; }

        public int SuccessfulTasks { get; }

        public int FaildTasks { get; }

        public int CompletedTasks { get { return SuccessfulTasks + FaildTasks; } }

        public override double Progress { get; }
    }
}
