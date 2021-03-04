namespace E.Data
{
    public partial class DataProcessor
    {
        private bool Check(in System.Uri source, in System.Uri target, out CloneDirectoryAsyncOperationImplement asyncOperation)
        {
            asyncOperation = null;

            return false;
        }

        private class CloneDirectoryAsyncOperationImplement : CloneDirectoryAsyncOperation
        {
            public double progress;

            public override double Progress => progress;
        }
    }
}
