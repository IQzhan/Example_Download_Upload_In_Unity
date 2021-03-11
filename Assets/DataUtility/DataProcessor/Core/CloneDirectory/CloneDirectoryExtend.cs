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
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
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

        private string GetOriginalString(in System.Uri uri)
        {
            if (uri.IsFile)
            { return uri.LocalPath; }
            else if ((uri.Scheme == System.Uri.UriSchemeHttp) || (uri.Scheme == System.Uri.UriSchemeHttps))
            { return uri.OriginalString; }
            else if (uri.Scheme == System.Uri.UriSchemeFtp)
            { return uri.OriginalString; }
            else { throw new System.ArgumentException("Unsupported uri scheme.", "uri"); }
        }

        private class CloneDirectoryAsyncOperationImplement : CloneDirectoryAsyncOperation
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new long Size { get { return base.Size; } set { base.Size = value; } }

            private long processedBytes = 0;

            private CloneAsyncOperation currentCloneAsyncOperation;

            public void SetCurrentCloneAsyncOperation(CloneAsyncOperation currentCloneAsyncOperation)
            {
                if(this.currentCloneAsyncOperation != currentCloneAsyncOperation)
                {
                    if (this.currentCloneAsyncOperation != null)
                    { processedBytes += this.currentCloneAsyncOperation.Size; }
                    this.currentCloneAsyncOperation = currentCloneAsyncOperation;
                }
            }

            private long GetProcessedBytes()
            {
                long value;
                if (currentCloneAsyncOperation != null)
                { value = processedBytes + currentCloneAsyncOperation.ProcessedBytes; }
                else { value = processedBytes; }
                return value;
            }

            public override long ProcessedBytes { get { return GetProcessedBytes(); } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }
        }
    }
}
