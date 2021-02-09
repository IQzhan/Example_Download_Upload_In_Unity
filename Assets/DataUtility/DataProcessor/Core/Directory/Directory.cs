using System.Collections.Generic;

namespace E.Data
{
    public partial class DataProcessor
    {
        public DirectoryAsyncOperation ListDirectory(string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return ListDirectory(targetUri);
            }
            catch(System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public DirectoryAsyncOperation ListDirectory(System.Uri target)
        {
            //HashSet<string> dirs = null;

            return null;
        }
    }
}