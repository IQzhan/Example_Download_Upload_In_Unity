namespace E.Data
{
    public partial class Cloner
    {
        public void Delete(in string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                Delete(targetUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
            }
        }

        public void Delete(in System.Uri target) 
        {
            if(Check(in target, out IStream targetStream))
            {
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            if (targetStream!=null && targetStream.Exists && targetStream.Delete())
                            { }
                        }
                        catch (System.Exception e)
                        {
                            ClonerDebug.LogException(e);
                        }
                    }
                    taskHandler.AddTask(taskAction);
                });
            }
        }

        public ClonerAsyncOperation Clone(in byte[] data, in string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                return Clone(data, targetUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return null;
            }
        }

        public ClonerAsyncOperation Clone(byte[] data, in System.Uri target)
        {
            if (Check(in data, in target, out IStream targetStream, out ClonerAsyncOperationImplement request))
            {
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            if (targetStream != null)
                            {
                                request.IsConnecting = true;
                                if (targetStream.Exists && targetStream.Delete()) { };
                                if (!targetStream.Create()) return;
                                byte[] temp = new byte[CacheSize];
                                long currentPosition = 0;
                                long length = data.LongLength;
                                request.Size = length;
                                request.IsConnecting = false;
                                request.IsProcessing = true;
                                while (currentPosition < length)
                                {
                                    long remainCount = length - currentPosition;
                                    int readCount = remainCount > CacheSize ? CacheSize : (int)remainCount;
                                    for (int i = 0; i < readCount; i++)
                                    {
                                        temp[i] = data[currentPosition + i];
                                    }
                                    targetStream.Write(temp, 0, readCount);
                                    currentPosition += readCount;
                                    request.ProcessedBytes = currentPosition;
                                }
                            }
                            else throw new System.IO.IOException("target is null");
                        }
                        catch (System.Exception e)
                        {
                            request.IsError = true;
                            ClonerDebug.LogException(e);
                        }
                        finally
                        {
                            targetStream?.Dispose();
                            request.Close();
                            commandHandler.AddCommand(() =>
                            {
                                request.onClose?.Invoke();
                            });
                        }
                    }
                    taskHandler.AddTask(taskAction);
                });
            }
            return request;
        }

        public ClonerAsyncOperation Clone(in string source)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                return Clone(sourceUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return null;
            }
        }

        public ClonerAsyncOperation Clone(in string source, in string target)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                System.Uri targetUri = new System.Uri(target);
                return Clone(sourceUri, targetUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return null;
            }
        }

        public ClonerAsyncOperation Clone(in System.Uri source)
        {
            return Clone(source, null);
        }

        /// <summary>
        /// Clone data from source to target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ClonerAsyncOperation Clone(in System.Uri source, in System.Uri target)
        {
            if (Check(in source, in target,
                out IStream sourceStream,
                out IStream targetStream,
                out ClonerAsyncOperationImplement request))
                commandHandler.AddCommand(() =>
                {
                    void commandAction()
                    {
                        byte[] data = null;
                        try
                        {
                            request.IsWorking = true;
                            request.IsConnecting = true;
                            if (sourceStream != null) sourceStream.Timeout = request.Timeout;
                            if (targetStream != null) targetStream.Timeout = request.Timeout;
                            bool sourceExists = sourceStream != null && sourceStream.Exists;
                            bool targetExists = targetStream != null && targetStream.Exists;
                            if(!sourceExists && !targetExists)
                            { throw new System.IO.IOException("source and target are both not exists."); }
                            string sourceVersion = sourceExists ? sourceStream.Version : null;
                            string targetVersion = targetExists ? targetStream.Version : null;
                            bool versionChanged = sourceExists && ((sourceVersion == null) || (sourceVersion != targetVersion));
                            if (targetExists && versionChanged)
                            { 
                                if (!targetStream.Delete()) return; 
                                targetExists = false; 
                            }
                            if (sourceExists && targetStream != null && !targetExists)
                            {
                                if (!targetStream.Create()) return;
                                targetExists = true;
                            }
                            bool complete = targetExists && targetStream.Complete;
                            bool targetCanRead = targetExists && targetStream.CanRead;
                            bool targetCanWrite = targetExists && targetStream.CanWrite;
                            bool sourceCanRead = sourceExists && sourceStream.CanRead;
                            //init length
                            long length = -1;
                            if (complete) length = targetStream.Length;
                            else if (sourceExists) length = sourceStream.Length;
                            if (length < 0) throw new System.IO.IOException("cause an error while try to get length of data.");
                            request.Size = length;
                            if (request.LoadData) { data = new byte[length]; }
                            long currentPosition = 0;
                            byte[] temp = new byte[CacheSize];
                            request.IsConnecting = false;
                            //preload form target if exists and need
                            if (request.LoadData && targetExists && targetCanRead
                            && (sourceExists || (!sourceExists && complete)) )
                            {
                                request.IsProcessing = true;
                                long targetLength = targetStream.Length;
                                while (currentPosition < targetLength)
                                {
                                    int readCount = targetStream.Read(temp, 0, CacheSize);
                                    for (int i = 0; i < readCount; i++)
                                    {
                                        data[currentPosition + i] = temp[i];
                                    }
                                    currentPosition += readCount;
                                    request.ProcessedBytes = currentPosition;
                                }
                            }
                            //set currentPosition to target length if exists
                            if (targetExists)
                            { 
                                currentPosition = targetStream.Length;
                                request.ProcessedBytes = currentPosition;
                                if (currentPosition == length) { targetStream.Complete = true; return; }
                            }
                            //get from source and add to target and load if need
                            if (sourceCanRead)
                            {
                                request.IsProcessing = true;
                                sourceStream.Position = currentPosition;
                                while (currentPosition < length)
                                {
                                    int readCount = sourceStream.Read(temp, 0, CacheSize);
                                    if (targetCanWrite) targetStream.Write(temp, 0, readCount);
                                    if (request.LoadData)
                                    {
                                        for (int i = 0; i < readCount; i++)
                                        { data[currentPosition + i] = temp[i]; }
                                    }
                                    currentPosition += readCount;
                                    request.ProcessedBytes = currentPosition;
                                }
                                if (currentPosition == length) { targetStream.Complete = true; return; }
                            }
                        }
                        catch (System.Exception e)
                        {
                            request.IsError = true;
                            data = null;
                            ClonerDebug.LogException(e);
                        }
                        finally
                        {
                            sourceStream?.Dispose();
                            targetStream?.Dispose();
                            sourceStream = null;
                            targetStream = null;
                            request.Data = data;
                            request.Close();
                            commandHandler.AddCommand(() =>
                            {
                                request.onClose?.Invoke();
                            });
                        }
                    }
                    taskHandler.AddTask(commandAction);
                });
            return request;
        }
    }
}
