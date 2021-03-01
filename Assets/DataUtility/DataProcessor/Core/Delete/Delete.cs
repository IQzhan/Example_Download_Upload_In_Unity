namespace E.Data
{
    public partial class DataProcessor
    {
        public DeleteAsyncOperation Delete(in string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return Delete(targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public DeleteAsyncOperation Delete(in System.Uri target)
        {
            if (Check(in target, out DataStream targetStream, out DeleteAsyncOperationImplement asyncOperation))
            {
                commandHandler?.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            if (asyncOperation.IsClosed) return;
                            asyncOperation.IsWorking = true;
                            targetStream.Timeout = asyncOperation.Timeout;
                            targetStream.SetAccount(asyncOperation.targetAccount.username, asyncOperation.targetAccount.password);
                            if (asyncOperation.IsClosed) return;
                            if (targetStream.TestConnection(asyncOperation.ForceTestConnection))
                            { asyncOperation.progress = 0.1f; }
                            else throw new System.Exception(@"connecting faild.");
                            if (targetStream.Exists)
                            {
                                if (asyncOperation.IsClosed) return;
                                if (targetStream.Delete())
                                { asyncOperation.progress = 1; }
                                else { throw new System.Exception(@"delete faild."); }
                            }
                            else { asyncOperation.progress = 1; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            throw new System.Exception("cause an error while delete from " + targetStream?.uri, e);
                        }
                    }
                    void cleanTask()
                    {
                        asyncOperation.Close();
                        targetStream?.Dispose();
                        targetStream = null;
                        commandHandler?.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(taskAction, cleanTask);
                });
            }
            return asyncOperation;
        }
    }
}