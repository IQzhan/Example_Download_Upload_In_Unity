namespace E.Data
{
    public partial class DataProcessor
    {
        private bool Check(in System.IO.Stream data, in System.Uri target, out DataStream targetStream, out CloneAsyncOperationImplement asyncOperation)
        {
            targetStream = null;
            asyncOperation = null;
            try
            {
                if (data == null)
                    throw new System.ArgumentNullException("data");
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
                asyncOperation = new CloneAsyncOperationImplement();
                TryAddAsyncOperation(asyncOperation);
                return true;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
        }

        private bool Check(in byte[] data, in System.Uri target, out DataStream targetStream, out CloneAsyncOperationImplement asyncOperation)
        {
            targetStream = null;
            asyncOperation = null;
            try
            {
                if (data == null) 
                    throw new System.ArgumentNullException("data");
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
                asyncOperation = new CloneAsyncOperationImplement();
                TryAddAsyncOperation(asyncOperation);
                return true;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
        }

        private bool Check(in System.Uri source, in System.Uri target,
            out DataStream sourceStream, out DataStream targetStream, out CloneAsyncOperationImplement asyncOperation)
        {
            sourceStream = null;
            targetStream = null;
            asyncOperation = null;
            try
            {
                if (!(source != null && source.IsAbsoluteUri)) 
                    throw new System.ArgumentException("must be absolute uri", "source");
                sourceStream = streamFactory.GetStream(source);
                if (target != null)
                {
                    if (!target.IsAbsoluteUri) throw new System.ArgumentException("must be absollute uri", "target");
                    targetStream = streamFactory.GetStream(target);
                }
                asyncOperation = new CloneAsyncOperationImplement();
                TryAddAsyncOperation(asyncOperation);
                return true;
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
        }

        private class CloneAsyncOperationImplement : CloneAsyncOperation
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new byte[] Data { get { return base.Data; } set { base.Data = value; } }

            public new long Size { get { return base.Size; } set { base.Size = value; } }

            public new long ProcessedBytes { get { return base.ProcessedBytes; } set { base.ProcessedBytes = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }
    }
}
