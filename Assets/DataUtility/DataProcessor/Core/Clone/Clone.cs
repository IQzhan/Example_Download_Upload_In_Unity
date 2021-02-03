namespace E.Data
{
    public partial class DataProcessor
    {
        public CloneAsyncOperation Clone(in System.IO.Stream data, in string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return Clone(data, targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneAsyncOperation Clone(System.IO.Stream data, in System.Uri target)
        {
            if(Check(data, target, out DataStream targetStream, out CloneAsyncOperationImplement asyncOperation))
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
                            bool targetAllowed = targetStream.TestConnection();
                            if (!targetAllowed) throw new System.IO.IOException("target connecting faild.");
                            if (targetStream.Exists && !targetStream.Delete()) throw new System.IO.IOException("delete target faild.");
                            targetStream.LastModified = System.DateTime.Now;
                            if (!targetStream.Create()) throw new System.IO.IOException("craete target faild.");
                            byte[] temp = new byte[CacheSize];
                            long currentPosition = 0;
                            long length = data.Length;
                            data.Position = 0;
                            asyncOperation.Size = length;
                            asyncOperation.ProcessedBytes = currentPosition;
                            while (currentPosition < length)
                            {
                                long remainCount = length - currentPosition;
                                int readCount = remainCount > CacheSize ? CacheSize : (int)remainCount;
                                readCount = data.Read(temp, 0, readCount);
                                targetStream.Write(temp, 0, readCount);
                                currentPosition += readCount;
                                asyncOperation.ProcessedBytes = currentPosition;
                            }
                            if (currentPosition == length) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogError("cause an error while clone from byte[] to " + targetStream.uri
                                + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace);
                        }
                    }
                    void cleanTask()
                    {
                        targetStream.Dispose();
                        targetStream = null;
                        asyncOperation.Close();
                        commandHandler.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(asyncOperation, taskAction, cleanTask);
                });
            }
            return asyncOperation;
        }

        public CloneAsyncOperation Clone(in byte[] data, in string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return Clone(data, targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneAsyncOperation Clone(byte[] data, in System.Uri target)
        {
            if (Check(in data, in target, out DataStream targetStream, out CloneAsyncOperationImplement asyncOperation))
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
                            bool targetAllowed = targetStream.TestConnection();
                            if (!targetAllowed) throw new System.IO.IOException("target connecting faild.");
                            if (targetStream.Exists && !targetStream.Delete()) throw new System.IO.IOException("delete target faild.");
                            targetStream.LastModified = System.DateTime.Now;
                            if (!targetStream.Create()) throw new System.IO.IOException("craete target faild.");
                            byte[] temp = new byte[CacheSize];
                            long currentPosition = 0;
                            long length = data.LongLength;
                            asyncOperation.Size = length;
                            asyncOperation.ProcessedBytes = currentPosition;
                            while (currentPosition < length)
                            {
                                long remainCount = length - currentPosition;
                                int readCount = remainCount > CacheSize ? CacheSize : (int)remainCount;
                                for (int i = 0; i < readCount; i++)
                                { temp[i] = data[currentPosition + i]; }
                                targetStream.Write(temp, 0, readCount);
                                currentPosition += readCount;
                                asyncOperation.ProcessedBytes = currentPosition;
                            }
                            if (currentPosition == length) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogError("cause an error while clone from byte[] to " + targetStream.uri 
                                + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace);
                        }
                    }
                    void cleanTask()
                    {
                        targetStream.Dispose();
                        targetStream = null;
                        asyncOperation.Close();
                        commandHandler.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(asyncOperation, taskAction, cleanTask);
                });
            }
            return asyncOperation;
        }

        public CloneAsyncOperation Clone(in string source)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                return Clone(sourceUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneAsyncOperation Clone(in string source, in string target)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                System.Uri targetUri = new System.Uri(target);
                return Clone(sourceUri, targetUri);
            }
            catch (System.Exception e)
            {
                DataProcessorDebug.LogException(e);
                return null;
            }
        }

        public CloneAsyncOperation Clone(in System.Uri source)
        {
            return Clone(source, null);
        }

        /// <summary>
        /// Clone data from source to target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public CloneAsyncOperation Clone(in System.Uri source, in System.Uri target)
        {
            if (Check(in source, in target,
                out DataStream sourceStream,
                out DataStream targetStream,
                out CloneAsyncOperationImplement asyncOperation))
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        byte[] data = null;
                        try
                        {
                            asyncOperation.IsWorking = true;
                            if (sourceStream != null) 
                            {
                                sourceStream.Timeout = asyncOperation.Timeout;
                                sourceStream.SetAccount(asyncOperation.sourceAccount.username, asyncOperation.sourceAccount.password);
                            }
                            if (targetStream != null) 
                            {
                                targetStream.Timeout = asyncOperation.Timeout;
                                targetStream.SetAccount(asyncOperation.targetAccount.username, asyncOperation.targetAccount.password);
                            }
                            bool targetAllowed = targetStream != null && targetStream.TestConnection();
                            bool sourceExists = sourceStream != null && sourceStream.Exists;
                            bool targetExists = targetStream != null && targetStream.Exists;
                            if(!sourceExists && !targetExists)
                            { throw new System.IO.IOException("source and target are both not exists."); }
                            string sourceVersion = sourceExists ? sourceStream.Version : null;
                            string targetVersion = targetExists ? targetStream.Version : null;
                            bool versionChanged = sourceExists && ((sourceVersion == null) || (sourceVersion != targetVersion));
                            if (targetExists && versionChanged)
                            { 
                                if (!targetStream.Delete()) throw new System.IO.IOException("delete faild."); 
                                targetExists = false; 
                            }
                            if (targetAllowed && sourceExists && targetStream != null && !targetExists)
                            {
                                targetStream.LastModified = sourceStream.LastModified;
                                if (!targetStream.Create()) throw new System.IO.IOException("create faild.");
                                targetExists = true;
                            }
                            bool complete = targetExists && targetStream.Complete;
                            //init length
                            long length = -1;
                            if (complete) length = targetStream.Length;
                            else if (sourceExists) length = sourceStream.Length;
                            if (length < 0) throw new System.IO.IOException("cause an error while try to get length of data.");
                            asyncOperation.Size = length;
                            if (asyncOperation.LoadData) { data = new byte[length]; }
                            long currentPosition = 0;
                            byte[] temp = new byte[CacheSize];
                            //preload form target if exists and need
                            if (asyncOperation.LoadData && targetExists
                            && (sourceExists || (!sourceExists && complete)) )
                            {
                                long targetLength = targetStream.Length;
                                while (currentPosition < targetLength)
                                {
                                    int readCount = targetStream.Read(temp, 0, CacheSize);
                                    for (int i = 0; i < readCount; i++)
                                    {
                                        data[currentPosition + i] = temp[i];
                                    }
                                    currentPosition += readCount;
                                    asyncOperation.ProcessedBytes = currentPosition;
                                }
                            }
                            //set currentPosition to target length if exists
                            if (targetExists)
                            { 
                                currentPosition = targetStream.Length;
                                asyncOperation.ProcessedBytes = currentPosition;
                                if (currentPosition == length) { targetStream.Complete = true; return; }
                            }
                            //get from source and add to target and load if need
                            sourceStream.Position = currentPosition;
                            while (currentPosition < length)
                            {
                                int readCount = sourceStream.Read(temp, 0, CacheSize);
                                targetStream.Write(temp, 0, readCount);
                                if (asyncOperation.LoadData)
                                {
                                    for (int i = 0; i < readCount; i++)
                                    { data[currentPosition + i] = temp[i]; }
                                }
                                currentPosition += readCount;
                                asyncOperation.ProcessedBytes = currentPosition;
                            }
                            if (currentPosition == length) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            data = null;
                            DataProcessorDebug.LogError("cause an error while clone from " + sourceStream.uri + " to " + targetStream.uri
                                + System.Environment.NewLine + e.Message + System.Environment.NewLine + e.StackTrace);
                        }
                        finally
                        {
                            asyncOperation.Data = data;
                        }
                    }
                    void cleanTask()
                    {
                        sourceStream?.Dispose();
                        targetStream?.Dispose();
                        sourceStream = null;
                        targetStream = null;
                        asyncOperation.Close();
                        commandHandler.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(asyncOperation, taskAction, cleanTask);
                });
            return asyncOperation;
        }
    }
}
