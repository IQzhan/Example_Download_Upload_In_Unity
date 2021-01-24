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
            }
            return null;
        }

        public DeleteAsyncOperation Delete(in System.Uri target)
        {
            if (Check(in target, out DataStream targetStream, out DeleteAsyncOperationImplement asyncOperation))
            {
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            asyncOperation.IsWorking = true;
                            if (targetStream != null && targetStream.Exists && targetStream.Delete())
                            { asyncOperation.Progress = 1; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogException(e);
                        }
                        finally
                        {
                            targetStream?.Dispose();
                            targetStream = null;
                            asyncOperation?.Close();
                            commandHandler.AddCommand(() =>
                            {
                                asyncOperation.onClose?.Invoke();
                            });
                        }
                    }
                    taskHandler.AddTask(taskAction);
                });
            }
            return asyncOperation;
        }

    }
}
