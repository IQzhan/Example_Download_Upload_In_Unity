namespace E.Data
{
    public partial class DataProcessor
    {
        public DirectoryAsyncOperation GetFileSystemEntries(string target, bool topOnly = false)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return GetFileSystemEntries(targetUri, topOnly);
            }
            catch(System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public DirectoryAsyncOperation GetFileSystemEntries(System.Uri target, bool topOnly = false)
        {
            if(Check(target, out DataStream targetStream, out DirectoryAsyncOperationImplement asyncOperation))
            { commandHandler?.AddCommand(() => { taskHandler?.AddTask(asyncOperation, taskAction, cleanAction); }); }
            return asyncOperation; 
            void taskAction()
            {
                try
                {
                    if (asyncOperation.IsClosed) return;
                    asyncOperation.IsWorking = true;
                    targetStream.Timeout = asyncOperation.Timeout;
                    targetStream.SetAccount(asyncOperation.targetAccount.username, asyncOperation.targetAccount.password);
                    targetStream.AsCollection = true;
                    if (asyncOperation.IsClosed) return;
                    if (targetStream.TestConnection(asyncOperation.ForceTestConnection))
                    { asyncOperation.Progress = 0.1f; }
                    else throw new System.Exception(@"connecting faild.");
                    bool targetExists = targetStream.Exists;
                    if (targetExists)
                    { 
                        asyncOperation.Entries = targetStream.GetFileSystemEntries(topOnly);
                        if(asyncOperation.Entries == null)
                        { throw new System.Exception("GetFileSystemEntries faild."); }
                        asyncOperation.Progress = 1;
                    }
                    else
                    { throw new System.IO.DirectoryNotFoundException("not exists."); }
                }
                catch(System.Exception e)
                {
                    asyncOperation.IsError = true;
                    DataProcessorDebug.LogError("cause an error while try list directory with " + targetStream.uri
                        + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace);
                }
            }
            void cleanAction()
            {
                targetStream?.Dispose(); targetStream = null;
                commandHandler?.AddCommand(() => { asyncOperation.onClose?.Invoke(); });
            }
        }
    }
}