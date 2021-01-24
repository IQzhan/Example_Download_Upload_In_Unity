namespace E.Data
{
    public partial class DataProcessor
    {
        private bool Check(in System.Uri target, out DataStream targetStream, out DeleteAsyncOperationImplement asyncOperation)
        {
            targetStream = null;
            asyncOperation = null;
            try
            {
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
                asyncOperation = new DeleteAsyncOperationImplement();
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
            return true;
        }

        private class DeleteAsyncOperationImplement : DeleteAsyncOperation
        {
            public new double Progress { get { return base.Progress; } set { progress = value; } }

            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }
    }
}
