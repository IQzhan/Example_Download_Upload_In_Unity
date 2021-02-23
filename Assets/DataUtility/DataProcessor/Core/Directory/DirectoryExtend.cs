using System.Collections.Generic;

namespace E.Data
{
    public partial class DataProcessor
    {
        private bool Check(in System.Uri target, out DataStream targetStream, out DirectoryAsyncOperationImplement asyncOperation)
        {
            targetStream = null;
            asyncOperation = null;
            try
            {
                if (!(target != null && target.IsAbsoluteUri))
                    throw new System.ArgumentException("must be absolute uri", "target");
                targetStream = streamFactory.GetStream(target);
                asyncOperation = new DirectoryAsyncOperationImplement();
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return false;
            }
            return true;
        }

        private class DirectoryAsyncOperationImplement : DirectoryAsyncOperation
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new bool IsError { get { return base.IsError; } set { base.IsError = value; } }

            public new double Progress { get { return base.progress; } set { base.progress = value; } }

            public new SortedList<string, DataStream.FileSystemEntry> Entries { get { return base.Entries; } set { base.Entries = value; } }
        }
    }
}
