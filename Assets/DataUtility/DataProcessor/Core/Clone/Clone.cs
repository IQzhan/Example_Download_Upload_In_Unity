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
                            bool targetAllowed = targetStream.TestConnection(asyncOperation.ForceTestConnection);
                            if (!targetAllowed) throw new System.IO.IOException("target connecting faild.");
                            if (asyncOperation.IsClosed) return;
                            if (targetStream.Exists && !targetStream.Delete()) throw new System.IO.IOException("delete target faild.");
                            if (asyncOperation.IsClosed) return;
                            targetStream.LastModified = System.DateTime.Now;
                            if (!targetStream.Create()) throw new System.IO.IOException("craete target faild.");
                            byte[] temp = new byte[CacheSize];
                            long currentPosition = 0;
                            long length = data.Length;
                            data.Position = 0;
                            asyncOperation.Size = length;
                            asyncOperation.processedBytes = currentPosition;
                            while (currentPosition < length)
                            {
                                if (asyncOperation.IsClosed) return;
                                long remainCount = length - currentPosition;
                                int readCount = remainCount > CacheSize ? CacheSize : (int)remainCount;
                                readCount = data.Read(temp, 0, readCount);
                                if (asyncOperation.IsClosed) return;
                                targetStream.Write(temp, 0, readCount);
                                currentPosition += readCount;
                                asyncOperation.processedBytes = currentPosition;
                            }
                            if (asyncOperation.IsClosed) return;
                            if (currentPosition == length) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogError("cause an error while clone from System.IO.Stream to " + targetStream?.uri
                                + System.Environment.NewLine + e.Message + e.StackTrace);
                            throw e;
                        }
                    }
                    void cleanTask()
                    {
                        asyncOperation.Close();
                        targetStream.Dispose();
                        targetStream = null;
                        commandHandler?.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(taskAction, cleanTask);
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
                            bool targetAllowed = targetStream.TestConnection(asyncOperation.ForceTestConnection);
                            if (!targetAllowed) throw new System.IO.IOException("target connecting faild.");
                            if (asyncOperation.IsClosed) return;
                            if (targetStream.Exists && !targetStream.Delete()) throw new System.IO.IOException("delete target faild.");
                            if (asyncOperation.IsClosed) return;
                            targetStream.LastModified = System.DateTime.Now;
                            if (asyncOperation.IsClosed) return;
                            if (!targetStream.Create()) throw new System.IO.IOException("craete target faild.");
                            byte[] temp = new byte[CacheSize];
                            long currentPosition = 0;
                            long length = data.LongLength;
                            asyncOperation.Size = length;
                            asyncOperation.processedBytes = currentPosition;
                            while (currentPosition < length)
                            {
                                if (asyncOperation.IsClosed) return;
                                long remainCount = length - currentPosition;
                                int readCount = remainCount > CacheSize ? CacheSize : (int)remainCount;
                                for (int i = 0; i < readCount; i++)
                                { temp[i] = data[currentPosition + i]; }
                                targetStream.Write(temp, 0, readCount);
                                currentPosition += readCount;
                                asyncOperation.processedBytes = currentPosition;
                            }
                            if (asyncOperation.IsClosed) return;
                            if (currentPosition == length) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            DataProcessorDebug.LogError("cause an error while clone from byte[] to " + targetStream?.uri
                                + System.Environment.NewLine + e.Message + e.StackTrace);
                            throw e;
                        }
                    }
                    void cleanTask()
                    {
                        asyncOperation.Close();
                        targetStream.Dispose();
                        targetStream = null;
                        commandHandler?.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(taskAction, cleanTask);
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
                commandHandler?.AddCommand(() =>
                {
                    void taskAction()
                    {
                        byte[] data = null;
                        try
                        {
                            if (asyncOperation.IsClosed) return;
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
                            bool sourceAllowed = sourceStream != null && sourceStream.TestConnection(asyncOperation.ForceTestConnection);
                            bool targetAllowed = targetStream != null && targetStream.TestConnection(asyncOperation.ForceTestConnection);
                            bool sourceExists = sourceAllowed && sourceStream.Exists;
                            bool targetExists = targetAllowed && targetStream.Exists;
                            if (asyncOperation.IsClosed) return;
                            if (!sourceExists && !targetExists)
                            { throw new System.IO.IOException("source and target are both not exists."); }
                            string sourceVersion = sourceExists ? sourceStream.Version : null;
                            string targetVersion = targetExists ? targetStream.Version : null;
                            bool versionChanged = sourceExists && ((sourceVersion == null) || (sourceVersion != targetVersion));
                            if (targetExists && versionChanged)
                            {
                                if (asyncOperation.IsClosed) return;
                                if (!targetStream.Delete()) throw new System.IO.IOException("delete target faild.");
                                targetExists = false;
                            }
                            if (targetAllowed && sourceExists && targetStream != null && !targetExists)
                            {
                                if (asyncOperation.IsClosed) return;
                                targetStream.LastModified = sourceStream.LastModified;
                                if (asyncOperation.IsClosed) return;
                                if (!targetStream.Create()) 
                                {
                                    throw new System.IO.IOException("create target faild."); 
                                }
                                targetExists = true;
                            }
                            if (asyncOperation.IsClosed) return;
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
                                    if (asyncOperation.IsClosed) return;
                                    int readCount = targetStream.Read(temp, 0, CacheSize);
                                    for (int i = 0; i < readCount; i++)
                                    { data[currentPosition + i] = temp[i]; }
                                    currentPosition += readCount;
                                    asyncOperation.processedBytes = currentPosition;
                                }
                            }
                            //set currentPosition to target length if exists
                            if (targetExists)
                            {
                                if (asyncOperation.IsClosed) return;
                                currentPosition = targetStream.Length;
                                asyncOperation.processedBytes = currentPosition;
                                if (currentPosition == length) { targetStream.Complete = true; return; }
                                targetStream.Position = currentPosition;
                            }
                            //get from source and add to target and load if need
                            sourceStream.Position = currentPosition;
                            while (currentPosition < length)
                            {
                                if (asyncOperation.IsClosed) return;
                                int readCount = sourceStream.Read(temp, 0, CacheSize);
                                if (asyncOperation.IsClosed) return;
                                if (targetExists) { targetStream.Write(temp, 0, readCount); }
                                if (asyncOperation.LoadData)
                                {
                                    for (int i = 0; i < readCount; i++)
                                    { data[currentPosition + i] = temp[i]; }
                                }
                                currentPosition += readCount;
                                asyncOperation.processedBytes = currentPosition;
                            }
                            if (asyncOperation.IsClosed) return;
                            if (currentPosition == length && targetExists) { targetStream.Complete = true; return; }
                        }
                        catch (System.Exception e)
                        {
                            asyncOperation.IsError = true;
                            data = null;
                            DataProcessorDebug.LogError("cause an error while clone from " + sourceStream?.uri + " to " + targetStream?.uri
                                + System.Environment.NewLine + e.Message + e.StackTrace);
                            throw e;
                        }
                        finally { asyncOperation.Data = data; }
                    }
                    void cleanTask()
                    {
                        asyncOperation.Close();
                        sourceStream?.Dispose();
                        targetStream?.Dispose();
                        sourceStream = null;
                        targetStream = null;
                        commandHandler?.AddCommand(() =>
                        { asyncOperation.onClose?.Invoke(); });
                    }
                    taskHandler.AddTask(taskAction, cleanTask);
                });
            return asyncOperation;
        }
    }
}