namespace E.Data
{
    public class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        public int TotalTasks { get; protected set; }

        public int SuccessfulTasks { get; protected set; }

        public int CompletedTasks { get; protected set; }

        public override double Progress => (double)SuccessfulTasks / TotalTasks;
    }
}
