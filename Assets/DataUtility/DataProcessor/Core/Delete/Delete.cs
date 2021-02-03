﻿namespace E.Data
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
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            asyncOperation.IsWorking = true;
                            targetStream.Timeout = asyncOperation.Timeout;
                            targetStream.SetAccount(asyncOperation.targetAccount.username, asyncOperation.targetAccount.password);
                            if (targetStream.Exists && targetStream.Delete())
                            { asyncOperation.Progress = 1; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogError("cause an error while delete from " + targetStream.uri
                                + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace);
                        }
                    }
                    void cleanTask()
                    {
                        targetStream?.Dispose();
                        targetStream = null;
                        asyncOperation?.Close();
                        commandHandler.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(asyncOperation, taskAction, cleanTask);
                });
            }
            return asyncOperation;
        }
    }
}