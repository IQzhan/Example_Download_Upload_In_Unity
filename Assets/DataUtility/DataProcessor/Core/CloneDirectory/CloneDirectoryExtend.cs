namespace E.Data
{
    public partial class DataProcessor
    {
        private bool Check(in System.Uri source, in System.Uri target, out CloneDirectoryAsyncOperationImplement asyncOperation)
        {
            asyncOperation = null;
            try
            {
                if (!(source != null && source.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "source");
                if (target != null)
                {
                    if (!target.IsAbsoluteUri) throw new System.ArgumentException("must be absollute uri", "target");
                }
                asyncOperation = new CloneDirectoryAsyncOperationImplement();
                TryAddAsyncOperation(asyncOperation);
                return true;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
        }

        private class CloneDirectoryAsyncOperationImplement : CloneDirectoryAsyncOperation
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new long Size { get { return base.Size; } set { base.Size = value; } }

            public new long ProcessedBytes { get { return base.ProcessedBytes; } set { base.ProcessedBytes = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }
    }
}
