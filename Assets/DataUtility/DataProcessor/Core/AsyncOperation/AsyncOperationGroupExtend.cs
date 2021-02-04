namespace E.Data
{
    public partial class DataProcessor
    {
        private AsyncOperationGroupImplement asyncOperationGroup;

        public AsyncOperationGroup StartAsyncOperationGroup()
        {
            asyncOperationGroup = new AsyncOperationGroupImplement();
            return asyncOperationGroup;
        }

        public void EndAsyncOperationGroup()
        {
            asyncOperationGroup.Close();
            asyncOperationGroup = null;
        }

        private class AsyncOperationGroupImplement : AsyncOperationGroup 
        {
            public new int TotalTasks { get { return base.TotalTasks; } set { base.TotalTasks = value; } }

            public new int SuccessfulTasks { get { return base.SuccessfulTasks; } set { base.SuccessfulTasks = value; } }

            public new int CompletedTasks { get { return base.CompletedTasks; } set { base.CompletedTasks = value; } }
        }
    }
}