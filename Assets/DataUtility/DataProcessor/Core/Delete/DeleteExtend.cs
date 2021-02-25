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
                TryAddAsyncOperation(asyncOperation);
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
            public double progress;

            public override double Progress => progress;

            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }
    }
}