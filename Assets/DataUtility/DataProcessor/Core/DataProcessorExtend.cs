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
            //TODO
        }
    }
}
