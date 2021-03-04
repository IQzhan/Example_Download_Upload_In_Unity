namespace E.Data
{
    public partial class DataProcessor
    {
        public CloneDirectoryAsyncOperation CloneDirectory(in string source, in string target)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                System.Uri targetUri = new System.Uri(target);
                return CloneDirectory(sourceUri, targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneDirectoryAsyncOperation CloneDirectory(in System.Uri source, in System.Uri target)
        {
            if(Check(source, target, out CloneDirectoryAsyncOperationImplement asyncOperation))
            {

            }
            return asyncOperation;
        }
    }
}