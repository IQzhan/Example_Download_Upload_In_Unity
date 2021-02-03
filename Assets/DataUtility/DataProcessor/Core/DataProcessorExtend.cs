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
            public new long Total { get { return base.Total; } set { base.Total = value; } }

            public new long Processed { get { return base.Processed; } set { base.Processed = value; } }
        }
    }
}